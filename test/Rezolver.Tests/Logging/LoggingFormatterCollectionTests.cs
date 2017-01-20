using Rezolver.Logging;
using Rezolver.Targets;
using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Logging
{
    public class LoggingFormatterCollectionTests
    {
		[Fact]
		public void DefaultFormatterShouldFormatNumberCorrectly()
		{
			var s = ObjectFormatterCollection.Default.Format("{0:0.000}", 1.234);
			Assert.Equal(s, (1.234).ToString("0.000"));
		}

		[Fact]
		public void ShouldFormatCustomTypeCorrectlyViaStandardFormatString()
		{
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormatter(new LoggedTypeFormatter());
			var toLog = new LoggedType() { IntValue = 3, StringValue = "Wa" };
			var result = loggingFormatters.Format("{0}", toLog);
			Assert.Equal(LoggedTypeFormatter.Expected(toLog), result);
		}

		[Fact]
		public void ShouldReFormatCustomTypeCorrectlyFromInterpolatedString()
		{
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormatter(new LoggedTypeFormatter());
			var toLog = new LoggedType() { IntValue = 5, StringValue = "Yo" };
			var result = loggingFormatters.Format(format: $"{ toLog }");
			Assert.Equal(LoggedTypeFormatter.Expected(toLog), result);
		}
		
		[Fact]
		public void ShouldFormatCustomTypeAfterAddingFromAssembly()
		{
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormattersFromAssembly(typeof(LoggingFormatterCollectionTests).GetTypeInfo().Assembly);
			var toLog = new LoggedType() { IntValue = 4, StringValue = "W" };
			var result = loggingFormatters.Format(format: $"{ toLog }");
			Assert.Equal(LoggedTypeFormatter.Expected(toLog), result);
		}

		[Fact]
		public void ShouldChooseFormatterForBaseType()
		{
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormatter(new LoggedTypeFormatter());
			var toLog = new LoggedTypeExtra() { DecimalValue = 1.0M, IntValue = 1, StringValue = "A string" };
			var result = loggingFormatters.Format(format: $"{ toLog }");
			Assert.Equal(LoggedTypeFormatter.Expected(toLog), result);
		}

		[Fact]
		public void ShouldChooseFormatterForInterface()
		{
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormatter(typeof(ILoggedType_Decimal), new LoggedTypeInterfaceFormatter());
			var toLog = new LoggedTypeExtra() { DecimalValue = 1.75M };
			var result = loggingFormatters.Format("{0}", toLog);
			Assert.Contains(LoggedTypeInterfaceFormatter.Expected((ILoggedType_Decimal)toLog), result);
			Assert.Contains(LoggedTypeInterfaceFormatter.Expected((ILoggedType_Int)toLog), result);
			Assert.Contains(LoggedTypeInterfaceFormatter.Expected((ILoggedType_String)toLog), result);
		}

		[Fact]
		public void ShouldChooseAutoRegisteredFormatterForInterface()
		{
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormattersFromAssembly(TypeHelpers.GetAssembly(this.GetType()));
			var toLog = new LoggedTypeExtra() { IntValue= 7, StringValue = "combined", DecimalValue = 2.23M };
			var result = loggingFormatters.Format("{0}", toLog);
			Assert.Contains(LoggedTypeInterfaceFormatter.Expected((ILoggedType_Decimal)toLog), result);
			Assert.Contains(LoggedTypeInterfaceFormatter.Expected((ILoggedType_Int)toLog), result);
			Assert.Contains(LoggedTypeInterfaceFormatter.Expected((ILoggedType_String)toLog), result);
		}

		[Fact]
		public void ShouldPassThroughFormatPlaceholderFormatStrings()
		{
			//test demonstrates how the loggingformattercollection's format implementation will support 
			//'{n:[format]}' placeholders
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormattersFromAssembly(TypeHelpers.GetAssembly(this.GetType()));
			var toLog = new LoggedType_Decimal() { DecimalValue = 2.23M };
			var result = loggingFormatters.Format("{0:alt}", toLog);
			Assert.Equal(LoggedTypeInterfaceFormatter.Expected(toLog, "alt"), result);
		}

		[Fact]
		public void ShouldPassThroughFormatPlaceholderFormatStringsFromStringFormat()
		{
			//similar test to above, except this time we're checking whether passing the logging formatter
			//collection as the IFormatProvider to string.Format correctly receives the 'alt' format string
			//and is then handled properly.
			var loggingFormatters = new ObjectFormatterCollection();
			loggingFormatters.AddFormattersFromAssembly(TypeHelpers.GetAssembly(this.GetType()));
			var toLog = new LoggedType_Decimal() { DecimalValue = 2.23M };
			var result = string.Format(loggingFormatters, "{0:alt}", toLog);
			Assert.Equal(LoggedTypeInterfaceFormatter.Expected(toLog, "alt"), result);
		}

		[Fact]
		public void Foo()
		{
			var target = ConstructorTarget.Auto(typeof(NoDefaultConstructor2));
			var target2 = ConstructorTarget.WithArgs<NoDefaultConstructor2>(new { value = (1).AsObjectTarget() });
			var str = ObjectFormatterCollection.Default.Format(target);
			var str2 = ObjectFormatterCollection.Default.Format(target2);
			var logger = new Xunit.ConsoleRunnerLogger(true);
			logger.LogMessage(str);
			logger.LogMessage(str2);
		}
    }
}

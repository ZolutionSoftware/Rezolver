using Rezolver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Logging
{
    public class ObjectFormattersTests
    {
		[Fact]
		public void DefaultFormatterShouldFormatNumberCorrectly()
		{
			var s = LoggingFormatterCollection.Default.Format("{0:0.000}", 1.234);
			Assert.Equal(s, (1.234).ToString("0.000"));
		}

		private class CustomType
		{
			static CustomType()
			{

			}

			public int Count { get; set; }
			public string StringValue { get; set; }
		}

		private class CustomTypeFormatter : LoggingFormatter<CustomType>
		{
			public override string Format(CustomType obj, string format = null, LoggingFormatterCollection formatters = null)
			{
				return $"Custom { string.Join(",", Enumerable.Range(0, obj.Count).Select(i => obj.StringValue)) }";
			}
		}

		[Fact]
		public void ShouldFormatCustomTypeCorrectlyViaStandardFormatString()
		{
			var loggingFormatters = new LoggingFormatterCollection();
			loggingFormatters.AddFormatter(new CustomTypeFormatter());
			var result = loggingFormatters.Format("{0}", new CustomType() { Count = 3, StringValue = "Wa" });
			Assert.Equal("Custom Wa,Wa,Wa", result);
		}

		[Fact]
		public void ShouldReFormatCustomTypeCorrectlyFromInterpolatedString()
		{
			var loggingFormatters = new LoggingFormatterCollection();
			loggingFormatters.AddFormatter(new CustomTypeFormatter());
			var result = loggingFormatters.Format(format: $"{ new CustomType() { Count = 5, StringValue = "Yo" }}");
			Assert.Equal("Custom Yo,Yo,Yo,Yo,Yo", result);
		}
    }
}

using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
	public class GenericConstructorExamples
	{

		[Fact]
		public void ShouldGetDataFormatters()
		{
			//<example1>
			var container = new Container();
			container.RegisterType(typeof(DataFormatter<>));

			Assert.NotNull(container.Resolve<DataFormatter<int>>());
			Assert.NotNull(container.Resolve<DataFormatter<string>>());
			Assert.NotNull(container.Resolve<DataFormatter<IMyService>>());
			//</example1>
		}


		[Fact]
		public void ShouldGetIDataFormatters()
		{
			//<example2>
			// Same as above, just requesting via interface
			var container = new Container();
			container.RegisterType(typeof(DataFormatter<>), typeof(IDataFormatter<>));

			Assert.NotNull(container.Resolve<IDataFormatter<int>>());
			Assert.NotNull(container.Resolve<IDataFormatter<string>>());
			Assert.NotNull(container.Resolve<IDataFormatter<IMyService>>());
			//</example2>
		}

		[Fact]
		public void ShouldInjectIDataFormatterForGenericType()
		{
			//<example3>
			var container = new Container();
			container.RegisterType(typeof(RequiresIDataFormatter<>));
			container.RegisterType(typeof(DataFormatter<>), typeof(IDataFormatter<>));

			Assert.NotNull(container.Resolve<RequiresIDataFormatter<int>>().Formatter);
			Assert.NotNull(container.Resolve<RequiresIDataFormatter<string>>().Formatter);
			Assert.NotNull(container.Resolve<RequiresIDataFormatter<IMyService>>().Formatter);
			//</example3>
		}

		[Fact]
		public void ShouldCorrectlyMapJumbledTypeArgsOfBaseOfABase()
		{
			//<example10>
			var container = new Container();
			container.RegisterType(typeof(FinalGeneric<,,>), typeof(BaseGeneric<,,>));

			// Type arguments should be transformed as follows:
			// - BaseGeneric<T1, T2, T3> is a base of
			// - MidGeneric<T3, T2, T1>, which is a base of
			// - FinalGeneric<T2, T3, T1>
			// Which is what the container should give us

			var result = Assert.IsType<FinalGeneric<T2, T3, T1>>(
				container.Resolve<BaseGeneric<T1, T2, T3>>()
			);
			//</example10>
		}

		[Fact]
		public void ShouldCorrectlyMapClosingGeneric()
		{
			//<example11>
			var container = new Container();
			container.RegisterType(typeof(ClosingGeneric<,>), typeof(BaseGeneric<,,>));

			var result = Assert.IsType<ClosingGeneric<T2, T3>>(
				container.Resolve<BaseGeneric<string, T2, T3>>()
			);
			//</example11>
		}

		[Fact]
		public void ShouldCorrectlyMapSinglyNestedGeneric()
		{
			//<example20>
			var container = new Container();

			
			container.RegisterType(
				typeof(GenericEnumerableService<>),
				typeof(IGenericService<>).MakeGenericType(typeof(IEnumerable<>))
			);

			var result = Assert.IsType<GenericEnumerableService<string>>(
				container.Resolve<IGenericService<IEnumerable<string>>>()
			);
			//</example20>
		}

		[Fact]
		public void ShouldCorrectlyMapDoubleNestedGeneric()
		{
			//<example21>
			var container = new Container();

			// Even more .MakeGenericType jiggery pokery required here
			container.RegisterType(
				typeof(GenericEnumerableNullableService<>),
				typeof(IGenericService<>).MakeGenericType(
					typeof(IEnumerable<>).MakeGenericType(
						typeof(Nullable<>)
					)
				)
			);

			var result = Assert.IsType<GenericEnumerableNullableService<int>>(
				container.Resolve<IGenericService<IEnumerable<Nullable<int>>>>()
			);
			//</example21>
		}
	}
}

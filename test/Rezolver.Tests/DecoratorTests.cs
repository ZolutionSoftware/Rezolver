using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class DecoratorTests : TestsBase
	{
		[Fact]
		public void ShouldDecorateDecoratedType()
		{
			Builder builder = new Builder();
			builder.RegisterType<DecoratedType, IDecorated>();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IDecorated>();
			Assert.IsType<DecoratorType>(result);
			//yes, we've checked the type - but we also need to check the 
			//argument is passed into the constructor.  Obviously, we could do this
			//by null-checking in the constructor, but this is more fun.
			Assert.Equal("Hello World", result.DoSomething());
		}

		[Fact]
		public void ShouldDecorateDecoratedTypeAddedAfterDecorator()
		{
			Builder builder = new Builder();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			builder.RegisterType<DecoratedType, IDecorated>();
			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IDecorated>();
			Assert.IsType<DecoratorType>(result);

			Assert.Equal("Hello World", result.DoSomething());
		}

		[Fact]
		public void ShouldDecorateDecorator()
		{
			//see if stacking multiple decorators works.
			Builder builder = new Builder();
			builder.RegisterType<DecoratedType, IDecorated>();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			builder.RegisterDecorator<AnotherDecoratorType, IDecorated>();

			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IDecorated>();
			Assert.IsType<AnotherDecoratorType>(result);

			Assert.Equal("OMG: Hello World", result.DoSomething());
		}

		[Fact]
		public void ShouldCreateEnumerableOfDecoratedObjects()
		{
			Builder builder = new Builder();
			builder.RegisterType<DecoratedType, IDecorated>();
			builder.RegisterType<DecoratedType2, IDecorated>();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IEnumerable<IDecorated>>().ToArray();
			Assert.Equal(2, result.Length);
			Assert.Single(result, r => r.DoSomething() == "Hello World");
			Assert.Single(result, r => r.DoSomething() == "Goodbye World");
		}

		[Fact]
		public void ShouldCreateEnumerableOfDecoratedObjectsAddedAfterDecorator()
		{
			Builder builder = new Builder();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			builder.RegisterType<DecoratedType, IDecorated>();
			builder.RegisterType<DecoratedType2, IDecorated>();
			
			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IEnumerable<IDecorated>>().ToArray();
			Assert.Equal(2, result.Length);
			Assert.Single(result, r => r.DoSomething() == "Hello World");
			Assert.Single(result, r => r.DoSomething() == "Goodbye World");
		}

		[Fact]
		public void ShouldUseGenericDecorator()
		{
			Builder builder = new Builder();
			builder.RegisterType<StringHandler, IHandler<string>>();
			builder.RegisterType<DoubleHandler, IHandler<double>>();
			builder.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IHandler<string>>();
			var result2 = rezolver.Resolve<IHandler<double>>();
			Assert.IsType<GenericDecoratingHandler<string>>(result);
			Assert.IsType<GenericDecoratingHandler<double>>(result2);
		}

		[Fact]
		public void ShouldUseGenericDecoratorRegisteredBeforeTypes()
		{
			//check that the decorator is registration order agnostic.
			Builder builder = new Builder();
			builder.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			builder.RegisterType<StringHandler, IHandler<string>>();
			builder.RegisterType<DoubleHandler, IHandler<double>>();
			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IHandler<string>>();
			var result2 = rezolver.Resolve<IHandler<double>>();
			Assert.IsType<GenericDecoratingHandler<string>>(result);
			Assert.IsType<GenericDecoratingHandler<double>>(result2);
		}

		[Fact]
		public void ShouldDecorateGenericDecorator()
		{
			Builder builder = new Builder();
			builder.RegisterType<StringHandler, IHandler<string>>();
			builder.RegisterType<DoubleHandler, IHandler<double>>();
			builder.RegisterDecorator(typeof(GenericDecoratingHandler<>), typeof(IHandler<>));
			builder.RegisterDecorator(typeof(GenericDecoratingHandler2<>), typeof(IHandler<>));
			var rezolver = new Container(builder);
			var result = rezolver.Resolve<IHandler<string>>();
			Assert.IsType<GenericDecoratingHandler2<string>>(result);
			Assert.Equal("((This is a string: Hello World) Decorated) Decorated again :)", result.Handle("Hello World"));
		}
	}
}

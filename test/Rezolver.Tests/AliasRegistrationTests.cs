using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class AliasRegistrationTests
	{
		[Fact]
		public void AliasShouldWorkForBaseTypes()
		{
			Builder builder = new Builder();
			builder.RegisterObject(1);
			builder.RegisterAlias<object, int>();

			var container = new Container(builder);

			object o = container.Resolve<object>();

			Assert.IsType(typeof(int), o);
		}

		[Fact]
		public void AliasShouldWorkForDerivedTypes()
		{
			Builder builder = new Builder();
			builder.RegisterObject(1, typeof(object));
			builder.RegisterAlias<int, object>();

			var container = new Container(builder);
			int i = container.Resolve<int>();

			Assert.Equal(1, i);
		}
		
		public interface ISingletonTest
		{
			int ThisCounter { get; }
		}

		//now verify that singletons are honoured correctly
		public class SingletonTest : ISingletonTest
		{
			private static int _counter = 0;

			public static int LastCounter { get { return _counter; } }

			public int ThisCounter { get; private set; }
			public SingletonTest()
			{
				ThisCounter = ++_counter;
			}
		}

		[Fact]
		public void AliasShouldYieldSameSingleton()
		{
			Builder builder = new Builder();
			builder.RegisterSingleton<SingletonTest>();
			builder.RegisterAlias<ISingletonTest, SingletonTest>();

			var container = new Container(builder);
			var first = container.Resolve<SingletonTest>();
			var second = container.Resolve<SingletonTest>();
			var third = container.Resolve<ISingletonTest>();

			Assert.Equal(SingletonTest.LastCounter, first.ThisCounter);
			Assert.Equal(first.ThisCounter, second.ThisCounter);
			Assert.Equal(second.ThisCounter, third.ThisCounter);

			Assert.Same(first, second);
		}

	}
}

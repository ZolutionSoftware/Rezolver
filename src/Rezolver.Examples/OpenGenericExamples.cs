using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Examples
{
	[TestClass]
	public class OpenGenericExamples
	{
		#region direct registration
		public class OpenGeneric<T>
		{
			public T Instance { get; private set; }

			public OpenGeneric(T instance)
			{
				Instance = instance;
			}
		}

		public class GenericArg
		{
			public int Value { get; private set; }

			public GenericArg(int value)
			{
				Assert.AreEqual(7, value);

				Value = value;
			}
		}

		[TestMethod]
		public void RegisterOpenGenericDirectly()
		{
			var resolver = new DefaultRezolver();

			resolver.RegisterType<GenericArg>();
			//you can't pass an open generic type as a generic argument, so we 
			//have to use the overload that takes a type reference instead.
			resolver.RegisterType(typeof(OpenGeneric<>));
			resolver.RegisterObject(7);

			var instance1 = resolver.Resolve<OpenGeneric<int>>();
			Assert.AreEqual(7, instance1.Instance);

			var instance2 = resolver.Resolve<OpenGeneric<GenericArg>>();
			Assert.IsNotNull(instance2.Instance);
		}

		#endregion

		#region By open generic abstraction

		public interface IOpenGeneric1<T>
		{
			T Instance1 { get; }
		}

		public interface IOpenGeneric2<T, U>
		{
			T Instance2 { get; }
			U Instance3 { get; }
		}

		public class OpenGeneric1<T> : IOpenGeneric1<T>
		{

			public T Instance1
			{
				get;
				private set;
			}

			public OpenGeneric1(T instance1)
			{
				Instance1 = instance1;
			}
		}

		// notice here that the order of the generic arguments is reversed
		public class OpenGeneric2<T,U> : IOpenGeneric2<U, T>
		{

			public U Instance2
			{
				get;
				private set;
			}

			public T Instance3
			{
				get;
				private set;
			}

			public OpenGeneric2(U instance2, T instance3)
			{
				Instance2 = instance2;
				Instance3 = instance3;
			}
		}

		[TestMethod]
		public void RegisterByInterface1()
		{
			var resolver = new DefaultRezolver();
			resolver.RegisterType(typeof(OpenGeneric1<>), typeof(IOpenGeneric1<>));
			//we'll re-use the same GenericArg type as in the previous example
			resolver.RegisterType<GenericArg>();
			resolver.RegisterObject(7);

			var instance1 = resolver.Resolve<IOpenGeneric1<int>>();
			Assert.IsInstanceOfType(instance1, typeof(OpenGeneric1<int>));
			Assert.AreEqual(7, instance1.Instance1);

			var instance2 = resolver.Resolve<IOpenGeneric1<GenericArg>>();
			Assert.IsInstanceOfType(instance2, typeof(OpenGeneric1<GenericArg>));
		}

		[TestMethod]
		public void RegisterByInterface2()
		{
			var resolver = new DefaultRezolver();
			resolver.RegisterType(typeof(OpenGeneric2<,>), typeof(IOpenGeneric2<,>));
			resolver.RegisterType<GenericArg>();
			resolver.RegisterObject(7);

			var instance1 = resolver.Resolve <IOpenGeneric2<GenericArg, int>>();
			//because of the way the IOpenGeneric2<,> interface is mapped by OpenGeneric2<,>, 
			//the type arguments will be reversed
			Assert.IsInstanceOfType(instance1, typeof(OpenGeneric2<int, GenericArg>));
			Assert.AreEqual(7, instance1.Instance3);

			var instance2 = resolver.Resolve<IOpenGeneric2<int, GenericArg>>();
			Assert.IsInstanceOfType(instance2, typeof(OpenGeneric2<GenericArg, int>));
			Assert.AreEqual(7, instance2.Instance2);
		}
		#endregion

		#region nested generics
		//similar to the previous example, except now we have types that are going to express
		//dependencies via generics, and we're also going to have specific registrations for 
		//'partial specialisations' of generic interfaces
		public interface IComponent<TValue>
		{
			TValue Value { get; }
		}

		public interface IComponentContainer<TValue>
		{
			IComponent<TValue> Component { get; }
		}

		public class GenericComponent<TValue> : IComponent<TValue>
		{
			public TValue Value { get; private set; }
			public GenericComponent(TValue value)
			{
				Value = value;
			}
		}

		public class DateTimeComponent : IComponent<DateTime>
		{
			public DateTime Value { get; private set; }

			public DateTimeComponent(string parseableDateTimeString)
			{
				//clearly, a bit of a rubbish example but it should demonstrate it.
				Value = DateTime.Parse(parseableDateTimeString);
			}
		}

		//when TValue is anything other than datetime, then the component will be 
		//a GenericComponent<TValue>.  For DateTime, it'll use the special DateTimeComponent
		public class GenericComponentContainer<TValue> : IComponentContainer<TValue>
		{
			public IComponent<TValue> Component { get; private set; }
			//so here we've a dependency on a generic based on one of 
			//this type's own generic arguments
			public GenericComponentContainer(IComponent<TValue> component)
			{
				Component = component;
			}
		}

		[TestMethod]
		public void OpenGenericDependencyAndPartialSpecialisation()
		{
			var resolver = new DefaultRezolver();
			//register the open generic container
			resolver.RegisterType(typeof(GenericComponentContainer<>), typeof(IComponentContainer<>));
			//register the generic component
			resolver.RegisterType(typeof(GenericComponent<>), typeof(IComponent<>));
			//register the specialisation of IComponent<DateTime>
			resolver.RegisterType(typeof(DateTimeComponent), typeof(IComponent<DateTime>));
			resolver.RegisterObject(15);

			DateTime now = DateTime.Now;
			//take off the milliseconds as the default ToString()  will not produce a string
			//that contains the milliseconds or smaller.
			now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
			//string for the DateTimeComponent constructor
			resolver.RegisterObject(now.ToString());

			var container1 = resolver.Resolve<IComponentContainer<int>>();
			Assert.IsNotNull(container1);
			Assert.IsNotNull(container1.Component);
			Assert.IsInstanceOfType(container1.Component, typeof(GenericComponent<int>));
			Assert.AreEqual(15, container1.Component.Value);

			//resolving IComponentContainer<DateTime>, yields GenericComponentContainer<DateTime> which,
			//in turn, will request an instance of IComponent<DateTime> - which we have registered above
			//to be implemented by DateTimeComponent
			var container2 = resolver.Resolve<IComponentContainer<DateTime>>();
			Assert.IsNotNull(container2);
			Assert.IsNotNull(container2.Component);
			Assert.IsInstanceOfType(container2.Component, typeof(DateTimeComponent));
			Assert.AreEqual(now, container2.Component.Value);
		}
		#endregion

		#region an extension of the above example, showing how we can have a partial specialisation based on a nested open generic
		//almost decorator pattern, except you have to know you want a wrapped instance.
		//decorator support isn't yet in Rezolver.
		public class WrappedComponent<T> : IComponent<IComponent<T>>, IComponent<T>
		{
			public WrappedComponent(IComponent<T> inner)
			{
				Value = inner;
			}

			public IComponent<T> Value
			{
				get;
				private set;
			}

			T IComponent<T>.Value
			{
				get { return Value.Value; }
			}
		}

		[TestMethod]
		public void PartialSpecialisationOfNestedOpenGeneric(){
			var resolver = new DefaultRezolver();
			//register our 'standard' generic target
			resolver.RegisterType(typeof(GenericComponent<>), typeof(IComponent<>));
			//note here - we need more System.Type wizardry - the only way you can pass an open generic 
			//as a type argument to a generic type is via Type.MakeGenericType
			resolver.RegisterType(typeof(WrappedComponent<>), 
				typeof(IComponent<>).MakeGenericType(typeof(IComponent<>)));
			resolver.RegisterObject("Hello strange generic");

			var result = resolver.Resolve<IComponent<IComponent<string>>>();

			Assert.IsInstanceOfType(result, typeof(WrappedComponent<string>));
			Assert.IsInstanceOfType(result.Value, typeof(GenericComponent<string>));
		}

		#endregion

	}
}

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class GenericConstructorTargetTests : TestsBase
	{
		#region diagnostic stuff
		private string GetTypeReportString(Type type)
		{
			var genericArgs = type.GetGenericArguments() ?? Type.EmptyTypes;
			return string.Format("{0}<{1}>:", type.Name, string.Join(", ", genericArgs.Select(typeParam => string.Format("({0}{1})", typeParam.IsGenericParameter ? "*" : "", typeParam.Name))));
		}

		private void WriteType(Type type, string typeType)
		{
			Console.WriteLine("{0} {1}", typeType, GetTypeReportString(type));
			var interfaces = type.GetInterfaces();
			if (interfaces.Length != 0)
			{
				Console.WriteLine("Interfaces for {0}: ", type);
				foreach (var i in interfaces)
				{
					WriteType(i, "Interface");
				}
			}
			if (type.BaseType != null && type.BaseType != typeof(object))
				WriteType(type.BaseType, "Base");
		}

		//[TestMethod]
		public void ShouldBuildTypeParameterMap()
		{
			var types = new[] { typeof(IBaseInterface<>), typeof(IDerivedInterface<,>), typeof(IFinalInterface<,,>),
				typeof(BaseInterfaceClass<>), typeof(DerivedInterfaceClass<,>), typeof(FinalInterfaceClass<,,>) };

			foreach (var type in types)
			{
				WriteType(type, "Type");
				Console.Write("----------------------------------------------");
				Console.WriteLine();
			}

		}
		#endregion

		public interface IBaseInterface<T1> { }
		public interface IDerivedInterface<Ta, Tb> : IBaseInterface<Tb> { }
		public interface IFinalInterface<Tx, Ty, Tz> : IDerivedInterface<Tz, Ty> { }
		public class BaseInterfaceClass<Ta1> : IBaseInterface<Ta1> { }
		public class DerivedInterfaceClass<Taa, Tab> : BaseInterfaceClass<Taa>, IDerivedInterface<Taa, Tab> { }
		public class FinalInterfaceClass<Tax, Tay, Taz> : DerivedInterfaceClass<Tay, Tax>, IFinalInterface<Tax, Tay, Tay> { }

		public interface IGeneric<T>
		{
			T Value { get; }
		}

		public class GenericNoCtor<T> : IGeneric<T>
		{
			public T Value { get; set; }
		}

		public class Generic<T> : IGeneric<T>
		{
			private T _value;

			public Generic(T value)
			{
				_value = value;
			}

			public T Value
			{
				get { return _value; }
			}
		}

		[TestMethod]
		public void ShouldCreateGenericNoCtorClass()
		{
			IRezolveTarget t = GenericConstructorTarget.Auto(typeof(GenericNoCtor<>));
			Assert.IsNotNull(t);
			Assert.AreEqual(typeof(GenericNoCtor<>), t.DeclaredType);
			//try and build an instance 
			var instance = GetValueFromTarget<GenericNoCtor<int>>(t);
			Assert.IsNotNull(instance);
			Assert.AreEqual(default(int), instance.Value);
		}

		[TestMethod]
		public void ShouldResolveAGenericNoCtorClass()
		{
			//similar test to above, but we're testing that it works when you put the target inside the
			//default resolver.
			var rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));
			var instance = (GenericNoCtor<int>)rezolver.Resolve(typeof(GenericNoCtor<int>));
			Assert.IsNotNull(instance);
		}

		[TestMethod]
		public void ShouldCreateAGenericClass()
		{
			//use a rezolver mock for cross-referencing the int parameter
			var rezolverMock = new Mock<IRezolver>();
			rezolverMock.Setup(r => r.Fetch(typeof(int), It.IsAny<string>())).Returns((1).AsObjectTarget());
			rezolverMock.Setup(r => r.Compiler).Returns(new RezolveTargetDelegateCompiler());
			IRezolveTarget t = GenericConstructorTarget.Auto(typeof(Generic<>));
			Assert.IsNotNull(t);
			Assert.AreEqual(typeof(Generic<>), t.DeclaredType);
			var instance = GetValueFromTarget<Generic<int>>(t, rezolverMock.Object);
			Assert.IsNotNull(instance);
			Assert.AreEqual(1, instance.Value);
		}

		[TestMethod]
		public void ShouldRezolveAGenericClass()
		{
			//in this one, using DefaultRezolver, we're going to test a few parameter types
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register((3).AsObjectTarget(typeof(int?)));
			rezolver.Register("hello world".AsObjectTarget());
			var instance1 = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			var instance2 = (Generic<string>)rezolver.Resolve(typeof(Generic<string>));
			var instance3 = (Generic<int?>)rezolver.Resolve(typeof(Generic<int?>));

			Assert.AreEqual(2, instance1.Value);
			Assert.AreEqual("hello world", instance2.Value);
			Assert.AreEqual(3, instance3.Value);
		}

		//now test that the target should work when used as the target of a dependency look up
		//just going to use the DefaultRezolver for this as it's far easier to setup the test.

		public class HasGenericDependency
		{
			public Generic<int> Dependency { get; private set; }
			public HasGenericDependency(Generic<int> dependency)
			{
				Dependency = dependency;
			}
		}

		[TestMethod]
		public void ShouldRezolveAClosedGenericDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<HasGenericDependency>());

			var result = (HasGenericDependency)rezolver.Resolve(typeof(HasGenericDependency));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(2, result.Dependency.Value);
		}

		[TestMethod]
		public void ShouldRezolveNestedGenericDependency()
		{
			//this one is more complicated.  Passing a closed generic as a type argument to another
			//generic.
			//note that this isn't the most complicated it can get, however: that would be using
			//the type argument as a type argument to another open generic dependency.  That one is on it's way.
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));

			var result = (Generic<GenericNoCtor<int>>)rezolver.Resolve(typeof(Generic<GenericNoCtor<int>>));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Value);
			Assert.AreEqual(0, result.Value.Value);
		}

		public class HasOpenGenericDependency<T>
		{
			public Generic<T> Dependency { get; set; }
			public HasOpenGenericDependency(Generic<T> dependency)
			{
				Dependency = dependency;
			}
		}

		//this one is the open generic nested dependency check

		[TestMethod]
		public void ShouldResolveNestedeOpenGenericDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			rezolver.Register((10).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericDependency<>)));

			var result = (HasOpenGenericDependency<int>)rezolver.Resolve(typeof(HasOpenGenericDependency<int>));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(10, result.Dependency.Value);
		}

		//now moving on to rezolving interface instead of the type directly

		[TestMethod]
		public void ShouldResolveGenericViaInterface()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((20).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));

			var result = (IGeneric<int>)rezolver.Resolve(typeof(IGeneric<int>));
			Assert.IsInstanceOfType(result, typeof(Generic<int>));
			Assert.AreEqual(20, result.Value);
		}

		public class GenericConstructorTarget : RezolveTargetBase
		{
			private Type _genericType;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="genericType">The type of the object that is to be built (open generic of course)</param>
			public GenericConstructorTarget(Type genericType)
			{
				_genericType = genericType;
			}

			public override bool SupportsType(Type type)
			{
				if (!base.SupportsType(type))
				{
					//scenario - requested type is a closed generic built from this target's open generic
					if (!type.IsGenericType)
						return false;

					var genericType = type.GetGenericTypeDefinition();
					return genericType == DeclaredType;
				}
				return true;
			}

			protected override System.Linq.Expressions.Expression CreateExpressionBase(CompileContext context)
			{
				//always create a constructor target from new
				//basically this class simply acts as a factory for other constructor targets.

				var expectedType = context.TargetType;
				if (expectedType == null)
					throw new ArgumentException("GenericConstructorTarget requires a concrete to be passed in the CompileContext - by definition it cannot simply create a default instance of the target type.", "context");
				if (!expectedType.IsGenericType)
					throw new ArgumentException("The compile context requested an instance of a non-generic type to be built.", "context");

				var genericType = expectedType.GetGenericTypeDefinition();
				Type[] suppliedTypeArguments = Type.EmptyTypes;
				if (genericType == DeclaredType)
				{
					//will need, at some point to map the type arguments of this target to the type arguments supplied,
					//but, for the moment, no.
					suppliedTypeArguments = expectedType.GetGenericArguments();
				}

				//make the generic type
				var typeToBuild = DeclaredType.MakeGenericType(suppliedTypeArguments);
				//construct the constructortarget
				var target = ConstructorTarget.Auto(typeToBuild);

				return target.CreateExpression(context);
			}

			public override System.Type DeclaredType
			{
				get { return _genericType; }
			}


			//in order for this to work, we're going to need a dummy type that we can use, because
			//you can't pass open generics as type parameters.
			public static GenericConstructorTarget Auto<TGeneric>()
			{
				throw new NotImplementedException();
			}

			internal static IRezolveTarget Auto(Type type)
			{
				//I might relax this constraint later - since we could implement partially open generics.
				if (!type.IsGenericTypeDefinition)
					throw new ArgumentException("The passed type must be an open generic type");
				return new GenericConstructorTarget(type);
			}
		}
	}
}

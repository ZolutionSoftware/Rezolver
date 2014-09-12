using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class GenericConstructorTargetTests : TestsBase
	{
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
		
		private string GetTypeReportString(Type type)
		{
			var genericArgs = type.GetGenericArguments() ?? Type.EmptyTypes;
			return string.Format("{0}<{1}>:", type.Name, string.Join(", ", genericArgs.Select(typeParam => string.Format("({0}{1})", typeParam.IsGenericParameter ? "*" : "", typeParam.Name))));
		}

		private void WriteType(Type type, string typeType)
		{
			Console.WriteLine("{0} {1}", typeType, GetTypeReportString(type));
			var interfaces = type.GetInterfaces();
			if(interfaces.Length != 0)
			{
				Console.WriteLine("Interfaces for {0}: ", type);
				foreach(var i in interfaces)
				{
					WriteType(i, "Interface");
				}
			}
			if (type.BaseType != null && type.BaseType != typeof(object))
				WriteType(type.BaseType, "Base");
		}

		[TestMethod]
		public void ShouldBuildTypeParameterMap()
		{
			var types = new[] { typeof(IBaseInterface<>), typeof(IDerivedInterface<,>), typeof(IFinalInterface<,,>),
				typeof(BaseInterfaceClass<>), typeof(DerivedInterfaceClass<,>), typeof(FinalInterfaceClass<,,>) };

			foreach(var type in types)
			{
				WriteType(type, "Type");
				Console.Write("----------------------------------------------");
				Console.WriteLine();
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
			var instance2 = (GenericNoCtor<int>)rezolver.Resolve(typeof(GenericNoCtor<int>));
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
				if(!base.SupportsType(type))
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
				if(genericType == DeclaredType)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rezolver.Targets;
using Xunit;

namespace Rezolver.Tests.Targets
{
	/// <summary>
	/// Testing the base functionality of the ConstructorTarget (no compilation)
	/// </summary>
	public class ConstructorTargetTests
    {
		private class SimpleType { }

		private class OneCtorType
		{
			public OneCtorType(int param1)
			{

			}
		}

		[Fact]
		public void ShouldCreateTargetForType()
		{
			var target = new ConstructorTarget(typeof(SimpleType));

			Assert.Equal(typeof(SimpleType), target.DeclaredType);
			//constructor should not be bound up front
			Assert.Null(target.Ctor);
			Assert.Null(target.MemberBindingBehaviour);
			Assert.Equal(0, target.NamedArgs.Count);
			Assert.Equal(0, target.ParameterBindings.Count);
		}

		[Fact]
		public void ShouldCreateTargetForCtor()
		{
			var ctor = TypeHelpers.GetConstructor(typeof(SimpleType), Type.EmptyTypes);
			var target = new ConstructorTarget(ctor);
			
			//declared type should be lifted from the ctor
			Assert.Equal(typeof(SimpleType), target.DeclaredType);
			Assert.Same(ctor, target.Ctor);
			Assert.Null(target.MemberBindingBehaviour);
			Assert.Equal(0, target.NamedArgs.Count);
			Assert.Equal(0, target.ParameterBindings.Count);
		}

		[Fact]
		public void ShouldSetMemberBindingBehaviour()
		{
			//test both Ctors here
			var ctor = TypeHelpers.GetConstructor(typeof(SimpleType), Type.EmptyTypes);
			var target1 = new ConstructorTarget(typeof(SimpleType), memberBindingBehaviour: DefaultMemberBindingBehaviour.Instance);
			var target2 = new ConstructorTarget(ctor, memberBindingBehaviour: DefaultMemberBindingBehaviour.Instance);

			Assert.Same(target1.MemberBindingBehaviour, DefaultMemberBindingBehaviour.Instance);
			Assert.Same(target2.MemberBindingBehaviour, DefaultMemberBindingBehaviour.Instance);
		}

		[Fact]
		public void ShouldSetNamedArgs()
		{
			//named arguments are used when create a JIT-bound target for a type
			//NOTE: it doesn't matter that the argument doesn't have a matching parameter
			Dictionary<string, ITarget> namedArgs = new Dictionary<string, ITarget>() {
				["arg"] = new TestTarget()
			};

			var target = new ConstructorTarget(typeof(SimpleType), namedArgs: namedArgs);

			Assert.Equal(1, target.NamedArgs.Count);
			Assert.Same(target.NamedArgs["arg"], namedArgs["arg"]);
		}

		[Fact]
		public void ShouldSetParameterBindings()
		{
			var ctor = TypeHelpers.GetConstructor(typeof(OneCtorType), new[] { typeof(int) });
			var bindings = new[] { new ParameterBinding(ctor.GetParameters()[0], new TestTarget(typeof(int))) };

			var target = new ConstructorTarget(ctor, parameterBindings: bindings);

			Assert.Equal(1, target.ParameterBindings.Count);
			//bindings should not be cloned
			Assert.Same(bindings[0], target.ParameterBindings[0]);
		}

		//on to the bindings tests.
		//need to test:
		//1) Type-only constructor binding
		//  - a) Default constructor (simple)
		//	- b) Greedy matching with multiple ctors and no optional params
		//  - c) Greedy matching with multiple ctors, disambiguating by number of optional params ONLY
		//  - d) Greedy matching with multiple ctors, disambiguating by number of resolved args
		//  - e) Greedy matching multiple ctors, disambiguating by named arg matches
		//2) Constructor-specific binding
		//  - a) Supplied default constructor
		//  - b) Supplied constructor with parameters, but no parameter bindings
		//  - c) Supplied constructor with parameters + all parameter bindings
		//  - d) Supplied constructor with parameters + some bindings (with others being auto-created: NEW feature)
		// Exceptions:
		// 1) No constructors found
		// 2) Type-only binding can't choose between 2 or more possible matches
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rezolver.Targets;
using Xunit;
using System.Reflection;
using Xunit.Abstractions;
using Rezolver.Tests.Types;

namespace Rezolver.Tests.Targets
{
	/// <summary>
	/// Testing the base functionality of the ConstructorTarget (no compilation)
	/// </summary>
	public class ConstructorTargetTests : TargetTestsBase
	{
		public ConstructorTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldCreateTargetForType()
		{
			var target = new ConstructorTarget(typeof(NoCtor));

			Assert.Equal(typeof(NoCtor), target.DeclaredType);
			//constructor should not be bound up front
			Assert.Null(target.Ctor);
			Assert.Null(target.MemberBindingBehaviour);
			Assert.Equal(0, target.NamedArgs.Count);
			Assert.Equal(0, target.ParameterBindings.Count);
		}

		[Fact]
		public void ShouldCreateTargetForCtor()
		{
			var ctor = TypeHelpers.GetConstructor(typeof(NoCtor), new Type[0]);
			var target = new ConstructorTarget(ctor);

			//declared type should be lifted from the ctor
			Assert.Equal(typeof(NoCtor), target.DeclaredType);
			Assert.Same(ctor, target.Ctor);
			Assert.Null(target.MemberBindingBehaviour);
			Assert.Equal(0, target.NamedArgs.Count);
			Assert.Equal(0, target.ParameterBindings.Count);
		}

		[Fact]
		public void ShouldSetMemberBindingBehaviour()
		{
			//test both Ctors here
			var ctor = TypeHelpers.GetConstructor(typeof(NoCtor), new Type[0]);
			var target1 = new ConstructorTarget(typeof(NoCtor), memberBinding: MemberBindingBehaviour.BindAll);
			var target2 = new ConstructorTarget(ctor, memberBinding: MemberBindingBehaviour.BindAll);

			Assert.Same(target1.MemberBindingBehaviour, MemberBindingBehaviour.BindAll);
			Assert.Same(target2.MemberBindingBehaviour, MemberBindingBehaviour.BindAll);
		}

		[Fact]
		public void ShouldSetNamedArgs()
		{
			//named arguments are used when create a JIT-bound target for a type
			//NOTE: it doesn't matter that the argument doesn't have a matching parameter
			Dictionary<string, ITarget> namedArgs = new Dictionary<string, ITarget>()
			{
				["arg"] = new TestTarget()
			};

			var target = new ConstructorTarget(typeof(NoCtor), namedArgs: namedArgs);

			Assert.Equal(1, target.NamedArgs.Count);
			Assert.Same(target.NamedArgs["arg"], namedArgs["arg"]);
		}

		[Fact]
		public void ShouldSetParameterBindings()
		{
			var ctor = TypeHelpers.GetConstructor(typeof(OneCtor), new[] { typeof(int) });
			var bindings = new[] { new ParameterBinding(ctor.GetParameters()[0], new TestTarget(typeof(int))) };

			var target = new ConstructorTarget(ctor, parameterBindings: bindings);

			Assert.Equal(1, target.ParameterBindings.Count);
			//bindings should not be cloned
			Assert.Same(bindings[0], target.ParameterBindings[0]);
		}

		//on to the bindings tests.
		//1) Type-only (JIT) constructor binding
		//  - a) Default constructor (simple)
		//  - b) Binding one non-default constructor
		//	- c) Greedy matching with multiple ctors (default + one with params) and no optional params
		//  - d) Greedy matching with multiple ctors, disambiguating by number of optional params ONLY
		//  - e) Greedy matching with multiple ctors, disambiguating by number of resolved args
		//  - f) Greedy matching multiple ctors, disambiguating by named arg matches

		/// <summary>
		/// The workhorse for our expected type bindings theories run by <see cref="ShouldBindToConstructor(ExpectedJITBinding)"/>.
		/// 
		/// Covers searching for the best constructor based on a type, the services
		/// available in the container and, optionally, a provided set of named argument
		/// bindings.
		/// </summary>
		public class ExpectedJITBinding
		{
			private static Func<IContainer> DefaultContainerFactory = () => null;
			private static Func<IDictionary<string, ITarget>> DefaultNamedArgsFactory = () => null;

			public Type Type { get; }
			public ConstructorInfo ExpectedConstructor { get; }
			public Func<IContainer> ContainerFactory { get; }
			public Func<IDictionary<string, ITarget>> NamedArgsFactory { get; }
			public string Description { get; }

			public ExpectedJITBinding(Type type, 
				ConstructorInfo expectedConstructor, 
				Func<IContainer> containerFactory = null, 
				Func<Dictionary<string, ITarget>> namedArgsFactory = null,
				string description = null)
			{
				Type = type;
				ExpectedConstructor = expectedConstructor;
				ContainerFactory = containerFactory ?? DefaultContainerFactory;
				NamedArgsFactory = namedArgsFactory ?? DefaultNamedArgsFactory;
				Description = description;
			}

			public override string ToString()
			{
				return $"Type: { Type }, Expected ctor: { ExpectedConstructor }, Container?: { ContainerFactory != DefaultContainerFactory }, Named Args?: { NamedArgsFactory != DefaultNamedArgsFactory }";
			}

			public void RunBindingTest(ConstructorTargetTests host)
			{
				host.Output.WriteLine("Running JIT Binding Test \"{0}\".", Description ?? "Unknown Bindings Test");
				host.Output.WriteLine(ToString());
				var namedArgs = NamedArgsFactory();
				var target = new ConstructorTarget(Type, namedArgs: namedArgs);
				var compileContext = host.GetCompileContext(target, ContainerFactory());

				var binding = target.Bind(compileContext);

				Xunit.Assert.NotNull(binding);
				Xunit.Assert.Same(ExpectedConstructor, binding.Constructor);

				if(namedArgs != null && namedArgs.Count != 0)
				{
					Assert.All(namedArgs, kvp => {
						var boundArg = binding.BoundArguments.SingleOrDefault(p => p.Parameter.Name == kvp.Key);
						Assert.NotNull(boundArg);
						Assert.Same(kvp.Value, boundArg.Target);
					});
				}
				//
				host.Output.WriteLine("Test Complete");
			}
		}

		public static IEnumerable<object[]> JITBindingData()
		{
			return new object[][]
			{
				new[] {  new ExpectedJITBinding(
					typeof(NoCtor),
					TypeHelpers.GetConstructor(typeof(NoCtor), new Type[0]),
					description: "Default constructor"
				)},
				new[] {  new ExpectedJITBinding(
					typeof(OneCtor),
					TypeHelpers.GetConstructor(typeof(OneCtor), new[] { typeof(int) }),
					description: "Constructor with parameters"
				)},
				new[] {  new ExpectedJITBinding(
					typeof(TwoCtors),
					TypeHelpers.GetConstructor(typeof(TwoCtors), new[] { typeof(string), typeof(int) }),
					description: "Constructor with greatest number of parameters (i.e. 'greedy')"
				)},

				new [] { new ExpectedJITBinding(
					typeof(TwoCtorsOneNoOptional),
					TypeHelpers.GetConstructor(typeof(TwoCtorsOneNoOptional), new[] { typeof(string), typeof(int), typeof(object) }),
					description: "Constructor with least number of optional parameters when more than one have the largest number of parameters"
				)},
				new[] { new ExpectedJITBinding(
					typeof(TwoCtors),
					TypeHelpers.GetConstructor(typeof(TwoCtors), new [] { typeof(string) }),
					() => {
						var container = new Container();
						//fallback switched off for the target because we're simulating a direct match
						container.Register(new TestTarget(typeof(string), useFallBack: false));
						return container;
					},
					description: "Constructor with greatest number of resolved arguments supersedes greedy behaviour"
				)},
				new[] { new ExpectedJITBinding(
					typeof(TwoCtors),
					TypeHelpers.GetConstructor(typeof(TwoCtors), new[] { typeof(string) }),
					namedArgsFactory: () => {
						return new Dictionary<string, ITarget>() {
							["s"] = new TestTarget(typeof(string), useFallBack: false)
						};
					},
					description: "Constructor with matched named args supersedes greedy behaviour"
				)}
			};
		}

		[Theory]
		[MemberData(nameof(JITBindingData))]
		public void ShouldJITBind(ExpectedJITBinding test)
		{
			test.RunBindingTest(this);
		}

		//2) Constructor-specific binding
		//  - a) Supplied default constructor
		//  - b) Supplied constructor with parameters, but no parameter bindings
		//  - c) Supplied constructor with parameters + all parameter bindings
		//  - d) Supplied constructor with parameters + some bindings (with others being auto-created: NEW feature)
		// Exceptions:
		// 1) No constructors found
		// 2) Type-only binding can't choose between 2 or more possible matches

		public class UpfrontBinding
		{
			private static Func<ConstructorInfo, ParameterBinding[]> DefaultSuppliedBindingsFactory = c => null;

			public ConstructorInfo Constructor { get; }
			public Func<ConstructorInfo, ParameterBinding[]> SuppliedBindingsFactory { get; }
			public string Description { get; }

			public UpfrontBinding(ConstructorInfo constructor, Func<ConstructorInfo, ParameterBinding[]> suppliedBindingsFactory = null, string description = null)
			{
				Constructor = constructor;
				SuppliedBindingsFactory = suppliedBindingsFactory ?? DefaultSuppliedBindingsFactory;
				Description = description;
			}

			public override string ToString()
			{
				return $"Ctor: { Constructor }, Supplied Bindings? { SuppliedBindingsFactory != DefaultSuppliedBindingsFactory }";
			}

			public void Run(ConstructorTargetTests host)
			{
				host.Output.WriteLine("Running Upfront Binding Test \"{0}\"", Description ?? "Unknown binding test");
				host.Output.WriteLine(ToString());

				//we check that each parameter is bound; and that the bindings are the same as the ones
				//we were given or added dynamically if not supplied
				var parameterBindings = SuppliedBindingsFactory(Constructor) ?? new ParameterBinding[0];
				ConstructorTarget target = new ConstructorTarget(Constructor, parameterBindings: parameterBindings);
				var compileContext = host.GetCompileContext(target);

				var binding = target.Bind(compileContext);

				ParameterBinding[] expectedBoundParameters = null;
				var allDefaultBoundParameters = Constructor.GetParameters()
					.Select(p => new ParameterBinding(p, new TestTarget(p.ParameterType, useFallBack: true))).ToArray();

				if (parameterBindings.Length != Constructor.GetParameters().Length)
				{
					//join those bindings which match; generate 'fake' ones for those which don't
					expectedBoundParameters = allDefaultBoundParameters.Select(b => parameterBindings.SingleOrDefault(pb => pb.Parameter == b.Parameter) ?? b).ToArray();
				}
				else
					expectedBoundParameters = allDefaultBoundParameters;

				Assert.NotNull(binding);

				Assert.Collection(binding.BoundArguments, expectedBoundParameters.Select(bb => new Action<ParameterBinding>(b => {
					Assert.Same(bb.Parameter, b.Parameter);
					//non fallback TestTarget means 'should be bound to this one'
					//expected fallback TestTarget means 'must be non-null'
					if (!(bb.Target is TestTarget) || !bb.Target.UseFallback)
						Assert.Same(b.Target, bb.Target);
					else
						Assert.NotNull(bb.Target);
				})).ToArray());
			}
		}

		public static IEnumerable<object[]> UpfrontBindingData()
		{
			return new object[][]
			{
				new[] {  new UpfrontBinding(
					TypeHelpers.GetConstructor(typeof(NoCtor), new Type[0]),
					description: "Default constructor"
				)},
				new[] {  new UpfrontBinding(
					TypeHelpers.GetConstructor(typeof(OneCtor), new[] { typeof(int) }),
					description: "Constructor with parameters (no bindings)"
				)},
				new[] {  new UpfrontBinding(
					TypeHelpers.GetConstructor(typeof(TwoCtors), new[] { typeof(string), typeof(int) }),
					//auto-bound parameters which default to resolving values
					suppliedBindingsFactory: c => c.GetParameters().Select(p => new ParameterBinding(p)).ToArray(),
					description: "Constructor with two parameters (all bound)"
				)},
				new [] { new UpfrontBinding(
					TypeHelpers.GetConstructor(typeof(TwoCtors), new[] { typeof(string), typeof(int) }),
					suppliedBindingsFactory: c => c.GetParameters().Take(1).Select(
						p => new ParameterBinding(p, new TestTarget(p.ParameterType, useFallBack: false))).ToArray(),
					description: "Constructor with two parameters (one bound up front)"
				)}
			};
		}

		[Theory]
		[MemberData(nameof(UpfrontBindingData))]
		public void ShouldBindSpecificConstructor(UpfrontBinding test)
		{
			test.Run(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	public abstract partial class RezolveTargetCompilerTestsBase : TestsBase
	{
		private readonly Lazy<ObjectTarget> _stringObjectTarget = new Lazy<ObjectTarget>(() => StringForObjectTarget.AsObjectTarget());
		private const string StringForObjectTarget = "hello";

		private readonly Lazy<ObjectTarget> _intObjectTarget = new Lazy<ObjectTarget>(() => IntForObjectTarget.AsObjectTarget());
		private const int IntForObjectTarget = 2;

		private const int MultipleForIntObjectTarget = 5;

		private readonly Lazy<ObjectTarget> _nullableIntObjectTarget =
			new Lazy<ObjectTarget>(() => NullableIntForObjectTarget.AsObjectTarget());
		private static readonly int? NullableIntForObjectTarget = 1;

		private readonly Lazy<IRezolveTarget> _requiresIntTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<RequiresInt>());

		private readonly Lazy<IRezolveTarget> _transientConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<Transient>());

		private readonly Lazy<SingletonTarget> _singletonConstructorTarget
			= new Lazy<SingletonTarget>(() => new SingletonTarget(ConstructorTarget.Auto<Singleton>()));

		private readonly Lazy<IRezolveTarget> _compositeConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<Composite>());

		private readonly Lazy<IRezolveTarget> _superComplexConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<SuperComplex>());

		private readonly Lazy<IRezolveTarget> _scopedSingletonTestTypeConstructorTarget
			= new Lazy<IRezolveTarget>(() => ConstructorTarget.Auto<ScopedSingletonTestClass>());


		protected abstract IRezolveTargetCompiler CreateCompilerBase(string callingMethod);
		protected abstract void ReleaseCompiler(IRezolveTargetCompiler compiler);

		private IRezolveTargetCompiler _currentCompiler;

		protected IRezolveTargetCompiler CreateCompiler([CallerMemberName]string callingMethod = null)
		{
			return _currentCompiler = CreateCompilerBase(callingMethod);
		}

		protected IRezolver CreateRezolverMock(IRezolveTargetCompiler compiler)
		{
			Assert.IsNotNull(compiler, "The compiler must be passed when setting up the rezolver mock for this test");
			var mock = new Mock<IRezolver>();
			mock.Setup(c => c.Compiler).Returns(compiler);
			return mock.Object;
		}

		protected class Mocks
		{
			public Mock<IRezolverBuilder> BuilderMock;
			public Mock<IRezolver> RezolverMock;
		}

		protected Mock<IRezolverBuilder> CreateDefaultMockForBuilder()
		{
			var mock = new Mock<IRezolverBuilder>();
			return mock;
		}

		protected Mocks CreateDefaultMockForRezolver(IRezolveTargetCompiler compiler)
		{
			var toReturn = new Mocks();
			toReturn.BuilderMock = CreateDefaultMockForBuilder();
			toReturn.RezolverMock = new Mock<IRezolver>();
			AddCompilerToRezolverMock(toReturn.RezolverMock, compiler);
			toReturn.RezolverMock.Setup(c => c.Resolve(It.IsAny<RezolveContext>())).Callback<RezolveContext>(
				(r) => { throw new InvalidOperationException(string.Format("Type {0} not mocked", r.RequestedType)); });
			toReturn.RezolverMock.Setup(c => c.Builder).Returns(() => toReturn.BuilderMock.Object);
			return toReturn;
		}

		protected void AddCompilerToRezolverMock(Mock<IRezolver> mock, IRezolveTargetCompiler compiler)
		{
			mock.Setup(r => r.Compiler).Returns(compiler);
		}

		protected void AddTargetToMocks(Mocks mocks, Type forType, IRezolveTarget target)
		{
			forType = forType ?? typeof(int);
			mocks.BuilderMock.Setup(r => r.Fetch(forType, null)).Returns(CreateRezolverEntryForTarget(target, forType));
		}

		protected void AddIntTargetToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof (int), _intObjectTarget.Value);
		}

		protected void AddNullableIntTargetToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof (int?), _nullableIntObjectTarget.Value);
		}

		protected void AddStringTargetToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof (string), _stringObjectTarget.Value);
		}

		protected void AddTransientTargetToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof (ITransient), _transientConstructorTarget.Value);
		}

		protected void AddSingletonTargetToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof(ISingleton), _singletonConstructorTarget.Value);
		}

		protected void AddCompositeTargetToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof (IComposite), _compositeConstructorTarget.Value);
		}

		protected void AddSuperComplexTargetToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof(ISuperComplex), _superComplexConstructorTarget.Value);
		}

		protected void AddScopedSingletonTestTypeToMocks(Mocks mock, Type forType = null)
		{
			AddTargetToMocks(mock, forType ?? typeof(ScopedSingletonTestClass), _scopedSingletonTestTypeConstructorTarget.Value);
		}
	}
}

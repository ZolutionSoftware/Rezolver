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
	public abstract partial class RezolveTargetCompilerTestsBase
	{
		private readonly Lazy<ObjectTarget> _stringObjectTarget = new Lazy<ObjectTarget>(() => StringForObjectTarget.AsObjectTarget());
		private const string StringForObjectTarget = "hello";

		private readonly Lazy<ObjectTarget> _intObjectTarget = new Lazy<ObjectTarget>(() => IntForObjectTarget.AsObjectTarget());
		private const int IntForObjectTarget = 2;

		private const int MultipleForIntObjectTarget = 5;

		private readonly Lazy<ObjectTarget> _nullableIntObjectTarget =
			new Lazy<ObjectTarget>(() => NullableIntForObjectTarget.AsObjectTarget());
		private static readonly int? NullableIntForObjectTarget = 1;

		private readonly Lazy<ConstructorTarget> _requiresIntTarget
			= new Lazy<ConstructorTarget>(() => ConstructorTarget.Auto<RequiresInt>());

		private readonly Lazy<ConstructorTarget> _transientConstructorTarget
			= new Lazy<ConstructorTarget>(() => ConstructorTarget.Auto<Transient>());

		private readonly Lazy<SingletonTarget> _singletonConstructorTarget
			= new Lazy<SingletonTarget>(() => new SingletonTarget(ConstructorTarget.Auto<Singleton>()));

		private readonly Lazy<ConstructorTarget> _compositeConstructorTarget
			= new Lazy<ConstructorTarget>(() => ConstructorTarget.Auto<Composite>());

		private readonly Lazy<ConstructorTarget> _superComplexConstructorTarget
			= new Lazy<ConstructorTarget>(() => ConstructorTarget.Auto<SuperComplex>());

		private readonly Lazy<ConstructorTarget> _scopedSingletonTestTypeConstructorTarget
			= new Lazy<ConstructorTarget>(() => ConstructorTarget.Auto<ScopedSingletonTestClass>());


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

		protected Mock<IRezolver> CreateDefaultMockForRezolver(IRezolveTargetCompiler compiler)
		{
			var mock = new Mock<IRezolver>();
			AddCompilerToRezolverMock(mock, compiler);
			mock.Setup(c => c.Resolve(It.IsAny<RezolveContext>())).Callback<RezolveContext>(
				(r) => { throw new InvalidOperationException(string.Format("Type {0} not mocked", r.RequestedType)); });
			return mock;
		}

		protected void AddCompilerToRezolverMock(Mock<IRezolver> mock, IRezolveTargetCompiler compiler)
		{
			mock.Setup(r => r.Compiler).Returns(compiler);
		}

		protected void AddTargetToRezolverMock(Mock<IRezolver> mock, Type forType, IRezolveTarget target)
		{
			forType = forType ?? typeof(int);
			mock.Setup(r => r.Fetch(forType, null)).Returns(target);
		}

		protected void AddIntTargetToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof (int), _intObjectTarget.Value);
		}

		protected void AddNullableIntTargetToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof (int?), _nullableIntObjectTarget.Value);
		}

		protected void AddStringTargetToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof (string), _stringObjectTarget.Value);
		}

		protected void AddTransientTargetToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof (ITransient), _transientConstructorTarget.Value);
		}

		protected void AddSingletonTargetToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof(ISingleton), _singletonConstructorTarget.Value);
		}

		protected void AddCompositeTargetToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof (IComposite), _compositeConstructorTarget.Value);
		}

		protected void AddSuperComplexTargetToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof(ISuperComplex), _superComplexConstructorTarget.Value);
		}

		protected void AddScopedSingletonTestTypeToRezolverMock(Mock<IRezolver> mock, Type forType = null)
		{
			AddTargetToRezolverMock(mock, forType ?? typeof(ScopedSingletonTestClass), _scopedSingletonTestTypeConstructorTarget.Value);
		}
	}
}

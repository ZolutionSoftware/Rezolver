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

		private readonly Lazy<ObjectTarget> _nullableIntObjectTarget =
			new Lazy<ObjectTarget>(() => NullableIntForObjectTarget.AsObjectTarget());
		private static readonly int? NullableIntForObjectTarget = 1;

		private readonly Lazy<ConstructorTarget> _transientConstructorTarget
			= new Lazy<ConstructorTarget>(ConstructorTarget.Auto<Transient>);

		private readonly Lazy<SingletonTarget> _singletonConstructorTarget
			= new Lazy<SingletonTarget>(() => new SingletonTarget(ConstructorTarget.Auto<SingletonTarget>()));

		private readonly Lazy<ConstructorTarget> _compositeConstructorTarget
			= new Lazy<ConstructorTarget>(ConstructorTarget.Auto<Composite>);

		private readonly Lazy<ConstructorTarget> _superComplexConstructorTarget
			= new Lazy<ConstructorTarget>(ConstructorTarget.Auto<SuperComplex>);

		protected abstract IRezolveTargetCompiler CreateCompilerBase(string callingMethod);
		protected abstract void ReleaseCompiler(IRezolveTargetCompiler compiler);

		private IRezolveTargetCompiler _currentCompiler;

		protected IRezolveTargetCompiler CreateCompiler([CallerMemberName]string callingMethod = null)
		{
			return _currentCompiler = CreateCompilerBase(callingMethod);
		}

		protected IRezolverContainer CreateContainerScopeMock(IRezolveTargetCompiler compiler)
		{
			Assert.IsNotNull(compiler, "The compiler must be passed when setting up the container mock for this test");
			var mock = new Mock<IRezolverContainer>();
			mock.Setup(c => c.Compiler).Returns(compiler);
			return mock.Object;
		}

		protected void AddTargetToScopeMock(Mock<IRezolverContainer> mock, Type forType, IRezolveTarget target)
		{
			forType = forType ?? typeof(int);
			mock.Setup(r => r.Fetch(forType, null)).Returns(target);
		}

		protected void AddIntObjectTargetToScopeMock(Mock<IRezolverContainer> mock, Type forType = null)
		{
			AddTargetToScopeMock(mock, forType ?? typeof (int), _intObjectTarget.Value);
		}

		protected void AddNullableIntTargetToScopeMock(Mock<IRezolverContainer> mock, Type forType = null)
		{
			AddTargetToScopeMock(mock, forType ?? typeof (int), _nullableIntObjectTarget.Value);
		}

		protected void AddStringTargetToScopeMock(Mock<IRezolverContainer> mock, Type forType = null)
		{
			AddTargetToScopeMock(mock, forType ?? typeof (string), _stringObjectTarget.Value);
		}

		protected void AddTransientTargetToScopeMock(Mock<IRezolverContainer> mock, Type forType = null)
		{
			AddTargetToScopeMock(mock, forType ?? typeof (ITransient), _transientConstructorTarget.Value);
		}

		protected void AddSingletonTargetToScopeMock(Mock<IRezolverContainer> mock, Type forType = null)
		{
			AddTargetToScopeMock(mock, forType ?? typeof(ISingleton), _singletonConstructorTarget.Value);
		}

		protected void AddSuperComplexTargetToScopeMock(Mock<IRezolverContainer> mock, Type forType = null)
		{
			AddTargetToScopeMock(mock, forType ?? typeof(ISuperComplex), _superComplexConstructorTarget.Value);
		}
	}
}

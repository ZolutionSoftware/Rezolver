using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
	public class CompiledRezolver : IRezolver
	{
		private static int _assemblyCounter = 0;

		public static AssemblyBuilder CreateAssemblyBuilder(
			AssemblyBuilderAccess assemblyBuilderAccess = AssemblyBuilderAccess.RunAndCollect)
		{
			return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(
				string.Format("Rezolver.Compiled{0}, Culture=neutral, Version=0.0.0.0",
					++_assemblyCounter)), assemblyBuilderAccess);
		}

		private readonly AssemblyBuilder _assemblyBuilder;
		private ModuleBuilder _moduleBuilder;
		private readonly string _assemblyModuleName;
		private readonly IRezolverBuilder _rezolverBuilder;

		private readonly AssemblyRezolveTargetCompiler _rezolveTargetCompiler;

		public AssemblyBuilder AssemblyBuilder { get { return _assemblyBuilder; } }

		public CompiledRezolver(IRezolverBuilder builder)
			: this(builder, CreateAssemblyBuilder())
		{

		}

		public CompiledRezolver(IRezolverBuilder builder, AssemblyBuilder assemblyBuilder)
		{
			if (assemblyBuilder == null) throw new ArgumentNullException("assemblyBuilder");
			_assemblyModuleName = assemblyBuilder.GetName().Name;
			_assemblyBuilder = assemblyBuilder;
			try
			{
				_moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyModuleName, _assemblyModuleName + ".dll");
			}
			catch (NotSupportedException)
			{
				_moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyModuleName);
			}

			//do this so the compiler generates code in the same assembly
			_rezolveTargetCompiler = new AssemblyRezolveTargetCompiler(_moduleBuilder);
			_rezolverBuilder = builder;
			Compile();
		}

		private void Compile()
		{
			//for each entry, compile a static method.
			//then emit a big switch statement calling each one, with a default case for bad juju

			TypeBuilder t = _moduleBuilder.DefineType("RezolverImpl");

			var methods = _rezolverBuilder.AllRegistrations.Select((kvp, i) => {
				var methodBuilder = t.DefineMethod(string.Format("Rezolve{0}", i), MethodAttributes.Static | MethodAttributes.Private);
				_rezolveTargetCompiler.CompileTargetToMethod(kvp.Value, new CompileContext(this), methodBuilder);
				return new { kvp = kvp, targetMethod = methodBuilder };
			});

			var masterResolveMethod = t.DefineMethod("ResolveMaster", MethodAttributes.Static);
			var rezolveContextParam = Expression.Parameter(typeof(RezolveContext), "context");
			var contextParamType = Expression.Property(rezolveContextParam, "RequestedType");
			var typeEqualMethod = MethodCallExtractor.ExtractCalledMethod((Type type) => type.Equals((Type)null));

			var contextParamName = Expression.Property(rezolveContextParam, "Name");
			
			List<SwitchCase> switches = new List<SwitchCase>();
			foreach(var method in methods)
			{
				switches.Add(Expression.SwitchCase(Expression.Call(method.targetMethod), Expression.Constant(method.kvp.Key.RequestedType)));
			}

			Expression switchExpr = Expression.Switch(contextParamType, Expression.Throw(Expression.New(typeof(InvalidOperationException))), typeEqualMethod,  switches.ToArray());
			Expression.Lambda(switchExpr, rezolveContextParam).CompileToMethod(masterResolveMethod);

			t.CreateType();
		}

		public IRezolveTargetCompiler Compiler
		{
			get { return _rezolveTargetCompiler; }
		}

		public bool CanResolve(RezolveContext context)
		{
			throw new NotImplementedException();
		}

		public object Resolve(RezolveContext context)
		{
			throw new NotImplementedException();
		}

		public bool TryResolve(RezolveContext context, out object result)
		{
			throw new NotImplementedException();
		}

		public ILifetimeScopeRezolver CreateLifetimeScope()
		{
			throw new NotSupportedException();
		}

		public ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			throw new NotSupportedException();
		}

		public IEnumerable<KeyValuePair<RezolveContext, IRezolveTarget>> AllRegistrations
		{
			get { throw new NotSupportedException(); }
		}

		public void Register(IRezolveTarget target, Type type = null, RezolverPath path = null)
		{
			throw new NotSupportedException();
		}

		public IRezolveTarget Fetch(Type type, string name = null)
		{
			throw new NotSupportedException();
		}

		public IRezolveTarget Fetch<T>(string name = null)
		{
			throw new NotSupportedException();
		}

		public INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
		{
			throw new NotSupportedException();
		}
	}
}

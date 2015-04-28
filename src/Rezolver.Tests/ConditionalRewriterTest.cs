using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class ConditionalRewriterTest
	{
		public class TypeWith2ConstructorArgs
		{
			public TypeWith2ConstructorArgs(int a, string b)
			{
			}
		}

		//[TestMethod]
		//public void TestMethod1()
		//{
		//	DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
		//	CompileContext sharedContext = new CompileContext(rezolver);

		//	rezolver.Register((1).AsObjectTarget());
		//	rezolver.Register("hello world".AsObjectTarget());
		//	rezolver.Register(ConstructorTarget.Auto<TypeWith2ConstructorArgs>());
		//	var ctorTarget = rezolver.Fetch(typeof(TypeWith2ConstructorArgs));
		//	var expression = ctorTarget.CreateExpression(sharedContext);

		//	RezolveTargetCompilerHelper.ConditionalRewriter rewriter = new RezolveTargetCompilerHelper.ConditionalRewriter();
		//	var rewritten = rewriter.Rewrite(expression);
		//	//should really test this
		//	Assert.IsNotNull(rewritten);
		//}
	}
}

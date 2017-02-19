﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	internal class TargetExpressionRewriter : ExpressionVisitor
    {
		readonly IExpressionCompileContext _sourceCompileContext;
		readonly IExpressionCompiler _compiler;
		
		public TargetExpressionRewriter(IExpressionCompiler compiler, IExpressionCompileContext context)
		{
			_compiler = compiler;
			_sourceCompileContext = context;
		}
		
		public override Expression Visit(Expression node)
		{
			if (node != null)
			{
				if (node.NodeType == ExpressionType.Extension)
				{
					TargetExpression te = node as TargetExpression;
					if (te != null)
						return _compiler.Build(te.Target, _sourceCompileContext.NewContext(te.Type));
				}
			}
			return base.Visit(node);
		}
	}
}
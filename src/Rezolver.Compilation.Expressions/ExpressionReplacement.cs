using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    internal class ExpressionReplacement
    {
		public Expression Original { get; private set; }
		public Expression Replacement { get; private set; }

		public ExpressionReplacement(Expression original, Expression replacement)
		{
			this.Original = original;
			this.Replacement = replacement;
		}
	}
}

using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	//Technically a suite of integration tests, but the individual components here all have unit tests
	//too, and the RegisterExpression extension methods are a unique integration of the TargetAdapter
	//and the TargetContainer
	//Lambdas have their own curiosities, too - in that their parameters have to be rewritten as local 
	//variable assignments from the container

	public class RegisterExpressionTests : TestsBase
	{
		
	}
}

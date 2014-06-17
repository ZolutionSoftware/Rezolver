using System.Runtime.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests 
{
	/// <summary>
	/// Tests for the DefaultTarget IRezolveTarget class.
	/// 
	/// The class returns default instances of types (null or uninitialised)
	/// </summary>
	[TestClass]
	public class DefaultTargetTests : TestsBase
	{
		[TestMethod]
		public void ShouldReturnDefaultInt()
		{
			DefaultTarget target = new DefaultTarget(typeof(int));
			Assert.AreEqual(default(int), GetValueFromTarget(target));

		}

		[TestMethod]
		public void ShouldReturnDefaultNullableInt()
		{
			DefaultTarget target = new DefaultTarget(typeof (int?));
			Assert.AreEqual((int?) null, GetValueFromTarget(target));

		}

		[TestMethod]
		public void ShouldReturnDefaultReferenceType_String()
		{
			DefaultTarget target = new DefaultTarget(typeof (string));
			Assert.AreEqual(null, GetValueFromTarget(target));
		}
	}
}

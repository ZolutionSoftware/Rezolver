using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.ConfigurationTests
{
	public interface IRequiresNothing
	{
		int InstanceNumber { get; }
	}
	public class RequiresNothing : IRequiresNothing
	{
		private static int _lastInstanceNumber = 0;

		public static int LastInstanceNumber { get { return _lastInstanceNumber; } }

		private readonly int _instanceNumber;
		public int InstanceNumber { get { return _instanceNumber; } }
		public RequiresNothing() {
			_instanceNumber = ++_lastInstanceNumber;
		}
	}
}

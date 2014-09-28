using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Rezolver
{
	public class RezolverDependencyResolver : IDependencyResolver
	{
		private static readonly Dictionary<Type, Type> _enumerableTypeLookup = new Dictionary<Type, Type>();
		private static readonly object _enumerableTypeLookupLocker = new object();

		private static Type GetEnumerableType(Type serviceType)
		{
			Type toReturn;
			lock (_enumerableTypeLookup)
			{
				if (!_enumerableTypeLookup.TryGetValue(serviceType, out toReturn))
					_enumerableTypeLookup[serviceType] = toReturn = typeof(IEnumerable<>).MakeGenericType(serviceType);
			}
			return toReturn;
		}

		private readonly IRezolver _rezolver;

		public IRezolver Rezolver { get { return _rezolver; } }

		public RezolverDependencyResolver(IRezolver rezolver)
		{
			if (rezolver == null)
				throw new ArgumentNullException("rezolver");
			_rezolver = rezolver;
		}

		public object GetService(Type serviceType)
		{
			object toReturn = null;
			_rezolver.TryResolve(serviceType, out toReturn);
			return toReturn;
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			object result = null;
			_rezolver.TryResolve(GetEnumerableType(serviceType), out result);
			if(result != null)
				return (IEnumerable<object>)result;
			else
				return Enumerable.Empty<object>();
		}
	}
}

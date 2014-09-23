using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;

namespace Rezolver
{
    public class RezolverServiceLocator : IServiceLocator
    {
			private static readonly Dictionary<Type, Type> _enumerableTypeLookup = new Dictionary<Type, Type>();
			private static readonly object _enumerableTypeLookupLocker = new object();

			private static Type GetEnumerableType(Type serviceType)
			{
				Type toReturn;
				lock(_enumerableTypeLookup)
				{
					if(!_enumerableTypeLookup.TryGetValue(serviceType, out toReturn))
						_enumerableTypeLookup[serviceType] = toReturn = typeof(IEnumerable<>).MakeGenericType(serviceType);
				}
				return toReturn;
			}

			private readonly IRezolver _rezolver;

			public RezolverServiceLocator(IRezolver rezolver)
			{
				if (rezolver == null)
					throw new ArgumentNullException("rezolver");

				_rezolver = rezolver;
			}

			public IEnumerable<TService> GetAllInstances<TService>()
			{
				object result;
				if (_rezolver.TryResolve(typeof(IEnumerable<TService>), out result))
					return (IEnumerable<TService>)result;
				return Enumerable.Empty<TService>();
			}

			public IEnumerable<object> GetAllInstances(Type serviceType)
			{
				//this is sub-optimal because of the MakeGenericType call
				object result;
				if (_rezolver.TryResolve(GetEnumerableType(serviceType), out result))
					return (IEnumerable<object>)result;
				return Enumerable.Empty<object>();
			}

			public TService GetInstance<TService>(string key)
			{
				return (TService)GetInstance(typeof(TService), key);
			}

			public TService GetInstance<TService>()
			{
				return (TService)GetInstance(typeof(TService), null);
			}

			public object GetInstance(Type serviceType, string key)
			{
				object result = null;
				if (!_rezolver.TryResolve(serviceType, key, out result))
					throw new ActivationException(string.Format("Could not resolve either a specific or default implementation of {0} with the key \"{1}\"", serviceType, key ?? "[null]"));
				return result;
			}

			public object GetInstance(Type serviceType)
			{
				return GetInstance(serviceType, null);
			}

			public object GetService(Type serviceType)
			{
				return GetInstance(serviceType, null);
			}
		}
}

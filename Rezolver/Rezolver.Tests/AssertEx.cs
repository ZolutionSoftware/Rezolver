using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;

namespace Rezolver.Tests
{
	public static class AssertEx
	{
		public class ThrowsException<TException> where TException : Exception
		{
			public ThrowsException()
			{

            }

			public ThrowsException(Action a, string message = null)
			{
				try
				{
					a();
					Assert.Fail(message ?? string.Format("Excepted exception of type {0}", typeof(TException)));
				}
				catch (TException)
				{

				}
			}

			public void ForEach<T>(IEnumerable<T> range, Action<T> action)
			{
				if (range == null)
					throw new ArgumentNullException("range");
				if (action == null)
					throw new ArgumentNullException("action");
				foreach (var item in range)
				{
					try
					{
						Console.WriteLine("Testing exception {0} raised for item \"{1}\"...", typeof(TException), item);
						action(item);
						Assert.Fail(string.Format("Expected {0} to be thrown for item \"{1}\"", typeof(TException), item));
					}
					catch (TException)
					{

					}

				}
			}
		}
		public static ThrowsException<TException> Throws<TException>(Action action = null) where TException : Exception
		{
			return action == null ? new ThrowsException<TException>() : new ThrowsException<TException>(action);
		}
	}
}
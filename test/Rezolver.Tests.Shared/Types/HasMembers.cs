using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class HasMembers
    {
        public int BindableInt { get; set; }
        public string BindableString { get; set; }
        public IEnumerable<double> ReadOnlyDoubles { get; }
        /// <summary>
        /// This should be bound as a read/write property - i.e. resolving a list of decimals
        /// </summary>
        public List<decimal> BindableListOfDecimals { get; set; }
        /// <summary>
        /// Because this is read only, the default behaviour will be to check whether the 
        /// type of the property can be initialised as a collection - i.e. when it's an IEnumerable{T} 
        /// which has a public Add() method which accepts a single argument of type T - 
        /// using an enumerable of T as the source of each instance to be added.
        /// </summary>
        public List<DateTime> CollectionBindableListOfDateTimes { get; }

        public HasMembers()
        {
            ReadOnlyDoubles = new[] { 1.0, 2.0, 3.0 };
            BindableListOfDecimals = new List<decimal>();
            CollectionBindableListOfDateTimes = new List<DateTime>();
        }
    }
}

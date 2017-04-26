using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Suggested base class for implementations of <see cref="IDependant{TDependency}"/>.
    /// 
    /// Provides functionality such as adding required as well as optional dependencies; plus shortcut 
    /// filters for declaring dependencies on all objects of a certain type, etc.
    /// </summary>
    /// <typeparam name="T">The type of object on which the object depends.  Should be equal to 
    /// the derived type - meaning that the object is dependent upon other objects of the same type.</typeparam>
    /// <remarks>In addition to providing the recommended implementation of <see cref="IDependant{TDependency}"/>,
    /// this class also provides numerous APIs for expressing required or optional dependencies between objects 
    /// of the same type.
    /// 
    /// When <see cref="GetDependencies(IEnumerable{T})"/> is called (this class' implementation of 
    /// <see cref="IDependant{TDependency}.GetDependencies(IEnumerable{TDependency})"/>), the dependencies that have
    /// been added through methods such as <see cref="Requires(T)"/>, <see cref="RequiresAny{TOther}"/> 
    /// 
    /// </remarks>
    public class DependantBase<T> : IDependant<T>
        where T : DependantBase<T>
    {
        private abstract class DependencyDescriptor
        {
            protected DependantBase<T> Owner { get; }
            protected bool Required { get; }
            public DependencyDescriptor(DependantBase<T> owner, bool required)
            {
                Owner = owner;
                Required = required;
            }
            public abstract IEnumerable<T> GetDependencies(IEnumerable<T> objects);
        }

        /// <summary>
        /// Represents a dependency on a specific instance
        /// </summary>
        private class ObjectDependency : DependencyDescriptor
        {
            private T Obj { get; }

            public ObjectDependency(T obj, DependantBase<T> owner, bool required)
                : base(owner, required)
            {
                Obj = obj;
            }

            public override IEnumerable<T> GetDependencies(IEnumerable<T> objects)
            {
                //note - we don't filter duplicates
                var toReturn = objects.Where(o => object.ReferenceEquals(Obj, o));
                if (Required && !toReturn.Any())
                    throw new InvalidOperationException($"Object { Owner } has a required dependency on { Obj } which was not found");
                return toReturn;
            }
        }

        /// <summary>
        /// Represents a dependency on a specific type of object
        /// </summary>
        private class TypeDependency : DependencyDescriptor
        {
            private Type Type { get; }
            public TypeDependency(Type type, DependantBase<T> owner, bool required)
                : base(owner, required)
            {
                Type = type;
            }

            public override IEnumerable<T> GetDependencies(IEnumerable<T> objects)
            {
                Type oType;
                DependantBase<T> oDependant;
                int count = 0;
                foreach (var o in objects)
                {
                    oType = o.GetType();
                    if (!TypeHelpers.IsAssignableFrom(Type, oType))
                        continue;
                    // if the object is also a DependantBase<T> object
                    // then we examine the other's dependency descriptors - if it also has 
                    // a TypeDependency for the same type and our owner matches it, then we
                    // don't add a dependency on it because we would end up with a circular
                    // dependency.
                    // Note we don't check whether our owner is in the objects enumerable - 
                    // we just assume that it is (because if it's dependencies are being 
                    // calculated, then it should be).
                    oDependant = o as DependantBase<T>;
                    if (oDependant != null)
                    {
                        foreach (var descriptor in oDependant.DependencyDescriptors.OfType<TypeDependency>())
                        {
                            if (descriptor.Type == Type && TypeHelpers.IsAssignableFrom(Type, Owner.GetType()))
                                continue;
                        }
                    }
                    count++;
                    yield return o;
                }
                if (Required && count == 0)
                    throw new InvalidOperationException($"Object { Owner } requires at least one object of type { Type }");
            }
        }

        private List<DependencyDescriptor> DependencyDescriptors { get; } = new List<DependencyDescriptor>();

        public IEnumerable<T> GetDependencies(IEnumerable<T> behaviours)
        {
            //this is where we filter out duplicates - by reference
            return DependencyDescriptors.SelectMany(d => d.GetDependencies(behaviours)).Distinct(ReferenceComparer<T>.Instance);
        }

        /// <summary>
        /// Configures this object with a required dependency on the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">Required.  The object that is required by this object.</param>
        /// <returns>The object on which the method is called, to allow further dependencies to be specified fluently.</returns>
        public T Requires(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            DependencyDescriptors.Add(new ObjectDependency(obj, this, true));
            return (T)this;
        }

        /// <summary>
        /// Configures this object with a set of required dependencies on the given <paramref name="objects"/>
        /// </summary>
        /// <param name="objects">Required.  The objects that are required by this object.  None of the items can be null.</param>
        /// <returns>The object on which the method is called, to allow further dependencies to be specified fluently.</returns>
        public T RequiresAll(IEnumerable<T> objects)
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            if (objects.Any(o => o == null)) throw new ArgumentException("All objects must be non-null", nameof(objects));

            foreach (var obj in objects)
            {
                DependencyDescriptors.Add(new ObjectDependency(obj, this, true));
            }
            return (T)this;
        }

        /// <summary>
        /// Configures this object with a set of required dependencies on the given <paramref name="objects"/>
        /// </summary>
        /// <param name="objects">Required.  The objects that are required by this object.  None of the items can be null.</param>
        /// <returns>The object on which the method is called, to allow further dependencies to be specified fluently.</returns>
        public T RequiresAll(params T[] objects)
        {
            return RequiresAll((IEnumerable<T>)objects);
        }

        /// <summary>
        /// Configures this object with a dependency on at least one other object of the type <typeparamref name="TOther"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of objects that are required by this object.  The type must inherit from the
        /// same <see cref="DependantBase{T}"/> as this object.</typeparam>
        /// <returns></returns>
        public T RequiresAny<TOther>()
            where TOther : DependantBase<T>
        {
            return RequiresAny(typeof(TOther));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tOther"></param>
        /// <returns></returns>
        public T RequiresAny(Type tOther)
        {
            if (tOther == null) throw new ArgumentNullException(nameof(tOther));
            DependencyDescriptors.Add(new TypeDependency(tOther, this, true));
            return (T)this;
        }
    }
}

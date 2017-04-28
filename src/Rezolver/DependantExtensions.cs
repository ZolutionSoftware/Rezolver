using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Extension methods for the <see cref="IDependant"/> interface
    /// </summary>
    public static class DependantExtensions
    {
        internal static T AddObjectDependency<T>(T obj, object dep, bool required)
            where T : IDependant
        {
            obj.Dependencies.Add(new ObjectDependency(dep, obj, required));
            return obj;
        }

        internal static T AddObjectDependencies<T>(T obj, IEnumerable<object> deps, bool required)
            where T : IDependant
        {
            obj.Dependencies.AddRange(deps.Where(o => o != null).Select(o => new ObjectDependency(o, obj, required)));
            return obj;
        }

        internal static T AddTypeDependency<T>(T obj, Type dependencyType, bool required)
            where T : IDependant
        {
            obj.Dependencies.Add(new TypeDependency(dependencyType, obj, required));
            return obj;
        }

        /// <summary>
        /// Adds a required dependency from the object on which this is called to <paramref name="obj"/>
        /// 
        /// When dependencies are resolved, if <paramref name="obj"/> is not found, then an exception will
        /// be thrown.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <typeparam name="TDependency">The type for the dependency being added.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="dep">The object upon which <paramref name="obj"/> is dependent upon.</param>
        /// <returns>The object this method is called on.</returns>
        public static T Requires<T, TDependency>(this T obj, TDependency dep)
            where T : IDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (dep == null) throw new ArgumentNullException(nameof(dep));

            return AddObjectDependency(obj, dep, true);
        }

        /// <summary>
        /// Adds a required dependency from the object on which this is called to all the objects in <paramref name="deps"/>
        /// 
        /// When dependencies are resolved, if *all* of the objects in <paramref name="deps"/> are not found, then an exception will
        /// be thrown.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <typeparam name="TDependency">The type for the dependency being added.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="deps">The objects upon which <paramref name="obj"/> is dependent upon.</param>
        /// <returns>The object this method is called on.</returns>
        public static T Requires<T, TDependency>(this T obj, IEnumerable<TDependency> deps)
            where T : IDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (deps == null) throw new ArgumentNullException(nameof(deps));

            return AddObjectDependencies(obj, deps, true);
        }

        /// <summary>
        /// Adds a required dependency from the object on which this is called to at least one object of the type
        /// <typeparamref name="TDependency"/>.  Use this when only the type of the dependency is known.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <typeparam name="TDependency">The type for the dependency being added.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <returns></returns>
        public static T RequiresAny<T, TDependency>(this T obj)
            where T : IDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return AddTypeDependency(obj, typeof(TDependency), true);
        }

        /// <summary>
        /// Adds a required dependency from the object on which this is called to at least one object of the type
        /// <paramref name="dependencyType"/>.  Use this when only the type of the dependency is known.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="dependencyType">The type for the dependency being added.</param>
        /// <returns></returns>
        public static T RequiresAny<T>(this T obj, Type dependencyType)
            where T : IDependant
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (TypeHelpers.IsValueType(dependencyType))
                throw new ArgumentException($"{ dependencyType } is a value type - only reference types are allowed", nameof(dependencyType));
            return AddTypeDependency(obj, dependencyType, true);
        }

        /// <summary>
        /// Shortcut method for resolving the dependencies for an <see cref="IDependant"/> from a set of objects.
        /// 
        /// All the method does is forward the call to the <see cref="DependencyMetadataCollection.GetDependencies{T}(IEnumerable{T})"/>
        /// method of the <see cref="DependencyMetadataCollection"/> belonging to the passed <see cref="IDependant"/> - <paramref name="obj"/>
        /// </summary>
        /// <typeparam name="T">The type of object on which the method was called.</typeparam>
        /// <typeparam name="TDependency">The common dependency type</typeparam>
        /// <param name="obj">Required - the object whose dependencies are to be resolved.</param>
        /// <param name="objects">Required - the range of objects from which dependencies are to be located.
        /// 
        /// Generally speaking, it's very usual for <paramref name="obj"/> to be present within this range somewhere.</param>
        /// <returns></returns>
        public static IEnumerable<TDependency> GetDependencies<T, TDependency>(this T obj, IEnumerable<TDependency> objects)
            where T : IDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            return obj.Dependencies.GetDependencies(objects);
        }
    }
}

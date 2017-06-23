using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Sdk
{
    /// <summary>
    /// Extension methods for the <see cref="IDependant"/> interface
    /// </summary>
    public static class DependantExtensions
    {
        internal static T AddObjectDependency<T>(T obj, object dep, bool required)
            where T : IMutableDependant
        {
            obj.Dependencies.Add(new ObjectDependency(dep, obj, required));
            return obj;
        }

        internal static T AddObjectDependencies<T>(T obj, IEnumerable<object> deps, bool required)
            where T : IMutableDependant
        {
            obj.Dependencies.AddRange(deps.Where(o => o != null).Select(o => new ObjectDependency(o, obj, required)));
            return obj;
        }

        internal static T AddTypeDependency<T>(T obj, Type dependencyType, bool required)
            where T : IMutableDependant
        {
            obj.Dependencies.Add(new TypeDependency(dependencyType, obj, required));
            return obj;
        }

        /// <summary>
        /// Adds a required dependency from the object on which this is called to <paramref name="dep"/>
        /// 
        /// The object <paramref name="dep"/> must be present in the input collection when dependencies are resolved.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <typeparam name="TDependency">The type for the dependency being added.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="dep">The object upon which <paramref name="obj"/> is dependent.</param>
        /// <returns>The object on which the method is called.</returns>
        public static T Requires<T, TDependency>(this T obj, TDependency dep)
            where T : IMutableDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (dep == null) throw new ArgumentNullException(nameof(dep));

            return AddObjectDependency(obj, dep, true);
        }

        /// <summary>
        /// Adds a required dependency from the object on which this is called to all the objects in <paramref name="deps"/>
        /// 
        /// Every object in <paramref name="deps"/> must be present in the input collection when dependencies are resolved.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <typeparam name="TDependency">The type for the dependency being added.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="deps">The objects upon which <paramref name="obj"/> is dependent.</param>
        /// <returns>The object on which the method is called.</returns>
        public static T Requires<T, TDependency>(this T obj, IEnumerable<TDependency> deps)
            where T : IMutableDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (deps == null) throw new ArgumentNullException(nameof(deps));

            return AddObjectDependencies(obj, deps, true);
        }

        /// <summary>
        /// Adds a required dependency from the object on which this is called to at least one object of the type
        /// <paramref name="dependencyType"/>.  Use this when only the type of the dependency is known, but the specific
        /// instance is not important.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="dependencyType">The type of objects upon which the object is dependent.</param>
        /// <returns>The object on which the method is called.</returns>
        public static T RequiresAny<T>(this T obj, Type dependencyType)
            where T : IMutableDependant
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (TypeHelpers.IsValueType(dependencyType))
                throw new ArgumentException($"{ dependencyType } is a value type - only reference types are allowed", nameof(dependencyType));
            return AddTypeDependency(obj, dependencyType, true);
        }

        /// <summary>
        /// Adds an optional dependency from the object on which this is called to <paramref name="dep"/>.
        /// 
        /// If the object <paramref name="dep"/> is present in the input collection when dependencies are resolved, then it
        /// will be identified.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <typeparam name="TDependency">The type for the dependency being added.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="dep">The object upon which <paramref name="obj"/> is optionally dependent.</param>
        /// <returns>The object on which the method is called.</returns>
        public static T After<T, TDependency>(this T obj, TDependency dep)
            where T : IMutableDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (dep == null) throw new ArgumentNullException(nameof(dep));

            return AddObjectDependency(obj, dep, false);
        }

        /// <summary>
        /// Adds an optional dependency from the object on which this is called to all the objects in <paramref name="deps"/>.
        /// 
        /// Any objects in <paramref name="deps"/> which are present in the input collection when dependencies are resolved will
        /// be identified as dependencies.  None of them will be required.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <typeparam name="TDependency">The type for the dependency being added.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="deps">The objects upon which <paramref name="obj"/> is optionally dependent.</param>
        /// <returns>The object on which the method is called.</returns>
        public static T After<T, TDependency>(this T obj, IEnumerable<TDependency> deps)
            where T : IMutableDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (deps == null) throw new ArgumentNullException(nameof(deps));

            return AddObjectDependencies(obj, deps, true);
        }

        /// <summary>
        /// Adds an optional dependency from the object on which this is called to any object of the type
        /// <paramref name="dependencyType"/>.  Use this when only the type of the dependency is known, but the specific
        /// instance is not important.
        /// </summary>
        /// <typeparam name="T">The type of object on which this is called.</typeparam>
        /// <param name="obj">The object to which a dependency is to be added.</param>
        /// <param name="dependencyType">The type for the dependency being added.</param>
        /// <returns>The object on which the method is called.</returns>
        public static T AfterAny<T>(this T obj, Type dependencyType)
            where T : IMutableDependant
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (TypeHelpers.IsValueType(dependencyType))
                throw new ArgumentException($"{ dependencyType } is a value type - only reference types are allowed", nameof(dependencyType));
            return AddTypeDependency(obj, dependencyType, true);
        }

        /// <summary>
        /// Shortcut method for resolving the dependencies for an <see cref="IDependant"/> from a set of objects.
        /// 
        /// All the method does is forward the call to the <see cref="DependencyEnumerableExtensions.GetDependencies{T}(IEnumerable{DependencyMetadata}, IEnumerable{T})"/>
        /// method of the <see cref="DependencyMetadataCollection"/> belonging to the passed <see cref="IDependant"/> - <paramref name="obj"/>
        /// </summary>
        /// <typeparam name="T">The type of object on which the method was called.</typeparam>
        /// <typeparam name="TDependency">The common dependency type</typeparam>
        /// <param name="obj">Required - the object whose dependencies are to be resolved.</param>
        /// <param name="objects">Required - the range of objects from which dependencies are to be located.
        /// 
        /// Generally speaking, it's typical for <paramref name="obj"/> to be a member of this range.</param>
        /// <returns></returns>
        public static IEnumerable<TDependency> GetDependencies<T, TDependency>(this T obj, IEnumerable<TDependency> objects)
            where T : IDependant
            where TDependency : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            return obj.Dependencies.GetDependencies(objects);
        }

        // TODO: Add the non-required public extensions when we need them (YAGNI): Follows/FollowsAll<> or After/AfterAll<>
    }
}

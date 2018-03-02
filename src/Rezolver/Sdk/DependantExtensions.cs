// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (dep == null)
            {
                throw new ArgumentNullException(nameof(dep));
            }

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (deps == null)
            {
                throw new ArgumentNullException(nameof(deps));
            }

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (TypeHelpers.IsValueType(dependencyType))
            {
                throw new ArgumentException($"{dependencyType} is a value type - only reference types are allowed", nameof(dependencyType));
            }

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (dep == null)
            {
                throw new ArgumentNullException(nameof(dep));
            }

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (deps == null)
            {
                throw new ArgumentNullException(nameof(deps));
            }

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (TypeHelpers.IsValueType(dependencyType))
            {
                throw new ArgumentException($"{dependencyType} is a value type - only reference types are allowed", nameof(dependencyType));
            }

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            return obj.Dependencies.GetDependencies(objects);
        }

        /// <summary>
        /// Creates a dependency for the object on which this method is invoked to objects of a particular type - either required or optional.
        /// </summary>
        /// <typeparam name="TDependency">The type of objects that will be the target of the dependency.</typeparam>
        /// <param name="obj">The dependant object for which the dependency is to be created.</param>
        /// <param name="required"><c>true</c> to make the dependency required; <c>false</c> to make it optional.</param>
        /// <returns>A new <see cref="DependencyMetadata"/> object representing a dependency from <paramref name="obj"/> to
        /// objects of type <typeparamref name="TDependency"/>.</returns>
        /// <remarks>As with <see cref="CreateObjectDependency{TDependency}(IDependant, TDependency, bool)"/>, this method creates a
        /// new <see cref="DependencyMetadata"/> object and returns it, as opposed to the methods
        /// <see cref="RequiresAny{T}(T, Type)"/> or <see cref="AfterAny{T}(T, Type)"/>, which create and add the dependency directly
        /// to an <see cref="IMutableDependant"/> object.
        ///
        /// This method is used most often by <see cref="IDependant"/> objects (i.e. those with read-only dependencies) which have a
        /// fixed set of required or optional dependencies that are known at construction-time.</remarks>
        public static DependencyMetadata CreateTypeDependency<TDependency>(this IDependant obj, bool required = true)
            where TDependency : class
        {
            return new TypeDependency(typeof(TDependency), obj, required);
        }

        /// <summary>
        /// Creates a dependency for the object which this method is invoked to a specific instance of a particular type - either
        /// required or optional.
        /// </summary>
        /// <typeparam name="TDependency">The type of the object that will be the target of the dependency.</typeparam>
        /// <param name="obj">The dependant object for which the dependency is to be created.</param>
        /// <param name="dependency">The object that will be the target of the dependency.</param>
        /// <param name="required"><c>true</c> to make the dependency required; <c>false</c> to make it optional.</param>
        /// <returns>A new <see cref="DependencyMetadata"/> object representing a dependency from <paramref name="obj"/> to
        /// the object <paramref name="dependency"/>.</returns>
        /// <remarks>As with <see cref="CreateTypeDependency{TDependency}(IDependant, bool)"/>, this method creates a
        /// new <see cref="DependencyMetadata"/> object and returns it, as opposed to the methods
        /// <see cref="Requires{T, TDependency}(T, TDependency)"/> or <see cref="After{T, TDependency}(T, TDependency)"/>,
        /// which create and add the dependency directly to an <see cref="IMutableDependant"/> object.
        ///
        /// This method is used most often by <see cref="IDependant"/> objects (i.e. those with read-only dependencies) which have a
        /// fixed set of required or optional dependencies that are known at construction-time.</remarks>
        public static DependencyMetadata CreateObjectDependency<TDependency>(this IDependant obj, TDependency dependency, bool required = true)
            where TDependency : class
        {
            return new ObjectDependency(dependency, obj, required);
        }
    }
}

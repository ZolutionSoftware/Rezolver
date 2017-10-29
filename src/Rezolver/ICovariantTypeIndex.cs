using System;
using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// For any type `T`, there exists a list of zero or more other types with which
    /// it is compatible when covariance is supported.  This index provides the mechanism
    /// by which Rezolver can perform this reverse lookup.
    /// </summary>
    /// <remarks>
    /// In an IOC container, contravariance is signficantly easier to support since the only knowledge
    /// required is the type you want to retrieve - e.g. `Action&lt;Derived&gt;`.  To build a list of all
    /// the generic versions of `Action&lt;&gt;` which will be assignment-compatible to that type simply
    /// requires walking 'up' the type hierarchy of `Derived`.
    /// 
    /// Covariance, on the other hand, is the opposite problem.  The caller requests a concrete generic
    /// type - e.g. `Func&lt;Base&gt; - where one or more of the arguments is passed to a covariant type 
    /// parameter.  In this case, there is a theoretically infinite set of types which might be assignment 
    /// compatible to `Base`, so how do you determine which versions of `Func&lt;&gt;` to search for if there
    /// is no registration for the exact type?
    /// 
    /// One way would be iterate all the registrations and filter out only those which are compatible, however
    /// this would take up quite a lot of time.  Rezolver, instead, uses this index, into which you add concrete 
    /// types that you *know* have got at least one registered <see cref="ITarget"/>.  Later, when a generic type is
    /// sought, you can call that type up from this index, and it'll return a list of all the known types that have 
    /// been added which are assignment compatible to that type by virtue of covariance.
    /// 
    /// This index provides two lookups based on types that are registered: pure covariant-compatible types (e.g.
    /// `IEnumerable&lt;Derived&gt;` can be assigned to a reference of type `IEnumerable&lt;Base&gt;`) and 
    /// assignment-compatible types (e.g `IEnumerable&lt;Base&gt;` can be assigned to a reference of type `IEnumerable`.
    /// 
    /// When searching for a generic type that matches one in-hand - the covariant compatibility lookup is the one you
    /// need.  If you are conducting a search for a generic type which has a covariant type parameter (e.g. such as
    /// `IEnumerable&lt;&gt;` then you want the full assignment-compatibility lookup which does covariance as well as
    /// base/interface compatibility.
    /// </remarks>
    public interface ICovariantTypeIndex
    {
        /// <summary>
        /// Called to add a new known service type to the index which is then mapped back to all the types with which it
        /// is assignment compatible via covariance and via base class or interface inheritance.
        /// 
        /// The <see cref="GetKnownCovariantTypes(Type)"/> will then return the <paramref name="serviceType"/>
        /// when called with one of these compatible types.
        /// </summary>
        /// <param name="serviceType">The type to be added to the index.</param>
        void AddKnownType(Type serviceType);
        /// <summary>
        /// This method returns all types, in order of most to least recently added - that have been added via the 
        /// <see cref="AddKnownType(Type)"/> method and which are of the same generic type but which have one or 
        /// more type arguments that are reference compatible to those in the passed <paramref name="serviceType"/>.
        /// So, if you pass `Func&lt;Base&gt;` then you might get back `Func&lt;Derived&gt;`
        /// if it has been added.
        /// </summary>
        /// <param name="serviceType">The type for which covariantly compatible known types are sought.</param>
        /// <returns>A non-null (possibly empty) enumerable of types which have been previously been added to the index
        /// via a call to <see cref="AddKnownType(Type)"/> and which are covariantly compatible with <paramref name="serviceType"/>.</returns>
        IEnumerable<Type> GetKnownCovariantTypes(Type serviceType);
        /// <summary>
        /// This method returns the same types as <see cref="GetKnownCovariantTypes(Type)"/> but also adds any other types which are
        /// assignment compatible with the <paramref name="serviceType"/>.  This includes derived types or types which implement the 
        /// interface, or other generics which are covariantly compatible via implemented interfaces.
        /// 
        /// This method is suited for use when you are searching for targets for a covariant type parameter - such as looking for
        /// targets which can be included in an `IEnumerable&lt;T&gt;` (which is exactly what Rezolver does for enumerables).
        /// 
        /// In this case, the order is most to least recent covariants, followed by most to least recent
        /// derived types.
        /// </summary>
        /// <param name="serviceType">The type for which all assignment-compatible known types are sought.</param>
        /// <returns>A non-null (possibly empty) enumerable of types which have been previously been added to the index
        /// via a call to <see cref="AddKnownType(Type)"/> and which are covariantly compatible with <paramref name="serviceType"/>
        /// either directly or by inheritance.</returns>
        IEnumerable<Type> GetKnownCompatibleTypes(Type serviceType);
    }
}
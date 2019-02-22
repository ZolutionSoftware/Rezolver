// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Reflection;

namespace Rezolver.Runtime
{

    /// <summary>
    /// Result returned from the <see cref="Targets.GenericConstructorTarget.MapType(Type)"/> function.  Represents various levels of success -
    /// from a completely incompatible mapping (<see cref="Success"/> = <c>false</c>), or a successful mapping from
    /// an open generic type to a closed generic type which can then be constructed (<see cref="Success"/> = <c>true</c>
    /// and <see cref="IsFullyBound"/> = <c>true</c>) or, a successful mapping from an open generic type to another open
    /// generic type (<see cref="Success"/> = <c>true</c> but <see cref="IsFullyBound"/> = <c>false</c>).
    ///
    /// This mapping is then used by both the <see cref="Targets.GenericConstructorTarget.SupportsType(Type)"/> and
    /// <see cref="Targets.GenericConstructorTarget.Bind(Compilation.ICompileContext)"/>
    /// functions.  Only fully bound mappings are supported by <see cref="Targets.GenericConstructorTarget.Bind(Compilation.ICompileContext)"/>, whereas
    /// <see cref="Targets.GenericConstructorTarget.SupportsType(Type)"/> will return <c>true</c> so long as the <see cref="Success"/> is true.
    ///
    /// The caller, therefore, must ensure it is aware of the difference between open and closed generics.
    /// </summary>
    public class GenericTypeMapping
    {
        /// <summary>
        /// Gets a string describing the reason why the type could not be mapped.  Can be used for exceptions, etc.
        ///
        /// Note that this can be set even if <see cref="Success"/> is <c>true</c> - because mappings exist between
        /// open generic types so that a target's <see cref="Targets.GenericConstructorTarget.SupportsType(Type)"/> returns <c>true</c>, but the
        /// <see cref="Targets.GenericConstructorTarget.Bind(Compilation.ICompileContext)"/> function throws an exception for the same type, since you can't create
        /// an instance of an open generic.
        /// </summary>
        /// <value>The binding error message.</value>
        public string BindErrorMessage { get; }
        /// <summary>
        /// The type requested for mapping.  If this is an open generic, then the best result for this mapping will be
        /// that <see cref="Success"/> is <c>true</c> and <see cref="IsFullyBound"/> is <c>false</c>.
        /// </summary>
        public Type RequestedType { get; }
        /// <summary>
        /// If <see cref="Success"/> = <c>true</c>, gets the generic type to be used for the <see cref="RequestedType"/>.
        ///
        /// Note that this could be either an open or closed generic - the <see cref="IsFullyBound"/> offers a quick means
        /// by which to determine this.  If <see cref="IsFullyBound"/> is <c>true</c>, then the mapping will succeed when
        /// encountered by the <see cref="Targets.GenericConstructorTarget.Bind(Compilation.ICompileContext)"/> method.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }
        /// <summary>
        /// If a specific constructor is to be used, then this is it.
        /// </summary>
        public ConstructorInfo Constructor { get; }
        /// <summary>
        /// Gets a value indicating whether the <see cref="Targets.GenericConstructorTarget.DeclaredType"/> of the
        /// <see cref="Targets.GenericConstructorTarget"/>
        /// was successfully mapped to the requested type.  If so, and <see cref="IsFullyBound"/> is <c>true</c>, then an
        /// instance of <see cref="Type"/> will be compatible with the type that was requested.
        ///
        /// If <see cref="IsFullyBound"/> is <c>false</c>, then you can't create an instance of <see cref="Type"/> because it's
        /// an open generic - but you will be able to bind the same target to a closed generic of the same <see cref="Type"/>.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get { return Type != null; } }

        /// <summary>
        /// If true, then the <see cref="Type"/> is a fully closed generic type that can be constructed (and therefore would
        /// be successfully bound by the <see cref="Targets.GenericConstructorTarget.Bind(Compilation.ICompileContext)"/> method, which uses the
        /// <see cref="Targets.GenericConstructorTarget.MapType(Type)"/>
        /// method).  If this is <c>false</c> but <see cref="Success"/> is <c>true</c>, then while the target is technically
        /// compatible with the requested type, you can't create an instance.  The target will, however, be able to mapped to
        /// a closed generic type based on the same <see cref="Type"/>.
        /// </summary>
        public bool IsFullyBound { get { return Success ? Type.IsConstructedGenericType : false; } }

        internal GenericTypeMapping(Type requestedType, Type type, ConstructorInfo constructor = null, string bindErrorMessage = null)
        {
            RequestedType = requestedType;
            Type = type;
            Constructor = constructor;
            BindErrorMessage = bindErrorMessage;
        }

        internal GenericTypeMapping(Type requestedType, string errorMessage)
        {
            RequestedType = requestedType;
            BindErrorMessage = errorMessage;
        }
    }
}

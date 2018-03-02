// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver.Targets
{
    /// <summary>
    /// A target that simply creates a default instance of a given type.  I.e. the same
    /// as doing default(type) in C#.
    /// </summary>
    /// <remarks>The type also implements the <see cref="ICompiledTarget"/> interface for direct
    /// resolving.</remarks>
    public class DefaultTarget : TargetBase, ICompiledTarget, IDirectTarget
    {
        private static class Default<T>
        {
            public static readonly T Value = default(T);
        }

        private static readonly ConcurrentDictionary<Type, Func<object>> _defaultCallbacks = new ConcurrentDictionary<Type, Func<object>>();

        // internal to allow other classes take advantage of late-bound defaults
        internal static object GetDefault(Type type)
        {
            return _defaultCallbacks.GetOrAdd(type, t =>
            {
                var tDefault = typeof(Default<>).MakeGenericType(type);
                return Expression.Lambda<Func<object>>(
                    // the convert is important to handle boxing conversions for value types.
                    Expression.Convert(
                        Expression.Field(null, tDefault.GetStaticFields().Single(f => f.Name == "Value")), typeof(object)
                    )
                ).CompileForRezolver();
            })();
        }

        /// <summary>
        /// Override of <see cref="TargetBase.ScopeBehaviour"/> - always returns
        /// <see cref="ScopeBehaviour.None"/>.
        /// </summary>
        /// <value>The scope behaviour.</value>
        public override ScopeBehaviour ScopeBehaviour
        {
            get
            {
                return ScopeBehaviour.None;
            }
        }

        private readonly Type _declaredType;

        /// <summary>
        /// Always equal to the type for which the default value will be returned
        /// </summary>
        public override Type DeclaredType
        {
            get { return this._declaredType; }
        }

        /// <summary>
        /// Gets the actual default value represented by this instance.
        /// </summary>
        public object Value
        {
            get
            {
                return GetDefault(this.DeclaredType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTarget"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DefaultTarget(Type type)
        {
            type.MustNotBeNull("type");
            this._declaredType = type;
        }

        ITarget ICompiledTarget.SourceTarget => this;

        object ICompiledTarget.GetObject(IResolveContext context) => this.Value;

        object IDirectTarget.GetValue() => this.Value;
    }
}
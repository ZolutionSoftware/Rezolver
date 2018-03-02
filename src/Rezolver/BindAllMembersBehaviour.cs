// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rezolver.Compilation;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// This implementation of <see cref="IMemberBindingBehaviour"/> binds all publicly writable
    /// properties and public fields to values obtained from the container.
    /// </summary>
    /// <seealso cref="Rezolver.IMemberBindingBehaviour" />
    /// <remarks>This is a stateless singleton accessible through the <see cref="MemberBindingBehaviour.BindAll"/>
    /// static property.
    ///
    /// The class also serves as a good starting point for any custom binding behaviours you might need - as there
    /// are numerous virtual methods which allow you to change which fields and/or properties are selected for binding,
    /// as well as how those bindings are created.
    ///
    /// The default behaviour is to bind each member to a new <see cref="ResolvedTarget"/> whose
    /// <see cref="ResolvedTarget.DeclaredType"/> is set to the member's type.</remarks>
    public class BindAllMembersBehaviour : IMemberBindingBehaviour
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindAllMembersBehaviour"/> class.
        ///
        /// Can only be created by Rezolver or through inheritance.
        /// </summary>
        protected internal BindAllMembersBehaviour()
        {
        }

        /// <summary>
        /// Implementation of <see cref="IMemberBindingBehaviour.GetMemberBindings(ICompileContext, Type)"/>.
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are to be bound.</param>
        /// <remarks>The base implementation calls <see cref="GetBindableProperties(ICompileContext, Type)"/>, passing the resultant enumerable to
        /// the <see cref="BindProperties(ICompileContext, Type, IEnumerable{PropertyInfo})"/> function; it also does the same thing with
        /// <see cref="GetBindableFields(ICompileContext, Type)"/> and <see cref="BindFields(ICompileContext, Type, IEnumerable{FieldInfo})"/> -
        /// concatenating the two enumerables together and returning the result as an array of <see cref="MemberBinding"/> objects.</remarks>
        /// <returns>Bindings for all bindable properties and fields.</returns>
        public virtual MemberBinding[] GetMemberBindings(ICompileContext context, Type type)
        {
            // find all publicly writable properties and public fields, emit
            return this.BindProperties(context, type, this.GetBindableProperties(context, type))
              .Concat(this.BindFields(context, type, this.GetBindableFields(context, type))).ToArray();
        }

        /// <summary>
        /// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/> - iterates through the
        /// <paramref name="fields"/>, calling <see cref="CreateBinding(ICompileContext, Type, FieldInfo)"/> for each,
        /// and those which are non-null.
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are to be bound.</param>
        /// <param name="fields">The fields for which bindings are to be created.  This is fed by
        /// the function <see cref="GetBindableFields(ICompileContext, Type)"/></param>
        /// <returns>An enumerable containing bindings for each of the passed <paramref name="fields"></paramref></returns>
        protected virtual IEnumerable<MemberBinding> BindFields(ICompileContext context, Type type, IEnumerable<FieldInfo> fields)
        {
            return fields.Select(f => this.CreateBinding(context, type, f)).Where(b => b != null);
        }

        /// <summary>
        /// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/> - iterates through the
        /// <paramref name="properties"/>, calling <see cref="CreateBinding(ICompileContext, Type, PropertyInfo)"/> for each,
        /// and those which are non-null.
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are to be bound.</param>
        /// <param name="properties">The properties for which bindings are to be created.  This is fed by
        /// the function <see cref="GetBindableProperties(ICompileContext, Type)"/></param>
        /// <returns>An enumerable of <see cref="MemberBinding"/> objects representing the bindings to be used
        /// for each bindable property in <paramref name="properties"/>.</returns>
        protected virtual IEnumerable<MemberBinding> BindProperties(ICompileContext context, Type type, IEnumerable<PropertyInfo> properties)
        {
            return properties.Select(p => this.CreateBinding(context, type, p)).Where(b => b != null);
        }

        /// <summary>
        /// Creates a binding for the given field.
        ///
        /// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/>
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are to be bound.</param>
        /// <param name="field">The field for which a binding is to be created.</param>
        /// <remarks>Override this method to customise the binding that is create for the given
        /// field, or to prevent the binding from being created at all(return <c>null</c> if you want
        /// to abort binding the field).
        ///
        /// The base implementation simply creates a new <see cref="MemberBinding"/> whose
        /// <see cref="MemberBinding.Target"/> is set to a new <see cref="ResolvedTarget"/> for the type
        /// <see cref="FieldInfo.FieldType"/> - thus causing the field to be assigned a value
        /// resolved from the container when the instance is created.</remarks>
        /// <returns>An individual binding for the passed <paramref name="field"/></returns>
        protected virtual MemberBinding CreateBinding(ICompileContext context, Type type, FieldInfo field)
        {
            return new MemberBinding(field, new ResolvedTarget(field.FieldType));
        }

        /// <summary>
        /// Creates a binding for the given property.
        ///
        /// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/>
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are to be bound.</param>
        /// <param name="prop">The property for which a binding is to be created.</param>
        /// <remarks>Override this method to customise the binding that is create for the given
        /// property, or to prevent the binding from being created at all(return <c>null</c> if you want
        /// to abort binding the property).
        ///
        /// The base will create a <see cref="MemberBinding"/> for publicly writable properties; and a
        /// <see cref="ListMemberBinding"/> for publicly readable properties which follow the rules
        /// for types supporting .Net's collection initialisers
        /// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers#collection-initializers</remarks>
        /// <returns>An individual binding for the passed <paramref name="prop"/></returns>
        protected virtual MemberBinding CreateBinding(ICompileContext context, Type type, PropertyInfo prop)
        {
            if (prop.IsPubliclyWritable())
            {
                return new MemberBinding(prop, new ResolvedTarget(prop.PropertyType));
            }
            else if (prop.IsPubliclyReadable())
            {
                var bindable = prop.PropertyType.GetBindableCollectionTypeInfo();
                if (bindable != null)
                {
                    return new ListMemberBinding(prop, new ResolvedTarget(typeof(IEnumerable<>).MakeGenericType(bindable.ElementType)), bindable.ElementType, bindable.AddMethod);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the bindable fields on the <paramref name="type"/>.
        ///
        /// Used by <see cref="GetMemberBindings(ICompileContext, Type)"/> and passed to the
        /// <see cref="BindFields(ICompileContext, Type, IEnumerable{FieldInfo})"/> method.
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are to be bound.</param>
        /// <remarks>Override this method to filter the fields which can be bound.  The base implementation
        /// returns all public instance fields declared on the <paramref name="type"/>.
        /// </remarks>
        /// <returns>An enumerable of the fields that can be bound on the given <paramref name="type"/></returns>
        protected virtual IEnumerable<FieldInfo> GetBindableFields(ICompileContext context, Type type)
        {
            return type.GetInstanceFields().Where(this.ShouldBind);
        }

        /// <summary>
        /// Gets the bindable properties on the <paramref name="type"/>.
        ///
        /// Used by <see cref="GetMemberBindings(ICompileContext, Type)"/> and passed to the
        /// <see cref="BindProperties(ICompileContext, Type, IEnumerable{PropertyInfo})"/> method.
        /// </summary>
        /// <param name="context">The current compilation context.</param>
        /// <param name="type">The type whose members are to be bound.</param>
        /// <remarks>Override this method to filter the properties which can be bound.  The base implementation
        /// returns all non-indexer instance properties which have publicly accessible 'set' accessors.</remarks>
        /// <returns>An enumerable of the properties that can be bound on the given <paramref name="type"/></returns>
        protected virtual IEnumerable<PropertyInfo> GetBindableProperties(ICompileContext context, Type type)
        {
            return type.GetInstanceProperties().Where(this.ShouldBind);
        }

        /// <summary>
        /// Used by default by the <see cref="GetBindableProperties(ICompileContext, Type)"/> method to filter all
        /// instance properties on a type which can be bound.
        ///
        /// Returns true if the property does not have an index and is either publicly writable, or is a bindable collection
        /// type.  The latter follows the same rules as .Net collection initialisers,
        /// described here https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers#collection-initializers
        /// </summary>
        /// <param name="pi">The <see cref="PropertyInfo"/> of the property to be checked.</param>
        /// <returns><c>true</c> if the property <paramref name="pi"/> should be bound.</returns>
        protected virtual bool ShouldBind(PropertyInfo pi)
        {
            return pi.GetIndexParameters()?.Length == 0 && (pi.IsPubliclyWritable() || pi.PropertyType.IsBindableCollectionType());
        }

        /// <summary>
        /// Used by default by the <see cref="GetBindableFields(ICompileContext, Type)"/> method to filter all
        /// the instance fields down to those which should be bound.
        /// </summary>
        /// <param name="fi">The <see cref="FieldInfo"/> of the property to be checked.</param>
        /// <returns><c>true</c> if the field <paramref name="fi"/> should be bound.</returns>
        protected virtual bool ShouldBind(FieldInfo fi)
        {
            return fi.IsPublic;
        }
    }
}
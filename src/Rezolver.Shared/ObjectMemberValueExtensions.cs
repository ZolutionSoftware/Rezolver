// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rezolver
{
    internal static class ObjectMemberValueExtensions
    {
        /// <summary>
        /// Used when reading objects to turn them into dictionaries.
        /// </summary>
        internal class ObjectMemberValue
        {
            /// <summary>
            /// Gets the name of the object member (taken from the name of the fieldor property).
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; }
            /// <summary>
            /// Gets the value of the property or field on the object that this was created for.
            ///
            /// If an exception occurred during the read, then <see cref="ValueError"/> will be <c>true</c>
            /// and this property will contain the exception that occurred.
            /// </summary>
            public object Value { get; }
            /// <summary>
            /// Gets a value indicating whether the <see cref="Value"/> is an exception that was raised when attempting
            /// to read the property or field.
            /// </summary>
            public bool ValueError { get; }
            /// <summary>
            /// Gets the type of the object member.  Note - that's not the type of the <see cref="Value"/>,
            /// but the statically declared type of the field or property.
            /// </summary>
            public Type Type { get; }

            public ObjectMemberValue(object obj, MemberInfo member)
            {
                this.Name = member.Name;
                // initialise it to *something*!
                this.Type = typeof(object);
                Func<object, MemberInfo, object> accessor = null;
                if (member is FieldInfo)
                {
                    accessor = GetValueFromField;
                    this.Type = ((FieldInfo)member).FieldType;
                }
                else if (member is PropertyInfo)
                {
                    accessor = GetValueFromProperty;
                    this.Type = ((PropertyInfo)member).PropertyType;
                }

                if (accessor != null)
                {
                    try
                    {
                        this.Value = accessor(obj, member);
                    }
                    catch (Exception ex)
                    {
                        this.ValueError = true;
                        this.Value = ex;
                    }
                }
            }

            private static object GetValueFromProperty(object o, MemberInfo p)
            {
                return ((PropertyInfo)p).GetValue(o);
            }

            private static object GetValueFromField(object o, MemberInfo f)
            {
                return ((FieldInfo)f).GetValue(o);
            }
        }

        internal static IEnumerable<ObjectMemberValue> MemberValues(this object obj)
        {
            if (obj == null)
            {
                return Enumerable.Empty<ObjectMemberValue>();
            }

            return obj.GetType().GetInstanceProperties().PubliclyReadable().Cast<MemberInfo>().Concat(
                obj.GetType().GetInstanceFields().Public().Cast<MemberInfo>()).Select(m => new ObjectMemberValue(obj, m));
        }

        internal static Dictionary<string, object> ToMemberValueDictionary(this object obj)
        {
            return MemberValues(obj).ToDictionary(mv => mv.Name, mv => mv.Value);
        }

        internal static Dictionary<string, TValue> ToMemberValueDictionary<TValue>(this object obj)
        {
            return MemberValues(obj).Where(mv => mv.Value is TValue).ToDictionary(mv => mv.Name, mv => (TValue)mv.Value);
        }
    }
}

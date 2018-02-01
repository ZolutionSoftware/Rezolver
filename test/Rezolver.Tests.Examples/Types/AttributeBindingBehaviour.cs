// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Compilation;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    /// <summary>
    /// Class AttributeBindingBehaviour - we reuse the BindAllMembersBehaviour class
    /// because it has numerous virtual hooks we can use to customise behaviour, and takes care
    /// of reflecting the target type for us.
    /// </summary>
    /// <seealso cref="Rezolver.BindAllMembersBehaviour" />
    public class AttributeBindingBehaviour : BindAllMembersBehaviour
    {
        protected override IEnumerable<FieldInfo> GetBindableFields(
            ICompileContext context, Type type)
        {
            //filter the fields to those which have an InjectAttribute defined
            return base.GetBindableFields(context, type)
                .Where(f => f.IsDefined(typeof(InjectAttribute)));
        }

        protected override IEnumerable<PropertyInfo> GetBindableProperties(
            ICompileContext context, Type type)
        {
            return base.GetBindableProperties(context, type)
                .Where(f => f.IsDefined(typeof(InjectAttribute)));
        }


        protected override MemberBinding CreateBinding(
            ICompileContext context, Type type, FieldInfo field)
        {
            //the base method merely creates a new MemberBinding, bound to a new ResolvedTarget
            //whose type is set to the field type.
            //This is similar except we read the InjectAttribute's ResolveType, and use that
            //type if it's not null.
            var attr = field.GetCustomAttribute<InjectAttribute>();
            return new MemberBinding(field, new ResolvedTarget(attr.Type ?? field.FieldType));
        }

        protected override MemberBinding CreateBinding(
            ICompileContext context, Type type, PropertyInfo prop)
        {
            //identical to above
            var attr = prop.GetCustomAttribute<InjectAttribute>();
            return new MemberBinding(prop,
                new ResolvedTarget(attr.Type ?? prop.PropertyType));
        }
    }
    //</example>
}

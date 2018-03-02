// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    internal struct TypeAndTargetId : IEquatable<TypeAndTargetId>
    {
        public Type Type { get; }

        public Guid Id { get; }

        public TypeAndTargetId(Type type, ITarget target)
        {
            this.Type = type;
            this.Id = target.Id;
        }

        public TypeAndTargetId(Type type, Guid id)
        {
            this.Type = type;
            this.Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is TypeAndTargetId ttObj))
            {
                return false;
            }

            return this.Equals(ttObj);
        }

        public bool Equals(TypeAndTargetId other)
        {
            return this.Type == other.Type && this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() ^ this.Id.GetHashCode();
        }
    }
}

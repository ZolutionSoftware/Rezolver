// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    internal readonly struct TypeAndTargetId : IEquatable<TypeAndTargetId>
    {
        public Type Type { get; }

        public int Id { get; }

        public TypeAndTargetId(Type type, ITarget target)
        {
            Type = type;
            Id = target.Id;
        }

        public TypeAndTargetId(Type type, int id)
        {
            Type = type;
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals((TypeAndTargetId)obj);
        }

        public bool Equals(TypeAndTargetId other)
        {
            return Id == other.Id && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Id.GetHashCode();
        }
    }
}

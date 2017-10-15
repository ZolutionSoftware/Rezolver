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
            Type = type;
            Id = target.Id;
        }

        public TypeAndTargetId(Type type, Guid id)
        {
            Type = type;
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is TypeAndTargetId)) return false;
            return Equals((TypeAndTargetId)obj);
        }
        public bool Equals(TypeAndTargetId other)
        {
            return Type == other.Type && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Id.GetHashCode();
        }
    }
}

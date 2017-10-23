using System;

namespace Rezolver
{

    [Flags]
    internal enum Contravariance
    {
        None = 0,
        Bases = 1,
        Interfaces = 2,
        BasesAndInterfaces = Bases | Interfaces,
        Derived = 4
    }
}

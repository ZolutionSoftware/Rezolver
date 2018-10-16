using System;

namespace Rezolver
{
    public static partial class AutoFactoryRegistrationExtensions
    {
        private static void EnableAutoFactoryInternal(IRootTargetContainer targets, Type funcType)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            targets.AddKnownType(funcType);
            targets.SetOption<Options.EnableSpecificAutoFactory>(true, funcType);
        }
    }
}

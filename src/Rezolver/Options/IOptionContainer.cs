using System;

namespace Rezolver.Options
{
    // these types have attribute on them which force TargetDictionaryContainer's default behaviour of looking up
    // container types and container factories for to be sidestepped.
    // Without this, every target container causes a stack overflow as soon as it's constructed with any of the built-in
    // configs.  At least the hack is completely hidden from view.

    //[Runtime.GenericContainerTypeHelper]
    internal interface IOptionContainer<out TOption>
    {
        TOption Option { get; }
    }

    //[Runtime.GenericContainerTypeHelper]
    internal interface IOptionContainer<in TService, out TOption> : IOptionContainer<TOption>
    {

    }

    //[Runtime.GenericContainerTypeHelper]
    internal interface IAnyGenericOptionContainer<out TOption> : IOptionContainer<IOptionForAnyGeneric, TOption>
    {

    }
}
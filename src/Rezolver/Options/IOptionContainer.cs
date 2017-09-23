using Rezolver.Runtime;

namespace Rezolver.Options
{
    [Contravariance(false)]
    internal interface IOptionContainer<out TOption>
    {
        TOption Option { get; }
    }

    [Contravariance(true)]
    internal interface IOptionContainer<in TService, out TOption> : IOptionContainer<TOption>
    {

    }

    [Contravariance(false)]
    internal interface IAnyGenericOptionContainer<out TOption> : IOptionContainer<IOptionForAnyGeneric, TOption>
    {

    }
}
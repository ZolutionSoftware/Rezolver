namespace Rezolver.Options
{
    internal interface IOptionContainer<out TOption>
    {
        TOption Option { get; }
    }

    internal interface IOptionContainer<in TService, out TOption> : IOptionContainer<TOption>
    {

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rezolver.Compilation;
using Rezolver.Tests.Types;

namespace Rezolver.Tests.Compilation.Expressions
{
  public abstract partial class TargetCompilerTestsBase
  {
    internal interface ITransient
    {
    }

    internal sealed class Transient : InstanceCountingTypeBase<Transient>, ITransient
    {

    }

    internal interface IRequiresInt
    {
      int Int { get; }
    }

    internal class RequiresInt : IRequiresInt
    {
      public RequiresInt(int @int)
      {
        Int = @int;
      }

      public int Int { get; private set; }
    }

    internal interface IRequiresNullableInt
    {
      int? NullableInt { get; }
    }

    internal class RequiresNullableInt : IRequiresNullableInt
    {
      public int? NullableInt { get; private set; }

      public RequiresNullableInt(int? nullableInt)
      {
        NullableInt = nullableInt;
      }
    }

    internal interface ISingleton { }

    //intended to be used as a singleton during compilation tests.
    internal sealed class Singleton : InstanceCountingTypeBase<Singleton>, ISingleton
    {
      public Singleton()
      {

      }
    }

    internal interface IComposite
    {
      ISingleton Singleton { get; }
      ITransient Transient { get; }
    }

    internal class Composite : IComposite
    {
      private readonly ISingleton _singleton;
      private readonly ITransient _transient;

      public Composite(ISingleton singleton, ITransient transient)
      {
        _singleton = singleton;
        _transient = transient;
      }

      public ISingleton Singleton
      {
        get { return _singleton; }
      }

      public ITransient Transient
      {
        get { return _transient; }
      }
    }

    internal class HasProperty
    {
      public IRequiresInt RequiresInt { get; set; }
    }

    /// <summary>
    /// interface for the type that will be used to test the registration and compilation of all the different target types.
    /// </summary>
    internal interface ISuperComplex
    {
      int Int { get; }
      int? NullableInt { get; }
      string String { get; }
      ITransient Transient { get; }
      ISingleton Singleton { get; }
      IComposite Composite { get; }
    }

    internal class SuperComplex : ISuperComplex
    {
      public SuperComplex(int @int,
        int? nullableInt,
        string @string,
        ITransient transient,
        ISingleton singleton,
        IComposite composite)
      {
        Int = @int;
        NullableInt = nullableInt;
        String = @string;
        Transient = transient;
        Singleton = singleton;
        Composite = composite;
      }

      public int Int { get; private set; }
      public int? NullableInt { get; private set; }
      public string String { get; private set; }
      public ITransient Transient { get; private set; }
      public ISingleton Singleton { get; private set; }
      public IComposite Composite { get; private set; }
    }

    internal class ScopedSingletonTestClass : InstanceCountingTypeBase<ScopedSingletonTestClass>
    {

    }
  }
}

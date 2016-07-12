// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Rezolver
{
  /// <summary>
  /// </summary>
  public class Container : CachingContainerBase
  {
    /// <summary>
    /// Main IOC container class.  Requires an <see cref="ITargetContainer"/> to provide the <see cref="ITarget"/>s which will
    /// be compiled when resolving objects.
    /// </summary>
    /// <param name="targets">The targets which will be used to resolve objects.  If left null, then a new <see cref="Targets"/> will be created.</param>
    /// <param name="compiler">The compiler to be used to turn the <see cref="ITarget"/>s obtained from the <paramref name="targets"/> 
    /// into <see cref="ICompiledTarget"/></param>
    public Container(ITargetContainer targets = null, ITargetCompiler compiler = null)
      : base(targets, compiler)
    {
      
    }
  }
}
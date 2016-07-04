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
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="compiler"></param>
		/// <param name="registerToBuilder"></param>
		public Container(ITargetContainer builder = null, ITargetCompiler compiler = null, bool registerToBuilder = true)
		{
			_builder = builder ?? new Builder();
			_compiler = compiler;
			//auto-register this instance to the underlying builder.  This is so that framework-style components that
			//need to use dependency resolving instead of so-called 'pure' IOC can do so
			if(registerToBuilder)
			{
				_builder.Register(this.AsObjectTarget(), typeof(IContainer));
			}
		}

		private ITargetCompiler _compiler;
		public override ITargetCompiler Compiler
		{
			get { return _compiler ?? TargetCompiler.Default; }
		}

		private ITargetContainer _builder;
		public override ITargetContainer Builder
		{
			get { return _builder; }
		}

	}
}
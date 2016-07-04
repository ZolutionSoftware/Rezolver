// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class RezolveTargetMetadataList : RezolveTargetMetadataBase, IRezolveTargetMetadataList
	{
		private readonly List<IRezolveTargetMetadata> _targets;

		public override ITypeReference DeclaredType
		{
			get { throw new NotSupportedException(); }
		}

		public override IRezolveTargetMetadata Bind(ITypeReference[] targetTypes)
		{
			return new RezolveTargetMetadataList(Targets.Select(t => t.Bind(targetTypes)));
		}

		protected override IRezolveTargetMetadata BindBase(ITypeReference[] targetTypes)
		{
			throw new NotImplementedException();
		}

		public RezolveTargetMetadataList()
			: this(null)
		{

		}

		public RezolveTargetMetadataList(IEnumerable<IRezolveTargetMetadata> range)
			: base(RezolveTargetMetadataType.MetadataList)
		{
			_targets = new List<IRezolveTargetMetadata>(range ?? Enumerable.Empty<IRezolveTargetMetadata>());
		}

		public IList<IRezolveTargetMetadata> Targets
		{
			get { return _targets; }
		}

		protected override ITarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			throw new NotSupportedException("Cannot create a single target from a metadata list.  Please call CreateRezolveTargets");
		}

		public IEnumerable<ITarget> CreateRezolveTargets(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			if (context == null) throw new ArgumentNullException("context");

			//the problem with the targetTypes here might be that they're collection types, not types to be used 
			//for each individual target.  That said, we don't use target types very often in the base set of metadata
			//implementations anyway (relying instead on the logic already present in each of the target implementations
			//to determine whether 
			foreach(var item in Targets)
			{
				yield return item.CreateRezolveTarget(targetTypes, context, entry);
			}
		}
	}
}

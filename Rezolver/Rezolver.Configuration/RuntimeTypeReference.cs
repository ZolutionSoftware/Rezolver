using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// An ITypeReference that has been built directly from a runtime type.
	/// </summary>
	public sealed class RuntimeTypeReference : ITypeReference
	{

		public string TypeName
		{
			get { return RuntimeType.AssemblyQualifiedName; }
		}

		public ITypeReference[] GenericArguments
		{
			get
			{
				if (RuntimeType.IsGenericType)
				{
					return RuntimeType.GetGenericArguments().Select(t => new RuntimeTypeReference(t)).ToArray();
				}
				else
					return TypeReference.NoGenericArguments;
			}
		}

		public bool IsOpenGenericTypeArgument
		{
			//this isn't *quite* right, but I think it'll do for most cases.  Await bug report, fix if required.
			get { return RuntimeType.IsGenericParameter && RuntimeType.DeclaringType.IsGenericTypeDefinition; }
		}

		public bool IsUnbound
		{
			get { return false; }
		}

		public bool IsArray
		{
			get { return RuntimeType.IsArray; }
		}

		public int? StartLineNo
		{
			get { return null; }
		}

		public int? StartLinePos
		{
			get { return null; }
		}

		public int? EndLineNo
		{
			get { return null; }
		}

		public int? EndLinePos
		{
			get { return null; }
		}

		public Type RuntimeType
		{
			get;
			private set;
		}

		public RuntimeTypeReference(Type runtimeType)
		{
			if (runtimeType == null) throw new ArgumentNullException("runtimeType");
			RuntimeType = runtimeType;
		}
	}
}

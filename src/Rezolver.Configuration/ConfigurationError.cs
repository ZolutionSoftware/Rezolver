// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class ConfigurationError : IConfigurationError
	{
		public Exception Exception { get; private set; }
		public bool IsException
		{
			get
			{
				return Exception != null;
			}
		}
		public string ErrorMessage
		{
			get;
			private set;
		}

		public IConfigurationLineInfo LineInfo
		{
			get; private set;
		}
		public ConfigurationError(string errorMessage, IConfigurationLineInfo lineInfo)
		{
			ErrorMessage = errorMessage ?? "The configuration is invalid";
			LineInfo = lineInfo;
		}

		public ConfigurationError(Exception exception, IConfigurationLineInfo lineInfo)
		{
			Exception = exception;
			LineInfo = lineInfo;
			ErrorMessage = exception.Message;
		}
		
		private string _errorMessageWithLineInfo;
		public string ErrorMessageWithLineInfo
		{
			get
			{
				if(_errorMessageWithLineInfo == null)
				{
					string locationInfo = "(-1:-1)";
					if (LineInfo != null)
						locationInfo = string.Format("({0}:{1}-{2}:{3})", LineInfo.StartLineNo, LineInfo.StartLinePos, LineInfo.EndLineNo, LineInfo.EndLinePos);

					_errorMessageWithLineInfo = string.Format("{0} - {1}", locationInfo, ErrorMessage);
				}

				return _errorMessageWithLineInfo;
			}
		}

		public static ConfigurationError UnresolvedType(ITypeReference typeReference)
		{
			return new ConfigurationError(string.Format("Could not resolve type \"{0}\"", typeReference.TypeName), typeReference);
		}

		public static ConfigurationError UnresolvedType(string typeName, IConfigurationLineInfo lineInfo)
		{
			return new ConfigurationError(string.Format("Could not resolve type \"{0}\"", typeName), lineInfo);
		}

		public static ConfigurationError UnexpectedMetadataType(RezolveTargetMetadataType type, Type expected, Type actual, IConfigurationLineInfo lineInfo)
		{
			return new ConfigurationError(string.Format("An IRezolveTargetMetadata instance with Type equal to {0} should have a runtime type of {1} - the actual type {2} is not compatible.",
				type, expected, actual), lineInfo);
		}
	}
}

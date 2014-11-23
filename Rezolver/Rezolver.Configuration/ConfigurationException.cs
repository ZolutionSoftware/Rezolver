using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class ConfigurationException : Exception
	{
		private readonly IConfiguration _configuration;
		private readonly List<IConfigurationError> _errors;

		public ConfigurationException(IConfiguration configuration, List<IConfigurationError> errors)
		{
			this._configuration = configuration;
			this._errors = errors;
		}

		private string _message = null;
		public override string Message
		{
			get
			{
				if(_message == null)
				{
					string fileName = _configuration != null && !string.IsNullOrWhiteSpace(_configuration.FileName) ?
						_configuration.FileName : "[Unknown file]";

					if (_errors != null && _errors.Count > 0)
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendFormat("{0} error(s) found in {1}",  _errors.Count, fileName);
						sb.AppendLine();
						int counter = 1;
						foreach (var error in _errors)
						{
							sb.AppendLine(string.Format("{0} - {1}", counter++, error.ErrorMessageWithLineInfo));
						}
						_message = sb.ToString();
					}
					else
						_message = "The configuration is invalid";
				}

				return _message;
			}
		}
	}
}

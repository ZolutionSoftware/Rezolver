// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
  /// <summary>
  /// Used by this library's default implementation of IConfigurationAdapter, the ConfigurationAdapter class, 
  /// to create an instance of the ConfigurationAdapterContext class for a given adapter that's processing a
  /// given configuration.
  /// </summary>
  public interface IConfigurationAdapterContextFactory
  {
    ConfigurationAdapterContext CreateContext(ConfigurationAdapter adapter, IConfiguration configuration);
  }
}

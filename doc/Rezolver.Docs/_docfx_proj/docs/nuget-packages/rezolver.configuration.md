# Nuget Package: Rezolver.Configuration

This package provides the Rezolver configuration object model, whose purpose is to describe how a Target Container
should be constructed and configured based on a configuration file/script loaded at runtime.

It doesn't define any specific code for parsing configuration files - but it does provide a standard implementation of the @Rezolver.Configuration.IConfigurationAdapter,
whose job it is to create a @Rezolver.ITargetContainer from a @Rezolver.Configuration.IConfiguration instance.

You can implement your own configuration file formats with this simply by writing code to read your desired format and getting your parser
to create an instance of @Rezolver.Configuration.IConfiguration
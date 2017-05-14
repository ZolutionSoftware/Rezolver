# IOCScript

*This is a preliminary name for this project.  The idea is a universal script language for automating the 
configuration of containers.  Initially I'm only looking at building a Rezolver adapter for it.*

## Projects

### 1. Rezolver.IOCScript.Codegen

This is a 'dead' project whose purpose is purely to act as host to the
[ANTLR4 build target](https://github.com/tunnelvisionlabs/antlr4cs) in order to generate the parser/lexer
and listener from the iocscript grammar defined in iocscript.g4

*The issue being that custom tools are not supported in .Net Core/.Net Standard projects - so we need a project
which does support them.  The output files are dumped in `obj/$(Configuration)/` and then included in the
'proper' `Rezolver.IOCScript` via an extra glob in the project file*

### 2. Rezolver.IOCScript

.Net Standard project which hosts the parser and contains the code which can build Rezolver Containers from 
IOC script.

grammar IOCScript;

scriptfile: (assemblyRef
	|	using
	|	registration)+;

assemblyRef:     'reference' 'all'         # referenceAll
           |     'reference' assembly+     # references
    ;

assembly:   AssemblyName        # assemblyName
        |   AssemblyPath        # assemblyPath
        ;

using:              'using' Namespace+;
registration:       'register';

AssemblyPath:       '<' (~[>]*) '>';
AssemblyName:       '{' IDAny+ ('.' IDAny+)* (',' AssemblyPropertyValue)* '}';
AssemblyPropertyValue: ID '=' ~[,]*;
Namespace:          ID ('.' ID)*;

fragment IDAny:     [0-9a-zA-Z_];
fragment ID:        [a-zA-Z_] IDAny*;
COMMENT:            '#' ~[\r\n]* -> channel(HIDDEN);
WS:                 [ \r\n\t]+ -> channel(HIDDEN);
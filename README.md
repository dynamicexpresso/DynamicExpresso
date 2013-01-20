
Dynamic Expresso (v. 0.1)
----------------

Dynamic Expresso is a expression interpreter for simple C# statements. 
Dynamic Expresso embeds it's own parsing logic, and really interprets the statements by converting it to .NET delegates that can be invoked. 
It doesn't generate assembly but it creates dynamic expression/delegate on the fly. 

Using Dynamic Expresso developers can create scriptable applications and execute .NET code without compilation. 
Statements are written using a subset of C# language specifications.

TODO Some examples of supported expressions:


Quick start
===========

Dynamic Expresso is available on [NuGet]. You can install the package using:

	PM> Install-Package DynamicExpresso.Core

Source code and symbols (.pdb files) for debugging are available on [Symbol Source].

Usages and examples
===================

TODO Counter Catch (filter counters and transform values)
TODO xrc (page declaration)
TODO ZenoSetup (for setup commands)
TODO Application console (for diagnostic or advanced features) (desktop/web)
TODO Linq dynamic where / filter api

Configurations
==============


Features
========

- Supported operators and constructors
- Unit tests
- Performance
- Small footprint
- Easy to use
- 100 % managed code



History
=======

This project is based on two past works:
- "Converting String expressions to Funcs with FunctionFactory by Matthew Abbott" (http://www.fidelitydesign.net/?p=333) 
- DynamicQuery - Dynamic LINQ - Visual Studio 2008 sample:
	- http://msdn.microsoft.com/en-us/vstudio/bb894665.aspx 
	- http://weblogs.asp.net/scottgu/archive/2008/01/07/dynamic-linq-part-1-using-the-linq-dynamic-query-library.aspx


Other resources or similar projects
===================================

- Roslyn Project - compilar as a services - http://msdn.microsoft.com/en-us/vstudio/roslyn.aspx
	- If Roslyn will be available in the future this project can directly use the Roslyn compiler/interpreter.
- Jint - Javascript interpreter for .NET - http://jint.codeplex.com/
- Jurassic - Javascript compiler for .NET - http://jurassic.codeplex.com/
- Javascrpt.net - javascript V8 engine - http://javascriptdotnet.codeplex.com/
- CS-Script - http://www.csscript.net/
- IronJS, IronRuby, IronPython


License
=======

*[MIT License]* 

Copyright (c) 2013 Davide Icardi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
- The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



[MIT License]: http://opensource.org/licenses/mit-license.php


Dynamic Expresso (v. 0.3 Alpha)
----------------

Dynamic Expresso is a expression interpreter for simple C# statements. 
Dynamic Expresso embeds it's own parsing logic, and really interprets the statements by converting it to .NET delegates that can be invoked. 
It doesn't generate assembly but it creates dynamic expression/delegate on the fly. 

Using Dynamic Expresso developers can create scriptable applications and execute .NET code without compilation. 
Statements are written using a subset of C# language specifications.

Here an example of what you can do:

	var interpreter = new Interpreter();
	var result = interpreter.Eval("8 / 2 + 2");

or another more complex scenario:

	var service = new ServiceExample();
    var interpreter = new Interpreter()
                        .SetVariable("service", service);
	
	string expression = "x > 4 ? service.SomeMethod() : service.AnotherMethod()";
    var myExpr = interpreter.Parse(expression, 
                            new FunctionParam("x", typeof(int)));

    myExpr.Invoke(new FunctionParam("x", 5));

Features
========

TODO

- Supported operators and constructors
- Unit tests
- Performance
- Small footprint
- Easy to use
- 100 % managed code


Quick start
===========

Dynamic Expresso is available on [NuGet]. You can install the package using:

	PM> Install-Package DynamicExpresso.Core

Source code and symbols (.pdb files) for debugging are available on [Symbol Source].

Usages and examples
===================

TODO

- ContinuousPackager
- Counter Catch (filter counters and transform values)
- xrc (page declaration)
- ZenoSetup (for setup commands)
- Application console (for diagnostic or advanced features) (desktop/web)
- Linq dynamic where / filter api


Extensibility
=============

TODO (variables, keywords, types)

Performance consideration
=============

TODO Performance, multithreading

History
=======

This project is based on two past works:
- "Converting String expressions to Funcs with FunctionFactory by Matthew Abbott" (http://www.fidelitydesign.net/?p=333) 
- DynamicQuery - Dynamic LINQ - Visual Studio 2008 sample:
	- http://msdn.microsoft.com/en-us/vstudio/bb894665.aspx 
	- http://weblogs.asp.net/scottgu/archive/2008/01/07/dynamic-linq-part-1-using-the-linq-dynamic-query-library.aspx


Other resources or similar projects
===================================

Below you can find a list of some similar projects that I have evaluated or that can be interesting to study. 
For one reason or another none of these projects exactly fit my needs so I decided to write my own interpreter.

- Roslyn Project - Compiler as a service - http://msdn.microsoft.com/en-us/vstudio/roslyn.aspx
	- When Roslyn will be available this project can probably directly use the Roslyn compiler/interpreter.
- Mono.CSharp - C# Compiler Service and Runtime Evaulator - http://docs.go-mono.com/index.aspx?link=N%3AMono.CSharp
- C sharp Eval http://kamimucode.com/Home.aspx/C-sharp-Eval/1
	- Interesting but a little complex and no more updated
- CSharp Eval http://csharp-eval.com/
- C# Expression Evaluator http://csharpeval.codeplex.com/
- Jint - Javascript interpreter for .NET - http://jint.codeplex.com/
- Jurassic - Javascript compiler for .NET - http://jurassic.codeplex.com/
- Javascrpt.net - javascript V8 engine - http://javascriptdotnet.codeplex.com/
- CS-Script - http://www.csscript.net/
- IronJS, IronRuby, IronPython
- paxScript.NET http://eco148-88394.innterhost.net/paxscriptnet/


License
=======

*[MIT License]* 

Copyright (c) 2013 Davide Icardi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
- The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



[MIT License]: http://opensource.org/licenses/mit-license.php
[NuGet]: https://nuget.org/packages/DynamicExpresso.Core

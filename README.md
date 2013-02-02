
Dynamic Expresso (Beta version)
----------------

Dynamic Expresso is an expression interpreter for simple C# statements.
Dynamic Expresso embeds it's own parsing logic, and really interprets C# statements by converting it to .NET delegate that can be invoked as any standard delegate.
It doesn't generate assembly but it creates dynamic expression/delegate on the fly. 

Using Dynamic Expresso developers can create scriptable applications and execute .NET code without compilation. 
Statements are written using a subset of C# language specifications. Global variables or parameters can be injected and used inside expressions.

![dynamic expresso workflow](https://raw.github.com/davideicardi/DynamicExpresso/master/docs/workflow.png "dynamic expresso workflow")

Here an example of what you can do:

	var interpreter = new Interpreter();
	var result = interpreter.Eval("8 / 2 + 2");

or

    var interpreter = new Interpreter()
                    .SetVariable("service", new ServiceExample());
	
	string expression = "x > 4 ? service.SomeMethod() : service.AnotherMethod()";

    Lambda parsedExpression = interpreter.Parse(expression, 
                            new Parameter("x", typeof(int)));

    parsedExpression.Invoke(5);

Live demo
=========

Dynamic Expresso live demo: [http://dynamic-expresso.azurewebsites.net/](http://dynamic-expresso.azurewebsites.net/)

Quick start
===========

Dynamic Expresso is available on [NuGet]. You can install the package using:

	PM> Install-Package DynamicExpresso.Core

Source code and symbols (.pdb files) for debugging are available on [Symbol Source].


Features
=============

- Expressions can be written using a subset of C# syntax (see Syntax section for more information)
- Support for variables and parameters
- Full suite of unit tests
- Good performance compared to other similar projects
- Small footprint, generated expressions are managed classes, can be unloaded and can be executed in a single appdomain
- Easy to use and deploy, it is all contained in a single assembly without other external dependencies
- 100 % managed code written in C# 4.0
- Open source (MIT license)

### Return value

You can parse and execute void expression (without a return value) or you can return any valid .NET type. 
The built-in parser can understand the return type of any given expression so you can check if the expression returns what you expect.

### Variables

Variables can be used inside your expressions using the `Interpreter.SetVariable` method:

	var target = new Interpreter()
                    .SetVariable("myVar", 23);

    Assert.AreEqual(23, target.Eval("myVar"));

Variables can be primitive types or custom complex types (classes, structures, delegates, arrays, collections, ...).

Custom functions can be passed with delegate variables using `Interpreter.SetFunction` method:

	Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
	var target = new Interpreter()
				.SetFunction("pow", pow);

	Assert.AreEqual(9.0, target.Eval("pow(3, 2)"));

Custom [Expression](http://msdn.microsoft.com/en-us/library/system.linq.expressions.expression.aspx) can be passed by using `Interpreter.SetExpression` method.

### Parameters

Parsed expressions can accept a variable number of parameters:

	var interpreter = new Interpreter();

	var parameters = new[] {
					new Parameter("x", 23),
					new Parameter("y", 7)
					};

	Assert.AreEqual(30, interpreter.Eval("x + y", parameters));

Parameters can be primitive value types or custom complex types. You can parse an expression once and invoke it multiple times with different values:

    var target = new Interpreter();

    var parameters = new[] {
                    new Parameter("x", typeof(int)),
                    new Parameter("y", typeof(int))
                    };

    var myFunc = target.Parse("x + y", parameters);

    Assert.AreEqual(30, myFunc.Invoke(23, 7));
    Assert.AreEqual(30, myFunc.Invoke(32, -2));

### Built-in types and custom types

Currently the predefined types available in any expression are:

	Object object 
	Boolean bool 
	Char char
	String string
	SByte Byte byte
	Int16 UInt16 Int32 int UInt32 Int64 long UInt64 
	Single Double double Decimal decimal 
	DateTime TimeSpan
	Guid
	Math Convert

You can easily reference any custom .NET type by using `Interpreter.Reference` method:

	var target = new Interpreter()
					.Reference(typeof(Uri));

	Assert.AreEqual(typeof(Uri), target.Eval("typeof(Uri)"));
	Assert.AreEqual(Uri.UriSchemeHttp, target.Eval("Uri.UriSchemeHttp"));

Syntax and operators
====================

All statments can be written using a subset of the C# syntax. Here a list of the supported operators:

### Operators

<table>
	<thead>
		<tr>
			<th>Category</th><th>Operators</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Primary</td><td><code>x.y  f(x)  a[x]  new  typeof</code></td>
		</tr>
		<tr>
			<td>Unary</td><td><code>+  -  !  (T)x</code></td>
		</tr>
		<tr>
			<td>Multiplicative</td><td><code>*  /  %</code></td>
		</tr>
		<tr>
			<td>Additive</td><td><code>+  -</code></td>
		</tr>
		<tr>
			<td>Relational and type testing</td><td><code>&lt;  &gt;  &lt;=  &gt;=  is  as</code></td>
		</tr>
		<tr>
			<td>Equality</td><td><code>==  !=</code></td>
		</tr>
		<tr>
			<td>Conditional AND</td><td><code>&&</code></td>
		</tr>
		<tr>
			<td>Conditional OR</td><td><code>||</code></td>
		</tr>
		<tr>
			<td>Conditional</td><td><code>?:</code></td>
		</tr>
	</tbody>
</table>

### Literals

<table>
	<thead>
		<tr>
			<th>Category</th><th>Operators</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Constants</td><td><code>true  false  null</code></td>
		</tr>
		<tr>
			<td>Numeric</td><td><code>f  m</code></td>
		</tr>
		<tr>
			<td>String/char</td><td><code>""  ''</code></td>
		</tr>
	</tbody>
</table>

### Generate dynamic delegates

You can use the `Interpreter.Parse<TDelegate>` method to directly parse an expression into a .NET delegate type that can be normally invoked. 
In the example below I generate a `Func<Customer, bool>` delegate that can be used in a LINQ where expression.

	class Customer
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public char Gender { get; set; }
	}

	[TestMethod]
	public void Linq_Where()
	{
		var customers = new List<Customer> { 
								new Customer() { Name = "David", Age = 31, Gender = 'M' },
								new Customer() { Name = "Mary", Age = 29, Gender = 'F' },
								new Customer() { Name = "Jack", Age = 2, Gender = 'M' },
								new Customer() { Name = "Marta", Age = 1, Gender = 'F' },
								new Customer() { Name = "Moses", Age = 120, Gender = 'M' },
								};

		string whereExpression = "customer.Age > 18 && customer.Gender == 'F'";

		var interpreter = new Interpreter();
		Func<Customer, bool> dynamicWhere = interpreter.Parse<Func<Customer, bool>>(whereExpression, "customer");

		Assert.AreEqual(1, customers.Where(dynamicWhere).Count());
	}

This is the preferred way to parse an expression that you known at compile time what parameters can accept and what value must return.

Performance and multithreading
==============================

The `Interpreter` class can be used by multiple threads but without modify it.
In essence only `Parse` and `Eval` methods are thread safe. Other methods (`SetVariable`, `Reference`, ...) must be called in an initialization phase.
`Lambda` and `Parameter` classes are completely thread safe.

If you need to run the same expression multiple times with different parameters I suggest to parse it one time and then invoke the parsed expression multiple times.

Security
=============

If you allow an end user to write expression you must consider some security implications.

Parsed expressions can access only the .NET types that you have referenced using the `Interpreter.Reference` method or types that you pass as a variable or parameter. 
You must pay attention of what types you expose.
In any case generated delegates are executed as any other delegate and standard security .NET rules can be applied (for more info see [Security in the .NET Framework](http://msdn.microsoft.com/en-us/library/fkytk30f.aspx)). 

	
Usage scenarios
===================

Here are some possible usage scenarios of Dynamic Expresso:

- Programmable applications

	I have used Dynamic Expresso to allow an user to interact with a console like interface.
	See live demo: [http://dynamic-expresso.azurewebsites.net/](http://dynamic-expresso.azurewebsites.net/) or source code on [github](https://github.com/davideicardi/DynamicExpresso/tree/master/sample/DynamicExpressoWebShell)

- Allow the user to inject customizable rules and logic without recompiling

	In a tool used to collect Performance Counter data I have used Dynamic Expresso to filter and transform the output data. 
	In this way the user can insert custom filter or transform logic. For more information see [Counter Catch](https://github.com/davideicardi/CounterCatch).

- Evaluate dynamic functions or commands
- LINQ dynamic query

Future roadmap
===========

- Allow to reference types using full name (support namespace and Using method)
- Extend the Web Shell project to allow its use in an external application
- Support `throw` operator
- Support generic type declaration
- Best error messages

Help and support
================

If you need help you can try one of the following:

- [FAQ](https://github.com/davideicardi/DynamicExpresso/wiki/FAQ) wiki page
- Post your questions on [stackoverflow.com](http://stackoverflow.com/questions/tagged/dynamic-expresso) using `dynamic-expresso` tag
- github [official repository](https://github.com/davideicardi/DynamicExpresso)

Credits
=======

This project is based on two old works:
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
[Symbol Source]: http://www.symbolsource.org
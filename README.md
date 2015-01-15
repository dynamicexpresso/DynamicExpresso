
Dynamic Expresso
================

Dynamic Expresso is an expression interpreter for simple C# statements.
Dynamic Expresso embeds its own parsing logic, and really interprets C# statements by converting it to .NET delegates that can be invoked as any standard delegate.
It doesn't generate assembly but it creates dynamic expressions/delegates on the fly. 

Using Dynamic Expresso developers can create scriptable applications and execute .NET code without compilation. 
Statements are written using a subset of C# language specifications. Global variables or parameters can be injected and used inside expressions.

![dynamic expresso workflow](https://raw.github.com/davideicardi/DynamicExpresso/master/docs/workflow.png "dynamic expresso workflow")

Here an example of what you can do:

	var interpreter = new Interpreter();
	var result = interpreter.Eval("8 / 2 + 2");

or

    var interpreter = new Interpreter()
                    .SetVariable("service", new ServiceExample());
	
	string expression = "x > 4 ? service.OneMethod() : service.AnotherMethod()";

    Lambda parsedExpression = interpreter.Parse(expression, 
                            new Parameter("x", typeof(int)));

    parsedExpression.Invoke(5);

## Live demo

Dynamic Expresso live demo: [http://dynamic-expresso.azurewebsites.net/](http://dynamic-expresso.azurewebsites.net/)

## Quick start

Dynamic Expresso is available on [NuGet]. You can install the package using:

	PM> Install-Package DynamicExpresso.Core

Source code and symbols (.pdb files) for debugging are available on [Symbol Source].


## Features

- Expressions can be written using a subset of C# syntax (see Syntax section for more information)
- Support for variables and parameters
- Can generate delegates or lambda expression
- Full suite of unit tests
- Good performance compared to other similar projects
- Partial support for generic, params array and extension methods
- Case insensitive expressions (default is case sensitive)
- Ability to discover identifiers (variables, types, parameters) of a given expression
- Small footprint, generated expressions are managed classes, can be unloaded and can be executed in a single appdomain
- Easy to use and deploy, it is all contained in a single assembly without other external dependencies
- 100 % managed code written in C# 4.0
- Open source (MIT license)

### Return value

You can parse and execute void expression (without a return value) or you can return any valid .NET type. 
When parsing an expression you can specify the expected expression return type. For example you can write:

	var target = new Interpreter();

	double result = target.Eval<double>("Math.Pow(x, y) + 5",
											new Parameter("x", typeof(double), 10),
											new Parameter("y", typeof(double), 2));


The built-in parser can also understand the return type of any given expression so you can check if the expression returns what you expect.

### Variables

Variables can be used inside expressions with `Interpreter.SetVariable` method:

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

Parsed expressions can accept one or more parameters:

	var interpreter = new Interpreter();

	var parameters = new[] {
					new Parameter("x", 23),
					new Parameter("y", 7)
					};

	Assert.AreEqual(30, interpreter.Eval("x + y", parameters));

Parameters can be primitive types or custom types. You can parse an expression once and invoke it multiple times with different parameter values:

    var target = new Interpreter();

    var parameters = new[] {
                    new Parameter("x", typeof(int)),
                    new Parameter("y", typeof(int))
                    };

    var myFunc = target.Parse("x + y", parameters);

    Assert.AreEqual(30, myFunc.Invoke(23, 7));
    Assert.AreEqual(30, myFunc.Invoke(32, -2));

### Built-in types and custom types

Currently predefined types available are:

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

You can reference any other custom .NET type by using `Interpreter.Reference` method:

	var target = new Interpreter()
					.Reference(typeof(Uri));

	Assert.AreEqual(typeof(Uri), target.Eval("typeof(Uri)"));
	Assert.AreEqual(Uri.UriSchemeHttp, target.Eval("Uri.UriSchemeHttp"));


### Generate dynamic delegates

You can use the `Interpreter.ParseAsDelegate<TDelegate>` method to directly parse an expression into a .NET delegate type that can be normally invoked. 
In the example below I generate a `Func<Customer, bool>` delegate that can be used in a LINQ where expression.

	class Customer
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public char Gender { get; set; }
	}

	[Test]
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

### Generate lambda expressions

You can use the `Interpreter.ParseAsExpression<TDelegate>` 
	method to directly parse an expression into a .NET lambda expression (`Expression<TDelegate>`). 

In the example below I generate a `Expression<Func<Customer, bool>>` expression that can be used in a Queryable LINQ where expression 
	or in any other place where an expression is required. Like Entity Framework or other similar libraries.

	class Customer
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public char Gender { get; set; }
	}

	[Test]
	public void Linq_Queryable_Expression_Where()
	{
		IQueryable<Customer> customers = (new List<Customer> { 
			new Customer() { Name = "David", Age = 31, Gender = 'M' },
			new Customer() { Name = "Mary", Age = 29, Gender = 'F' },
			new Customer() { Name = "Jack", Age = 2, Gender = 'M' },
			new Customer() { Name = "Marta", Age = 1, Gender = 'F' },
			new Customer() { Name = "Moses", Age = 120, Gender = 'M' },
		}).AsQueryable();

		string whereExpression = "customer.Age > 18 && customer.Gender == 'F'";

		var interpreter = new Interpreter();
		Expression<Func<Customer, bool>> expression = interpreter.ParseAsExpression<Func<Customer, bool>>(whereExpression, "customer");

		Assert.AreEqual(1, customers.Where(expression).Count());
	}


## Syntax and operators

Statements can be written using a subset of the C# syntax. Here you can find a list of the supported expressions: 

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
		<tr>
			<td>Assignment</td><td><code>=</code></td>
		</tr>
	</tbody>
</table>

Operators precedence is respected following [C# rules (Operator precedence and associativity)](http://msdn.microsoft.com/en-us/library/aa691323(v=vs.71).aspx).

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

The following character escape sequences are supported inside string or char literals:

- `\'` - single quote, needed for character literals
- `\"` - double quote, needed for string literals
- `\\` - backslash
- `\0` - Unicode character 0
- `\a` - Alert (character 7)
- `\b` - Backspace (character 8)
- `\f` - Form feed (character 12)
- `\n` - New line (character 10)
- `\r` - Carriage return (character 13)
- `\t` - Horizontal tab (character 9)
- `\v` - Vertical quote (character 11)

### Type's members invocation

Any standard .NET method, field, property or constructor can be invoked.

	var x = new MyTestService();
	var target = new Interpreter().SetVariable("x", x);

	Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()"));
	Assert.AreEqual(x.AProperty, target.Eval("x.AProperty"));
	Assert.AreEqual(x.AField, target.Eval("x.AField"));

	var target = new Interpreter();
	Assert.AreEqual(new DateTime(2015, 1, 24), target.Eval("new DateTime(2015, 1, 24)"));

Dynamic Expresso also supports:

- Extension methods

	var x = new int[] { 10, 30, 4 };
	var target = new Interpreter()
													.Reference(typeof(System.Linq.Enumerable))
													.SetVariable("x", x);
	Assert.AreEqual(x.Count(), target.Eval("x.Count()"));

- Indexer methods (like array[0])
- Generics, only partially supported (only implicit, you cannot invoke an explicit generic method)
- Params array (see C# `params` keyword)

### Case sensitive/insensitive

By default all expressions are considered case sensitive (`VARX` is different than `varx`, as in C#).
There is an option to use a case insensitive parser. For example:

	var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

	double x = 2;
	var parameters = new[] {
											new Parameter("x", x.GetType(), x)
											};

	Assert.AreEqual(x, target.Eval("x", parameters));
	Assert.AreEqual(x, target.Eval("X", parameters));

## Identifiers detection

Sometimes you need to check which identifiers (variables, types, parameters) are used in expression before parsing it.
Maybe because you want to validate it or you want to ask the user to enter parameters value of a given expression.
Because if you parse an expression without the right parameter an exception is throwed.

In these cases you can use `Interpreter.DetectIdentifiers` method to obtain a list of used identifiers, both known and unknown.

	var target = new Interpreter();

	var detectedIdentifiers = target.DetectIdentifiers("x + y");

	CollectionAssert.AreEqual(
		new []{ "x", "y"}, 
		detectedIdentifiers.UnknownIdentifiers.ToArray());

## Limitations

Not every C# syntaxes are supported. Here some examples of NOT supported features:

- Multiline expressions
- for/foreach/while/do operators
- Array/list/dictionary initialization
- Explicit generic invocation (like `method<type>(arg)`) 
- Lambda/delegate declaration (delegate and lamda are only supported as variables or parameters or as a return type of the expression)

## Exceptions

If there is an error during the parsing always an exception of type `ParseException` is throwed. 
`ParseException` has several specialization classes based on the type of error (UnknownIdentifierException, NoApplicableMethodException. ...).

## Performance and multithreading

The `Interpreter` class can be used by multiple threads but without modify it.
In essence only get properties, `Parse` and `Eval` methods are thread safe. Other methods (`SetVariable`, `Reference`, ...) must be called in an initialization phase.
`Lambda` and `Parameter` classes are completely thread safe.

If you need to run the same expression multiple times with different parameters I suggest to parse it one time and then invoke the parsed expression multiple times.

## Security

If you allow an end user to write expression you must consider some security implications.

Parsed expressions can access only the .NET types that you have referenced using the `Interpreter.Reference` method or types that you pass as a variable or parameter. 
You must pay attention of what types you expose.
In any case generated delegates are executed as any other delegate and standard security .NET rules can be applied (for more info see [Security in the .NET Framework](http://msdn.microsoft.com/en-us/library/fkytk30f.aspx)). 

	
## Usage scenarios

Here are some possible usage scenarios of Dynamic Expresso:

- Programmable applications
- Allow the user to inject customizable rules and logic without recompiling
- Evaluate dynamic functions or commands
- LINQ dynamic query

## Future roadmap

See [github open issues and milestones](https://github.com/davideicardi/DynamicExpresso/issues).

## Help and support

If you need help you can try one of the following:

- [FAQ](https://github.com/davideicardi/DynamicExpresso/wiki/FAQ) wiki page
- Post your questions on [stackoverflow.com](http://stackoverflow.com/questions/tagged/dynamic-expresso) using `dynamic-expresso` tag
- github [official repository](https://github.com/davideicardi/DynamicExpresso)

## Credits

This project is based on two old works:
- "Converting String expressions to Funcs with FunctionFactory by Matthew Abbott" (http://www.fidelitydesign.net/?p=333) 
- DynamicQuery - Dynamic LINQ - Visual Studio 2008 sample:
	- http://msdn.microsoft.com/en-us/vstudio/bb894665.aspx 
	- http://weblogs.asp.net/scottgu/archive/2008/01/07/dynamic-linq-part-1-using-the-linq-dynamic-query-library.aspx


## Other resources or similar projects

Below you can find a list of some similar projects that I have evaluated or that can be interesting to study. 
For one reason or another none of these projects exactly fit my needs so I decided to write my own interpreter.

- Roslyn Project - Compiler as a service - http://msdn.microsoft.com/en-us/vstudio/roslyn.aspx
	- When Roslyn will be available this project can probably directly use the Roslyn compiler/interpreter.
- Mono.CSharp - C# Compiler Service and Runtime Evaulator - http://docs.go-mono.com/index.aspx?link=N%3AMono.CSharp
- NCalc - Mathematical Expressions Evaluator for .NET - http://ncalc.codeplex.com/
- David Wynne CSharpEval https://github.com/DavidWynne/CSharpEval
- CSharp Eval http://csharp-eval.com/
- C# Expression Evaluator http://csharpeval.codeplex.com/
- Jint - Javascript interpreter for .NET - http://jint.codeplex.com/
- Jurassic - Javascript compiler for .NET - http://jurassic.codeplex.com/
- Javascrpt.net - javascript V8 engine - http://javascriptdotnet.codeplex.com/
- CS-Script - http://www.csscript.net/
- IronJS, IronRuby, IronPython
- paxScript.NET http://eco148-88394.innterhost.net/paxscriptnet/


## Release notes

- 1.1.0

	- Added support for equal assignement operator (<code>=</code>). [#24](https://github.com/davideicardi/DynamicExpresso/issues/24)

- 1.0.0
	
	- Added Interpreter.DetectIdentifiers method to discovery identifiers (variables, parameters, types) used in expression before parsing it. ([#23](https://github.com/davideicardi/DynamicExpresso/issues/23))
	- Added `CaseSensitive`, `ReferencedTypes`, `Identifiers` properties to understand how the `Interpreter` object was constructed.
	- Added new methods for registering variables and types (see SetIdentifier and Reference).
	- Added `LanguageConstants` class containing the most common types and identifiers used by default.
	- Removed `Interpreter.Analyze` method because not very useful. To reproduce this feature just catch the exception.
	- Extended `Lambda` class with used Types, Identifiers, and Parameters of the parsed expression. Basically you can understand what types, variables and parameters a specified expression has used.
	- Added ability to Compile to a typed delegate directly from the Lambda.
	- Internal code refactoring
    - Now parsed lambda only contains the actually used parameters, not all the parameters provided.

- 0.11.4

	- FIX: Custom binary operators not supported, again! ([#21](https://github.com/davideicardi/DynamicExpresso/issues/21))

- 0.11.3

	- FIX: Custom binary operators not supported ([#21](https://github.com/davideicardi/DynamicExpresso/issues/21))

- 0.11.2
	
	- FIX: Calling method on integer literals (like: `5.ToString()` ) ([#20](https://github.com/davideicardi/DynamicExpresso/issues/20))

- 0.11.1

	- FIX: Parsing of expression with cast fails ([#19](https://github.com/davideicardi/DynamicExpresso/issues/19))

- 0.11.0
	
	- Improved exception when there is an unknown identifier. 
	- Added Analyze method to check if an expression is valid. See `Interpreter.Analyze`.

- 0.10.0

	- Added support for case insensitive expressions. See `InterpreterOptions` enum.

- 0.9.2

	FIX: Support for empty string literals ([#17](https://github.com/davideicardi/DynamicExpresso/issues/17))

- 0.9.1

	FIX: Invoke object methods on interface types ([#13](https://github.com/davideicardi/DynamicExpresso/issues/13))

- 0.9.0
	
	- Expression return type is automatically converted if needed ([#9](https://github.com/davideicardi/DynamicExpresso/issues/9))
	- Eval typed expression ([#8](https://github.com/davideicardi/DynamicExpresso/issues/8))
	- Implicit conversion support ([#7](https://github.com/davideicardi/DynamicExpresso/issues/7))
	- Nullable types support ([#5](https://github.com/davideicardi/DynamicExpresso/issues/5))
	- Extension methods support ([#2](https://github.com/davideicardi/DynamicExpresso/issues/2))
	- Allow to specify a default interpreter's configuration ([#12](https://github.com/davideicardi/DynamicExpresso/issues/12))
	- Params array support ([#6](https://github.com/davideicardi/DynamicExpresso/issues/6))
	- Enumerable extensions support ([#3](https://github.com/davideicardi/DynamicExpresso/issues/3))

- 0.8.1
	
	FIX: API hangs on bad formula ([#1](https://github.com/davideicardi/DynamicExpresso/issues/1))

- 0.8.0

	Small api improvements (Invoke() without parameters)

- 0.7.0

	Support for escape sequences inside string or character literals (es. `\t`)

- 0.6.0

	First official beta release

## License

*[MIT License]* 

Copyright (c) 2014 Davide Icardi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
- The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



[MIT License]: http://opensource.org/licenses/mit-license.php
[NuGet]: https://nuget.org/packages/DynamicExpresso.Core
[Symbol Source]: http://www.symbolsource.org

# Dynamic Expresso

[![NuGet version](https://badge.fury.io/nu/DynamicExpresso.Core.svg)](http://badge.fury.io/nu/DynamicExpresso.Core)
[![.NET CI](https://github.com/davideicardi/DynamicExpresso/actions/workflows/ci.yml/badge.svg)](https://github.com/davideicardi/DynamicExpresso/actions/workflows/ci.yml)

Available platforms: .NET Core 2.0, .NET 4.5, .NET 4.6.1

Dynamic Expresso is an interpreter for simple C# statements written in .NET Standard 2.0.
Dynamic Expresso embeds its own parsing logic, really interprets C# statements by converting it to .NET lambda expressions or delegates.

Using Dynamic Expresso developers can create scriptable applications, execute .NET code without compilation or create dynamic linq statements. 

Statements are written using a subset of C# language specifications. Global variables or parameters can be injected and used inside expressions. It doesn't generate assembly but it creates an expression tree on the fly. 

![dynamic expresso workflow](https://raw.github.com/davideicardi/DynamicExpresso/master/docs/workflow.png "dynamic expresso workflow")

For example you can evaluate math expressions:
```csharp
var interpreter = new Interpreter();
var result = interpreter.Eval("8 / 2 + 2");
```
or parse an expression with variables or parameters and invoke it multiple times:
```csharp
var interpreter = new Interpreter().SetVariable("service", new ServiceExample());
string expression = "x > 4 ? service.OneMethod() : service.AnotherMethod()";
Lambda parsedExpression = interpreter.Parse(expression, new Parameter("x", typeof(int)));
var result = parsedExpression.Invoke(5);
```
or generate delegates and lambda expressions for LINQ queries:
```csharp
var prices = new [] { 5, 8, 6, 2 };
var whereFunction = new Interpreter().ParseAsDelegate<Func<int, bool>>("arg > 5");
var count = prices.Where(whereFunction).Count();
```
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
- Partial support of generic, params array and extension methods (only with implicit generic arguments detection)
- Partial support of `dynamic` (`ExpandoObject` for get properties, method invocation and indexes(#142), see #72. `DynamicObject` for get properties and indexes, see #142)
- Case insensitive expressions (default is case sensitive)
- Ability to discover identifiers (variables, types, parameters) of a given expression
- Small footprint, generated expressions are managed classes, can be unloaded and can be executed in a single appdomain
- Easy to use and deploy, it is all contained in a single assembly without other external dependencies
- Written in .NET Standard 2.0
	- Build available for .NET 4.6.1 and .NET Core 2.0
- Open source (MIT license)

### Return value
You can parse and execute void expression (without a return value) or you can return any valid .NET type. 
When parsing an expression you can specify the expected expression return type. For example you can write:
```csharp
var target = new Interpreter();
double result = target.Eval<double>("Math.Pow(x, y) + 5",
				    new Parameter("x", typeof(double), 10),
				    new Parameter("y", typeof(double), 2));
```
The built-in parser can also understand the return type of any given expression so you can check if the expression returns what you expect.

### Variables
Variables can be used inside expressions with `Interpreter.SetVariable` method:
```csharp
var target = new Interpreter().SetVariable("myVar", 23);

Assert.AreEqual(23, target.Eval("myVar"));
```
Variables can be primitive types or custom complex types (classes, structures, delegates, arrays, collections, ...).

Custom functions can be passed with delegate variables using `Interpreter.SetFunction` method:
```csharp
Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
var target = new Interpreter().SetFunction("pow", pow);

Assert.AreEqual(9.0, target.Eval("pow(3, 2)"));
```
Custom [Expression](http://msdn.microsoft.com/en-us/library/system.linq.expressions.expression.aspx) can be passed by using `Interpreter.SetExpression` method.


### Parameters
Parsed expressions can accept one or more parameters:
```csharp
var interpreter = new Interpreter();

var parameters = new[] {
	new Parameter("x", 23),
	new Parameter("y", 7)
};

Assert.AreEqual(30, interpreter.Eval("x + y", parameters));
```
Parameters can be primitive types or custom types. You can parse an expression once and invoke it multiple times with different parameter values:
```csharp
var target = new Interpreter();

var parameters = new[] {
	new Parameter("x", typeof(int)),
	new Parameter("y", typeof(int))
};

var myFunc = target.Parse("x + y", parameters);

Assert.AreEqual(30, myFunc.Invoke(23, 7));
Assert.AreEqual(30, myFunc.Invoke(32, -2));
```

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
```csharp
var target = new Interpreter().Reference(typeof(Uri));

Assert.AreEqual(typeof(Uri), target.Eval("typeof(Uri)"));
Assert.AreEqual(Uri.UriSchemeHttp, target.Eval("Uri.UriSchemeHttp"));
```

### Generate dynamic delegates
You can use the `Interpreter.ParseAsDelegate<TDelegate>` method to directly parse an expression into a .NET delegate type that can be normally invoked. 
In the example below I generate a `Func<Customer, bool>` delegate that can be used in a LINQ where expression.
```csharp
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
	Func<Customer, bool> dynamicWhere = interpreter.ParseAsDelegate<Func<Customer, bool>>(whereExpression, "customer");

	Assert.AreEqual(1, customers.Where(dynamicWhere).Count());
}
```
This is the preferred way to parse an expression that you known at compile time what parameters can accept and what value must return.

### Generate lambda expressions
You can use the `Interpreter.ParseAsExpression<TDelegate>` method to directly parse an expression into a .NET lambda expression (`Expression<TDelegate>`). 

In the example below I generate a `Expression<Func<Customer, bool>>` expression that can be used in a Queryable LINQ where expression or in any other place where an expression is required. Like Entity Framework or other similar libraries.
```csharp
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
```

## Syntax and operators
Statements can be written using a subset of the C# syntax. Here you can find a list of the supported expressions: 

### Operators

Supported operators:

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
			<td>Logical AND</td><td><code>&</code></td>
		</tr>
		<tr>
			<td>Logical OR</td><td><code>|</code></td>
		</tr>
		<tr>
			<td>Logical XOR</td><td><code>^</code></td>
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
		<tr>
			<td>Null coalescing</td><td><code>??</code></td>
		</tr>
	</tbody>
</table>

Operators precedence is respected following [C# rules (Operator precedence and associativity)](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/).

Some operators, like the assignment operator, can be disabled for security reason.

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
			<td>Real literal suffixes</td><td><code>d  f  m</code></td>
		</tr>
		<tr>
			<td>Integer literal suffixes</td><td><code>u l ul lu</code></td>
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
```csharp
var x = new MyTestService();
var target = new Interpreter().SetVariable("x", x);

Assert.AreEqual(x.HelloWorld(), target.Eval("x.HelloWorld()"));
Assert.AreEqual(x.AProperty, target.Eval("x.AProperty"));
Assert.AreEqual(x.AField, target.Eval("x.AField"));
```
```csharp
var target = new Interpreter();
Assert.AreEqual(new DateTime(2015, 1, 24), target.Eval("new DateTime(2015, 1, 24)"));
```
Dynamic Expresso also supports:

- Extension methods
```csharp
var x = new int[] { 10, 30, 4 };
var target = new Interpreter()
	.Reference(typeof(System.Linq.Enumerable))
	.SetVariable("x", x);
Assert.AreEqual(x.Count(), target.Eval("x.Count()"));
```
- Indexer methods (like `array[0]`)
- Generics, only partially supported (only implicit, you cannot invoke an explicit generic method)
- Params array (see C# `params` keyword)

### Case sensitive/insensitive
By default all expressions are considered case sensitive (`VARX` is different than `varx`, as in C#).
There is an option to use a case insensitive parser. For example:
```csharp
var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);

double x = 2;
var parameters = new[] {
	new Parameter("x", x.GetType(), x)
};

Assert.AreEqual(x, target.Eval("x", parameters));
Assert.AreEqual(x, target.Eval("X", parameters));
```

## Identifiers detection
Sometimes you need to check which identifiers (variables, types, parameters) are used in expression before parsing it.
Maybe because you want to validate it or you want to ask the user to enter parameters value of a given expression.
Because if you parse an expression without the right parameter an exception is throwed.

In these cases you can use `Interpreter.DetectIdentifiers` method to obtain a list of used identifiers, both known and unknown.
```csharp
var target = new Interpreter();

var detectedIdentifiers = target.DetectIdentifiers("x + y");

CollectionAssert.AreEqual(new[] { "x", "y" }, 
			  detectedIdentifiers.UnknownIdentifiers.ToArray());
```

## Default number type
In C #, numbers are usually interpreted as integers or doubles if they have decimal places.

In some cases it may be useful to be able to configure the default type of numbers if no particular suffix is ​​specified: for example in financial calculations, where usually numbers are interpreted as decimal type.

In these cases you can set the default number type using `Interpreter.SetDefaultNumberType`  method.

```csharp
var target = new Interpreter();

target.SetDefaultNumberType(DefaultNumberType.Decimal);

Assert.IsInstanceOf(typeof(System.Decimal), target.Eval("45"));
Assert.AreEqual(10M/3M, target.Eval("10/3")); // 3.33333333333 instead of 3
```

## Limitations
Not every C# syntaxes are supported. Here some examples of NOT supported features:

- Multiline expressions
- for/foreach/while/do operators
- Array/list/dictionary initialization
- Explicit generic invocation (like `method<type>(arg)`) 
- Lambda/delegate declaration (delegate and lamda are only supported as variables or parameters or as a return type of the expression)
- Array/list/dictionary element assignment (set indexer operator)
- Other operations on `dynamic` objects (only property, method invocation and index now are supported)

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

If expressions test can be written directly by users you must ensure that only certain features are available. Here some guidelines:

For example you can disable assignment operators, to ensure that the user cannot change some values that you don't expect. 
By default assignment operators are enables, by you can disable it using:
```csharp
var target = new Interpreter().EnableAssignment(AssignmentOperators.None);
```
From version 1.3 to prevent malicious users to call unexpected types or assemblies within an expression, 
some reflection methods are blocked. For example you cannot write:
```csharp
var target = new Interpreter();
target.Eval("typeof(double).GetMethods()");
// or
target.Eval("typeof(double).Assembly");
```
The only exception to this rule is the `Type.Name` property that is permitted for debugging reasons.
 To enable standard reflection features you can use `Interpreter.EnableReflection` method, like:
```csharp
var target = new Interpreter().EnableReflection();
```

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
- github [official repository](https://github.com/davideicardi/DynamicExpresso)


## Credits
This project is based on two old works:
- "Converting String expressions to Funcs with FunctionFactory by Matthew Abbott" (http://www.fidelitydesign.net/?p=333) 
- DynamicQuery - Dynamic LINQ - Visual Studio 2008 sample:
	- http://msdn.microsoft.com/en-us/vstudio/bb894665.aspx 


Thanks to JetBrain for helping me with a license of Resharper.
[![JetBrain Resharper](https://github.com//davideicardi/DynamicExpresso/blob/master/docs/jetbrains.png?raw=true)](https://www.jetbrains.com/)

## Other resources or similar projects
Below you can find a list of some similar projects that I have evaluated or that can be interesting to study. 
For one reason or another none of these projects exactly fit my needs so I decided to write my own interpreter.

- Roslyn Project - Scripting API - https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples
	- This is the new Microsoft Official Compiler as a service library. I suggest to consider using Roslyin instead of DynamicExpresso whenever possible.
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

## Continuous Integration

A continuous integration pipeline is configured using Github Actions, see `.github/workflows` folder.

Whenever a new [Release](https://github.com/davideicardi/DynamicExpresso/releases) is created, Nuget packages are published. For snapshot releases packages are published only to Github.
For official releases packages are published to both GitHub and Nuget.

## Compiling and run tests

To compile the solution you can run:

	dotnet build DynamicExpresso.sln -c Release

To create nuget packages:

	dotnet pack DynamicExpresso.sln -c Release

To run unit tests:

	dotnet test DynamicExpresso.sln -c Release

or run unit tests for a specific project with a specific framework:

	dotnet test DynamicExpresso.sln --no-restore -c Release --verbosity normal -f netcoreapp3.1

Add `--logger:trx` to generate test results for VSTS.

## Release notes

See [releases page](https://github.com/davideicardi/DynamicExpresso/releases).

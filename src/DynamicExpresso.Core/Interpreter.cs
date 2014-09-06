using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso
{
	/// <summary>
	/// Class used to parse and compile a text expression into an Expression or a Delegate that can be invoked. Expression are written using a subset of C# syntax.
	/// Only Parse and Eval methods are thread safe.
	/// </summary>
	public class Interpreter
	{
		readonly ParserSettings _settings;

		/// <summary>
		/// Create a new Interpreter using InterpreterOptions.Default.
		/// </summary>
		public Interpreter()
			: this(InterpreterOptions.Default)
		{
		}

		/// <summary>
		/// Create a new Interpreter using the specified options.
		/// </summary>
		/// <param name="options"></param>
		public Interpreter(InterpreterOptions options)
		{
			var caseInsensitive = options.HasFlag(InterpreterOptions.CaseInsensitive);

			_settings = new ParserSettings(caseInsensitive);

			if ((options & InterpreterOptions.SystemKeywords) == InterpreterOptions.SystemKeywords)
			{
				FillSystemKeywords();
			}

			if ((options & InterpreterOptions.PrimitiveTypes) == InterpreterOptions.PrimitiveTypes)
			{
				FillPrimitiveTypes();
			}

			if ((options & InterpreterOptions.CommonTypes) == InterpreterOptions.CommonTypes)
			{
				FillCommonTypes();
			}
		}

		public bool CaseInsensitive
		{
			get
			{
				return _settings.CaseInsensitive;
			}
		}

		/// <summary>
		/// Get a list of registeres types. Add types by using the Reference method.
		/// </summary>
		public IEnumerable<KnownType> KnownTypes
		{
			get
			{
				return _settings.KnownTypes
					.Select(p => new KnownType(p.Key, p.Value))
					.ToList();
			}
		}

		/// <summary>
		/// Get a list of known identifiers. Add identifiers using SetVariable, SetFunction or SetExpression methods.
		/// </summary>
		public IEnumerable<Identifier> Identifiers
		{
			get
			{
				return _settings.Identifiers
					.Select(p => new Identifier(p.Key, p.Value))
					.ToList();
			}
		}

		/// <summary>
		/// Allow the specified function delegate to be called from a parsed expression.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public Interpreter SetFunction(string name, Delegate value)
		{
			return SetVariable(name, value);
		}

		/// <summary>
		/// Allow the specified variable to be used in a parsed expression.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public Interpreter SetVariable(string name, object value)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			return SetExpression(name, Expression.Constant(value));
		}

		/// <summary>
		/// Allow the specified variable to be used in a parsed expression.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public Interpreter SetVariable(string name, object value, Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			return SetExpression(name, Expression.Constant(value, type));
		}

		/// <summary>
		/// Allow the specified Expression to be used in a parsed expression.
		/// Basically add the specified expression as a known identifier.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="expression"></param>
		/// <returns></returns>
		public Interpreter SetExpression(string name, Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			_settings.Identifiers[name] = expression;

			return this;
		}

		/// <summary>
		/// Allow the specified type to be used inside an expression. The type will be available using its name.
		/// If the type contains method extensions methods they will be available inside expressions.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Interpreter Reference(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return Reference(type, type.Name);
		}

		/// <summary>
		/// Allow the specified type to be used inside an expression by using a custom alias.
		/// If the type contains extensions methods they will be available inside expressions.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="typeName">Public name that can be used in the expression.</param>
		/// <returns></returns>
		public Interpreter Reference(Type type, string typeName)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (string.IsNullOrWhiteSpace(typeName))
				throw new ArgumentNullException("typeName");

			_settings.KnownTypes[typeName] = type;

			var extensions = GetExtensionMethods(type);
			foreach (var extensionMethod in extensions)
			{
				if (!_settings.ExtensionMethods.Contains(extensionMethod))
				{
					_settings.ExtensionMethods.Add(extensionMethod);
				}
			}

			return this;
		}

		/// <summary>
		/// Parse a text expression and returns a Lambda class that can be used to invoke it.
		/// </summary>
		/// <param name="expressionText">Expression statement</param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public Lambda Parse(string expressionText, params Parameter[] parameters)
		{
			return Parse(expressionText, typeof(void), parameters);
		}

		/// <summary>
		/// Parse a text expression and returns a Lambda class that can be used to invoke it.
		/// If the expression cannot be converted to the type specified in the expressionType parameter
		/// an exception is throw.
		/// </summary>
		/// <param name="expressionText">Expression statement</param>
		/// <param name="expressionType">The expected return type. Use void or object type if there isn't an expected return type.</param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public Lambda Parse(string expressionText, Type expressionType, params Parameter[] parameters)
		{
			var arguments = CreateExpressionParameters(parameters);

			var expression = ParseExpression(expressionText, expressionType, arguments);

			var lambdaExp = Expression.Lambda(expression, arguments);

			return new Lambda(lambdaExp);
		}

		/// <summary>
		/// Parse a text expression and convert it into a delegate.
		/// </summary>
		/// <typeparam name="TDelegate">Delegate to use</typeparam>
		/// <param name="expressionText">Expression statement</param>
		/// <param name="parametersNames">Names of the parameters. If not specified the parameters names defined inside the delegate are used.</param>
		/// <returns></returns>
		public TDelegate Parse<TDelegate>(string expressionText, params string[] parametersNames)
		{
			var delegateInfo = GetDelegateInfo(typeof(TDelegate), parametersNames);

			var expression = ParseExpression(expressionText, delegateInfo.ReturnType, delegateInfo.Parameters);

			var lambdaExp = Expression.Lambda<TDelegate>(expression, delegateInfo.Parameters);

			return lambdaExp.Compile();
		}

		/// <summary>
		/// Parse and invoke the specified expression.
		/// </summary>
		/// <param name="expressionText"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public object Eval(string expressionText, params Parameter[] parameters)
		{
			return Eval(expressionText, typeof(void), parameters);
		}

		/// <summary>
		/// Parse and invoke the specified expression.
		/// </summary>
		/// <param name="expressionText"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public T Eval<T>(string expressionText, params Parameter[] parameters)
		{
			return (T)Eval(expressionText, typeof(T), parameters);
		}

		/// <summary>
		/// Parse and invoke the specified expression.
		/// </summary>
		/// <param name="expressionText"></param>
		/// <param name="expressionType">The return type of the expression. Use void or object if you don't know the expected return type.</param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public object Eval(string expressionText, Type expressionType, params Parameter[] parameters)
		{
			return Parse(expressionText, expressionType, parameters).Invoke(parameters);
		}

		/// <summary>
		/// Provide a way to run the parser without throwing an exception in case of error. 
		/// A ParserResult class is always returned with Lambda object in case of success or the Exception object in case of errors.
		/// </summary>
		/// <param name="expressionText"></param>
		/// <param name="expressionType"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public ParserResult TryParse(string expressionText, Type expressionType, params Parameter[] parameters)
		{
			try
			{
				var lambda = Parse(expressionText, expressionType, parameters);

				return ParserResult.Valid(lambda);
			}
			catch (ParseException ex)
			{
				return ParserResult.Invalid(ex);
			}
		}

		Expression ParseExpression(string expressionText, Type expressionType, params ParameterExpression[] parameters)
		{
			var parser = new ExpressionParser(expressionText, expressionType, parameters, _settings);

			return parser.Parse();
		}

		static ParameterExpression[] CreateExpressionParameters(Parameter[] parameters)
		{
			var arguments = (from p in parameters
											 select ParameterExpression.Parameter(p.Type, p.Name)).ToArray();
			return arguments;
		}

		static DelegateInfo GetDelegateInfo(Type delegateType, string[] parametersNames)
		{
			MethodInfo method = delegateType.GetMethod("Invoke");
			if (method == null)
				throw new ArgumentException("The specified type is not a delegate");

			var delegateParameters = method.GetParameters();
			var parameters = new ParameterExpression[delegateParameters.Length];

			bool useCustomNames = parametersNames != null && parametersNames.Length > 0;

			if (useCustomNames && parametersNames.Length != parameters.Length)
				throw new ArgumentException(string.Format("Provided parameters names doesn't match delegate parameters, {0} parameters expected.", parameters.Length));

			for (int i = 0; i < parameters.Length; i++)
			{
				var paramName = useCustomNames ? parametersNames[i] : delegateParameters[i].Name;
				var paramType = delegateParameters[i].ParameterType;

				parameters[i] = Expression.Parameter(paramType, paramName);
			}

			return new DelegateInfo()
			{
				Parameters = parameters,
				ReturnType = method.ReturnType
			};
		}

		IEnumerable<MethodInfo> GetExtensionMethods(Type type)
		{
			if (type.IsSealed && !type.IsGenericType && !type.IsNested)
			{
				var query = from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
										where method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
										select method;

				return query;
			}

			return new MethodInfo[0];
		}

		class DelegateInfo
		{
			public Type ReturnType { get; set; }
			public ParameterExpression[] Parameters { get; set; }
		}

		void FillSystemKeywords()
		{
			SetExpression("true", Expression.Constant(true));
			SetExpression("false", Expression.Constant(false));
			SetExpression("null", ParserConstants.nullLiteral);
		}

		static Type[] PrimitiveTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
        };

		void FillPrimitiveTypes()
		{
			foreach (Type type in PrimitiveTypes)
			{
				Reference(type);
			}

			Reference(typeof(object), "object");
			Reference(typeof(string), "string");
			Reference(typeof(char), "char");
			Reference(typeof(bool), "bool");
			Reference(typeof(byte), "byte");
			Reference(typeof(int), "int");
			Reference(typeof(long), "long");
			Reference(typeof(double), "double");
			Reference(typeof(decimal), "decimal");
		}

		static Type[] CommonTypes = {
            typeof(Math),
            typeof(Convert),
            typeof(System.Linq.Enumerable),
        };

		void FillCommonTypes()
		{
			foreach (Type type in CommonTypes)
			{
				Reference(type);
			}
		}
	}
}

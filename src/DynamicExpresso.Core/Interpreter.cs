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
		ParserSettings _settings = new ParserSettings();

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

			_settings.Keywords[name] = expression;

			return this;
		}

		/// <summary>
		/// Allow the specified type to be used inside an expression. The type will be available using its name.
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

			_settings.KnownTypes.Add(typeName, type);

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
			var arguments = GetParameters(parameters);

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

		Expression ParseExpression(string expressionText, Type expressionType, params ParameterExpression[] parameters)
		{
			var parser = new ExpressionParser(expressionText, expressionType, parameters, _settings);

			return parser.Parse();
		}

		static ParameterExpression[] GetParameters(Parameter[] parameters)
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

		class DelegateInfo
		{
			public Type ReturnType { get; set; }
			public ParameterExpression[] Parameters { get; set; }
		}
	}
}

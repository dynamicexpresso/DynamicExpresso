using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicExpresso
{
	/// <summary>
	/// Represents an expression parse result.
	/// </summary>
	public class ParseResult
	{
		private readonly Expression _expression;
		private readonly ParserArguments _parserArguments;

		internal ParseResult(Expression expression, ParserArguments parserArguments)
		{
			_expression = expression ?? throw new ArgumentNullException(nameof(expression));
			_parserArguments = parserArguments ?? throw new ArgumentNullException(nameof(parserArguments));
		}

		internal ParseResult(ParseResult other) : this(other._expression, other._parserArguments)
		{
		}

		public string ExpressionText => _parserArguments.ExpressionText;
		public virtual Type ReturnType => _expression.Type;

		/// <summary>
		/// Gets the parsed expression.
		/// </summary>
		/// <value>The expression.</value>
		public Expression Expression => _expression;

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		[Obsolete("Use UsedParameters or DeclaredParameters")]
		public IEnumerable<Parameter> Parameters => UsedParameters;

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		public IEnumerable<Parameter> UsedParameters => _parserArguments.UsedParameters;

		/// <summary>
		/// Gets the parameters declared when parsing the expression.
		/// </summary>
		/// <value>The declared parameters.</value>
		public IEnumerable<Parameter> DeclaredParameters => _parserArguments.DeclaredParameters;

		/// <summary>
		/// Gets the references types in parsed expression.
		/// </summary>
		/// <value>The references types.</value>
		public IEnumerable<ReferenceType> Types => _parserArguments.UsedTypes;

		/// <summary>
		/// Gets the identifiers in parsed expression.
		/// </summary>
		/// <value>The identifiers.</value>
		public IEnumerable<Identifier> Identifiers => _parserArguments.UsedIdentifiers;

		internal Lambda ToLambda()
		{
			return new Lambda(_expression, _parserArguments);
		}

		public override string ToString()
		{
			return ExpressionText;
		}
	}

	public class ParseResult<TDelegate> : ParseResult
	{
		internal ParseResult(ParseResult parseResult) : base(parseResult)
		{
		}
	}
}

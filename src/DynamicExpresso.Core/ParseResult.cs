using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicExpresso
{
	/// <summary>
	/// Represents an expression parse result.
	/// </summary>
	public class ParseResult
	{
		public ParseResult(
			Expression expression,
			IEnumerable<ParameterExpression> usedParameters,
			IEnumerable<ParameterExpression> declaredParameters,
			IEnumerable<ReferenceType> types,
			IEnumerable<Identifier> identifiers)
		{
			Expression = expression;
			UsedParameters = usedParameters;
			DeclaredParameters = declaredParameters;
			Types = types;
			Identifiers = identifiers;
		}

		/// <summary>
		/// Gets the parsed expression.
		/// </summary>
		/// <value>The expression.</value>
		public Expression Expression { get; }

		/// <summary>
		/// Gets the parameters actually used in the expression parsed.
		/// </summary>
		/// <value>The used parameters.</value>
		public IEnumerable<ParameterExpression> UsedParameters { get; }

		/// <summary>
		/// Gets the parameters declared when parsing the expression.
		/// </summary>
		/// <value>The declared parameters.</value>
		public IEnumerable<ParameterExpression> DeclaredParameters { get; }

		/// <summary>
		/// Gets the references types in parsed expression.
		/// </summary>
		/// <value>The references types.</value>
		public IEnumerable<ReferenceType> Types { get; }

		/// <summary>
		/// Gets the identifiers in parsed expression.
		/// </summary>
		/// <value>The identifiers.</value>
		public IEnumerable<Identifier> Identifiers { get; }
	}
}

using System.Linq.Expressions;

namespace DynamicExpresso
{
	public class Identifier
	{
		public Expression Expression { get; private set; }
		public string Name { get; private set; }

		public Identifier(string name, Expression expression)
		{
			Expression = expression;
			Name = name;
		}
	}
}

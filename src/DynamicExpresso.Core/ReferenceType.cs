using System;

namespace DynamicExpresso
{
	public class ReferenceType
	{
		public Type Type { get; private set; }

		/// <summary>
		/// Public name that must be used in the expression.
		/// </summary>
		public string Name { get; private set; }

		public ReferenceType(string name, Type type)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if (type == null)
				throw new ArgumentNullException("type");

			Type = type;
			Name = name;
		}

		public ReferenceType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			Type = type;
			Name = type.Name;
		}
	}
}

using System;

namespace DynamicExpresso
{
	public class KnownType
	{
		public Type Type { get; private set; }
		public string Name { get; private set; }

		public KnownType(string name, Type type)
		{
			Type = type;
			Name = name;
		}
	}
}

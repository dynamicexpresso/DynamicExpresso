using System.Collections.Generic;
using System.Linq;

namespace DynamicExpresso
{
	public class IdentifiersInfo
	{
		public IdentifiersInfo(
			IEnumerable<string> unknownIdentifiers,
			IEnumerable<Identifier> identifiers,
			IEnumerable<ReferenceType> types)
		{
			UnknownIdentifiers = unknownIdentifiers.ToList();
			Identifiers = identifiers.ToList();
			Types = types.ToList();
		}

		public IEnumerable<string> UnknownIdentifiers { get; private set; }
		public IEnumerable<Identifier> Identifiers { get; private set; }
		public IEnumerable<ReferenceType> Types { get; private set; }
	}
}

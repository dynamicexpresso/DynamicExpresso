using System.Collections.Generic;
using System.Linq;

namespace DynamicExpresso
{
	public class IdentifiersInfo
	{
		public IdentifiersInfo(IEnumerable<string> unknownIdentifiers, IEnumerable<string> knownIdentifiers)
		{
			UnknownIdentifiers = unknownIdentifiers.ToList();
			KnownIdentifiers = knownIdentifiers.ToList();
		}

		public IEnumerable<string> UnknownIdentifiers { get; private set; }
		public IEnumerable<string> KnownIdentifiers { get; private set; }
	}
}

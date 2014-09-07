using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso.Parser
{
	internal struct Token
	{
		public TokenId id;
		public string text;
		public int pos;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso
{
	[Flags]
	public enum InterpreterOptions
	{
		None = 0,
		/// <summary>
		/// Load primitive types like 'string', 'double', 'int', 'DateTime', 'Guid', ...
		/// </summary>
		PrimitiveTypes = 1,
		/// <summary>
		/// Load system keywords like 'true', 'false', 'null'
		/// </summary>
		SystemKeywords = 2,
		/// <summary>
		/// Load common types like 'System.Math', 'System.Convert', 'System.Linq.Enumerable'
		/// </summary>
		CommonTypes = 4,

		/// <summary>
		/// Load all default configurations: PrimitiveTypes + SystemKeywords + CommonTypes
		/// </summary>
		Default = PrimitiveTypes | SystemKeywords | CommonTypes
	}
}

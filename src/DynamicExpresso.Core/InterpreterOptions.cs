using System;

namespace DynamicExpresso
{
	[Flags]
	public enum InterpreterOptions
	{
		None = 0,
		/// <summary>
		/// Load primitive types like 'string', 'double', 'int', 'DateTime', 'Guid', ... See also LanguageConstants.CSharpPrimitiveTypes and LanguageConstants.PrimitiveTypes
		/// </summary>
		PrimitiveTypes = 1,
		/// <summary>
		/// Load system keywords like 'true', 'false', 'null'. See also LanguageConstants.Literals.
		/// </summary>
		SystemKeywords = 2,
		/// <summary>
		/// Load common types like 'System.Math', 'System.Convert', 'System.Linq.Enumerable'. See also LanguageConstants.CommonTypes.
		/// </summary>
		CommonTypes = 4,
		/// <summary>
		/// Variables and parameters names are case insensitive.
		/// </summary>
		CaseInsensitive = 8,
		/// <summary>
		/// Load all default configurations: PrimitiveTypes + SystemKeywords + CommonTypes
		/// </summary>
		Default = PrimitiveTypes | SystemKeywords | CommonTypes,
		/// <summary>
		/// Load all default configurations: PrimitiveTypes + SystemKeywords + CommonTypes + CaseInsensitive
		/// </summary>
		DefaultCaseInsensitive = PrimitiveTypes | SystemKeywords | CommonTypes | CaseInsensitive,
	}
}

namespace DynamicExpresso
{
	/// <summary>
	/// Setting the default number types when no suffix is specified
	/// </summary>
	public enum DefaultNumberType
	{
		Default = 0, //(Int by default or Double if real number)
		Int = 1,
		Long = 2,
		Single = 3,
		Double = 4,
		Decimal = 5
	}
}

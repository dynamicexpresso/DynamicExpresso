using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso
{
	[Flags]
	public enum AssignmentOperators
	{
		/// <summary>
		/// Disable all the assignment operators
		/// </summary>
		None = 0,
		/// <summary>
		/// Enable the assignment equal operator
		/// </summary>
		AssignmentEqual = 1,
		/// <summary>
		/// Enable all assignment operators
		/// </summary>
		All = AssignmentEqual
	}
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DynamicExpresso
{
    public interface IMemberAccessProvider
    {
		bool TryGetMemberAccess(Expression leftHand, string identifier, out Expression result);
	}
}

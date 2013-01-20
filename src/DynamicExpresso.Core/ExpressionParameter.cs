using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicExpresso
{
    public class ExpressionParameter
    {
        public ExpressionParameter(string name, Type type, object value = null)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }
        public object Value { get; private set; }
    }
}

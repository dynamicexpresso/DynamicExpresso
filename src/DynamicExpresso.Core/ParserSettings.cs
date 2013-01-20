using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DynamicExpresso
{
    public class ParserSettings
    {
        static Type[] predefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        Dictionary<string, object> keywords;

        Dictionary<string, object> externals;

        Dictionary<string, Type> knownTypes;

        public ParserSettings()
        {
            keywords = CreateKeywords();
            externals = new Dictionary<string, object>();
            knownTypes = new Dictionary<string, Type>();

            foreach (Type type in predefinedTypes)
                knownTypes.Add(type.Name, type);
            knownTypes.Add("string", typeof(string));
            knownTypes.Add("char", typeof(char));
            knownTypes.Add("bool", typeof(bool));
            knownTypes.Add("byte", typeof(byte));
            knownTypes.Add("int", typeof(int));
            knownTypes.Add("long", typeof(long));
            knownTypes.Add("double", typeof(double));
            knownTypes.Add("decimal", typeof(decimal));
        }

        Dictionary<string, object> CreateKeywords()
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("true", ParserConstants.trueLiteral);
            d.Add("false", ParserConstants.falseLiteral);
            d.Add("null", ParserConstants.nullLiteral);
            d.Add(ParserConstants.keywordIt, ParserConstants.keywordIt);
            d.Add(ParserConstants.keywordIif, ParserConstants.keywordIif);
            d.Add(ParserConstants.keywordNew, ParserConstants.keywordNew);

            return d;
        }

        public IDictionary<string, Type> KnownTypes
        {
            get { return knownTypes; }
        }
        public IDictionary<string, object> Externals
        {
            get { return externals; }
        }
        public IDictionary<string, object> Keywords
        {
            get { return keywords; }
        }
    }
}

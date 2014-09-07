using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso.Parser
{
	internal class ParserInputs
	{
		ParserSettings _settings;
		Dictionary<string, Expression> _parameters;

		public ParserInputs(ParserSettings settings, ParameterExpression[] parameters)
		{
			_settings = settings;
			_parameters = new Dictionary<string, Expression>(settings.SettingsKeyComparer);

			AddParameters(parameters);
		}

		public bool TryGetKnownType(string name, out Type type)
		{
			return _settings.KnownTypes.TryGetValue(name, out type);
		}

		public bool TryGetIdentifier(string name, out Expression expression)
		{
			return _settings.Identifiers.TryGetValue(name, out expression);
		}

		public bool TryGetParameters(string name, out Expression expression)
		{
			return _parameters.TryGetValue(name, out expression);
		}

		public IEnumerable<MethodInfo> GetExtensionMethods(string methodName)
		{
			return _settings.ExtensionMethods.Where(p => p.Name == methodName);
		}

		void AddParameter(string name, Expression value)
		{
			if (_parameters.ContainsKey(name))
				throw new DuplicateParameterException(name);

			_parameters.Add(name, value);
		}

		void AddParameters(IEnumerable<ParameterExpression> parameters)
		{
			foreach (ParameterExpression pe in parameters)
			{
				AddParameter(pe.Name, pe);
			}
		}
	}
}

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
		Dictionary<string, ParameterExpression> _parameters;

		IList<ParameterExpression> _usedParameters = new List<ParameterExpression>();
		IList<ReferenceType> _usedTypes = new List<ReferenceType>();
		IList<Identifier> _usedIdentifiers = new List<Identifier>();

		public ParserInputs(ParserSettings settings, ParameterExpression[] parameters)
		{
			_settings = settings;
			_parameters = new Dictionary<string, ParameterExpression>(settings.SettingsKeyComparer);

			AddParameters(parameters);
		}

		public IList<ParameterExpression> UsedParameters
		{
			get { return _usedParameters; }
		}
	
		public IList<ReferenceType> UsedTypes
		{
			get { return _usedTypes; }
		}

		public IList<Identifier> UsedIdentifiers
		{
			get { return _usedIdentifiers; }
		}

		public bool TryGetKnownType(string name, out Type type)
		{
			ReferenceType reference;
			var found = _settings.KnownTypes.TryGetValue(name, out reference);

			if (found)
				type = reference.Type;
			else
				type = null;

			return found;
		}

		public bool TryGetIdentifier(string name, out Expression expression)
		{
			Identifier identifier;
			var found = _settings.Identifiers.TryGetValue(name, out identifier);
			if (found)
				expression = identifier.Expression;
			else
				expression = null;

			return found;
		}

		public bool TryGetParameters(string name, out ParameterExpression expression)
		{
			var found = _parameters.TryGetValue(name, out expression);

			if (found)
				_usedParameters.Add(expression);

			return found;
		}

		public IEnumerable<MethodInfo> GetExtensionMethods(string methodName)
		{
			return _settings.ExtensionMethods.Where(p => p.Name == methodName);
		}

		void AddParameter(string name, ParameterExpression value)
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

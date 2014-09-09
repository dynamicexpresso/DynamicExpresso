using DynamicExpresso.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DynamicExpresso
{
	internal class ParserArguments
	{
		ParserSettings _settings;
		Dictionary<string, Parameter> _parameters;

		HashSet<Parameter> _usedParameters = new HashSet<Parameter>();
		HashSet<ReferenceType> _usedTypes = new HashSet<ReferenceType>();
		HashSet<Identifier> _usedIdentifiers = new HashSet<Identifier>();

		public ParserArguments(string expressionText, ParserSettings settings)
		{
			ExpressionText = expressionText;
			ExpressionReturnType = typeof(void);

			_settings = settings;
			_parameters = new Dictionary<string, Parameter>(settings.SettingsKeyComparer);
		}

		public bool CaseInsensitive { get { return _settings.CaseInsensitive; } }
		public string ExpressionText { get; set; }
		public Type ExpressionReturnType { get; set; }

		public IEnumerable<Parameter> UsedParameters
		{
			get { return _usedParameters; }
		}

		public IEnumerable<ReferenceType> UsedTypes
		{
			get { return _usedTypes; }
		}

		public IEnumerable<Identifier> UsedIdentifiers
		{
			get { return _usedIdentifiers; }
		}

		public bool TryGetKnownType(string name, out Type type)
		{
			ReferenceType reference;
			if (_settings.KnownTypes.TryGetValue(name, out reference))
			{
				_usedTypes.Add(reference);
				type = reference.Type;
				return true;
			}

			type = null;
			return false;
		}

		public bool TryGetIdentifier(string name, out Expression expression)
		{
			Identifier identifier;
			if (_settings.Identifiers.TryGetValue(name, out identifier))
			{
				_usedIdentifiers.Add(identifier);
				expression = identifier.Expression;
				return true;
			}

			expression = null;
			return false;
		}

		public bool TryGetParameters(string name, out ParameterExpression expression)
		{
			Parameter parameter;
			if (_parameters.TryGetValue(name, out parameter))
			{
				_usedParameters.Add(parameter);
				expression = parameter.Expression;
				return true;
			}

			expression = null;
			return false;
		}

		public IEnumerable<MethodInfo> GetExtensionMethods(string methodName)
		{
			return _settings.ExtensionMethods.Where(p => p.Name == methodName);
		}

		public void AddParameter(Parameter parameter)
		{
			try
			{
				_parameters.Add(parameter.Name, parameter);
			}
			catch (ArgumentException)
			{
				throw new DuplicateParameterException(parameter.Name);
			}
		}

		public void AddParameters(IEnumerable<Parameter> parameters)
		{
			foreach (Parameter pe in parameters)
			{
				AddParameter(pe);
			}
		}
	}
}

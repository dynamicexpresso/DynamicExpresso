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
		Dictionary<string, Parameter> _declaredParameters;

		List<Parameter> _usedParameters = new List<Parameter>();
		HashSet<ReferenceType> _usedTypes = new HashSet<ReferenceType>();
		HashSet<Identifier> _usedIdentifiers = new HashSet<Identifier>();

		public ParserArguments(
			string expressionText, 
			ParserSettings settings, 
			Type expressionReturnType,
			IEnumerable<Parameter> declaredParameters
		)
		{
			ExpressionText = expressionText;
			ExpressionReturnType = expressionReturnType;

			Settings = settings;
			_declaredParameters = new Dictionary<string, Parameter>(settings.KeyComparer);
			foreach (var pe in declaredParameters)
			{
				try
				{
					_declaredParameters.Add(pe.Name, pe);
				}
				catch (ArgumentException)
				{
					throw new DuplicateParameterException(pe.Name);
				}
			}
		}

		public ParserSettings Settings { get; private set;}
		public string ExpressionText { get; private set; }
		public Type ExpressionReturnType { get; private set; }
		public IEnumerable<Parameter> DeclaredParameters { get { return _declaredParameters.Values; } }

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
			if (Settings.KnownTypes.TryGetValue(name, out reference))
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
			if (Settings.Identifiers.TryGetValue(name, out identifier))
			{
				_usedIdentifiers.Add(identifier);
				expression = identifier.Expression;
				return true;
			}

			expression = null;
			return false;
		}

		/// <summary>
		/// Get the parameter and mark is as used.
		/// </summary>
		public bool TryGetParameters(string name, out ParameterExpression expression)
		{
			Parameter parameter;
			if (_declaredParameters.TryGetValue(name, out parameter))
			{
                AddToUsedParameters(parameter);
                expression = parameter.Expression;
				return true;
			}

			expression = null;
			return false;
		}

        private void AddToUsedParameters(Parameter parameter)
        {
            if (!_usedParameters.Contains(parameter))
            {
                var parametersNamesList = _parameters.Keys.ToList();
                _usedParameters.Add(parameter);
                _usedParameters.Sort((p1, p2) =>
                    (parametersNamesList.IndexOf(p1.Name) > parametersNamesList.IndexOf(p2.Name)) ? 1 : -1
                );
            }
        }

		public IEnumerable<MethodInfo> GetExtensionMethods(string methodName)
		{
			return Settings.ExtensionMethods.Where(p => p.Name == methodName);
		}
	}
}

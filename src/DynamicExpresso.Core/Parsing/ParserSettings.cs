using System;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicExpresso.Parsing
{
	internal class ParserSettings
	{
		private readonly Dictionary<string, Identifier> _identifiers;
		private readonly Dictionary<string, ReferenceType> _knownTypes;
		private readonly HashSet<MethodInfo> _extensionMethods;

		public ParserSettings(bool caseInsensitive, bool lateBindObject)
		{
			CaseInsensitive = caseInsensitive;

			LateBindObject = lateBindObject;

			KeyComparer = CaseInsensitive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;

			KeyComparison = CaseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

			_identifiers = new Dictionary<string, Identifier>(KeyComparer);

			_knownTypes = new Dictionary<string, ReferenceType>(KeyComparer);

			_extensionMethods = new HashSet<MethodInfo>();

			AssignmentOperators = AssignmentOperators.All;

			DefaultNumberType = DefaultNumberType.Default;

			LambdaExpressions = false;
		}

		private ParserSettings(ParserSettings other) : this(other.CaseInsensitive, other.LateBindObject)
		{
			_knownTypes = new Dictionary<string, ReferenceType>(other._knownTypes);
			_identifiers = new Dictionary<string, Identifier>(other._identifiers);
			_extensionMethods = new HashSet<MethodInfo>(other._extensionMethods);

			AssignmentOperators = other.AssignmentOperators;
			DefaultNumberType = other.DefaultNumberType;
			LambdaExpressions = other.LambdaExpressions;
		}

		/// <summary>
		/// Creates a deep copy of the current settings, so that the identifiers/types/methods can be changed
		/// without impacting the existing settings.
		/// </summary>
		public ParserSettings Clone()
		{
			return new ParserSettings(this);
		}

		public IDictionary<string, ReferenceType> KnownTypes
		{
			get { return _knownTypes; }
		}

		public IDictionary<string, Identifier> Identifiers
		{
			get { return _identifiers; }
		}

		public HashSet<MethodInfo> ExtensionMethods
		{
			get { return _extensionMethods; }
		}

		public bool CaseInsensitive
		{
			get;
		}

		public bool LateBindObject
		{
			get;
		}

		public StringComparison KeyComparison
		{
			get;
		}

		public IEqualityComparer<string> KeyComparer
		{
			get;
		}

		public DefaultNumberType DefaultNumberType
		{
			get;
			set;
		}

		public AssignmentOperators AssignmentOperators
		{
			get;
			set;
		}

		public bool LambdaExpressions
		{
			get;
			set;
		}
	}
}

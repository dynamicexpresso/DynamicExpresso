using System;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicExpresso.Parsing
{
	internal class ParserSettings
	{
		private Dictionary<string, Identifier> _identifiers;
		private Dictionary<string, ReferenceType> _knownTypes;
		private HashSet<MethodInfo> _extensionMethods;

		public ParserSettings(bool caseInsensitive,bool lateBindObject)
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

		/// <summary>
		/// Creates a deep copy of the current settings, so that the identifiers/types/methods can be changed
		/// without impacting the existing settings.
		/// </summary>
		internal ParserSettings Clone()
		{
			var clone = (ParserSettings)MemberwiseClone();
			clone._identifiers = new Dictionary<string, Identifier>(_identifiers);
			clone._knownTypes = new Dictionary<string, ReferenceType>(_knownTypes);
			clone._extensionMethods = new HashSet<MethodInfo>(_extensionMethods);
			return clone;
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
			private set;
		}

		public bool LateBindObject
		{
			get;
			private set;
		}

		public DefaultNumberType DefaultNumberType
		{
			get;
			set;
		}

		public StringComparison KeyComparison
		{
			get;
			private set;
		}

		public IEqualityComparer<string> KeyComparer
		{
			get;
			private set;
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

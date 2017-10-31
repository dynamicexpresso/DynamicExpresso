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

		public ParserSettings(bool caseInsensitive)
		{
			CaseInsensitive = caseInsensitive;

			KeyComparer = CaseInsensitive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;

			KeyComparison = CaseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

			_identifiers = new Dictionary<string, Identifier>(KeyComparer);

			_knownTypes = new Dictionary<string, ReferenceType>(KeyComparer);

			_extensionMethods = new HashSet<MethodInfo>();

			AssignmentOperators = AssignmentOperators.All;
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
	}
}

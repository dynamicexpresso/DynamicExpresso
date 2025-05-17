using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicExpresso.Resolution;

namespace DynamicExpresso.Reflection
{
	internal class MemberFinder
	{
		private struct MemberTypeKey
		{
			public Type _type;
			public string _memberName;
			public BindingFlags _flags;

			public MemberTypeKey(Type type, string memberName, BindingFlags flags)
			{
				_type = type;
				_memberName = memberName;
				_flags = flags;
			}
		}

		private class MemberTypeKeyEqualityComparer : IEqualityComparer<MemberTypeKey>
		{
			public static readonly MemberTypeKeyEqualityComparer Instance = new MemberTypeKeyEqualityComparer();

			public bool Equals(MemberTypeKey x, MemberTypeKey y)
			{
				return x._type.Equals(y._type) && x._flags == y._flags && x._memberName.Equals(y._memberName, StringComparison.Ordinal);
			}


			public int GetHashCode(MemberTypeKey obj)
			{
				return obj._type.GetHashCode() ^ obj._flags.GetHashCode() ^ obj._memberName.GetHashCode();
			}
		}

		private class MemberTypeKeyCaseInsensitiveEqualityComparer : IEqualityComparer<MemberTypeKey>
		{
			public static readonly MemberTypeKeyCaseInsensitiveEqualityComparer Instance = new MemberTypeKeyCaseInsensitiveEqualityComparer();
			public bool Equals(MemberTypeKey x, MemberTypeKey y)
			{
				return x._type.Equals(y._type) && x._flags == y._flags && x._memberName.Equals(y._memberName, StringComparison.OrdinalIgnoreCase);
			}

			public int GetHashCode(MemberTypeKey obj)
			{
				return obj._type.GetHashCode() ^ obj._flags.GetHashCode() ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj._memberName);
			}
		}

		private readonly ParserArguments _arguments;
		private readonly BindingFlags _bindingCase;
		private readonly MemberFilter _memberFilterCase;
		private readonly Dictionary<MemberTypeKey, MemberInfo[]> _members;

		public MemberFinder(ParserArguments arguments)
		{
			_arguments = arguments;
			_bindingCase = arguments.Settings.CaseInsensitive ? BindingFlags.IgnoreCase : BindingFlags.Default;
			_memberFilterCase = arguments.Settings.CaseInsensitive ? Type.FilterNameIgnoreCase : Type.FilterName;
			_members = new Dictionary<MemberTypeKey, MemberInfo[]>(arguments.Settings.CaseInsensitive ? (IEqualityComparer<MemberTypeKey>)MemberTypeKeyCaseInsensitiveEqualityComparer.Instance : MemberTypeKeyEqualityComparer.Instance);
		}

		public MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
						(staticAccess ? BindingFlags.Static : BindingFlags.Instance) | _bindingCase;

			//Not looping here because FindMembers loops internally and returns us the highest level member
			var members = FindMembers(type, MemberTypes.Property | MemberTypes.Field, flags, memberName);
			if (members.Length != 0)
			{
				return members[0];
			}
			return null;
		}

		private MemberInfo[] FindMembers(Type type, MemberTypes memberTypes, BindingFlags flags, string memberName)
		{
			var key = new MemberTypeKey(type, memberName, flags);
			if (_members.TryGetValue(key, out var members))
			{
				if (members != null)
				{
					return members;
				}
			}
			members = type.FindMembers(memberTypes, flags, _memberFilterCase, memberName);
			if (members.Length == 0)
			{
				foreach (var v in SelfAndBaseTypes(type))
				{
					if (v == type)
					{
						continue;
					}
					var subMembers = FindMembers(v, memberTypes, flags, memberName);
					if (members.Length == 0 && subMembers.Length > 0)
					{
						//We don't break here because there is a possibility that somebody outside here is also doing an additional tree prioritization (See FindMethods)
						members = subMembers;
					}
				}
			}
			_members[key] = members;
			return members;
		}

		public IList<MethodData> FindMethods(Type type, string methodName, bool staticAccess, Expression[] args)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
						(staticAccess ? BindingFlags.Static : BindingFlags.Instance) | _bindingCase;
			//Yes, FindMembers loops internally, but there are certain circumstances where we end up also needing this
			//An example is Type.GetMethods(), where there are multiple overloads on the base Type class
			//and one override on a derived class.  The DeclaredOnly flag will return the single method on the derived class unless we go lower.
			//The existing unit tests do pass if we get rid of DeclaredOnly and remove the loop, but I'm not sure of the actual impact that may have.
			foreach (var t in SelfAndBaseTypes(type))
			{
				var members = FindMembers(t, MemberTypes.Method, flags, methodName);
				var applicableMethods = MethodResolution.FindBestMethod(members.Cast<MethodBase>(), args);

				if (applicableMethods.Count > 0)
					return applicableMethods;
			}

			return Array.Empty<MethodData>();
		}

		// this method is static, because we know that the Invoke method of a delegate always has this exact name
		// and therefore we never need to search for it in case-insensitive mode
		public static MethodData FindInvokeMethod(Type type)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
						BindingFlags.Instance;
			foreach (var t in SelfAndBaseTypes(type))
			{
				var method = t.FindMembers(MemberTypes.Method, flags, Type.FilterName, "Invoke")
					.Cast<MethodBase>()
					.SingleOrDefault();

				if (method != null)
					return MethodData.Gen(method);
			}

			return null;
		}

		public IList<MethodData> FindExtensionMethods(string methodName, Expression[] args)
		{
			var matchMethods = _arguments.GetExtensionMethods(methodName);

			return MethodResolution.FindBestMethod(matchMethods, args);
		}

		public IList<MethodData> FindIndexer(Type type, Expression[] args)
		{
			foreach (var t in SelfAndBaseTypes(type))
			{
				var members = t.GetDefaultMembers();
				if (members.Length != 0)
				{
					var methods = members.
						OfType<PropertyInfo>().
						Select(p => (MethodData)new IndexerData(p));

					var applicableMethods = MethodResolution.FindBestMethod(methods, args);
					if (applicableMethods.Count > 0)
						return applicableMethods;
				}
			}

			return Array.Empty<MethodData>();
		}

		private static IEnumerable<Type> SelfAndBaseTypes(Type type)
		{
			if (type.IsInterface)
			{
				var types = new HashSet<Type>();
				foreach (var v in AddInterface(types, type))
				{
					yield return v;
				}

				yield return typeof(object);
				yield break;
			}
			foreach (var t in SelfAndBaseClasses(type))
			{
				yield return t;
			}
		}

		private static IEnumerable<Type> SelfAndBaseClasses(Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.BaseType;
			}
		}

		private static IEnumerable<Type> AddInterface(HashSet<Type> types, Type type)
		{
			if (types.Add(type))
			{
				yield return type;
				foreach (var i in type.GetInterfaces().SelectMany(x => AddInterface(types, x)))
				{
					yield return i;
				}
			}
		}
	}
}

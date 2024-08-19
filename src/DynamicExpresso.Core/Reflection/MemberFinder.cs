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
		private readonly ParserArguments _arguments;
		private readonly BindingFlags _bindingCase;
		private readonly MemberFilter _memberFilterCase;

		public MemberFinder(ParserArguments arguments)
		{
			_arguments = arguments;
			_bindingCase = arguments.Settings.CaseInsensitive ? BindingFlags.IgnoreCase : BindingFlags.Default;
			_memberFilterCase = arguments.Settings.CaseInsensitive ? Type.FilterNameIgnoreCase : Type.FilterName;
		}

		public MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
						(staticAccess ? BindingFlags.Static : BindingFlags.Instance) | _bindingCase;

			foreach (var t in SelfAndBaseTypes(type))
			{
				var members = t.FindMembers(MemberTypes.Property | MemberTypes.Field, flags, _memberFilterCase, memberName);
				if (members.Length != 0)
					return members[0];
			}
			return null;
		}

		public MethodData[] FindMethods(Type type, string methodName, bool staticAccess, Expression[] args)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
						(staticAccess ? BindingFlags.Static : BindingFlags.Instance) | _bindingCase;
			foreach (var t in SelfAndBaseTypes(type))
			{
				var members = t.FindMembers(MemberTypes.Method, flags, _memberFilterCase, methodName);
				var applicableMethods = MethodResolution.FindBestMethod(members.Cast<MethodBase>(), args);

				if (applicableMethods.Length > 0)
					return applicableMethods;
			}

			return new MethodData[0];
		}

		public MethodData FindInvokeMethod(Type type)
		{
			var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
						BindingFlags.Instance | _bindingCase;
			foreach (var t in SelfAndBaseTypes(type))
			{
				var method = t.FindMembers(MemberTypes.Method, flags, _memberFilterCase, "Invoke")
					.Cast<MethodBase>()
					.SingleOrDefault();

				if (method != null)
					return MethodData.Gen(method);
			}

			return null;
		}

		public MethodData[] FindExtensionMethods(string methodName, Expression[] args)
		{
			var matchMethods = _arguments.GetExtensionMethods(methodName);

			return MethodResolution.FindBestMethod(matchMethods, args);
		}

		public MethodData[] FindIndexer(Type type, Expression[] args)
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
					if (applicableMethods.Length > 0)
						return applicableMethods;
				}
			}

			return new MethodData[0];
		}

		private static IEnumerable<Type> SelfAndBaseTypes(Type type)
		{
			if (type.IsInterface)
			{
				var types = new List<Type>();
				AddInterface(types, type);

				types.Add(typeof(object));

				return types;
			}
			return SelfAndBaseClasses(type);
		}

		private static IEnumerable<Type> SelfAndBaseClasses(Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.BaseType;
			}
		}

		private static void AddInterface(List<Type> types, Type type)
		{
			if (!types.Contains(type))
			{
				types.Add(type);
				foreach (var t in type.GetInterfaces())
				{
					AddInterface(types, t);
				}
			}
		}
	}
}

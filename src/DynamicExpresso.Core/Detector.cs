using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using DynamicExpresso.Parsing;

namespace DynamicExpresso
{
	internal class Detector
	{
		private readonly ParserSettings _settings;

		private static readonly Regex RootIdentifierDetectionRegex =
			new Regex(@"(?<=[^\w@]|^)(?<id>@?[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}_]*)", RegexOptions.Compiled);

		private static readonly Regex ChildIdentifierDetectionRegex = new Regex(
			@"(?<=[^\w@]|^)(?<id>@?[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}_]*(\.[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}_]*)*)",
			RegexOptions.Compiled);


		private static readonly string Id = RootIdentifierDetectionRegex.ToString();
		private static readonly string Type = Id.Replace("<id>", "<type>");

		private static readonly Regex LambdaDetectionRegex =
			new Regex($@"(\((((?<withtype>({Type}\s+)?{Id}))(\s*,\s*)?)+\)|(?<withtype>{Id}))\s*=>",
				RegexOptions.Compiled);

		private static readonly Regex StringDetectionRegex =
			new Regex(@"(?<!\\)?"".*?(?<!\\)""", RegexOptions.Compiled);

		private static readonly Regex CharDetectionRegex =
			new Regex(@"(?<!\\)?'.{1,2}?(?<!\\)'", RegexOptions.Compiled);

		public Detector(ParserSettings settings)
		{
			_settings = settings;
		}

		public IdentifiersInfo DetectIdentifiers(string expression, DetectorOptions option)
		{
			expression = PrepareExpression(expression);

			var unknownIdentifiers = new HashSet<string>(_settings.KeyComparer);
			var knownIdentifiers = new HashSet<Identifier>();
			var knownTypes = new HashSet<ReferenceType>();

			// find lambda parameters
			var lambdaParameters = new Dictionary<string, Identifier>();
			foreach (Match match in LambdaDetectionRegex.Matches(expression))
			{
				var withtypes = match.Groups["withtype"].Captures;
				var types = match.Groups["type"].Captures;
				var identifiers = match.Groups["id"].Captures;

				// match identifier with its type
				var t = 0;
				for (var i = 0; i < withtypes.Count; i++)
				{
					var withtype = withtypes[i].Value;
					var identifier = identifiers[i].Value;
					var type = typeof(object);
					if (withtype != identifier)
					{
						var typeName = types[t].Value;
						if (_settings.KnownTypes.TryGetValue(typeName, out ReferenceType knownType))
							type = knownType.Type;

						t++;
					}

					// there might be several lambda parameters with the same name
					//  -> in that case, we ignore the detected type
					if (lambdaParameters.TryGetValue(identifier, out Identifier already) &&
					    already.Expression.Type != type)
						type = typeof(object);

					var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
					lambdaParameters[identifier] = new Identifier(identifier, Expression.Constant(defaultValue, type));
				}
			}

			var identifierRegex = option == DetectorOptions.IncludeChildren
				? ChildIdentifierDetectionRegex
				: RootIdentifierDetectionRegex;

			foreach (Match match in identifierRegex.Matches(expression))
			{
				var idGroup = match.Groups["id"];
				var identifier = idGroup.Value;

				if (IsReservedKeyword(identifier))
					continue;

				if (option == DetectorOptions.None && idGroup.Index > 0)
				{
					var previousChar = expression[idGroup.Index - 1];

					// don't consider member accesses as identifiers (e.g. "x.Length" will only return x but not Length)
					if (previousChar == '.')
						continue;

					// don't consider number literals as identifiers
					if (char.IsDigit(previousChar))
						continue;
				}

				if (_settings.Identifiers.TryGetValue(identifier, out Identifier knownIdentifier))
					knownIdentifiers.Add(knownIdentifier);
				else if (lambdaParameters.TryGetValue(identifier, out Identifier knownLambdaParam))
					knownIdentifiers.Add(knownLambdaParam);
				else if (_settings.KnownTypes.TryGetValue(identifier, out ReferenceType knownType))
					knownTypes.Add(knownType);
				else
					unknownIdentifiers.Add(identifier);
			}

			return new IdentifiersInfo(unknownIdentifiers, knownIdentifiers, knownTypes);
		}

		private static string PrepareExpression(string expression)
		{
			expression = expression ?? string.Empty;

			expression = RemoveStringLiterals(expression);

			expression = RemoveCharLiterals(expression);

			return expression;
		}

		private static string RemoveStringLiterals(string expression)
		{
			return StringDetectionRegex.Replace(expression, "");
		}

		private static string RemoveCharLiterals(string expression)
		{
			return CharDetectionRegex.Replace(expression, "");
		}

		private bool IsReservedKeyword(string identifier)
		{
			return ParserConstants.ReservedKeywords.Contains(identifier, _settings.KeyComparer);
		}
	}
}

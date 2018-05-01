using DynamicExpresso.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DynamicExpresso
{
	internal class Detector
	{
		private readonly ParserSettings _settings;
		private static readonly Regex IdentifiersDetectionRegex = new Regex(@"([^\.]|^)\b(?<id>[a-zA-Z_]\w*)\b", RegexOptions.Compiled);

		private static readonly Regex StringDetectionRegex = new Regex(@"(?<!\\)?"".*?(?<!\\)""", RegexOptions.Compiled);
		private static readonly Regex CharDetectionRegex = new Regex(@"(?<!\\)?'.{1,2}?(?<!\\)'", RegexOptions.Compiled);

		public Detector(ParserSettings settings)
		{
			_settings = settings;
		}

		public IdentifiersInfo DetectIdentifiers(string expression)
		{
			expression = PrepareExpression(expression);

			var unknownIdentifiers = new HashSet<string>(_settings.KeyComparer);
			var knownIdentifiers = new HashSet<Identifier>();
			var knownTypes = new HashSet<ReferenceType>();

			foreach (Match match in IdentifiersDetectionRegex.Matches(expression))
			{
				var identifier = match.Groups["id"].Value;

				if (IsReservedKeyword(identifier))
					continue;

				if (_settings.Identifiers.TryGetValue(identifier, out Identifier knownIdentifier))
					knownIdentifiers.Add(knownIdentifier);
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

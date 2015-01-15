using DynamicExpresso.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicExpresso
{
	internal class Detector
	{
		readonly ParserSettings _settings;
		static readonly Regex IDENTIFIERS_DETECTION_REGEX = new Regex(@"([^\.]|^)\b(?<id>[a-zA-Z_]\w*)\b", RegexOptions.Compiled);

		static readonly Regex STRING_DETECTION_REGEX = new Regex(@"(?<!\\)?"".*?(?<!\\)""", RegexOptions.Compiled);
		static readonly Regex CHAR_DETECTION_REGEX = new Regex(@"(?<!\\)?'.{1,2}?(?<!\\)'", RegexOptions.Compiled);

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

			foreach (Match match in IDENTIFIERS_DETECTION_REGEX.Matches(expression))
			{
				var identifier = match.Groups["id"].Value;

				Identifier knownIdentifier;
				ReferenceType knownType;

				if (IsReserverKeyword(identifier))
					continue;

				if (_settings.Identifiers.TryGetValue(identifier, out knownIdentifier))
					knownIdentifiers.Add(knownIdentifier);
				else if (_settings.KnownTypes.TryGetValue(identifier, out knownType))
					knownTypes.Add(knownType);
				else
					unknownIdentifiers.Add(identifier);
			}

			return new IdentifiersInfo(unknownIdentifiers, knownIdentifiers, knownTypes);
		}

		string PrepareExpression(string expression)
		{
			expression = expression ?? string.Empty;

			expression = RemoveStringLiterals(expression);

			expression = RemoveCharLiterals(expression);

			return expression;
		}

		string RemoveStringLiterals(string expression)
		{
			return STRING_DETECTION_REGEX.Replace(expression, "");
		}

		string RemoveCharLiterals(string expression)
		{
			return CHAR_DETECTION_REGEX.Replace(expression, "");
		}

		bool IsReserverKeyword(string identifier)
		{
			return ParserConstants.RESERVED_KEYWORDS.Contains(identifier, _settings.KeyComparer);
		}

		bool IsKnownIdentifier(string identifier)
		{
			return _settings.Identifiers.ContainsKey(identifier)
				|| _settings.KnownTypes.ContainsKey(identifier);
		}
	}
}

using LsnCore;
using LsnCore.Types;
using LSNr.Optimization;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LSNr
{
	/*
	 private IReadOnlyList<Token> PreParseGameValues(IReadOnlyList<Token> tokens)
		{
			var otherTokens = new List<Token>();
			for (int i = 0; i < tokens.Count; i++)
			{
				if (tokens[i].Type != TokenType.Keyword)
				{
					otherTokens.Add(tokens[i]);
					continue;
				}
				var val = tokens[i].Value;
				if ((val == "game" && (i + 1 < tokens.Count && string.Equals(tokens[++i].Value, "value", StringComparison.OrdinalIgnoreCase)))
					|| val == "gamevalue")
				{
					if (i + 4 >= tokens.Count)
						throw new LsnrParsingException(tokens.Last(), "Unexpected end to game value declaration", Path);
					i++;
					if (tokens[i].Type != TokenType.GameValue)
						throw new LsnrParsingException(tokens[i], $"Improperly formated game value name '{tokens[i].Value}'. A game value name must start with '$'.", Path);
					var name = tokens[i].Value;
					var typeTokens = new List<Token>();
					i++;
					if (tokens[i].Value != ":")
						throw new LsnrParsingException(tokens[i], $"Improperly formated declaration of game value '{name}'. Expected ':', recieved '{tokens[i].Value}'.", Path);
					i++;
					while (tokens[i].Value != ";")
					{
						typeTokens.Add(tokens[i]);
						if (++i >= tokens.Count)
							throw new LsnrParsingException(tokens.Last(), $"Unexpected end to declaration of game value '{name}'.", Path);
					}
					PreGameValues.Add(name, typeTokens.ToArray());
				}
				else otherTokens.Add(tokens[i]);
			}
			return otherTokens;
		}
	 */
}

using Tokens;
using Tokens.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//using System.Threading.Tasks;

namespace LSNr
{

	public static class Tokenizer
	{
		private readonly static Dictionary<string, string> MCHARSYM = new Dictionary<string, string>
		{
			["->"] = " → ",
			["=="] = " ≡ ",
			[">="] = " ≥ ",
			["<="] = " ≤ ",
			["!="] = " ≠ ",
			["++"] = " ‡ ",
			["--"] = " † ",
			["+="] = " ± ",
			["-="] = " ╧ ",
			["/="] = " ╤ ",
			["||"] = " ∨ ",
			["&&"] = " ∧ ",
			["??"] = " ⁇ "
		};

		private readonly static List<string> OPERATORS = new List<string> {
			"+","-","*","/","%","^","=",">","<","~","!","(",
			")","{","}","[","]",",",";",":","?","@","$",
			"∈","∊","∋","∍","⊂","⊃"
		};

		private static Converter<string, string> MCSConv = MCSConvM;

		//Converts char replacements for multi-char symbols back to their original form
		private static string MCSConvM(string pre_token)
		{
			if (pre_token.Length != 1) return pre_token;
			if (MCHARSYM.ContainsValue(" " + pre_token + " "))
			{
				return MCHARSYM.First(kv => kv.Value == " " + pre_token + " ").Key;
			}
			else { return pre_token; };
		}


		public static List<IToken> Tokenize(string source)
		{			
			//source = ProcessDirectives(source);
			source = RemoveComments(source);
			source = ProcessOperators(source);
			var tokens = Regex.Split(source, @"\s").Where(t => t!= "").ToList();
			return tokens.ConvertAll(MCSConv).ConvertAll(TokenFactory.getToken);
		}

		

		private static string RemoveComments(string source) => Regex.Replace(source, @"(?s)\/\*.*\*\/", "",RegexOptions.Singleline);

		private static string ProcessOperators(string source)
		{
			//'else if' -> 'elsif'
			source = Regex.Replace(source, @"(?i)else\s*if", "elsif");
			source = Regex.Replace(source, @"(?<a>\S)\.(?<b>\D)", @"\k<a> . \k<b>");
			StringBuilder src = new StringBuilder(source);
			foreach (KeyValuePair<string,string> pair in MCHARSYM)
			{
				src.Replace(pair.Key, pair.Value);
			}
			foreach (string op in OPERATORS)
			{
				src.Replace(op, $" {op} ");
			}
            return src.ToString();
		}

		internal static string ReplaceAndStore(string source, string rx, string name0, Dictionary<string,string> dict)
		{
			var mMatches = Regex.Matches(source, rx);
			Match[] x = new Match[mMatches.Count];
			mMatches.CopyTo(x, 0);
			var mList = x.ToList();
			int nameCount = 0;
			foreach(Match match in mList)
			{
				string name = name0 + nameCount++;
				dict.Add(name, match.ToString());
				source = source.Replace(match.ToString(), name);
			}
			return source;
		}
	}
}

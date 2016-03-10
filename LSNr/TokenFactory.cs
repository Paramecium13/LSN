using Tokens;
using Tokens.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	class TokenFactory
	{
		private static readonly string[] KEYWORDS = new string[]
		{
			//Core Language stuff
			"if", "else","elsif","let","mut","unless","struct","fn","for","match",
			"foreach","return","new","choose","turn","quest","virtual","stage","state",
			"record",

			//Statement headers
			"give","rotate","publish","add","remove","callcommonevent","cce","recover",
			"goto","open","fadein","fadeout","tint","flash","shake","play","show","end","exit",
			"wait","start","stop","change","say","turn",//move

			//Statement stuff
			"item","weapon","armor","armour","actor","video","image","me","se","bgm",
			"screen","moveroute","animation","ballon","picture","battle","shop","menu",
			"encounter","graphic","with","as","down","left","right","up","graphic",
			"direction","fix","route",

			//both
			"hp","level","lvl","exp","mp","skill","ability","g","gvar","gswitch","class","name",
			"nickname","state","tileset","off","on",

			//Get expression stuff
			"get","mapid","playtime","savecount","battlecount","number","of","keyitem","timer",
			"input","terraintag","region","at","tileid",

			//Things
			"my","common","all"
			//"int","double","num","complex","bool","string",
		};

		private static readonly string[] SYMBOLS = new string[]
		{
			";",",","[","]","(",")","{","}","->","=>",
			//Parameter type specifier
			":",
			//Choice conditional
			"?",
			//Member accessor (namespaces/modules) ?
			"::",
			//Struct field accessor (and other things?)
			".",
			//?
			"$",
			//Used (internally) to access members of collections ??
			"@"
		};
		//,"<-"
		private static readonly string[] OPERATORS = new string[]
		{
			//Arithmatic
			"+","*","/","%","^","&&","||","!",
			//Comparison
			">=","<=","==","!="
		};

		private static readonly string[] ASSIGNMENT = new string[]
		{
			"=","+=","-=","*=","/=","%=","--","++","~"
		};


		private static readonly string[] AMBIGUOUS = new string[]
		{
			"-","call","event","play","key","to","go","save","move"
		};

		private readonly Dictionary<string, string> Strings;

		public TokenFactory(Dictionary<string, string> strings)
		{
			Strings = strings;
		}

		public IToken GetToken(string pre_token)
		{
			if (KEYWORDS.Contains(pre_token.ToLower())) return new Keyword(pre_token.ToLower());
			else if (SYMBOLS.Contains(pre_token)) return new SyntaxSymbol(pre_token);
			else if (OPERATORS.Contains(pre_token)) return new Operator(pre_token);
			else if (AMBIGUOUS.Contains(pre_token)) return new Ambiguous(pre_token);
			else if (ASSIGNMENT.Contains(pre_token)) return new Assignment(pre_token);
			else if (Char.IsDigit(pre_token[0]))
			{
				if (pre_token.Contains('.'))
				{
					return new FloatToken(pre_token);
				}
				else return new IntToken(pre_token);
			}
			else if (pre_token.Length > PreScript.STRN.Length &&
				pre_token.Substring(0, PreScript.STRN.Length) == PreScript.STRN)
				return new StringToken(Strings[pre_token].Substring(1, Strings[pre_token].Length - 2));
			else if (pre_token.Length > PreScript.SUBN.Length &&
				pre_token.Substring(0, PreScript.SUBN.Length) == PreScript.SUBN)
				throw new NotImplementedException();
				//return new SubToken(The value to be substituted);
			else return new Identifier(pre_token);
		}


	}
}

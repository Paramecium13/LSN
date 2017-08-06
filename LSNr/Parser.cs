using LsnCore;
using Tokens;
using LsnCore.ControlStructures;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class Parser
	{
		private readonly IReadOnlyList<Token> Tokens;
		private int i = 0;
		private List<Token> TempTokens = new List<Token>();
		internal List<Component> Components = new List<Component>();
		private IPreScript Script;

		public Parser(IReadOnlyList<Token> tokens, IPreScript script)
		{
			Tokens = tokens;
			Script = script;
		}

		public void Parse()
		{
			for(; i< Tokens.Count; i++)
			{
				var token = Tokens[i];
				string t = token.Value;
				if (t == "{")
				{
					ParseControl();
				}
				else if (t == ";")
				{
					var comp = Create.State(TempTokens, Script);
					if (comp != null)
						Components.Add(comp);
					else
						Console.Write("");
					TempTokens.Clear();
				}
				else
				{
					TempTokens.Add(token);
				}
			}
		}

		private void ParseControl()
		{
			int openCount = 0;
			int closeCount = 0;
			var bodyTokens = new List<Token>();
			do
			{
				var token = Tokens[i];
				string t = token.Value;
				if (t == "{")
				{
					openCount++;
				}
				else if(t == "}")
				{
					closeCount++;
				}
				bodyTokens.Add(token); // If this happens when closeCount == openCount, it will add the ending '}'
				i++;
			} while (openCount != closeCount);
			var x = bodyTokens.Skip(1).Reverse().Skip(1).Reverse().ToList(); // Well that's one way to do it...
			var comp = Create.ControlStructure(TempTokens, x, Script);
			if (comp != null)
				Components.Add(comp);
			else
				Console.Write("");
			TempTokens.Clear();i--;
		}

		public static List<Component> Consolidate(List<Component> components)
		{
			var c = new List<Component>();
			for (int i = 0; i < components.Count; i++)
			{
				if (components[i] is IfControl)
				{
					IfElseControl f = new IfElseControl((components[i] as IfControl).Condition,(components[i] as IfControl).Body);
					i++;
					if (i >= components.Count)
						c.Add(f);
					while (i < components.Count)
					{
						if (components[i] is ElsifControl)
						{
							f.Elsifs.Add(components[i] as ElsifControl);
						}
						else if (components[i] is ElseControl)
						{
							f.ElseBlock = (components[i] as ElseControl).Body;
							c.Add(f);
							i++; // Move on to the next non-elseif/else component.
							break;
						}
						else
						{
							c.Add(f);
							//i++; // This component still needs to be analysed.
							break;
						}
						i++;
					}

				}
				else
				{
					c.Add(components[i]);
				}
			}
			return c;
		}

	}
}

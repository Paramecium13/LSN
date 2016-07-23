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
		private IReadOnlyList<IToken> Tokens;
		private int i = 0;
		private List<IToken> TempTokens = new List<IToken>();
		internal List<Component> Components = new List<Component>();
		private IPreScript Script;

		public Parser(IReadOnlyList<IToken> tokens, IPreScript script)
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
					Components.Add(Create.State(TempTokens,Script));
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
			var bodyTokens = new List<IToken>();
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
			var x = bodyTokens.Skip(1).Reverse().Skip(1).Reverse().ToList();
			Components.Add(Create.ControlStructure(TempTokens, x, Script));
			TempTokens.Clear();i--;
		}

		public static List<Component> Consolidate(List<Component> components)
		{
			var c = new List<Component>();
			for (int i = 0; i < components.Count; i++)
			{
				if (components[i] is IfControl)
				{
					IfElseControl f = new IfElseControl();
					f.Body = (components[i] as IfControl).Body;
					f.Condition = (components[i] as IfControl).Condition;
					i++;
					while (true)
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

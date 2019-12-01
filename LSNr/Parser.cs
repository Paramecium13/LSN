using LsnCore;

using LsnCore.ControlStructures;
using LsnCore.Statements;
using LsnCore.Utilities;
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
		private ISlice<Token> TempTokens
			=> Slice<Token>.Create(Tokens, TempStart, TempCount);
		private int TempStart;
		private int TempCount;

		internal List<Component> Components = new List<Component>();
		private readonly IPreScript Script;

		public Parser(IReadOnlyList<Token> tokens, IPreScript script)
		{
			Tokens = tokens;
			Script = script;
		}

		public void Parse()
		{
			for(; i< Tokens.Count; i++)
			{
				var t = Tokens[i].Value;
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
					TempCount = 0;
					TempStart = i+1;
				}
				else TempCount++;
			}
		}

		private void ParseControl()
		{
			var balance = 0;
			var start = i;
			var count = 0;
			do
			{
				var token = Tokens[i];
				string t = token.Value;
				if (t == "{")
					balance++;
				else if(t == "}")
					balance--;
				count++;
				i++;
			} while (balance != 0);
			var x = Slice<Token>.Create(Tokens, start + 1, count - 1);// Skips opening and closing braces...
			var comp = Create.ControlStructure(TempTokens, x, Script);
			if (comp != null)
				Components.Add(comp);
			else
				Console.Write("");
			TempCount = 0;
			TempStart = i;
			i--; // This is b/c the for loop in Parse() will increment i.
		}

		public static List<Component> Consolidate(List<Component> components)
		{
			var c = new List<Component>();
			for (int i = 0; i < components.Count; i++)
			{
				if (components[i] is IfControl ifc)
				{
					var f = new IfElseControl(ifc.Condition, ifc.Body);
					i++;
					if (i >= components.Count)
						c.Add(f);
					while (i < components.Count)
					{
						if (components[i] is ElsifControl)
						{
							f.Elsifs.Add(components[i] as ElsifControl);
							i++;
						}
						else if (components[i] is ElseControl)
						{
							f.ElseBlock = (components[i] as ElseControl).Body;
							c.Add(f);
							// Move on to the next non-elseif/else component.
							break;
						}
						else
						{
							c.Add(f);
							i--;
							break;
						}
					}
				}
				else
					c.Add(components[i]);
			}
			return c;
		}
	}
}

using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Converations
{
	public interface IConversationVariable
	{
		void Parse(ComponentFlattener flattener, IPreScript script);
	}

	public sealed class ConversationVariable : IConversationVariable
	{
		public string Name { get; }
		public bool Mutable { get; }

		private readonly ISlice<Token> Tokens;

		public ConversationVariable(string name, bool mutable, ISlice<Token> tokens)
		{
			Name = name; Mutable = mutable; Tokens = tokens;
		}

		public void Parse(ComponentFlattener flattener, IPreScript script)
		{
			var expr = Create.Express(Tokens, script);
			var sym = script.CheckSymbol(Name);
			switch (sym)
			{
				case SymbolType.Variable:
					throw new LsnrParsingException(Tokens[0], $"There is already a variable named '{Name}'.", script.Path);
				case SymbolType.UniqueScriptObject:
				case SymbolType.GlobalVariable:
				case SymbolType.Field:
				case SymbolType.Property:
				case SymbolType.ScriptClassMethod:
				case SymbolType.HostInterfaceMethod:
				case SymbolType.Function:
				case SymbolType.Type:
					throw new LsnrParsingException(Tokens[0], $"Cannot create a variable named '{Name}' as there is already a(n) {sym.ToString()} with that name.", script.Path);
				case SymbolType.Undefined:
				default:
					break;
			}

			// ToDo: Move this logic into AssignmentStatement, IScope, or Variable.
			var v = script.CurrentScope.CreateVariable(Name, Mutable, expr);
			var let = new AssignmentStatement(v, expr);
			v.Assignment = let;
			//v.AddUser(let); //??
			
			
			flattener.ConvPartialFlatten(new List<Component> { let }, "", null);
		}

	}
}

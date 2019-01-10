using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;


namespace LSNr
{
	public sealed class PreState : BasePreScriptClass
	{
		private readonly PreScriptClass Parent;

		public string StateName { get; }

		public int Index { get; }

		public PreState(PreScriptClass parent, string name, int index, IPreScript resource, IReadOnlyList<Token> tokens)
			:base(tokens,parent.Id,resource,parent.HostName)
		{
			Parent = parent; StateName = name; Index = index;
			HostId = parent.HostId;
		}

		internal override Property GetProperty(string val)		=> Parent.GetProperty(val);
		internal override int GetPropertyIndex(string val)		=> Parent.GetPropertyIndex(val);
		internal override Field GetField(string name)			=> Parent.GetField(name);
		internal override bool StateExists(string name)			=> Parent.StateExists(name);
		internal override int GetStateIndex(string name)		=> Parent.GetStateIndex(name);

		// There can only be one!!!!!!!!!!!
		public override bool IsMethodSignatureValid(FunctionSignature signature)
			=> !Methods.ContainsKey(signature.Name) && Parent.IsMethodSignatureValid(signature);

		public override SymbolType CheckSymbol(string name)
		{
			if (Methods.ContainsKey(name))
				return SymbolType.ScriptClassMethod; // It's a method local to this state.
			return Parent.CheckSymbol(name);
		}

		internal ScriptClassState PreParse()
		{
			var i = 0;
			while (i < Tokens.Count)
			{
				try
				{
					switch (Tokens[i].Value)
					{
						case "fn":
							i++;
							var fn = PreParseMethod(ref i);
							Methods.Add(fn.Name, fn);
							break;
						case "on":
							i++;
							var ev = PreParseEventListener(ref i);
							EventListeners.Add(ev.Definition.Name, ev);
							break;
						default:
							i++;
							break;
					}
				}
				catch (LsnrException e)
				{
					Logging.Log($"state '{StateName}' of script object '{Parent.Name}'.", e);
					Valid = false;
				}
				catch (Exception e)
				{
					Logging.Log($"state '{StateName}' of script object '{Parent.Name}'.", e, Path);
					Valid = false;
				}
			}
			return new ScriptClassState(Index, Methods, EventListeners);
		}

		internal bool Parse()
		{
			ParseMethods();
			ParseEventListeners();
			return Valid;
		}

		public override FunctionSignature GetMethodSignature(string name)
			=> Methods.ContainsKey(name) ? Methods[name].Signature : Parent.GetMethodSignature(name);
	}
}

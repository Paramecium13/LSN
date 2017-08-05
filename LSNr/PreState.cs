using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using Tokens;

namespace LSNr
{
	public sealed class PreState : BasePreScriptObject
	{

		private readonly PreScriptObject Parent;
		private readonly string _StateName;
		private readonly int _Index;

		public string StateName => _StateName;

		public int Index => _Index;

		public override IScope CurrentScope { get; set; }

		public override bool Mutable => Resource.Mutable;

		public override bool Valid
		{
			get { return Resource.Valid; }
			set { Resource.Valid = value; }
		}

		public PreState(PreScriptObject parent, string name, int index,PreResource resource, IReadOnlyList<Token> tokens)
			:base(tokens,parent.Id,resource,parent.HostName)
		{
			Parent = parent; _StateName = name; _Index = index; HostType = parent.HostType;
		}

		public override bool FunctionExists(string name) => Resource.FunctionExists(name);
		public override bool FunctionIsIncluded(string name) => Resource.FunctionIsIncluded(name);
		public override Function GetFunction(string name) => Resource.GetFunction(name);

		public override bool TypeExists(string name) => Parent.TypeExists(name);
		public override LsnType GetType(string name) => Resource.GetType(name);

		public override bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public override GenericType GetGenericType(string name) => Resource.GetGenericType(name);

		// There can only be one!!!!!!!!!!!
		public override bool IsMethodSignatureValid(FunctionSignature signature)
			=> !Methods.ContainsKey(signature.Name) && Parent.IsMethodSignatureValid(signature);

		public override TypeId GetTypeId(string name) => Parent.GetTypeId(name);


		internal override Property GetProperty(string val) => Parent.GetProperty(val);

		internal override int GetPropertyIndex(string val) => Parent.GetPropertyIndex(val);

		internal override Field GetField(string name)
		=> Parent.GetField(name);

		internal override bool StateExists(string name) => Parent.StateExists(name);

		internal override int GetStateIndex(string name) => Parent.GetStateIndex(name);


		public override SymbolType CheckSymbol(string name)
		{
			if (Methods.ContainsKey(name))
				return SymbolType.ScriptObjectMethod; // It's a method local to this state.
			return Parent.CheckSymbol(name);
		}



		internal void PreParse()
		{
			int i = 0;
			while (i < Tokens.Count)
			{
				var val = Tokens[i].Value;
				switch (val)
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
		}


		internal ScriptObjectState Parse()
		{
			ParseMethods();
			ParseEventListeners();

			return new ScriptObjectState(_Index, Methods, EventListeners);
		}

	}
}

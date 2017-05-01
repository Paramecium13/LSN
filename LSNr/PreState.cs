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
		private readonly string StateName;
		private readonly int Index;

		public PreState(PreScriptObject parent, string name, int index,PreResource resource, IReadOnlyList<IToken> tokens)
			:base(tokens,parent.Id,resource,parent.HostName)
		{
			Parent = parent; StateName = name; Index = index;
		}

		public override IScope CurrentScope { get; set; }

		public override bool Mutable => Resource.Mutable;

		public override bool Valid
		{
			get { return Resource.Valid; }
			set { Resource.Valid = value; }
		}

		public override bool FunctionExists(string name) => Resource.FunctionExists(name);
		public override bool FunctionIsIncluded(string name) => Resource.FunctionIsIncluded(name);
		public override Function GetFunction(string name) => Resource.GetFunction(name);

		public override bool TypeExists(string name) => Parent.TypeExists(name);
		public override LsnType GetType(string name) => Resource.GetType(name);

		public override bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public override GenericType GetGenericType(string name) => Resource.GetGenericType(name);


		public override bool IsMethodSignatureValid(FunctionSignature signature) => Parent.IsMethodSignatureValid(signature);
		// TODO: What about functions only defined in this state?
		public override SymbolType CheckSymbol(string name) => Parent.CheckSymbol(name);

		public override TypeId GetTypeId(string name) => Parent.GetTypeId(name);
	}
}

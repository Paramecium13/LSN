using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Converations
{
	sealed class BranchBuilder : IBranch
	{
		readonly ITypeContainer TypeContainer;

		public bool GenericTypeExists(string name) => TypeContainer.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => TypeContainer.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => TypeContainer.GetGenericType(name);
		public LsnType GetType(string name) => TypeContainer.GetType(name);
		public TypeId GetTypeId(string name) => TypeContainer.GetTypeId(name);
		public bool TypeExists(string name) => TypeContainer.TypeExists(name);

		public IExpression Condition { get; private set; }

		public PreStatement[] Code { get; private set; }
	}
}

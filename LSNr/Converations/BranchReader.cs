using LsnCore;
using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Converations
{
	interface IBranch : ITypeContainer
	{
		string Name { get; }
		IExpression Condition { get; }
		IExpression Prompt { get; }
	}

	class BranchReader
	{
	}
}

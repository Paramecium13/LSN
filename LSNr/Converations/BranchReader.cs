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
		IExpression Condition { get; }

		PreStatement[] Code { get; }
	}

	class BranchReader
	{
	}
}

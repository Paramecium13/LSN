using LsnCore;
using LsnCore.Expressions;
using LsnCore.Utilities;
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
		ISlice<Token> ConditionTokens { get; set; }
		IExpression Condition { get; set; }
		ISlice<Token> PromptTokens { get; set; }
		IExpression Prompt { get; set; }
		ISlice<Token> ActionTokens { get; }
	}

	class BranchReader
	{
	}
}

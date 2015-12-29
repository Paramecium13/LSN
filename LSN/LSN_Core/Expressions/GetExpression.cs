using LSN_Core.Compile;
using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	[Serializable]
	abstract class GetExpression : Expression
	{
		public static Expression CreateGet(List<IToken> tokens)
		{
			return null;
		}
	}
}

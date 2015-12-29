using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Expressions
{
	[Serializable]
	public abstract class ComponentExpression : Expression, IComponentExpression
	{
		public abstract string TranslateUniversal();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	// Can be pooled
	internal sealed class LsnVmCallBack
	{
		static Action<object> SignalProcess = (x) => { };

		LsnVmEvalStack Stack;
		object Process;

		internal LsnVmCallBack(LsnVmEvalStack stack, object process)
		{
			Stack = stack; Process = process;
		}

		internal void Invoke(LsnValue value)
		{
			if (Stack != null)
				Stack.Push(value);
			SignalProcess(Process);
		}

		internal void ReInit(LsnVmEvalStack stack, object process)
		{
			Stack = stack; Process = process;
		}

		internal void Pool()
		{
			Stack = null;
			Process = null;
		}
	}
}

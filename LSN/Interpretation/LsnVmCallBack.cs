using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	// Can be pooled
	internal sealed class LsnVmCallback
	{
		/// <summary>
		/// Notify the process that its awaited task has completed.
		/// </summary>
		static Action<object> SignalProcess = (x) => { };

		LsnVmEvalStack Stack;
		object Process;

		internal class Pool
		{
			internal static readonly Pool Instance = new Pool();

			readonly Stack<LsnVmCallback> _Pool = new Stack<LsnVmCallback>(256);

			internal int Limit { get; set; }

			readonly object Locker = new object();

			internal LsnVmCallback Request(LsnVmEvalStack stack, object process)
			{
				LsnVmCallback result;
				lock (Locker)
				{
					if (_Pool.Count > 0)
						result = _Pool.Pop();
					else result = new LsnVmCallback();
				}
				result.Stack = stack;
				result.Process = process;
				return result;
			}

			internal void Return(LsnVmCallback callback)
			{
				callback.Stack = null; callback.Process = null;
				lock (Locker)
				{
					if (_Pool.Count < Limit)
						_Pool.Push(callback);
				}
			}
		}

		private LsnVmCallback() { }

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

		internal void PrePool()
		{
			Stack = null;
			Process = null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	public sealed class LsnVmTask
	{
		internal readonly object Lock = new object();

		Action<LsnValue> Continuation;

		LsnValue Result;

		public TaskStatus Status { get; private set; } = TaskStatus.Created;

		public class CompletionSource
		{
			public readonly LsnVmTask Task;
			internal CompletionSource(LsnVmTask task) { Task = task; }

			public void SetResult(LsnValue value)
			{
				lock (Task.Lock)
				{
					Task.Result = value;
					Task.Status = TaskStatus.RanToCompletion;
					Task.Continuation?.Invoke(Task.Result);
				}
			}
		}

		public void ContinueWith(Action<LsnValue> continuation)
		{
			lock (Lock)
			{
				switch (Status)
				{
					case TaskStatus.Created:
					case TaskStatus.WaitingForActivation:
					case TaskStatus.WaitingToRun:
						throw new InvalidOperationException("This LsnVmTask is pooled and inactive.");
					case TaskStatus.Running:
					case TaskStatus.WaitingForChildrenToComplete:
						if (Continuation != null)
							Continuation += continuation;
						Continuation = continuation;
						break;
					case TaskStatus.RanToCompletion:
						continuation(Result);
						break;
					case TaskStatus.Canceled:
					case TaskStatus.Faulted:
						throw new NotImplementedException();
				}
			}
		}

		internal void Pool()
		{
			Status = TaskStatus.Created;
			Continuation = null;
			Result = LsnValue.Nil;
		}

	}
}

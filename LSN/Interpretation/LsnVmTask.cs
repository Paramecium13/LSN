using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	public sealed class LsnVmTask : IDisposable
	{
		internal readonly object Lock = new object();

		ManualResetEventSlim WaitHandle = new ManualResetEventSlim(false);

		LsnVmCallBack Continuation;

		LsnValue Result;

		Exception Exception;

		//public TaskStatus Status { get; private set; } = TaskStatus.Created;
		TaskStatus Status = TaskStatus.Created;

		internal bool Disposed { get; set; } // To detect redundant calls

		internal LsnVmTask() {}

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
					Task.Continuation.Invoke(Task.Result);
					Task.WaitHandle.Set();
				}
			}

			public void SetException(Exception e)
			{
				lock (Task.Lock)
				{
					Task.Exception = e;
					Task.WaitHandle.Set();
				}
			}
		}

		internal void ContinueWith(LsnVmCallBack continuation)
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
							throw new InvalidOperationException("This LsnVmTask already has a continuation.");
						Continuation = continuation;
						break;
					case TaskStatus.RanToCompletion:
						continuation.Invoke(Result);
						break;
					case TaskStatus.Canceled:
						throw new NotImplementedException();
					case TaskStatus.Faulted:
						throw Exception;
				}
			}
		}

		internal void Pool()
		{
			if (Disposed)
				throw new ObjectDisposedException("Cannot return a disposed LsnVmTask to the pool.");
			Status = TaskStatus.Created;
			Continuation = null;
			Result = LsnValue.Nil;
			Exception = null;
			WaitHandle.Reset();
		}

		public void Wait() => WaitHandle.Wait();

		#region IDisposable Support

		void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					WaitHandle.Dispose();
				}
				Continuation = null;
				Result = LsnValue.Nil;
				Disposed = true;
			}
		}
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion

	}
}

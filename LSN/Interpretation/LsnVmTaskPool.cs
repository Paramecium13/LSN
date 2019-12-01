using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	class LsnVmTaskPool
	{
		public static readonly LsnVmTaskPool Instance = new LsnVmTaskPool();

		public int Limit { get; set; } = 256;

		readonly Stack<LsnVmTask.CompletionSource> Pool = new Stack<LsnVmTask.CompletionSource>();

		readonly object Locker = new object();

		public LsnVmTask.CompletionSource RequestTask()
		{
			lock (Locker)
			{
				if (Pool.Count != 0)
					return Pool.Pop();
			}
			return new LsnVmTask().Completion;
		}

		internal void Return(LsnVmTask task)
		{
			task.Pool();
			lock (Locker)
			{
				if(Pool.Count < Limit)
					Pool.Push(task.Completion);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Runtime.Types;
using LsnCore.Values;

namespace LsnCore.Runtime.Values
{
	/// <summary>
	/// Use this for events that do not interrupt the normal game flow.
	/// </summary>
	public abstract class EventInstance : IEventInstance
	{
		public readonly LsnCore.Types.EventDefinition Definition;
		public readonly string Name;

		private readonly List<ScriptObject> Subscribers = new List<ScriptObject>();

		protected EventInstance(LsnCore.Types.EventDefinition definition)
		{
			Definition = definition; Name = definition.Name;
		}


		public virtual void Subscribe(ScriptObject obj)
		{
			lock (Subscribers)
				Subscribers.Add(obj);
		}


		public virtual void Unsubscribe(ScriptObject obj)
		{
			lock (Subscribers)
				Subscribers.Remove(obj);
		}


		/*public Task Fire(LsnValue[] args)
		{
			if (Subscribers.Count == 0)
				return Task.CompletedTask;
			ScriptObject[] subs;
			lock (Subscribers)
			{
				subs = Subscribers.ToArray();
			}

			// Don't lock subscribers here!
			int n = subs.Length;
			int c = args.Length+1;
			return Task.Run(() => 
				Parallel.For(0, n, (i) => {
					var a = new LsnValue[c];
					a[0] = new LsnValue(subs[i]);
					args.CopyTo(a, 1);
					Fire(subs[i].GetEventListener(Name), a);
				})
			);
		}*/

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		protected abstract void Fire(EventListener eventListener, LsnValue[] args);


	}
}

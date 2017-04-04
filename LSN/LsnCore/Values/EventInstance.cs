using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	public abstract class EventInstance
	{
		public readonly EventDefinition Definition;
		public readonly string Name;

		private readonly List<ScriptObject> Subscribers = new List<ScriptObject>();

		protected EventInstance(EventDefinition definition)
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


		public Task Fire(LsnValue[] args)
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
					Fire(subs[i]/*.GetEventListener(Name)*/, a);
				})
			);
		}

		protected abstract void Fire(/*EventListener*/object eventListener, LsnValue[] args);


	}
}

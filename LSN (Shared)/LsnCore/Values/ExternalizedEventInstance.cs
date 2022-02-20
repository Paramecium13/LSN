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
	/// Use this for events that interrupt the normal game flow.
	/// </summary>
	public class ExternalizedEventInstance: IEventInstance
	{

		private readonly Action<EventListener, LsnValue[]> RegisterEventForExecution;

		public readonly LsnCore.Types.EventDefinition Definition;
		public readonly string Name;

		private readonly List<ScriptObject> _Subscribers = new List<ScriptObject>();

		//protected IReadOnlyList<ScriptObject> Subscribers => _Subscribers;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="def"></param>
		/// <param name="firingFunction">The function that is called when an event should be executed.</param>
		public ExternalizedEventInstance(LsnCore.Types.EventDefinition def, Action<EventListener, LsnValue[]> firingFunction)
		{
			Definition = def; Name = def.Name; RegisterEventForExecution = firingFunction;
		}


		public void Subscribe(ScriptObject obj)
		{
			lock(_Subscribers) _Subscribers.Add(obj);
		}


		public void Unsubscribe(ScriptObject obj)
		{
			lock (_Subscribers) _Subscribers.Remove(obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <remarks>Don't pass a null args or an args of the wrong size or format. This will not waste timing checking that args is correct.</remarks>
		/// <returns>A completed task.</returns>
		/*public virtual Task Fire(LsnValue[] args)
		{
			ScriptObject[] subs;
			lock (_Subscribers)
			{
				subs = _Subscribers.ToArray();
			}
			int n = subs.Length;
			if (n == 0) return Task.CompletedTask;
			int c = args.Length + 1;
			for(int i = 0; i < n; i++)
			{
				var a = new LsnValue[c];
				var s = subs[i];
				a[0] = new LsnValue(s);
				args.CopyTo(a, 1);
				RegisterEventForExecution(s.GetEventListener(Name),a);
			}

			return Task.CompletedTask;
		}*/

	}
}

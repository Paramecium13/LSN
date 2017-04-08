using LsnCore.Expressions;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class ScriptObjectState
	{
		//public readonly string Name;
		public readonly int Id;


		private readonly IReadOnlyDictionary<string, ScriptObjectMethod> ScriptObjectMethods;


		// Events
		public readonly IReadOnlyList<string> EventsListenedTo;

		private readonly IReadOnlyDictionary<string, EventListener> EventListeners;


		public ScriptObjectState(/*string name,*/ int id, IReadOnlyDictionary<string, ScriptObjectMethod> methods, IReadOnlyDictionary<string, EventListener>  eventListeners)
		{
			Id = id; ScriptObjectMethods = methods; EventListeners = eventListeners;
			EventsListenedTo = EventListeners.Keys.ToList();
		}


		public bool HasMethod(string name) => ScriptObjectMethods.ContainsKey(name);


		public ScriptObjectMethod GetMethod(string name) => ScriptObjectMethods[name];


		public bool HasEventListener(string name) => EventListeners.ContainsKey(name);


		public EventListener GetEventListener(string name) => EventListeners[name];

	}
}

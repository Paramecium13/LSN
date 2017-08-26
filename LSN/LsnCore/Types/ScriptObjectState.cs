using LsnCore.Expressions;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class ScriptObjectState
	{
		//public readonly string Name;
		public readonly int Id;


		private readonly IReadOnlyDictionary<string, ScriptObjectMethod> ScriptObjectMethods;


		// Events
		public readonly IReadOnlyList<string> EventsListenedTo;

		private readonly IReadOnlyDictionary<string, EventListener> EventListeners;


		public ScriptObjectState(int id, IReadOnlyDictionary<string, ScriptObjectMethod> methods, IReadOnlyDictionary<string, EventListener>  eventListeners)
		{
			Id = id; ScriptObjectMethods = methods; EventListeners = eventListeners;
			EventsListenedTo = EventListeners.Keys.ToList();
		}


		public bool HasMethod(string name) => ScriptObjectMethods.ContainsKey(name);


		public ScriptObjectMethod GetMethod(string name) => ScriptObjectMethods[name];


		public bool HasEventListener(string name) => EventListeners.ContainsKey(name);


		public EventListener GetEventListener(string name) => EventListeners[name];


		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write(Id);
			writer.Write((ushort)ScriptObjectMethods.Count);
			foreach (var method in ScriptObjectMethods.Values)
				method.Serialize(writer);

			writer.Write((ushort)EventListeners.Count);
			foreach (var listener in EventListeners.Values)
				listener.Serialize(writer);
		}

		public static ScriptObjectState Read(BinaryDataReader reader, ITypeIdContainer typeContainer, TypeId type, string resourceFilePath)
		{
			var id = reader.ReadInt32();

			var nMethods = reader.ReadUInt16();
			var methods = new Dictionary<string, ScriptObjectMethod>(nMethods);
			for (int i = 0; i < nMethods; i++)
			{
				var method = ScriptObjectMethod.Read(reader, typeContainer, type, resourceFilePath);
				methods.Add(method.Name, method);
			}

			var nListeners = reader.ReadUInt16();
			var listeners = new Dictionary<string, EventListener>();
			for (int i = 0; i < nListeners; i++)
			{
				var listener = EventListener.Read(reader, typeContainer, resourceFilePath);
				listeners.Add(listener.Definition.Name, listener);
			}

			return new ScriptObjectState(id, methods, listeners);
		}

	}
}

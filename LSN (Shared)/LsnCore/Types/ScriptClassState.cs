using LsnCore.Expressions;
using LsnCore.Serialization;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class ScriptClassState
	{
		//public readonly string Name;
		public readonly int Id;

		private readonly IReadOnlyDictionary<string, ScriptClassMethod> ScriptObjectMethods;

		// Events
		public readonly IReadOnlyList<string> EventsListenedTo;

		private readonly IReadOnlyDictionary<string, EventListener> EventListeners;

		public ScriptClassState(int id, IReadOnlyDictionary<string, ScriptClassMethod> methods, IReadOnlyDictionary<string, EventListener>  eventListeners)
		{
			Id = id; ScriptObjectMethods = methods; EventListeners = eventListeners;
			EventsListenedTo = EventListeners.Keys.ToList();
		}

		public bool HasMethod(string name) => ScriptObjectMethods.ContainsKey(name);

		public ScriptClassMethod GetMethod(string name) => ScriptObjectMethods[name];

		public bool HasEventListener(string name) => EventListeners.ContainsKey(name);

		public EventListener GetEventListener(string name) => EventListeners[name];

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((int)Id);

			writer.Write((ushort)ScriptObjectMethods.Count);
			foreach (var method in ScriptObjectMethods.Values)
				method.Serialize(writer, resourceSerializer);

			writer.Write((ushort)EventListeners.Count);
			foreach (var listener in EventListeners.Values)
				listener.Serialize(writer, resourceSerializer);
		}

		public static ScriptClassState Read(BinaryDataReader reader, ITypeIdContainer typeContainer, TypeId type, string resourceFilePath, ResourceDeserializer resourceDeserializer)
		{
			var id = reader.ReadInt32();

			var nMethods = reader.ReadUInt16();
			var methods = new Dictionary<string, ScriptClassMethod>(nMethods);
			for (int i = 0; i < nMethods; i++)
			{
				var method = ScriptClassMethod.Read(reader, typeContainer, type, resourceFilePath, resourceDeserializer);
				methods.Add(method.Name, method);
			}

			var nListeners = reader.ReadUInt16();
			var listeners = new Dictionary<string, EventListener>();
			for (int i = 0; i < nListeners; i++)
			{
				var listener = EventListener.Read(reader, typeContainer, resourceFilePath, resourceDeserializer);
				listeners.Add(listener.Definition.Name, listener);
			}

			return new ScriptClassState(id, methods, listeners);
		}
	}
}

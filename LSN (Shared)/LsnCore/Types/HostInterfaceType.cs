using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class HostInterfaceType : LsnType
	{
		// Method definitions
		internal readonly IReadOnlyDictionary<string, FunctionSignature> MethodDefinitions;

		// Event definitions
		internal readonly IReadOnlyDictionary<string, EventDefinition> EventDefinitions;


		public HostInterfaceType(TypeId id, Dictionary<string, FunctionSignature> methods, Dictionary<string, EventDefinition> events)
		{
			Name = id.Name; Id = id; MethodDefinitions = methods; EventDefinitions = events;
			id.Load(this);
		}

		public override LsnValue CreateDefaultValue() => LsnValue.Nil;

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool HasMethod(string name) => MethodDefinitions.ContainsKey(name);

		/// <summary>
		/// Note: Does not have a 'self' parameter.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public FunctionSignature GetMethodDefinition(string name)
		{
			if (MethodDefinitions.ContainsKey(name))
				return MethodDefinitions[name];
			throw new ArgumentException($"No method named {name} exists.", nameof(name));
		}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool HasEventDefinition(string name)
			=> EventDefinitions.ContainsKey(name);

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public EventDefinition GetEventDefinition(string name)
		{
			if (EventDefinitions.ContainsKey(name))
				return EventDefinitions[name];
			throw new ArgumentException($"No event named {name} exists.", nameof(name));
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(Name);

			writer.Write((ushort)EventDefinitions.Count);
			foreach (var ev in EventDefinitions.Values)
				ev.Serialize(writer, resourceSerializer);

			writer.Write((ushort)MethodDefinitions.Count);
			foreach (var mdef in MethodDefinitions.Values)
				mdef.Serialize(writer, resourceSerializer);
		}


		public static HostInterfaceType Read(BinaryDataReader reader, ITypeIdContainer typeContainer)
		{
			var name = reader.ReadString();

			var type = typeContainer.GetTypeId(name);

			var nEventDefs = reader.ReadUInt16();
			var eventDefs = new Dictionary<string, EventDefinition>(nEventDefs);
			for (int i = 0; i < nEventDefs; i++)
			{
				var ev = EventDefinition.Read(reader, typeContainer);
				eventDefs.Add(ev.Name, ev);
			}

			var nMethodDefs = reader.ReadUInt16();
			var methodDefs = new Dictionary<string, FunctionSignature>(nMethodDefs);
			for (int i = 0; i < nMethodDefs; i++)
			{
				var m = FunctionSignature.Read(reader, typeContainer);
				methodDefs.Add(m.Name, m);
			}

			return new HostInterfaceType(type, methodDefs, eventDefs);
		}
	}
}

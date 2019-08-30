using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LsnCore.Values;
using LsnCore.Types;

namespace LsnCore.Serialization
{
	public class LsnSaveSystem
	{

		internal ILsnSerializer StartSave()
			=> throw new NotImplementedException();

	}

	/// <summary>
	/// 1. Save attached script objects by host.
	/// 2. Save unattached unique script objects.
	/// 3. Save unattached non-unique script objects,
	///	   records, lists, and vectors.
	/// 4. Save strings.
	/// </summary>
	public class LsnSerializer : ILsnSerializer
	{
		class HostsSegment
		{
			readonly BinaryDataWriter Writer;

			uint Count;

			readonly Offset Offset;

			internal HostsSegment(BinaryDataWriter writer)
			{
				Writer = writer;
				Offset = Writer.ReserveOffset();
			}

			internal void Write(IHostInterface host, LsnSerializer serializer)
			{
				Writer.Write(host.NumericId);                                       // Id
				var lengthField = Writer.ReserveOffset();
				var startPos = Writer.Position;
				var scripts = host.GetScripts();
				Writer.Write((ushort)scripts.Length);								// Num scripts
				foreach (var script in scripts)
				{
					Writer.Write(serializer.Ids[script]);
					Writer.Write(serializer.TypeSegment.GetIndex(script.Type));
					script.ScriptClass.WriteValue(script, serializer, Writer);      // Scripts
				}
				lengthField.Satisfy((int)Writer.Position - (int)lengthField.Position - 4);
				Count++;
			}

			internal void Finish()
			{
				Offset.Satisfy((int)Count);
			}
		}
		readonly HostsSegment Hosts;

		class FreeObjectsSegment
		{
			readonly BinaryDataWriter Writer;
			uint Count;
			readonly Offset Offset;

			internal FreeObjectsSegment(BinaryDataWriter writer)
			{
				Writer = writer;
				Offset = Writer.ReserveOffset();
			}

			internal void Write(ILsnValue value, LsnSerializer serializer)
			{
				var id = serializer.Ids[value];
				Writer.Write(id);
				var type = (LsnReferenceType)value.Type.Type;
				type.WriteValue(value, serializer, Writer);
				Count++;
			}

			internal void Finish()
			{
				Offset.Satisfy((int)Count);
			}
		}
		readonly FreeObjectsSegment FreeObjects;

		uint NextId;
		readonly Dictionary<ILsnValue, uint> Ids = new Dictionary<ILsnValue, uint>();

		//readonly HashSet<uint> Serialized = new HashSet<uint>();

		readonly Queue<ILsnValue> SaveQueue = new Queue<ILsnValue>();

		uint NextStringId;
		readonly Dictionary<string, uint> Strings = new Dictionary<string, uint>();

		readonly HashSet<IHostInterface> SavedHosts = new HashSet<IHostInterface>();

		readonly SaveFileTypeSegment TypeSegment;

		readonly MemoryStream HostsData;

		readonly MemoryStream FreeObjData;

		internal LsnSerializer(SaveFileTypeSegment typeSegment)
		{
			TypeSegment = typeSegment;
			HostsData = new MemoryStream();
			Hosts = new HostsSegment(new BinaryDataWriter(HostsData, true));
			FreeObjData = new MemoryStream();
			FreeObjects = new FreeObjectsSegment(new BinaryDataWriter(FreeObjData, true));
		}

		uint GetId(ILsnValue value)
		{
			if (Ids.ContainsKey(value)) return Ids[value];
			var id = NextId++;
			Ids.Add(value, id);
			return id;
		}

		void WriteHost(IHostInterface host)
		{
			SavedHosts.Add(host);
			Hosts.Write(host, this);
		}

		public uint SaveList(ILsnValue list)
		{
			if(!Ids.ContainsKey(list))
			{
				Ids.Add(list, NextId++);
				SaveQueue.Enqueue(list);
			}
			return Ids[list];
		}

		public uint SaveRecord(ILsnValue record)
		{
			if (!Ids.ContainsKey(record))
			{
				Ids.Add(record, NextId++);
				SaveQueue.Enqueue(record);
			}
			return Ids[record];
		}

		public uint SaveVector(ILsnValue vector)
		{
			if (!Ids.ContainsKey(vector))
			{
				Ids.Add(vector, NextId++);
				SaveQueue.Enqueue(vector);
			}
			return Ids[vector];
		}
		
		public uint SaveScriptObject(ScriptObject scriptObject)
		{
			var h = scriptObject.GetHost().Value;
			if (h == null)
			{
				if (!Ids.ContainsKey(scriptObject))
				{
					Ids.Add(scriptObject, NextId++);
					SaveQueue.Enqueue(scriptObject);
				}
				return Ids[scriptObject];
			}
			if (!Ids.ContainsKey(scriptObject))
				SaveHost(h as IHostInterface);
			return Ids[scriptObject];
		}

		public uint SaveString(string value)
		{
			if (!Strings.ContainsKey(value))
			{
				var id = NextStringId++;
				Strings.Add(value, id);
				return id;
			}
			return Strings[value];
		}

		public void SaveHost(IHostInterface hostInterface)
		{
			if (hostInterface.HasScripts && !SavedHosts.Contains(hostInterface))
				WriteHost(hostInterface);
		}

		public void SaveFreeObjects()
		{
			while (SaveQueue.Count != 0)
				FreeObjects.Write(SaveQueue.Dequeue(), this);
		}

		/// <summary>
		/// Call this after saving all the hosts.
		/// </summary>
		public void Save(Stream stream)
		{
			SaveFreeObjects();
			Hosts.Finish();
			HostsData.WriteTo(stream);
			FreeObjects.Finish();
			FreeObjData.WriteTo(stream);
			using (var writer = new BinaryDataWriter(stream,true))
			{
				writer.Write((uint)Strings.Count);
				foreach (var pair in Strings)
				{
					writer.Write(pair.Value);
					writer.Write(pair.Key);
				}
			}
		}

	}

	public class LsnDeserializer
	{

	}
}

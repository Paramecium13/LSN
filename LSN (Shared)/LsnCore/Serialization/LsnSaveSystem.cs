using System;
using System.Collections.Generic;
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

	public class LsnSerializer : ILsnSerializer
	{
		uint NextId;
		readonly Dictionary<ILsnValue, uint> Ids = new Dictionary<ILsnValue, uint>();

		//readonly HashSet<uint> Serialized = new HashSet<uint>();

		readonly Queue<ILsnValue> SaveQueue = new Queue<ILsnValue>();

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
			if(scriptObject.GetHost().Value == null)
			{
				if (!Ids.ContainsKey(scriptObject))
				{
					Ids.Add(scriptObject, NextId++);
					SaveQueue.Enqueue(scriptObject);
				}
				return Ids[scriptObject];
			}

			throw new NotImplementedException();
		}

		public uint SaveString(string value)
		{
			throw new NotImplementedException();
		}
	}
}

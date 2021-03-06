﻿using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class EventDefinition
	{
		public readonly string Name;
		public readonly IReadOnlyList<Parameter> Parameters;

		public EventDefinition(string name, IReadOnlyList<Parameter> paramaters)
		{
			if (paramaters.Any(p => !p.DefaultValue.IsNull)) throw new ArgumentException("Event parameters cannot have default values.", "parameters");
			Parameters = paramaters.ToList();
			Name = name;
		}

		public bool Equivalent(EventDefinition other, bool requireMatchingNames = false)
		{
			if (Name != other.Name) return false;
			if (Parameters.Count != other.Parameters.Count) return false;

			for(int i = 0; i < Parameters.Count; i++)
			{
				var param = Parameters[i];
				var otherParam = other.Parameters[i];
				if (param.Type != otherParam.Type) return false;
				if (param.Index != otherParam.Index) return false;
				if (requireMatchingNames && param.Name != otherParam.Name) return false;
			}

			return true;
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(Name);
			writer.Write((ushort)Parameters.Count);
			foreach (var param in Parameters)
				param.Serialize(writer, resourceSerializer);
		}

		public static EventDefinition Read(BinaryDataReader reader, ITypeIdContainer typeContainer)
		{
			var name = reader.ReadString();
			var nParams = reader.ReadUInt16();
			var parameters = new Parameter[nParams];
			for (ushort i = 0; i < nParams; i++)
			{
				parameters[i] = Parameter.Read(i, reader, typeContainer);
			}
			return new EventDefinition(name, new List<Parameter>(parameters));
		}
	}
}

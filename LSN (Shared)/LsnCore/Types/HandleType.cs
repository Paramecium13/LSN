using Syroot.BinaryData;
using System;
using System.Collections.Generic;

namespace LsnCore.Types
{
	public class HandleType : LsnType
	{
		readonly HashSet<TypeId> ParentTypes;

		internal HandleType(TypeId id, TypeId[] parents)
		{
			Id = id; Name = id.Name; ParentTypes = new HashSet<TypeId>(parents);
			Id.Load(this);
		}

#if LSNR
		public HandleType(string name, out TypeId typeId)
		{
			Name = name; ParentTypes = new HashSet<TypeId>();
			Id = new TypeId(this); typeId = Id;
		}

		event Action<HandleType> ParentAdded;

		public void AddParent(HandleType parent)
		{
			if (this == parent)
				throw new InvalidOperationException("Cyclic inheritance!!!");
			if (!ParentTypes.Add(parent.Id))
				return;
			parent.ParentAdded += AddParent;
			ParentAdded?.Invoke(parent);
			foreach (var grand in parent.ParentTypes)
			{
				try
				{
					AddParent((HandleType)grand.Type);
				}
				catch (InvalidCastException)
				{
					throw new ApplicationException($"{grand.Name} is not a handle.");
				}
			}
		}
#endif

		public override bool Subsumes(LsnType type)
			=> (type is HandleType handle && handle.IsA(this));

		public bool IsA(HandleType type)
		{
			return this == type || ParentTypes.Contains(type.Id);
		}

		public override LsnValue CreateDefaultValue()
		{
#if LSNR
			return new LsnValue(0u, this.Id);
#else
			return new LsnValue(0u);
#endif
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			resourceSerializer.WriteTypeId(Id, writer);
			writer.Write((ushort)ParentTypes.Count);
			foreach (var parent in ParentTypes)
				resourceSerializer.WriteTypeId(parent, writer);
		}

		public static HandleType Read(BinaryDataReader reader, ITypeIdContainer typeContainer)
		{
			var id = typeContainer.GetTypeId(reader.ReadUInt16());
			var nParents = reader.ReadUInt16();
			var parents = new TypeId[nParents];
			for (int i = 0; i < nParents; i++)
				parents[i] = typeContainer.GetTypeId(reader.ReadUInt16());

			return new HandleType(id, parents);
		}
	}
}

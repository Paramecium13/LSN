using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;

namespace LsnCore
{
	internal enum StatementCode:ushort
	{
		AssignVariable = 0,			// Assignment
		AssignField,
		AssignValueInCollection,

		Jump = 16,					// Flow Control
		ConditionalJump,
		SetTarget,
		JumpToTarget,

		Return = 32,				// Functions and stuff
		ReturnValue,

		EvaluateExpression = 48,	// ?

		SetState = 64,				// Script objects, etc.
		Detach,
		AttachNewScriptObject,

		Say = 128,					// Game stuff
		RegisterChoice,
		DisplayChoices,
		GoTo,
		GiveItem,
		GiveGold,
		Save,
		Load,

		Extension1 = 256,
		Extension2
	}

	internal enum ExpressionCode:byte
	{
		DoubleValueConstant,
		TabledConstant,
		Variable,
		FieldAccess,
		CollectionValueAccess,
		PropertyAccess,
		HostInterfaceAccess,
		UniqueScriptObjectAccess,
		BinaryExpression,
		NotExpression,
		FunctionCall,
		MethodCall,
		ScriptObjectMethodCall,
		HostInterfaceMethodCall,
		RecordConstructor,
		StructConstructor,
		ListConstructor
	}

	internal enum ConstantCode:byte
	{
		DoubleOrInt,
		String,
		Record,
		Struct,
		Vector,
		List
	}

	public class ResourceSerializer
	{
		private readonly List<ILsnValue> ConstantTable = new List<ILsnValue>();

		private readonly TypeId[] TypeIds;

		public ResourceSerializer(TypeId[] typeIds)
		{
			TypeIds = typeIds;
		}

		internal ushort TableConstant(ILsnValue value)
		{
			if(ConstantTable.Contains(value))
				return (ushort)ConstantTable.IndexOf(value);
			ConstantTable.Add(value);
			return (ushort)(ConstantTable.Count - 1);
		}

		internal void WriteConstantTable(BinaryDataWriter writer)
		{
			writer.Write((ushort)ConstantTable.Count);
			for (int i = 0; i < ConstantTable.Count; i++)
				ConstantTable[i].Serialize(writer);
		}

		internal void WriteTypeId(TypeId typeId, BinaryDataWriter writer)
		{
			var i = (ushort)Array.IndexOf(TypeIds, typeId);
			writer.Write(i);
		}
	}
}

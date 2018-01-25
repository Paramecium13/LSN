using LsnCore.Types;
using Syroot.BinaryData;
using System.Collections.Generic;

namespace LsnCore
{
	internal enum StatementCode:ushort
	{
		AssignVariable,
		AssignField,
		AssignValueInCollection,

		Jump,
		ConditionalJump,

		Return,
		ReturnValue,

		EvaluateExpression,
		SetState,

		RegisterChoice = 128,
		DisplayChoices,
		Say,
		GoTo,
		GiveItem,
		GiveGold,
		Save,
		Load
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
	}
}

using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	internal enum StatementCode:byte
	{
		Return,
		ReturnValue,
		Jump,
		ConditionalJump,
		EvaluateExpression,
		AssignVariable,
		AssignField,
		AssignValueInCollection,
		SetState,
		RegisterChoice,
		DisplayChoices,
		Say,
		GoTo,
		GiveItem,
		GiveGold
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

		internal ushort TableConstant(ILsnValue value)
		{
			if(ConstantTable.Contains(value))
				return (ushort)ConstantTable.IndexOf(value);
			ConstantTable.Add(value);
			return (ushort)(ConstantTable.Count - 1);
		}

		internal void Serialize(Stream stream)
		{
			using (var writer = new BinaryDataWriter(stream))
			{
				writer.Write((ushort)ConstantTable.Count);
				for (int i = 0; i < ConstantTable.Count; i++)
					ConstantTable[i].Serialize(writer);
			}
		}
	}
}

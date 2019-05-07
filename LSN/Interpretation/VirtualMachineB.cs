using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Interpretation
{
	struct InstructionB
	{
		public readonly OpCodeB Operation;
		public readonly short Data;

		internal InstructionB(OpCodeB op, short data) { Operation = op; Data = data; }
	}

	class FileData
	{
		public int[] Ints;
		public float[] Floats;
		public double[] Doubles;
		public StringValue[] Strings;
		public Vector2[] Vector2s;
		public LsnType[] Types;
	}

	class LsnVMStackB
	{
		public int GetI32(int i) => throw new NotImplementedException();
		public void SetI32(int i, int val) => throw new NotImplementedException();

		public float GetF32(int i) => throw new NotImplementedException();
		public void SetF32(int i, float val) => throw new NotImplementedException();

		public double GetF64(int i) => throw new NotImplementedException();
		public void SetF64(int i, double val) => throw new NotImplementedException();

		public Vector2 GetVec2(int i) => throw new NotImplementedException();
		public void SetVec2(int i, Vector2 val) => throw new NotImplementedException();

		public ILsnValue GetObj(int i) => throw new NotImplementedException();
		public void SetObj(int i, ILsnValue val) => throw new NotImplementedException();
	}

	static class Ext
	{
		internal static Vector2 GetVec2Val(this LsnValue self) => new Vector2(self.X, self.Y);
	}

	class VirtualMachineB
	{
		readonly Stack<int> IntStack = new Stack<int>();
		readonly Stack<float> FloatStack = new Stack<float>();
		readonly Stack<double> DoubleStack = new Stack<double>();
		readonly Stack<Vector2> Vec2Stack = new Stack<Vector2>();
		readonly Stack<ILsnValue> ObjStack = new Stack<ILsnValue>();

		LsnVMStackB Locals;
		FileData CurrentFileData;

		void PushTrue() => IntStack.Push(-1);
		void PushFalse() => IntStack.Push(0);

		void PushI32(int v) => IntStack.Push(v);
		void PushF32(float v) => FloatStack.Push(v);
		void PushF64(double v) => DoubleStack.Push(v);
		void PushVec2(Vector2 v) => Vec2Stack.Push(v);
		void PushVec2(float x, float y) => Vec2Stack.Push(new Vector2(x, y));
		void PushObj(ILsnValue v) => ObjStack.Push(v);

		int PopI32() => IntStack.Pop();
		float PopF32() => FloatStack.Pop();
		double PopF64() => DoubleStack.Pop();
		Vector2 PopVec2() => Vec2Stack.Pop();
		ILsnValue PopObj() => ObjStack.Pop();

		ScriptObject GetSelf() => Locals.GetObj(0) as ScriptObject;

		void Exec(InstructionB instruction)
		{
			var opCode = instruction.Operation;
			var data = instruction.Data;
			switch (opCode)
			{
				#region Arithmetic
				case OpCodeB.Add_I32:	PushI32(PopI32() + PopI32());		break;
				case OpCodeB.Add_F32:	PushF32(PopF32() + PopF32());		break;
				case OpCodeB.Add_F64:	PushF64(PopF64() + PopF64());		break;
				case OpCodeB.Add_Vec2:	PushVec2(PopVec2() + PopVec2());	break;

				case OpCodeB.Sub_I32:	PushI32(PopI32() - PopI32());		break;
				case OpCodeB.Sub_F32:	PushF32(PopF32() - PopF32());		break;
				case OpCodeB.Sub_F64:	PushF64(PopF64() - PopF64());		break;
				case OpCodeB.Sub_Vec2:	PushVec2(PopVec2() - PopVec2());	break;

				case OpCodeB.Mul_I32:	PushI32(PopI32() * PopI32());		break;
				case OpCodeB.Mul_F32:	PushF32(PopF32() * PopF32());		break;
				case OpCodeB.Mul_F64:	PushF64(PopF64() * PopF64());		break;
				case OpCodeB.Mul_Vec2:	PushVec2(PopVec2() * PopVec2());	break;

				case OpCodeB.Div_I32:	PushI32(PopI32() / PopI32());		break;
				case OpCodeB.Div_F64:	PushF32(PopF32() / PopF32());		break;
				case OpCodeB.Div_F32:	PushF64(PopF64() / PopF64());		break;
				case OpCodeB.Div_Vec2:	PushVec2(PopVec2() / PopVec2());	break;

				case OpCodeB.Rem_I32:	PushI32(PopI32() % PopI32());		break;

				case OpCodeB.Pow_F32:	PushF32((float)Math.Pow(PopF32(), PopF32()));	break;
				case OpCodeB.Pow_F64:	PushF64(Math.Pow(PopF64(), PopF64()));			break;

				case OpCodeB.Neg_I32:	PushI32(-PopI32());	break;
				case OpCodeB.Neg_F32:	PushF32(-PopF32());	break;
				case OpCodeB.Neg_F64:	PushF64(-PopF64());	break;
				#endregion
				#region Vec2
				case OpCodeB.Vec2_GetX:			PushF32(PopVec2().X);						break;
				case OpCodeB.Vec2_GetY:			PushF32(PopVec2().Y);						break;
				case OpCodeB.Construct_Vec2:		PushVec2(new Vector2(PopF32(), PopF32()));	break;
				case OpCodeB.DotProduct_Vec2:	PushF32(Vector2.Dot(PopVec2(), PopVec2()));	break;
				#endregion
				#region Strings
				case OpCodeB.Concat:
					{
						var a = (PopObj() as StringValue).Value;
						var b = (PopObj() as StringValue).Value;
						PushObj(new StringValue(a + b));
					}
					break;
				case OpCodeB.I32toStr:	PushObj(new StringValue(PopI32().ToString()));	break;
				case OpCodeB.F64toStr:	PushObj(new StringValue(PopF64().ToString()));	break;
				#endregion Strings
				#region Compare
				case OpCodeB.Lt_I32:
					{
						var a = PopI32(); var b = PopI32();
						if (a < b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Lt_F32:
					{
						var a = PopF32(); var b = PopF32();
						if (a < b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Lt_F64:
					{
						var a = PopF64(); var b = PopF64();
						if (a < b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Lte_I32:
					{
						var a = PopI32(); var b = PopI32();
						if (a <= b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Lte_F32:
					{
						var a = PopF32(); var b = PopF32();
						if (a <= b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Lte_F64:
					{
						var a = PopF64(); var b = PopF64();
						if (a <= b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Eq_I32:
					{
						var a = PopI32(); var b = PopI32();
						if (a == b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Eq_F32:
					{
						var a = PopF32(); var b = PopF32();
						if (a == b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Eq_F64:
					{
						var a = PopF64(); var b = PopF64();
						if (a == b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Eq_F32_epsilon:
				case OpCodeB.Eq_F64_epsilon:
					throw new NotImplementedException();
				case OpCodeB.Eq_Str:
					{
						var a = (PopObj() as StringValue).Value;
						var b = (PopObj() as StringValue).Value;
						if (a == b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Neq_I32:
					{
						var a = PopI32(); var b = PopI32();
						if (a != b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Neq_F32:
					{
						var a = PopF32(); var b = PopF32();
						if (a != b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Neq_F64:
					{
						var a = PopF64(); var b = PopF64();
						if (a != b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Neq_F32_epsilon:
				case OpCodeB.Neq_F64_epsilon:
					throw new NotImplementedException();
				case OpCodeB.Neq_Str:
					{
						var a = (PopObj() as StringValue).Value;
						var b = (PopObj() as StringValue).Value;
						if (a != b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Gte_I32:
					{
						var a = PopI32(); var b = PopI32();
						if (a >= b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Gte_F32:
					{
						var a = PopF32(); var b = PopF32();
						if (a >= b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Gte_F64:
					{
						var a = PopF64(); var b = PopF64();
						if (a >= b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Gt_I32:
					{
						var a = PopI32(); var b = PopI32();
						if (a > b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Gt_F32:
					{
						var a = PopF32(); var b = PopF32();
						if (a > b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.Gt_F64:
					{
						var a = PopF64(); var b = PopF64();
						if (a > b) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.NonNan_F64:
					{
						var x = PopF64();
						if (x == x) PushTrue();
						else PushFalse();
					}
					break;
				case OpCodeB.NonNull:
					{
						var x = PopObj();
						if (x != null) PushTrue();
						else PushFalse();
					}
					break;
				#endregion Compare
				#region Logic
				case OpCodeB.And:	PushI32(PopI32() & PopI32());	break;
				case OpCodeB.Or:		PushI32(PopI32() | PopI32());	break;
				case OpCodeB.Not:	PushI32(~PopI32());				break;
				#endregion
				#region Convert
				case OpCodeB.Conv_I32_F64: PushF64(PopI32());		break;
				case OpCodeB.Conv_I32_F32: PushF32(PopI32());		break;
				case OpCodeB.Conv_F32_F64: PushF64(PopF32());		break;
				case OpCodeB.Cast_F32_I32: PushI32((int)PopF32());	break;
				case OpCodeB.Cast_F64_I32: PushI32((int)PopF64());	break;
				case OpCodeB.Cast_F64_F32: PushF32((float)PopF64());	break;
				#endregion
				#region Jump
				case OpCodeB.Jump:
					break;
				case OpCodeB.Jump_True:
					break;
				case OpCodeB.Jump_Target:
					break;
				case OpCodeB.Set_Target:
					break;
				#endregion
				#region Call
				case OpCodeB.CallFn:
					break;
				case OpCodeB.CallMethod:
					break;
				case OpCodeB.CallScriptClassMethod:
					break;
				case OpCodeB.CallScriptClassVirtMethod:
					break;
				case OpCodeB.CallHostInterfaceMethod:
					break;
				#endregion
				#region Load Const
				case OpCodeB.LoadConst_I32:			PushI32(CurrentFileData.Ints[data]);		break;
				case OpCodeB.LoadConst_I32_Short:	PushI32(data);								break;
				case OpCodeB.LoadConst_F32:			PushF32(CurrentFileData.Floats[data]);		break;
				case OpCodeB.LoadConst_F64:			PushF64(CurrentFileData.Doubles[data]);		break;
				case OpCodeB.LoadConst_Vec2:			PushVec2(CurrentFileData.Vector2s[data]);	break;
				case OpCodeB.LoadConst_I32_0:		PushI32(0);									break;
				case OpCodeB.LoadConst_I32_Neg1:		PushI32(-1);								break;
				case OpCodeB.LoadConst_Str:			PushObj(CurrentFileData.Strings[data]);		break;
				case OpCodeB.LoadConst_Null:			PushObj(null);								break;
				case OpCodeB.LoadConst_NaN_F64:		PushF64(double.NaN);						break;
				#endregion
				case OpCodeB.Load_UniqueScriptClass: throw new NotImplementedException();
				#region Load Local
				case OpCodeB.LoadLocal_I32:	PushI32(Locals.GetI32(data));			break;
				case OpCodeB.LoadLocal_F32:	PushF32(Locals.GetF32(data));			break;
				case OpCodeB.LoadLocal_F64:	PushF64(Locals.GetF64(data));			break;
				case OpCodeB.LoadLocal_Vec2:	PushVec2(Locals.GetVec2(data));			break;
				case OpCodeB.LoadLocal_Obj:	PushObj(Locals.GetObj(data));			break;
				#endregion
				#region Store Local
				case OpCodeB.StoreLocal_I32:		Locals.SetI32(data, PopI32());		break;
				case OpCodeB.StoreLocal_F32:		Locals.SetF32(data, PopF32());		break;
				case OpCodeB.StoreLocal_F64:		Locals.SetF64(data, PopF64());		break;
				case OpCodeB.StoreLocal_Vec2:	Locals.SetVec2(data, PopVec2());	break;
				case OpCodeB.StoreLocal_Obj:		Locals.SetObj(data, PopObj());		break;
				#endregion
				#region Load Element
				case OpCodeB.LoadElem_I32:
					{
						var col = PopObj() as ICollectionValue;
						var val = col.GetValue(PopI32());
						PushI32(val.IntValue);
					}
					break;
				case OpCodeB.LoadElem_F32:
					{
						var col = PopObj() as ICollectionValue;
						var val = col.GetValue(PopI32());
						PushF32((float)val.DoubleValue);
					}
					break;
				case OpCodeB.LoadElem_F64:
					{
						var col = PopObj() as ICollectionValue;
						var val = col.GetValue(PopI32());
						PushF64(val.DoubleValue);
					}
					break;
				case OpCodeB.LoadElem_Vec2:
					{
						var col = PopObj() as ICollectionValue;
						var val = col.GetValue(PopI32());
						PushVec2(new Vector2(val.X, val.Y));
					}
					break;
				case OpCodeB.LoadElem_Obj:
					{
						var col = PopObj() as ICollectionValue;
						var val = col.GetValue(PopI32());
						PushObj(val.Value);
					}
					break;
				#endregion
				#region Store Element
				case OpCodeB.StoreElem_I32:
					{
						var col = PopObj() as LsnList;
						col[PopI32()] = new LsnValue(PopI32());
					}
					break;
				case OpCodeB.StoreElem_F32:
					{
						var col = PopObj() as LsnList;
						col[PopI32()] = new LsnValue(PopF32());
					}
					break;
				case OpCodeB.StoreElem_F64:
					{
						var col = PopObj() as LsnList;
						col[PopI32()] = new LsnValue(PopF64());
					}
					break;
				case OpCodeB.StoreElem_Vec2:
					{
						var col = PopObj() as LsnList;
						var v = PopVec2();
						col[PopI32()] = new LsnValue(v.X, v.Y);
					}
					break;
				case OpCodeB.StoreElem_Obj:
					{
						var col = PopObj() as LsnList;
						col[PopI32()] = new LsnValue(PopObj());
					}
					break;
				#endregion
				#region LSN
				case OpCodeB.GoTo:		// TBD
					break;
				case OpCodeB.Say:		// Pop 3 objects, send to LSN Runtime
					break;
				case OpCodeB.RegisterChoice:	// Pop 1 int & 1 object, send to LSN Runtime
					break;
				case OpCodeB.CallChoice:		// Call fn in LSN Runtime, Target = result, jump to target.
					break;
				#endregion
				#region Script Class 
				// Using 'Locals.GetObj(0)' b/c strict encapsulation is enforced.
				// Data is index of type.
				case OpCodeB.Construct_SC:		throw new NotImplementedException();
				case OpCodeB.SetState:			GetSelf().SetState(data);									break;
				case OpCodeB.LoadSCField_I32:	PushI32(GetSelf().GetFieldValue(data).IntValue);			break;
				case OpCodeB.LoadSCField_F32:	PushF32((float)GetSelf().GetFieldValue(data).DoubleValue);	break;
				case OpCodeB.LoadSCField_F64:	PushF64(GetSelf().GetFieldValue(data).DoubleValue);			break;
				case OpCodeB.LoadSCField_Vec2:	PushVec2(GetSelf().GetFieldValue(data).GetVec2Val());		break;
				case OpCodeB.LoadSCField_Obj:	PushObj(GetSelf().GetFieldValue(data).Value);				break;
				case OpCodeB.SetSCField_I32:		GetSelf().SetFieldValue(data, new LsnValue(PopI32()));		break;
				case OpCodeB.SetSCField_F32:		GetSelf().SetFieldValue(data, new LsnValue(PopF32()));		break;
				case OpCodeB.SetSCField_F64:		GetSelf().SetFieldValue(data, new LsnValue(PopF64()));		break;
				case OpCodeB.SetSCField_Obj:
					break;
				case OpCodeB.SetSCField_Vec2:
					{
						var v = PopVec2();
						GetSelf().SetFieldValue(data, new LsnValue(v.X, v.Y));
					}
					break;
				#endregion
				#region List
				case OpCodeB.Construct_List_I32:
					break;
				case OpCodeB.Construct_List_F32:
					break;
				case OpCodeB.Construct_List_F64:
					break;
				case OpCodeB.Construct_List_Vec2:
					break;
				// Data is index of type.
				case OpCodeB.Construct_List_Obj:
					break;
				#endregion
				#region Records
				// Data is index of type.
				case OpCodeB.Construct_Record:
					break;
				case OpCodeB.LoadRecField_I32:	PushI32((PopObj() as RecordValue).GetFieldValue(data).IntValue);			break;
				case OpCodeB.LoadRecField_F32:	PushF32((float)(PopObj() as RecordValue).GetFieldValue(data).DoubleValue);	break;
				case OpCodeB.LoadRecField_F64:	PushF64((PopObj() as RecordValue).GetFieldValue(data).DoubleValue);			break;
				case OpCodeB.LoadRecField_Vec2:
					break;
				case OpCodeB.LoadRecField_Obj:
					break;
				#endregion
				#region Structs
				// Data is index of type.
				case OpCodeB.Construct_Struct:
					break;
				case OpCodeB.Copy_Struct:
					break;
				case OpCodeB.LoadStructField_I32:
					break;
				case OpCodeB.LoadStructField_F32:
					break;
				case OpCodeB.LoadStructField_F64:
					break;
				case OpCodeB.LoadStructField_Vec2:
					break;
				case OpCodeB.LoadStructField_Obj:
					break;
				#endregion
				case OpCodeB.Ret:
					break;
				default:
					break;
			}
		}

	}
}

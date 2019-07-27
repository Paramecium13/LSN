namespace LsnCore
{
	// Operate on a stack machine...
	// Bools are ints...
	//		(Trinary value (F64): 1.0, 0.0, Nan)
	enum OpCodeB : ushort
	{
		// Most of these pop two args from the stack and push the result.
		// Both have to be of the same type.
		#region Arithmetic
		Add_I32,
		Add_F32,
		Add_F64,
		Add_Vec2,

		Sub_I32,
		Sub_F32,
		Sub_F64,
		Sub_Vec2,

		Mul_I32,
		Mul_F32,
		Mul_F64,
		/// <summary>Element multiplication</summary>
		Mul_Vec2,

		Div_I32,
		Div_F64,
		Div_F32,
		/// <summary>Element division</summary>
		Div_Vec2,

		/// <summary>Remainder</summary>
		Rem_I32,

		Pow_F32,
		Pow_F64,

		// Unary - operator...
		Neg_I32,
		Neg_F32,
		Neg_F64,
		#endregion
		#region Vec2
		Vec2_GetX,
		Vec2_GetY,
		Construct_Vec2,
		DotProduct_Vec2,
		#endregion
		#region String Arithmetic
		Concat,
		I32toStr,
		F64toStr,
		// I32 formated to string?
		// F64 formated to string?
		#endregion
		#region Compare
		Lt_I32,
		Lt_F32,
		Lt_F64,

		Lte_I32,
		Lte_F32,
		Lte_F64,

		Eq_I32,
		Eq_F32,
		Eq_F64,
		Eq_F32_epsilon,
		Eq_F64_epsilon,
		Eq_Str,

		Neq_I32,
		Neq_F32,
		Neq_F64,
		Neq_F32_epsilon,
		Neq_F64_epsilon,
		Neq_Str,

		Gte_I32,
		Gte_F32,
		Gte_F64,

		Gt_I32,
		Gt_F32,
		Gt_F64,

		NonNan_F64,
		NonNull,		
		#endregion
		#region Logic
		And,
		Or,
		/*Xor,
		Nor,
		Nand,*/
		Not,
		#endregion
		// Terminology: Conv -> up cast, Cast -> down cast
		#region Convert
		/// <summary>Convert an I32 to an F64</summary>
		Conv_I32_F64,
		Conv_I32_F32,
		Conv_F32_F64,

		Cast_F32_I32,
		Cast_F64_I32,
		Cast_F64_F32,
		#endregion
		#region Jump/branch
		/// <summary>Unconditional jump. Arg:[address]</summary>
		Jump,
		/// <summary>Jump if true. Arg:[address]</summary>
		Jump_True,
		/// <summary>Jump to target</summary>
		Jump_Target,
		Set_Target,
		#endregion		
		// the args should be on the eval stack...
		#region Call
		CallFn,
		CallMethod,
		CallScriptClassMethod,
		CallScriptClassVirtMethod,
		CallHostInterfaceMethod,
		// TBD!!!
		#endregion
		// These instructions push a constant onto the stack
		#region Load Const
		LoadConst_I32,
		LoadConst_I32_Short,
		LoadConst_F32,
		LoadConst_F64,
		LoadConst_Vec2,
		LoadConst_I32_0,
		LoadConst_I32_Neg1, //(True)
		/*LoadConst_F32_0,
		LoadConst_F64_0,
		LoadConst_Vec2_UnitX,
		LoadConst_I32_1,
		LoadConst_F32_1,
		LoadConst_F64_1,
		LoadConst_Vec2_UnitY,*/
		//LoadConst_Vec3,
		LoadConst_Str,
		LoadConst_Null,
		LoadConst_NaN_F64,
		#endregion
		Load_UniqueScriptClass,
		// All these have an argument of the local index...
		#region Load Local
		LoadLocal_I32,
		LoadLocal_F32,
		LoadLocal_F64,
		LoadLocal_Vec2,
		//LoadLocal_Str,
		LoadLocal_Obj,
		#endregion
		#region Store Local
		StoreLocal_I32,
		StoreLocal_F32,
		StoreLocal_F64,
		StoreLocal_Vec2,
		//StoreLocal_Str,
		StoreLocal_Obj,
		#endregion
		// Stack:
		// array, index -> element
		#region Load Element
		LoadElem_I32,
		LoadElem_F32,
		LoadElem_F64,
		LoadElem_Vec2,
		//LoadElem_Str,
		LoadElem_Obj,
		#endregion
		#region Store Element
		StoreElem_I32,
		StoreElem_F32,
		StoreElem_F64,
		StoreElem_Vec2,
		//StoreElem_Str,
		StoreElem_Obj,
		#endregion
		#region LSN
		GoTo,
		Say,
		RegisterChoice,
		CallChoice,
		#endregion

		#region Script Class
		SetState,

		Construct_SC,

		LoadSCField_I32,
		LoadSCField_F32,
		LoadSCField_F64,
		LoadSCField_Vec2,
		LoadSCField_Obj,

		SetSCField_I32,
		SetSCField_F32,
		SetSCField_F64,
		SetSCField_Vec2,
		SetSCField_Obj,
		#endregion

		#region Lists
		Construct_List_I32,
		Construct_List_F32,
		Construct_List_F64,
		Construct_List_Vec2,
		Construct_List_Obj,
		#endregion

		#region Records
		Construct_Record,

		LoadRecField_I32,
		LoadRecField_F32,
		LoadRecField_F64,
		LoadRecField_Vec2,
		LoadRecField_Obj,
		#endregion

		#region Structs
		Construct_Struct,
		
		Copy_Struct,

		LoadStructField_I32,
		LoadStructField_F32,
		LoadStructField_F64,
		LoadStructField_Vec2,
		LoadStructField_Obj,
		#endregion

		#region Return
		Ret,
		#endregion
	}
}
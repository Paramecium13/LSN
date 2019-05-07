using System.Runtime.InteropServices;

namespace LsnCore
{
	enum OpCode : ushort
	{
		Nop,
		#region Arithmetic
		// don't need type conversions b/c i32 is stored as a f64...
		Add,
		Sub,
		Mul_I32,
		Mul_F64,
		Div_I32,
		Div_F64,

		/// <summary>Remainder</summary>
		Rem,
		Pow_F64,

		// Unary - operator...
		Neg,
		#endregion
		#region String Arithmetic
		Concat,
		IntToString,
		#endregion
		#region Compare
		Eq_I32,
		Eq_F64,
		Eq_Str,

		Neq_I32,
		Neq_F64,
		//Neq_F64_epsilon,
		Neq_Str,
		//NonNan_F64,
		NonNull,

		Lt,
		Lte,
		Gte,
		Gt,
		#endregion
		// , start,end -> ,range
		MakeRange,
		#region Logic
		And,
		Or,
		Not,
		#endregion
		#region Convert
		Conv_I32_F64,
		#endregion
		#region Jump/Branch
		Jump,
		Jump_True,
		Jump_Target,
		Set_Target,
		#endregion
		#region Call
		// Data is index. Places index into a temp register(not preserved when calling).
		// Used by instructions that need two indexes.
		LoadIndex,
		// index of file loaded by LoadIndex instruction, index of function in file is data
		CallFn,
		// index of file is first byte of data, index of function in file is second byte in data
		CallFn_Short,
		// file is the current file, index of fn is data.
		CallFn_Local,
		/// <summary>
		/// Index of method name is data
		/// </summary>
		CallMethod,
		/*// index of type loaded by LoadIndex. Index of method is data
		CallMethod,
		// index of type is first byte of data, index of method is second byte of data
		CallMethod_Short,*/

		// index of method name is data
		CallHostInterfaceMethod,
		#endregion
		Ret,
		#region Load Const
		LoadConst_I32_short,
		LoadConst_I32,
		LoadConst_F64_short,
		LoadConst_F64,
		LoadConst_Obj,
		LoadConst_Nil,
		#endregion
		// data is index of type id...
		Load_UniqueScriptClass,

		#region Variables
		// data is index
		LoadLocal,
		StoreLocal,
		#endregion

		#region Vectors and Lists
		// ,collection, index -> value
		LoadElement,
		// for lists of a struct type, loading an element but not storing it doesn't copy it...
		//		e.g. in 'ls[0].Foo = 1;', the expression 'ls[0]' does not make a copy.

		// ,collection, index, value -> ,
		StoreElement,
		// data is index of type?
		ConstructList,
		// data is number of values on the eval stack to put into the list
		InitializeList,
		// data is number of values on the eval stack to put into the vector
		InitializeVector,
		#endregion
		LoadField,
		StoreField,
		ConstructStruct,

		/// <summary>
		/// Placed after LoadField for struct fields of a record. Used when a struct is an R-value.
		/// Also placed before StoreLocal when the variable is a struct and before return when it returns a struct and this is needed.
		/// </summary>
		CopyStruct,
		ConstructRecord,

		#region ScriptClass
		// Data is state; local[0] is script class.
		SetState,
		// Stack: ,[args] -> ,script object
		ConstructScriptClass,
		// Stack: ,host,[args] -> ,script object
		ConstructAndAttachScriptClass,
		Detach,
		// see: HostInterfaceAccessExpression
		GetHost,
		#endregion
		#region LSN
		GoTo,
		ComeFrom,
		Say,
		RegisterChoice,
		CallChoice,
		GiveItem,
		GiveGold,
		#endregion
		Error,

		CRN,
		HCF = 0xF00F
	}

	[StructLayout(LayoutKind.Explicit)]
	struct Instruction
	{
		[FieldOffset(0)]
		public readonly OpCode OpCode;
		[FieldOffset(2)]
		public readonly short Data;
		[FieldOffset(2)]
		public readonly ushort Index;
		public Instruction(OpCode opCode, short data) { OpCode = opCode; Index = 0; Data = data; }
		public Instruction(OpCode opCode, ushort index) { OpCode = opCode; Data = 0; Index = index; }
	}
}
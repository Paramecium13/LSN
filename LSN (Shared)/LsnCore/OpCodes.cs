using System.Runtime.InteropServices;

namespace LsnCore
{

	/*
	 * File Structure Proposal
	 *	Header:
	 *		Info about this file...
	 *	Data Section (May be shared by multiple files to save space...?):
	 *		Id Strings segment:
	 *			...
	 *		Constant Pools:
	 *			WORD Pool:
	 *				...
	 *			LONG Pool:
	 *				...
	 *			String Pool:
	 *				...
	 *	Link Section
	 *		Usings Segment:
	 *			Index of name in Id Strings segment
	 *		Used Types Segment:
	 *			Index of containing file + 2 (0 is system, 1 is this file)
	 *			Index of name in Id Strings segment
	 *			Type of type [enum...]
	 *			Number of generics
	 *			generics[number of generics]:
	 *				index of generic parameter in used types segment.
	 *			...
	 *	Info Section
	 *		Non-Local Procedure Stubs:
	 *			Index of type in types segment
	 *			Index of name in Id Strings segment
	 *		Exported Types Segment:
	 *			Index of entry in used types segment...?
	 *			...
	 *		Procedures Segment:
	 *			Index of name in id strings segment
	 *			stack size
	 *			number of parameters
	 *			parameters:
	 *				Index of name
	 *				Index of type
	 *				Usage info bit-flags?
	 *			index of return type
	 *			offset of first instruction in code segment
	 *			attributes:
	 *				???
	 *		Local Types Segment?:
	 *			...
	 *	Code Section
	 */


	enum OpCode : ushort
	{
		#region Basic
		Nop,
		/// <summary>Duplicate the value on top of the stack.</summary>
		Dup,
		/// <summary> ,b,a -> ,a,b</summary>
		Swap,
		/// <summary> ,b,a -> ,a,b,a</summary>
		SwaDup,
		/// <summary>Pop and forget</summary>
		Pop,
		#endregion
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
		Neq_Str,

		/// <summary>
		/// if data != 0, data is 1 + index of epsilon in the file's F64 const storage. Otherwise use default System.Double.EPSILON.
		/// </summary>
		Eq_F64_epsilon,
		/// <summary>
		/// if data != 0, data is 1 + index of epsilon in the file's F64 const storage. Otherwise use default System.Double.EPSILON.
		/// </summary>
		Neq_F64_epsilon,


		//NonNan_F64,
		NonNull,
		/// <summary>Doesn't pop from the eval stack.</summary>
		NonNull_NoPop,

		Lt,
		Lte,
		Gte,
		Gt,
		#endregion
		/// <summary>,end, start -> ,range</summary>
		MakeRange,
		#region Logic
		And,
		Nand,
		Or,
		Xor,
		Nor,
		Xnor,
		Not,
		#endregion
		#region Convert
		//Conv_I32_F64,
		#endregion
		#region Jump/Branch
		/// <summary>Unconditional jump. Target is data</summary>
		Jump,
		/// <summary>Conditional jump. Target is data</summary>
		Jump_True,
		Jump_False,

		//https://llvm.org/docs/LangRef.html#switch-instruction
		/// <summary>Not yet implemented. </summary>
		Switch,

		/// <summary>Unconditional jump to Target</summary>
		JumpToTarget,
		/// <summary>Set Target register to data</summary>
		SetTarget,
		#endregion
		#region Call
		/// <summary>Data is index. Places index into a temp register(not preserved when calling).Used by instructions that need two indexes.</summary>
		LoadIndex,

		/* Two potential styles of non-local procedure indexing:
		 *	(1): Index of file containing procedure is loaded by LoadIndex, index of procedure name is data
		 * 
		 *	(2): Data is index of procedure stub. Procedure stubs are stored in the file, they contain:
		 *			* The index of the file containing the procedure.
		 *			* The name of the procedure.
		 *			* The offset of that procedure in its file's code segment. This is resolved at load time or when it is first called.
		 */

		/// <summary>index of file loaded by LoadIndex instruction, index of function name is data</summary>
		CallFn,
		/// <summary>index of file is first byte of data, index of function name is second byte in data</summary>
		CallFn_Short,
		/// <summary>file is the current file, index of fn is data.</summary>
		CallFn_Local,

		/// <summary>
		/// Loaded index is index of type. Data is index of method name. Not for int, double or bool! Also not for methods that have corresponding instructions...
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		CallNativeMethod,
		/// <summary>
		/// Loaded index is index of type. Data is index of method name.
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		CallLsnMethod,
		/// <summary>
		/// Call ScriptObject method. Loaded index is index of type. Data is index of method name.
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		CallScObjMethod,
		/// <summary>
		/// Call virtual ScriptObject method. Loaded index is index of type. Data is index of method name.
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		CallScObjVirtualMethod,
		/*/// <summary>
		/// Loaded index is index of type. Data is index of method name.
		/// </summary>
		CallMethod,*/

		/// <summary>
		/// Data is index of method name.
		/// {, arg_0,..., arg_N, object -> , result (if it returns a value) }
		/// </summary>
		CallHostInterfaceMethod,
		#endregion
		Ret,
		#region Load Const
		/// <summary> </summary>
		LoadConst_I32_short,
		/// <summary> </summary>
		LoadConst_I32,
		/// <summary> </summary>
		LoadConst_F64,
		/// <summary> </summary>
		LoadConst_F64_short,
		/// <summary> Push (index) /((double)ushort.MaxValue)</summary>
		LoadConst_F64_ShortRatio,
		/// <summary> </summary>
		LoadConst_Obj,
		/// <summary> </summary>
		LoadConst_Nil,
		#endregion
		// data is index of type id...
		Load_UniqueScriptClass,

		#region Variables
		// data is index
		/// <summary> </summary>
		LoadLocal_0,
		LoadLocal,
		/// <summary> </summary>
		StoreLocal,
		#endregion

		#region Vectors and Lists
		/// <summary>,index, collection -> ,value</summary>
		LoadElement,
		// for lists of a struct type, loading an element but not storing it doesn't copy it...
		//		e.g. in 'ls[0].Foo = 1;', the expression 'ls[0]' does not make a copy.

		/// <summary>, value, index, collection -> ,</summary>
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
		/// <summary>
		/// Data is index of type.
		/// {, arg_0,..., arg_N -> , struct }
		/// </summary>
		ConstructStruct,

		/// <summary>
		/// Placed after LoadField for struct fields of a record. Used when a struct is an R-value.
		/// Also placed before StoreLocal when the variable is a struct and before return when it returns a struct and this is needed.
		/// </summary>
		CopyStruct,
		/// <summary>Data is number of fields.</summary>
		FreeStruct,
		/// <summary>Data is index of type.</summary>
		ConstructRecord,
		/// <summary>Data is number of fields.</summary>
		FreeRecord,
		#region ScriptClass
		/// <summary>Data is state; local[0] is script class.</summary>
		SetState,
		/// <summary>
		/// Call the constructor for the script class
		/// Stack: , arg_0, ..., arg_N, (host) -> ,script object
		/// </summary>
		ConstructScriptClass,

		/// <summary>Called in the beginning of the constructor... Puts it in stack[0]</summary>
		CreateScriptClass,
		/// <summary>Called in the beginning of the constructor... Puts it in stack[0]</summary>
		CreateAndAttachScriptClass,
		/// <summary>Called at the end of the constructor of a script class that listens to its hosts events...</summary>
		RegisterScriptObjectForEvents,
			// This happens at the end of the constructor so that all of the script object's fields are initialized before any of its code is run.

		/// <summary>Local[0] is script class.</summary>
		Detach,
		/// <summary>Local[0] is script class.</summary>
		GetHost, // see: HostInterfaceAccessExpression
		#endregion
		#region LSN
		GoTo,
		ComeFrom,
		Say,
		/// <summary>Instruction index is data...</summary>
		RegisterChoice,
		/// <summary>...</summary>
		RegisterChoice_Pop,
		CallChoices,
		/// <summary>Call choice but instead of jumping, push result onto stack.</summary>
		CallChoices_Push, // Maybe use same OpCode as CallChoices but depend on data...
		GiveItem,
		GiveGold,
		#endregion
		#region Input
		ReadString,
		ReadInt,
		ReadDouble,
		#endregion
		#region Random
		Srand,
		/// <summary>Set PRNG seed to pseudo-random value, e.g. system time (in ticks).</summary>
		Srand_sysTime,
		Rand,
		RandInt,
		#endregion
		#region Debug
		Error,
		// LSNr cannot make sure that host interface methods actually return what they say they do.
		// These runtime checks can be placed after a host interface call to make sure it returned
		// what it is supposed to.
		AssertHostReturnIs_I32, // Or bool
		AssertHostReturnIs_F64,
		// Data is index of type.
		AssertHostReturnIs_Type,
		AssertHostReturnIs_Option_I32,
		AssertHostReturnIs_Option_F64,
		// Data is index of type.
		AssertHostReturnIs_Option_Type,
		#endregion
		#region More Math
		/// <summary>,a,b -> Min(a,b)</summary>
		Min,
		/// <summary>,a,b -> Max(a,b)</summary>
		Max,
		Floor,
		Ceil,
		Round,
		Abs,
		Sqrt,
		Sin,
		Cos,
		Tan,
		ASin,
		ACos,
		ATan,

		/// <summary>Increment var and push onto stack {++num}</summary>
		PreInc_Var,
		/// <summary>Increment element and push onto stack {++nums[i]}</summary>
		PreInc_Elem,
		/// <summary>Increment field and push onto stack {++foo.num}</summary>
		PreInc_Fld,

		/// <summary>Push var onto stack then increment {num++}</summary>
		PostInc_Var,
		/// <summary>Push element onto stack then increment {nums[i]++}</summary>
		PostInc_Elem,
		/// <summary>Push field onto stack then increment {foo.num++}</summary>
		PostInc_Fld,

		/// <summary>Decrement var and push onto stack {--num}</summary>
		PreDec_Var,
		/// <summary>Decrement element and push onto stack {--nums[i]}</summary>
		PreDec_Elem,
		/// <summary>Decrement field and push onto stack {--foo.num}</summary>
		PreDec_Fld,

		/// <summary>Push var onto stack then decrement {num--}</summary>
		PostDec_Var,
		/// <summary>Push element onto stack then increment {nums[i]--}</summary>
		PostDec_Elem,
		/// <summary>Push field onto stack then decrement {foo.num++--}</summary>
		PostDec_Fld,
		#endregion

		#region WORDS
		GetWord,
		// ,word -> ,word
		DECL,
		// ,word, word -> ,word
		DECL_LIKE,
		// ,word -> ,word
		CONJ,
		// ,word, word -> ,word
		CONJ_LIKE,
		#endregion

		// I might not implement these. Just thinking about it.
		#region Registers?
		/// <summary>$acc = 0</summary>
		SetAcc_0,
		/// <summary>$acc = pop</summary>
		SetAcc,
		/// <summary>$acc += pop</summary>
		AddAcc,// Maybe the above 3 can share same insruction but differ on data.
		/// <summary>Push $acc</summary>
		PushAcc,
		#endregion

		CRN,
		CFRN,
		HCF = 0xF00F
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Instruction
	{
		[FieldOffset(0)]
		internal readonly OpCode OpCode;
		[FieldOffset(2)]
		internal readonly short Data;
		[FieldOffset(2)]
		internal readonly ushort Index;
		internal Instruction(OpCode opCode, short data) { OpCode = opCode; Index = 0; Data = data; }
		internal Instruction(OpCode opCode, ushort index) { OpCode = opCode; Data = 0; Index = index; }
	}
}
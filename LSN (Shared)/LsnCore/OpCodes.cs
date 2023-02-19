using System;
using System.Runtime.InteropServices;

namespace LsnCore
{

	/*
	 * File Structure Proposal
	 *	Header:
	 *		Info about this file...
	 *	Data Section (May be shared by multiple files to save space...?):
	 *		Constant Pools:
	 *			LONG (8 bytes) Pool [Indexed]:
	 *				...
	 *			String Pool [Indexed]:
	 *				...
	 *	Link Section
	 *		Usings Segment [Indexed?]:
	 *			Name
	 *		Used Types Segment [Indexed]:
	 *			Index of containing file + 1 (0 is system)
	 *			Name
	 *			Type of type [enum...]
	 *			Number of generics
	 *			generics[number of generics]:
	 *				index of generic parameter in used types segment.
	 *			...
	 *			{During loading, the values in this are resolved to TypeIds}
	 *		Id Strings segment [Indexed]:
	 *			...
	 *	Info Section
	 *		Defined Type Ids [Indexed]
	 *			Name of type
	 *		Non-Local Procedure Stubs:
	 *			Index of Type in used types segment
	 *			Name
	 *		Exported Types Segment:
	 *			Index of entry in used types segment...?
	 *			...
	 *		Procedures Segment:
	 *			Index of name in id strings segment
	 *			stack size
	 *			number of parameters
	 *			parameters:
	 *				Name
	 *				Index of type [Type Index: A negative value indicates it is a locally defined type]
	 *				Usage info bit-flags?
	 *			index of return type
	 *			offset of first instruction in code segment
	 *			attributes:
	 *				???
	 *		Local Types Segment?:
	 *			...
	 *	Code Section
	 */


	public enum OpCode : ushort
	{
		#region Basic
		[OpCodeInfo("NOP","Basic",0,0)]
		Nop,
		/// <summary> Line number... </summary>
		[OpCodeInfo("DEBUG.LineNum", "Basic", 0, 0)]
		Line,
		/// <summary>Duplicate the value on top of the stack.</summary>
		[OpCodeInfo("DUP","Basic.Stack",0,1)]
		Dup,
		/// <summary> ,b,a -> ,a,b</summary>
		[OpCodeInfo("SWAP", "Basic.Stack", 0, 0)]
		Swap,
		/// <summary> ,b,a -> ,a,b,a</summary>
		// ReSharper disable once StringLiteralTypo
		[OpCodeInfo("SWADUP","Basic.Stack", 0, 1)]
		SwaDup,
		/// <summary>Pop and forget</summary>
		[OpCodeInfo("POP", "Basic.Stack", 1, 0)]
		Pop,
		#endregion
		#region Arithmetic
		// don't need type conversions b/c i32 is stored as a f64...
		[OpCodeInfo("ADD", "Arithmetic", 2,1)]
		Add,
		[OpCodeInfo("SUB", "Arithmetic", 2,1)]
		Sub,
		//Mul_I32,
		[OpCodeInfo("MUL", "Arithmetic", 2, 1)]
		Mul,
		[OpCodeInfo("DIV.I32", "Arithmetic", 2, 1)]
		Div_I32,

		/// <summary>
		/// , a, b -> , a/b
		/// </summary>
		[OpCodeInfo("DIV.F64", "Arithmetic", 2, 1)]
		Div_F64,

		/// <summary>Remainder</summary>
		[OpCodeInfo("REM.I32", "Arithmetic", 2, 1)]
		Rem_I32,
		[OpCodeInfo("REM.F64", "Arithmetic", 2, 1)]
		Rem_F64,
		[OpCodeInfo("POW", "Arithmetic", 2, 1)]
		Pow,

		// Unary - operator...
		[OpCodeInfo("NEG", "Arithmetic", 1, 1)]
		Neg,
		#endregion
		#region INC and DEC
		/// <summary>Increment var and push onto stack {++num}</summary>
		[OpCodeInfo("INC.Pre.Var", "Inc & Dec", 0,1)]
		PreInc_Var,
		/// <summary>Increment element and push onto stack {++nums[i]}</summary>
		[OpCodeInfo("INC.Pre.Elem", "Inc & Dec", 1,1)]
		PreInc_Elem,
		/// <summary>Increment field and push onto stack {++foo.num}</summary>
		[OpCodeInfo("INC.Pre.Fld", "Inc & Dec", 1, 1)]
		PreInc_Fld,

		/// <summary>Push var onto stack then increment {num++}</summary>
		[OpCodeInfo("INC.Post.Var", "Inc & Dec", 0, 1)]
		PostInc_Var,
		/// <summary>Push element onto stack then increment {nums[i]++}</summary>
		[OpCodeInfo("INC.Post.Elem", "Inc & Dec", 1, 1)]
		PostInc_Elem,
		/// <summary>Push field onto stack then increment {foo.num++}</summary>
		[OpCodeInfo("INC.Post.Fld", "Inc & Dec", 1, 1)]
		PostInc_Fld,

		/// <summary>Decrement var and push onto stack {--num}</summary>
		[OpCodeInfo("Dec.Pre.Var", "Inc & Dec", 0,1)]
		PreDec_Var,
		/// <summary>Decrement element and push onto stack {--nums[i]}</summary>
		[OpCodeInfo("Dec.Pre.Elem", "Inc & Dec", 1, 1)]
		PreDec_Elem,
		/// <summary>Decrement field and push onto stack {--foo.num}</summary>
		[OpCodeInfo("Dec.Pre.Fld", "Inc & Dec", 1, 1)]
		PreDec_Fld,

		/// <summary>Push var onto stack then decrement {num--}</summary>
		[OpCodeInfo("Dec.Post.Var", "Inc & Dec", 0, 1)]
		PostDec_Var,
		/// <summary>Push element onto stack then increment {nums[i]--}</summary>
		[OpCodeInfo("Dec.Post.Elem", "Inc & Dec", 1, 1)]
		PostDec_Elem,
		/// <summary>Push field onto stack then decrement {foo.num++--}</summary>
		[OpCodeInfo("Dec.Post.Fld", "Inc & Dec", 1, 1)]
		PostDec_Fld,
		#endregion
		#region Logic
		[OpCodeInfo("AND","Logic", 2,1)]
		And,
		//Nand,
		[OpCodeInfo("Or","Logic", 2,1)]
		Or,
		//Xor,
		//Nor,
		//Xnor,
		[OpCodeInfo("NOT","Logic",1,1)]
		Not,
		#endregion
		#region Convert
		//Conv_I32_F64,
		#endregion
		#region String Arithmetic
		[OpCodeInfo("CONCAT","String",2,1)]
		Concat,
		[OpCodeInfo("I32.To.Str", "String",2,1)]
		IntToString,
		/// <summary>
		/// Repeat a string several times.
		/// </summary>
		[OpCodeInfo("STR.MULT", "String",2,1)]
		StrMult,
		[OpCodeInfo("STR.Len", "String", 1,1)]
		StringLength,
		#endregion
		#region Compare
		[OpCodeInfo("EQ.I32", "Compare", 2, 1)]
		Eq_I32,
		[OpCodeInfo("EQ.F64", "Compare", 2, 1)]
		Eq_F64,
		[OpCodeInfo("EQ.Str", "Compare", 2, 1)]
		Eq_Str,
		[OpCodeInfo("EQ.Obj", "Compare", 2, 1)]
		Eq_Obj,

		[OpCodeInfo("NEQ.I32", "Compare", 2, 1)]
		Neq_I32,
		[OpCodeInfo("NEQ.F64", "Compare", 2, 1)]
		Neq_F64,
		[OpCodeInfo("NEQ.Str", "Compare", 2, 1)]
		Neq_Str,
		[OpCodeInfo("NEQ.Obj", "Compare", 2, 1)]
		Neq_Obj,

		/// <summary>
		/// if data != 0, data is 1 + index of epsilon in the file's F64 const storage. Otherwise use default System.Double.EPSILON.
		/// </summary>
		//Eq_F64_epsilon,
		/// <summary>
		/// if data != 0, data is 1 + index of epsilon in the file's F64 const storage. Otherwise use default System.Double.EPSILON.
		/// </summary>
		//Neq_F64_epsilon,


		//NonNan_F64,
		[OpCodeInfo("NOTNULL", "Compare", 1, 1)]
		NonNull,
		/// <summary>Doesn't pop from the eval stack.</summary>
		[OpCodeInfo("NOTNULL.NoPop", "Compare", 0, 0)]
		NonNull_NoPop,
		[OpCodeInfo("LT", "Compare", 2,1)]
		Lt,
		[OpCodeInfo("LTE", "Compare", 2, 1)]
		Lte,
		[OpCodeInfo("GT", "Compare", 2, 1)]
		Gt,
		[OpCodeInfo("GTE", "Compare", 2, 1)]
		Gte,
		#endregion
		/// <summary>,end, start -> ,range</summary>
		[OpCodeInfo("MK.RANGE","",2,1)]
		MakeRange,
		#region Jump/Branch
		/// <summary>Unconditional jump. Target is data</summary>
		[OpCodeInfo("JMP", "Jump/Branch",0,0)]
		Jump,
		/// <summary>Conditional jump. Target is data</summary>
		[OpCodeInfo("JMP.T", "Jump/Branch", 1,0)]
		Jump_True,
		/// <summary>
		/// Jump to target & pop when true. Otherwise value stays on stack...
		/// </summary>
		Jump_True_ConditionalPop,
		[OpCodeInfo("JMP.T.NP", "Jump/Branch", -1, 0, ConstNetStackChange = false)]
		Jump_True_NoPop,
		[OpCodeInfo("JMP.F", "Jump/Branch", 0,0)]
		Jump_False,
		/// <summary>
		/// Jump to target & pop when false. Otherwise value stays on stack...
		/// </summary>
		Jump_False_ConditionalPop,
		[OpCodeInfo("JMP.F.NP", "Jump/Branch", -1, 0, ConstNetStackChange = false)]
		Jump_False_NoPop,
		//https://llvm.org/docs/LangRef.html#switch-instruction
		/// <summary>Not yet implemented. </summary>
		Switch,

		/// <summary>Set Target register to data</summary>
		[OpCodeInfo("SET.TARGET", "Jump/Branch", 0,0)]
		SetTarget,
		/// <summary>Unconditional jump to Target</summary>
		[OpCodeInfo("JMP.TARGET", "Jump/Branch", 0,0)]
		JumpToTarget,
		#endregion
		#region Call

		/// <summary>
		/// Data is index. Places index into a temp register(not preserved when calling).
		/// Used by instructions that need two indexes or a 4 byte index.
		/// </summary>
		[OpCodeInfo("LOAD.TEMP", "Call",0,0)]
		LoadTempIndex,

		/* Two potential styles of non-local procedure indexing:
		 *	(1): Index of file containing procedure is loaded by LoadIndex, index of procedure name is data
		 * 
		 *	(2): Data is index of procedure stub. Procedure stubs are stored in the file, they contain:
		 *			* The index of the file containing the procedure.
		 *			* The name of the procedure.
		 *			* The offset of that procedure in its file's code segment. This is resolved at load time or when it is first called.
		 */

		/// <summary>
		/// Index of file loaded by LoadIndex instruction, index of function name is data.
		/// This <see cref="OpCode"/> can also be used to call non-virtual script object methods.
		/// </summary>
		[OpCodeInfo("CALL.FN", "Call", 0, 0, ConstNetStackChange = false)]
		CallFn,

		/// <summary>
		/// Index of file is first byte of data, index of function name is second byte in data.
		/// This <see cref="OpCode"/> can also be used to call non-virtual script object methods.
		/// </summary>
		[OpCodeInfo("CALL.FN.short", "Call", -1, -1, ConstNetStackChange = false)]
		CallFn_Short,

		/// <summary>
		/// File is the current file, index of the procedure is data.
		/// This <see cref="OpCode"/> can also be used to call non-virtual script object methods.
		/// </summary>
		[OpCodeInfo("CALL.FN.LOCAL", "Call", -1, -1, ConstNetStackChange = false)]
		CallFn_Local,

		/// <summary>
		/// Loaded index is index of type. Data is index of method name. Not for int, double or bool! Also not for methods that have corresponding instructions...
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		[OpCodeInfo("CALL.NATIVE", "Call", -1, -1, ConstNetStackChange = false)]
		CallNativeMethod,
		/// <summary>
		/// Call virtual ScriptObject method. Loaded index is index of type. Data is index of method name.
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		[OpCodeInfo("CALL.VIRT", "Call", -1, -1, ConstNetStackChange = false)]
		CallScObjVirtualMethod,

		/// <summary>
		/// Data is index of method signature stub.
		/// {, host_interface, arg_1,..., arg_N, -> , }
		/// </summary>
		[OpCodeInfo("CALL.HOST.VOID", "Call", -1, -1, ConstNetStackChange = false)]
		CallHostInterfaceMethodVoid,

		/// <summary>
		/// Data is index of method signature stub.
		/// {, host_interface, arg_1,..., arg_N, -> , result }
		/// </summary>
		[OpCodeInfo("CALL.HOST", "Call", -1, -1, ConstNetStackChange = false)]
		CallHostInterfaceMethod,
		[OpCodeInfo("RET", "Call", 0, 0)]
		Ret,
		#endregion
		#region Load Const
		/// <summary> Loads a 16 bit signed integer from the instruction's data onto the evaluation stack as a 32-bit signed integer. </summary>
		[OpCodeInfo("LOAD.I32.short", "Load Const", 0, 1)]
		LoadConst_I32_short,

		/// <summary>
		/// Loads a 32 bit signed integer onto the evaluation stack.
		///	The upper 16 bits are loaded from the temporary index register thingy,
		///	where they were previously put by <see cref="LoadTempIndex"/>. The lower 16 bits are
		/// in this instruction's data.
		/// </summary>
		[OpCodeInfo("LOAD.I32", "Load Const", 0, 1)]
		LoadConst_I32,

		/// <summary>
		/// Loads a 64-bit double precision floating point number onto the evaluation stack
		/// from the current file's F64 constant table at the position indicated by this instruction's data.
		/// </summary>
		[OpCodeInfo("LOAD.F64", "Load Const", 0, 1)]
		LoadConst_F64,
		// <summary> </summary>
		//LoadConst_F64_short,
		/// <summary> Push (index) /((double)ushort.MaxValue)</summary>
		[OpCodeInfo("LOAD.F64.U16RATIO","Load Const", 0, 1)]
		LoadConst_F64_ShortRatio,
		/// <summary>
		/// Load a string onto the evaluation stack from the current file's string constant table at the
		/// position indicated by this instruction's data.
		/// </summary>
		[OpCodeInfo("LOAD.STR", "Load Const", 0, 1)]
		LoadConst_String,
		/// <summary> </summary>
		[OpCodeInfo("LOAD.NULL", "Load Const", 0, 1)]
		LoadConst_Nil,
		[OpCodeInfo("LOAD.TRUE", "Load Const", 0, 1)]
		LoadConst_True,
		[OpCodeInfo("LOAD.FALSE", "Load Const", 0, 1)]
		LoadConst_False,
		#endregion
		// data is index of type id...
		[OpCodeInfo("LOAD.UQSC","Script Class", 0, 1)]
		Load_UniqueScriptClass,

		#region Variables, fields, and elements
		//LoadLocal_0,
		/// <summary> data is index</summary>
		[OpCodeInfo("LOAD.LOC", "Variables, fields, and elements", 0,1)]
		LoadLocal,
		/// <summary> data is index</summary>
		[OpCodeInfo("STORE.LOC", "Variables, fields, and elements", 1,0)]
		StoreLocal,
		/// <summary>, collection, index -> ,value</summary>
		[OpCodeInfo("LOAD.ELEM", "Variables, fields, and elements", 2,1)]
		LoadElement,
		// for lists of a struct type, loading an element but not storing it doesn't copy it...
		//		e.g. in 'ls[0].Foo = 1;', the expression 'ls[0]' does not make a copy.

		/// <summary>, collection, index, value -> ,</summary>
		[OpCodeInfo("STORE.ELEM", "Variables, fields, and elements", 2,0)]
		StoreElement,
		[OpCodeInfo("LOAD.FLD", "Variables, fields, and elements", 1,1)]
		LoadField,
		// object, value -> 
		[OpCodeInfo("STORE.FLD", "Variables, fields, and elements", 2,0)]
		StoreField,
		#endregion
		#region Atomic Field Operations

		#endregion

		/// <summary>
		/// Data is index of type.
		/// {, arg_0,..., arg_N -> , struct }
		/// </summary>
		[OpCodeInfo("MK.STRUCT","Structs", -1, 1, ConstNetStackChange = false)]
		ConstructStruct,

		/// <summary>
		/// Placed after LoadField for struct fields of a record. Used when a struct is an R-value.
		/// Also placed before StoreLocal when the variable is a struct and before return when it returns a struct and this is needed.
		/// </summary>
		/// <remarks> In the future, the data field of this might be the index of the struct type, though it it currently unused.</remarks>
		[OpCodeInfo("COPY.STRUCT", "Structs", 1, 1)]
		CopyStruct,
		/// <summary>Data is number of fields.</summary>
		[OpCodeInfo("FREE.STRUCT", "Structs", 1, 0)]
		FreeStruct,
		/// <summary>Data is index of type.</summary>
		[OpCodeInfo("MK.RECORD","Records", -1, 1, ConstNetStackChange = false)]
		ConstructRecord,
		/// <summary>Data is number of fields.</summary>
		[OpCodeInfo("FREE.RECORD", "Records", 1, 0)]
		FreeRecord,
		#region Arrays and Lists
		// data is index of type?
		[OpCodeInfo("MK.LST", "Arrays and Lists", 0,1)]
		ConstructList,
		[OpCodeInfo("MK.LST.I32", "Arrays and Lists", 0,1)]
		ConstructLiteIntList,
		[OpCodeInfo("FREE.LST.I32", "Arrays and Lists", 1,0)]
		FreeLiteIntList,
		[OpCodeInfo("MK.LST.F64", "Arrays and Lists", 0,1)]
		ConstructLiteDoubleList,
		[OpCodeInfo("FREE.LST.F64", "Arrays and Lists", 1,0)]
		FreeLiteDoubleList,
		// data is number of values on the eval stack to put into the list. Temp Index is index of type
		// Also creates the list.
		[OpCodeInfo("INIT.LST", "Arrays and Lists", -1, 1, ConstNetStackChange = false)]
		InitializeList,
		[OpCodeInfo("MK.ARRAY", "Arrays and Lists", 0, 1)]
		CreateArray,
		[OpCodeInfo("MK.ARRAY.I32", "Arrays and Lists", 1, 1)]
		ConstructLiteIntArray,
		[OpCodeInfo("FREE.ARRAY.I32", "Arrays and Lists", 1,0)]
		FreeLiteIntArray,
		[OpCodeInfo("MK.ARRAY.F64", "Arrays and Lists", 0,1)]
		ConstructLiteDoubleArray,
		[OpCodeInfo("FREE.ARRAY.F64", "Arrays and Lists", 1, 0)]
		FreeLiteDoubleArray,
		/// <summary>
		/// Creates and initializes an array of <see cref="ILsnValue"/>s. Temp Index is index of type.
		/// <i>data</i> is number of values on the eval stack to put into the array
		///	The value on top of the stack goes into the last slot of the array, ...
		/// </summary>
		[OpCodeInfo("INIT.ARRAY", "Arrays and Lists", -1, 1, ConstNetStackChange = false)]
		InitializeArray,
		[OpCodeInfo("FREE.ARRAY", "Arrays and Lists", 1, 0)]
		FreeArray,
		#endregion
		#region ScriptClass
		/// <summary>Data is state; local[0] is script class.</summary>
		[OpCodeInfo("SET.STATE", "Script Class", 0, 0)]
		SetState,
		/// <summary> Data is index of type. Calls the constructor... Puts it in stack[0]. </summary>
		[OpCodeInfo("MK.SC", "Script Class", -1, 1)]
		ConstructScriptClass,

		/// <summary> Data is index of type. Host is on stack. Calls the constructor... Puts it in stack[0]. </summary>
		[OpCodeInfo("MK.SC.ATTACH", "Script Class", -1, 1)]
		ConstructAndAttachScriptClass,

		/// <summary> Called at the end of the constructor of a script class that listens to its hosts events...</summary>
		[OpCodeInfo("REGISTER.SC", "Script Class", 0,0)]
		RegisterScriptObjectForEvents,
			// This happens at the end of the constructor so that all of the script object's fields are initialized before any of its code is run.

		/// <summary>Local[0] is script class.</summary>
		[OpCodeInfo("DETACH", "Script Class", 0, 0)]
		Detach,
		/// <summary>Local[0] is script class.</summary>
		[OpCodeInfo("GET.HOST", "Script Class", 0, 1)]
		GetHost, // see: HostInterfaceAccessExpression
		#endregion
		#region LSN
		GoTo,

		// Register event?
		ComeFrom,
		[OpCodeInfo("SAY", "LSN", -1, 0, ConstNetStackChange = false)]
		Say,
		/// <summary>Instruction index is data. Choice text is on stack. </summary>
		[OpCodeInfo("REGISTER.CHOICE", "LSN", 1, 0)]
		RegisterChoice,
		/// <summary>???</summary>
		RegisterChoice_Pop,
		/// <summary>
		/// Call choices, sets $PC to the result.
		/// </summary>
		[OpCodeInfo("CALL.CHOOSE", "LSN", 0, 0)]
		CallChoices,
		/// <summary>Call choice but instead of jumping, pushes the result onto the evaluation stack.</summary>
		CallChoices_Push, // Maybe use same OpCode as CallChoices but depend on data...
		[OpCodeInfo("GIVE.ITM", "LSN", -1, -1, ConstNetStackChange = false)]
		GiveItem,
		[OpCodeInfo("GIVE.GOLD", "LSN", -1, -1, ConstNetStackChange = false)]
		GiveGold,
		#endregion
		#region Input
		// ToDo: Is there a prompt?
		[OpCodeInfo("GET.STR", "Input", 0 , 1)]
		ReadString,
		// ToDo: Is there a prompt, min, max?
		[OpCodeInfo("GET.I32", "Input", 0, 1)]
		ReadInt,
		// ToDo: Is there a prompt, min, max?
		[OpCodeInfo("GET.F64", "Input", 0, 1)]
		ReadDouble,
		#endregion
		#region Random
		[OpCodeInfo("SRAND", "Random", 1, 0)]
		Srand,
		/// <summary>Set PRNG seed to pseudo-random value, e.g. system time (in ticks).</summary>
		[OpCodeInfo("SRAND.TIME", "Random", 0, 0)]
		Srand_sysTime,
		[OpCodeInfo("RAND", "Random", 0,1)]
		Rand,
		/// <summary>
		/// min, max -> random
		/// </summary>
		[OpCodeInfo("RAND.I32", "Random", 2, 1)]
		RandInt,
		#endregion
		#region Debug
		[OpCodeInfo("ERROR", "Debug", 0, 0)]
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
		[OpCodeInfo("MIN", "Math", 2, 1)]
		Min,
		/// <summary>,a,b -> Max(a,b)</summary>
		[OpCodeInfo("MAX", "Math", 2,1)]
		Max,
		[OpCodeInfo("FLOOR", "Math", 1, 1)]
		Floor,
		[OpCodeInfo("CEIL", "Math", 1, 1)]
		Ceil,
		[OpCodeInfo("ROUND", "Math", 1, 1)]
		Round,
		[OpCodeInfo("ABS", "Math", 1, 1)]
		Abs,
		[OpCodeInfo("SQRT", "Math", 1, 1)]
		Sqrt,
		[OpCodeInfo("INVSQRT", "Math", 1, 1)]
		InvSqrt,
		[OpCodeInfo("SIN", "Math", 1, 1)]
		Sin,
		[OpCodeInfo("COS", "Math", 1, 1)]
		Cos,
		[OpCodeInfo("TAN", "Math", 1, 1)]
		Tan,
		[OpCodeInfo("ASIN", "Math", 1, 1)]
		ASin,
		[OpCodeInfo("ACOS", "Math", 1, 1)]
		ACos,
		[OpCodeInfo("ATAN", "Math", 1, 1)]
		ATan,
		[OpCodeInfo("LOG", "Math", 1, 1)]
		Log,
		[OpCodeInfo("LOG10", "Math", 1, 1)]
		Log10,
		[OpCodeInfo("LOG2", "Math", 1, 1)]
		Log2,
		[OpCodeInfo("ERF", "Math", 1, 1)]
		Erf,
		[OpCodeInfo("GAMMA", "Math", 1, 1)]
		Gamma,
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
		AddAcc,// Maybe the above 3 can share same instruction but differ on data.
		/// <summary>Push $acc</summary>
		PushAcc,
		#endregion
		[OpCodeInfo("CRN", "Other", 1, 1)]
		CRN,
		[OpCodeInfo("CFRN", "Other", 1, 1)]
		CFRN,
		[OpCodeInfo("HCF","Other", 0, 0)]
		HCF = 0xF00F,
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

	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class OpCodeInfoAttribute : Attribute
	{
		/// <summary>
		/// Gets the assembly text for this instruction.
		/// </summary>
		public string AsmText { get; }

		/// <summary>
		/// Gets the category.
		/// </summary>
		public string Category { get; }

		/// <summary>
		/// Gets the net stack change.
		/// </summary>
		///<remarks>
		/// Note: The actual net stack change may depend on the instruction's data.
		/// </remarks>
		/// <seealso cref="ConstNetStackChange"/>
		public int NetStackChange => HasVariableArgs ? -137 : Push - Pop;

		public int Push { get; }

		public int Pop { get; }

		public bool HasVariableArgs => Pop < 0;

		/// <summary>
		/// Is the net stack change independent of the data and context? For example, in call instructions, this is false.
		/// </summary>
		public bool ConstNetStackChange { get; set; } = true;

		// See the attribute guidelines at 
		//  http://go.microsoft.com/fwlink/?LinkId=85236
		public OpCodeInfoAttribute(string asmText, string category, int pop, int push)
		{
			AsmText = asmText;
			Category = category;
			Pop = pop;
			Push = push;
		}
	}
}
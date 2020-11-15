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
		[OpCodeInfo("NOP","Basic")]
		Nop,
		/// <summary> Line number... </summary>
		[OpCodeInfo("DEBUG.LineNum","Basic")]
		Line,
		/// <summary>Duplicate the value on top of the stack.</summary>
		[OpCodeInfo("DUP","Basic.Stack",NetStackChange = 1)]
		Dup,
		/// <summary> ,b,a -> ,a,b</summary>
		[OpCodeInfo("SWAP","Basic.Stack")]
		Swap,
		/// <summary> ,b,a -> ,a,b,a</summary>
		// ReSharper disable once StringLiteralTypo
		[OpCodeInfo("SWADUP","Basic.Stack", NetStackChange = 1)]
		SwaDup,
		/// <summary>Pop and forget</summary>
		[OpCodeInfo("POP", "Basic.Stack", NetStackChange = -1)]
		Pop,
		#endregion
		#region Arithmetic
		// don't need type conversions b/c i32 is stored as a f64...
		[OpCodeInfo("ADD", "Arithmetic", NetStackChange = -1)]
		Add,
		[OpCodeInfo("SUB", "Arithmetic", NetStackChange = -1)]
		Sub,
		//Mul_I32,
		[OpCodeInfo("MUL", "Arithmetic", NetStackChange = -1)]
		Mul,
		[OpCodeInfo("DIV.I32", "Arithmetic", NetStackChange = -1)]
		Div_I32,

		/// <summary>
		/// , a, b -> , a/b
		/// </summary>
		[OpCodeInfo("DIV.F64", "Arithmetic", NetStackChange = -1)]
		Div_F64,

		/// <summary>Remainder</summary>
		[OpCodeInfo("REM.I32", "Arithmetic", NetStackChange = -1)]
		Rem_I32,
		[OpCodeInfo("REM.F64", "Arithmetic", NetStackChange = -1)]
		Rem_F64,
		[OpCodeInfo("POW", "Arithmetic", NetStackChange = -1)]
		Pow,

		// Unary - operator...
		[OpCodeInfo("NEG", "Arithmetic")]
		Neg,
		#endregion
		#region INC and DEC
		/// <summary>Increment var and push onto stack {++num}</summary>
		[OpCodeInfo("INC.Pre.Var", "Inc & Dec", NetStackChange = 1)]
		PreInc_Var,
		/// <summary>Increment element and push onto stack {++nums[i]}</summary>
		[OpCodeInfo("INC.Pre.Elem", "Inc & Dec", NetStackChange = 1)]
		PreInc_Elem,
		/// <summary>Increment field and push onto stack {++foo.num}</summary>
		[OpCodeInfo("INC.Pre.Fld", "Inc & Dec", NetStackChange = 1)]
		PreInc_Fld,

		/// <summary>Push var onto stack then increment {num++}</summary>
		[OpCodeInfo("INC.Post.Var", "Inc & Dec", NetStackChange = 1)]
		PostInc_Var,
		/// <summary>Push element onto stack then increment {nums[i]++}</summary>
		[OpCodeInfo("INC.Post.Elem", "Inc & Dec", NetStackChange = 1)]
		PostInc_Elem,
		/// <summary>Push field onto stack then increment {foo.num++}</summary>
		[OpCodeInfo("INC.Post.Fld", "Inc & Dec", NetStackChange = 1)]
		PostInc_Fld,

		/// <summary>Decrement var and push onto stack {--num}</summary>
		[OpCodeInfo("Dec.Pre.Var", "Inc & Dec", NetStackChange = 1)]
		PreDec_Var,
		/// <summary>Decrement element and push onto stack {--nums[i]}</summary>
		[OpCodeInfo("Dec.Pre.Elem", "Inc & Dec", NetStackChange = 1)]
		PreDec_Elem,
		/// <summary>Decrement field and push onto stack {--foo.num}</summary>
		[OpCodeInfo("Dec.Pre.Fld", "Inc & Dec", NetStackChange = 1)]
		PreDec_Fld,

		/// <summary>Push var onto stack then decrement {num--}</summary>
		[OpCodeInfo("Dec.Post.Var", "Inc & Dec", NetStackChange = 1)]
		PostDec_Var,
		/// <summary>Push element onto stack then increment {nums[i]--}</summary>
		[OpCodeInfo("Dec.Post.Elem", "Inc & Dec", NetStackChange = 1)]
		PostDec_Elem,
		/// <summary>Push field onto stack then decrement {foo.num++--}</summary>
		[OpCodeInfo("Dec.Post.Fld", "Inc & Dec", NetStackChange = 1)]
		PostDec_Fld,
		#endregion
		#region Logic
		[OpCodeInfo("AND","Logic", NetStackChange = -1)]
		And,
		//Nand,
		[OpCodeInfo("Or","Logic", NetStackChange = -1)]
		Or,
		//Xor,
		//Nor,
		//Xnor,
		[OpCodeInfo("NOT","Logic")]
		Not,
		#endregion
		#region Convert
		//Conv_I32_F64,
		#endregion
		#region String Arithmetic
		[OpCodeInfo("CONCAT","String",NetStackChange = -1)]
		Concat,
		[OpCodeInfo("I32.To.Str", "String")]
		IntToString,
		[OpCodeInfo("STR.Len", "String")]
		StringLength,
		#endregion
		#region Compare
		[OpCodeInfo("EQ.I32", "Compare", NetStackChange = -1)]
		Eq_I32,
		[OpCodeInfo("EQ.F64", "Compare", NetStackChange = -1)]
		Eq_F64,
		[OpCodeInfo("EQ.Str", "Compare", NetStackChange = -1)]
		Eq_Str,
		[OpCodeInfo("EQ.Obj", "Compare", NetStackChange = -1)]
		Eq_Obj,

		[OpCodeInfo("NEQ.I32", "Compare", NetStackChange = -1)]
		Neq_I32,
		[OpCodeInfo("NEQ.F64", "Compare", NetStackChange = -1)]
		Neq_F64,
		[OpCodeInfo("NEQ.Str", "Compare", NetStackChange = -1)]
		Neq_Str,
		[OpCodeInfo("NEQ.Obj", "Compare", NetStackChange = -1)]
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
		[OpCodeInfo("NOTNULL", "Compare")]
		NonNull,
		/// <summary>Doesn't pop from the eval stack.</summary>
		[OpCodeInfo("NOTNULL.NoPop", "Compare", NetStackChange = 1)]
		NonNull_NoPop,
		[OpCodeInfo("LT", "Compare", NetStackChange = -1)]
		Lt,
		[OpCodeInfo("LTE", "Compare", NetStackChange = -1)]
		Lte,
		[OpCodeInfo("GT", "Compare", NetStackChange = -1)]
		Gt,
		[OpCodeInfo("GTE", "Compare", NetStackChange = -1)]
		Gte,
		#endregion
		/// <summary>,end, start -> ,range</summary>
		[OpCodeInfo("MK.RANGE","",NetStackChange = -1)]
		MakeRange,
		#region Jump/Branch
		/// <summary>Unconditional jump. Target is data</summary>
		[OpCodeInfo("JMP", "Jump/Branch")]
		Jump,
		
		/// <summary>Conditional jump. Target is data</summary>
		[OpCodeInfo("JMP.T", "Jump/Branch", NetStackChange = -1)]
		Jump_True,
		/// <summary>
		/// Jump to target & pop when true. Otherwise value stays on stack...
		/// </summary>
		Jump_True_ConditionalPop,
		[OpCodeInfo("JMP.T.NP", "Jump/Branch", NetStackChange = 0)]
		Jump_True_NoPop,
		[OpCodeInfo("JMP.F", "Jump/Branch", NetStackChange = -1)]
		Jump_False,
		/// <summary>
		/// Jump to target & pop when false. Otherwise value stays on stack...
		/// </summary>
		Jump_False_ConditionalPop,
		[OpCodeInfo("JMP.F.NP", "Jump/Branch", NetStackChange = 0)]
		Jump_False_NoPop,
		//https://llvm.org/docs/LangRef.html#switch-instruction
		/// <summary>Not yet implemented. </summary>
		Switch,

		/// <summary>Set Target register to data</summary>
		[OpCodeInfo("SET.TARGET", "Jump/Branch", NetStackChange = 0)]
		SetTarget,
		/// <summary>Unconditional jump to Target</summary>
		[OpCodeInfo("JMP.TARGET", "Jump/Branch", NetStackChange = 0)]
		JumpToTarget,
		#endregion
		#region Call

		/// <summary>
		/// Data is index. Places index into a temp register(not preserved when calling).
		/// Used by instructions that need two indexes or a 4 byte index.
		/// </summary>
		[OpCodeInfo("LOAD.TEMP", "Call")]
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
		[OpCodeInfo("CALL.FN", "Call", ConstNetStackChange = false)]
		CallFn,

		/// <summary>
		/// Index of file is first byte of data, index of function name is second byte in data.
		/// This <see cref="OpCode"/> can also be used to call non-virtual script object methods.
		/// </summary>
		[OpCodeInfo("CALL.FN.short", "Call", ConstNetStackChange = false)]
		CallFn_Short,

		/// <summary>
		/// File is the current file, index of the procedure is data.
		/// This <see cref="OpCode"/> can also be used to call non-virtual script object methods.
		/// </summary>
		[OpCodeInfo("CALL.FN.LOCAL", "Call", ConstNetStackChange = false)]
		CallFn_Local,

		/// <summary>
		/// Loaded index is index of type. Data is index of method name. Not for int, double or bool! Also not for methods that have corresponding instructions...
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		[OpCodeInfo("CALL.NATIVE", "Call", ConstNetStackChange = false)]
		CallNativeMethod,
		/// <summary>
		/// Call virtual ScriptObject method. Loaded index is index of type. Data is index of method name.
		/// {object, arg_1,..., arg_N-> , result (if it returns a value) }
		/// </summary>
		[OpCodeInfo("CALL.VIRT", "Call", ConstNetStackChange = false)]
		CallScObjVirtualMethod,

		/// <summary>
		/// Data is index of method signature stub.
		/// {, host_interface, arg_1,..., arg_N, -> , }
		/// </summary>
		[OpCodeInfo("CALL.HOST.VOID", "Call", ConstNetStackChange = false)]
		CallHostInterfaceMethodVoid,

		/// <summary>
		/// Data is index of method signature stub.
		/// {, host_interface, arg_1,..., arg_N, -> , result }
		/// </summary>
		[OpCodeInfo("CALL.HOST", "Call", ConstNetStackChange = false)]
		CallHostInterfaceMethod,
		[OpCodeInfo("RET", "Call")]
		Ret,
		#endregion
		#region Load Const
		/// <summary> Loads a 16 bit signed integer from the instruction's data onto the evaluation stack as a 32-bit signed integer. </summary>
		[OpCodeInfo("LOAD.I32.short", "Load Const", NetStackChange = 1)]
		LoadConst_I32_short,

		/// <summary>
		/// Loads a 32 bit signed integer onto the evaluation stack.
		///	The upper 16 bits are loaded from the temporary index register thingy,
		///	where they were previously put by <see cref="LoadTempIndex"/>. The lower 16 bits are
		/// in this instruction's data.
		/// </summary>
		[OpCodeInfo("LOAD.I32", "Load Const", NetStackChange = 1)]
		LoadConst_I32,

		/// <summary>
		/// Loads a 64-bit double precision floating point number onto the evaluation stack
		/// from the current file's F64 constant table at the position indicated by this instruction's data.
		/// </summary>
		[OpCodeInfo("LOAD.F64", "Load Const", NetStackChange = 1)]
		LoadConst_F64,
		// <summary> </summary>
		//LoadConst_F64_short,
		/// <summary> Push (index) /((double)ushort.MaxValue)</summary>
		LoadConst_F64_ShortRatio,
		/// <summary>
		/// Load a string onto the evaluation stack from the current file's string constant table at the
		/// position indicated by this instruction's data.
		/// </summary>
		[OpCodeInfo("LOAD.STR", "Load Const", NetStackChange = 1)]
		LoadConst_String,
		/// <summary> </summary>
		[OpCodeInfo("LOAD.NULL", "Load Const", NetStackChange = 1)]
		LoadConst_Nil,
		[OpCodeInfo("LOAD.TRUE", "Load Const", NetStackChange = 1)]
		LoadConst_True,
		[OpCodeInfo("LOAD.FALSE", "Load Const", NetStackChange = 1)]
		LoadConst_False,
		#endregion
		// data is index of type id...
		[OpCodeInfo("LOAD.UQSC","Script Class", NetStackChange = 1)]
		Load_UniqueScriptClass,

		#region Variables, fields, and elements
		//LoadLocal_0,
		/// <summary> data is index</summary>
		[OpCodeInfo("LOAD.LOC", "Variables, fields, and elements", NetStackChange = 1)]
		LoadLocal,
		/// <summary> data is index</summary>
		[OpCodeInfo("STORE.LOC", "Variables, fields, and elements", NetStackChange = -1)]
		StoreLocal,
		/// <summary>, collection, index -> ,value</summary>
		[OpCodeInfo("LOAD.ELEM", "Variables, fields, and elements", NetStackChange = 1)]
		LoadElement,
		// for lists of a struct type, loading an element but not storing it doesn't copy it...
		//		e.g. in 'ls[0].Foo = 1;', the expression 'ls[0]' does not make a copy.

		/// <summary>, collection, index, value -> ,</summary>
		[OpCodeInfo("STORE.ELEM", "Variables, fields, and elements", NetStackChange = -2)]
		StoreElement,
		[OpCodeInfo("LOAD.FLD", "Variables, fields, and elements", NetStackChange = 1)]
		LoadField,
		// object, value -> 
		[OpCodeInfo("STORE.FLD", "Variables, fields, and elements", NetStackChange = -2)]
		StoreField,
		#endregion
		#region Atomic Field Operations

		#endregion

		/// <summary>
		/// Data is index of type.
		/// {, arg_0,..., arg_N -> , struct }
		/// </summary>
		[OpCodeInfo("MK.STRUCT","Structs", ConstNetStackChange = false)]
		ConstructStruct,

		/// <summary>
		/// Placed after LoadField for struct fields of a record. Used when a struct is an R-value.
		/// Also placed before StoreLocal when the variable is a struct and before return when it returns a struct and this is needed.
		/// </summary>
		/// <remarks> In the future, the data field of this might be the index of the struct type, though it it currently unused.</remarks>
		[OpCodeInfo("COPY.STRUCT", "Structs")]
		CopyStruct,
		/// <summary>Data is number of fields.</summary>
		[OpCodeInfo("FREE.STRUCT", "Structs", NetStackChange = -1)]
		FreeStruct,
		/// <summary>Data is index of type.</summary>
		ConstructRecord,
		/// <summary>Data is number of fields.</summary>
		FreeRecord,
		#region Arrays and Lists
		// data is index of type?
		[OpCodeInfo("MK.LST", "Arrays and Lists", NetStackChange = 1)]
		ConstructList,
		[OpCodeInfo("MK.LST.I32", "Arrays and Lists", NetStackChange = 1)]
		ConstructLiteIntList,
		[OpCodeInfo("FREE.LST.I32", "Arrays and Lists", NetStackChange = -1)]
		FreeLiteIntList,
		[OpCodeInfo("MK.LST.F64", "Arrays and Lists", NetStackChange = 1)]
		CostructLiteDoubleList,
		[OpCodeInfo("FREE.LST.F64", "Arrays and Lists", NetStackChange = -1)]
		FreeLiteDoubleList,
		// data is number of values on the eval stack to put into the list. Temp Index is index of type
		// Also creates the list.
		[OpCodeInfo("INIT.LST", "Arrays and Lists", ConstNetStackChange = false)]
		InitializeList,
		[OpCodeInfo("MK.ARRAY", "Arrays and Lists", NetStackChange = 1)]
		CreateArray,
		[OpCodeInfo("MK.ARRAY.I32", "Arrays and Lists", NetStackChange = 1)]
		ConstructLiteIntArray,
		[OpCodeInfo("FREE.ARRAY.I32", "Arrays and Lists", NetStackChange = -1)]
		FreeLiteIntArray,
		[OpCodeInfo("MK.ARRAY.F64", "Arrays and Lists", NetStackChange = 1)]
		CostructLiteDoubleArray,
		[OpCodeInfo("FREE.ARRAY.F64", "Arrays and Lists", NetStackChange = -1)]
		FreeLiteDoubleArray,
		// data is number of values on the eval stack to put into the vector
		//	The value on top of the stack is the goes into the last slot of the array, ...
		//	Also creates the array...
		[OpCodeInfo("INIT.ARRAY", "Arrays and Lists", ConstNetStackChange = false)]
		InitializeArray,
		[OpCodeInfo("FREE.ARRAY", "Arrays and Lists", NetStackChange = -1)]
		FreeArray,
		#endregion
		#region ScriptClass
		/// <summary>Data is state; local[0] is script class.</summary>
		[OpCodeInfo("SET.STATE", "Script Class")]
		SetState,
		/// <summary> Data is index of type. Calls the constructor... Puts it in stack[0]. </summary>
		[OpCodeInfo("MK.SC", "Script Class")]
		ConstructScriptClass,

		/// <summary> Data is index of type. Host is on stack. Calls the constructor... Puts it in stack[0]. </summary>
		[OpCodeInfo("MK.SC.ATTACH", "Script Class")]
		ConstructAndAttachScriptClass,

		/// <summary> Called at the end of the constructor of a script class that listens to its hosts events...</summary>
		[OpCodeInfo("REGISTER.SC", "Script Class")]
		RegisterScriptObjectForEvents,
			// This happens at the end of the constructor so that all of the script object's fields are initialized before any of its code is run.

		/// <summary>Local[0] is script class.</summary>
		[OpCodeInfo("DETACH", "Script Class")]
		Detach,
		/// <summary>Local[0] is script class.</summary>
		[OpCodeInfo("GET.HOST", "Script Class", NetStackChange = 1)]
		GetHost, // see: HostInterfaceAccessExpression
		#endregion
		#region LSN
		GoTo,

		// Register event?
		ComeFrom,
		[OpCodeInfo("SAY", "LSN", ConstNetStackChange = false)]
		Say,
		/// <summary>Instruction index is data. Choice text is on stack. </summary>
		[OpCodeInfo("REGISTER.CHOICE", "LSN", NetStackChange = -1)]
		RegisterChoice,
		/// <summary>???</summary>
		RegisterChoice_Pop,
		/// <summary>
		/// Call choices, sets $PC to the result.
		/// </summary>
		[OpCodeInfo("CALL.CHOOSE", "LSN")]
		CallChoices,
		/// <summary>Call choice but instead of jumping, pushes the result onto the evaluation stack.</summary>
		CallChoices_Push, // Maybe use same OpCode as CallChoices but depend on data...
		[OpCodeInfo("GIVE.ITM", "LSN", ConstNetStackChange = false)]
		GiveItem,
		[OpCodeInfo("GIVE.GOLD", "LSN", ConstNetStackChange = false)]
		GiveGold,
		#endregion
		#region Input
		// ToDo: Is there a prompt?
		[OpCodeInfo("GET.STR", "Input", NetStackChange = 1)]
		ReadString,
		// ToDo: Is there a prompt, min, max?
		[OpCodeInfo("GET.I32", "Input", NetStackChange = 1)]
		ReadInt,
		// ToDo: Is there a prompt, min, max?
		[OpCodeInfo("GET.F64", "Input", NetStackChange = 1)]
		ReadDouble,
		#endregion
		#region Random
		[OpCodeInfo("SRAND", "Random", NetStackChange = -1)]
		Srand,
		/// <summary>Set PRNG seed to pseudo-random value, e.g. system time (in ticks).</summary>
		[OpCodeInfo("SRAND.TIME", "Random")]
		Srand_sysTime,
		[OpCodeInfo("RAND", "Random", NetStackChange = 1)]
		Rand,
		/// <summary>
		/// min, max -> random
		/// </summary>
		[OpCodeInfo("RAND.I32", "Random", NetStackChange = 1)]
		RandInt,
		#endregion
		#region Debug
		[OpCodeInfo("ERROR", "Debug")]
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
		[OpCodeInfo("MIN", "Math", NetStackChange = -1)]
		Min,
		/// <summary>,a,b -> Max(a,b)</summary>
		[OpCodeInfo("MAX", "Math", NetStackChange = -1)]
		Max,
		[OpCodeInfo("FLOOR", "Math")]
		Floor,
		[OpCodeInfo("CEIL", "Math")]
		Ceil,
		[OpCodeInfo("ROUND", "Math")]
		Round,
		[OpCodeInfo("ABS", "Math")]
		Abs,
		[OpCodeInfo("SQRT", "Math")]
		Sqrt,
		[OpCodeInfo("SIN", "Math")]
		Sin,
		[OpCodeInfo("COS", "Math")]
		Cos,
		[OpCodeInfo("TAN", "Math")]
		Tan,
		[OpCodeInfo("ASIN", "Math")]
		ASin,
		[OpCodeInfo("ACOS", "Math")]
		ACos,
		[OpCodeInfo("ATAN", "Math")]
		ATan,
		[OpCodeInfo("LOG", "Math")]
		Log,
		[OpCodeInfo("LOG10", "Math")]
		Log10,
		[OpCodeInfo("LOG2", "Math")]
		Log2,
		[OpCodeInfo("ERF", "Math")]
		Erf,
		[OpCodeInfo("GAMMA", "Math")]
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
		[OpCodeInfo("CRN","Other")]
		CRN,
		[OpCodeInfo("CFRN","Other")]
		CFRN,
		[OpCodeInfo("HCF","Other")]
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
		public int NetStackChange { get; set; }

		/// <summary>
		/// Is the net stack change independent of the data and context? For example, in call instructions, this is false.
		/// </summary>
		public bool ConstNetStackChange { get; set; } = true;

		// See the attribute guidelines at 
		//  http://go.microsoft.com/fwlink/?LinkId=85236
		public OpCodeInfoAttribute(string asmText, string category)
		{
			AsmText = asmText;
			Category = category;
		}
	}
}
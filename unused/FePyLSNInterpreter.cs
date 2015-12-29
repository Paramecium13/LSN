using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// Interprets LSN using FePyLSNScopes and an IronPython engine.
	/// </summary>
	public abstract class FePyLSNInterpreter : IInterpreter
	{
		public bool PassVariablesByName { get { return false; } }

		public ILSN_Value ReturnValue { get; set; }
		protected FePyLSNScope Scope;
		protected Stack<FePyLSNScope> ScopeStack = new Stack<FePyLSNScope>();
		protected List<ILSN_Value> LSN_Objects = new List<ILSN_Value>();
		protected ScriptEngine Engine = Python.CreateEngine();
		
		public FePyLSNInterpreter()
		{
			Scope = new FePyLSNScope(Engine);
		}

		/// <summary>
		/// The first 4 bytes at a location store the size of an individual element.
		/// </summary>
		protected Dictionary<int, IntPtr> Arrays = new Dictionary<int, IntPtr>();

		/// <summary>
		/// Enters a new scope, that still has access to variables defined in the previuos scope.
		/// </summary>
		public virtual void EnterScope()
		{
			Scope = Scope.Push();
		}

		/// <summary>
		/// Exits the current scope.
		/// </summary>
		public virtual void ExitScope() { Scope = Scope.Pop(); }

		/// <summary>
		/// Creates a new variable with the provided name and value.
		/// </summary>
		/// <param name="name">The name of the variable to create.</param>
		/// <param name="val">The initial value to assign it.</param>
		public virtual void AddVariable(string name, ILSN_Value val)
		{
			Scope.AddVariable(name, val);
		}

		/// <summary>
		/// Assigns a new value to a variable.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="val">The new value to assign it.</param>
		public virtual void ReAssignVariable(string name, ILSN_Value val)
		{
			Scope.ReAssignVariable(name, val);
		}

		/// <summary>
		/// Gets the value of the specified variable.
		/// </summary>
		/// <param name="name">The name of the variable whose value is requested.</param>
		/// <returns>The value of the variable.</returns>
		public virtual ILSN_Value GetValue(string name) => Scope.GetValue(name);

		/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		public virtual void EnterFunctionScope(LSN_Environment env)
		{
			ScopeStack.Push(Scope);
			Scope = new FePyLSNScope(Engine);
		}

		/// <summary>
		/// Exits the scope of the current function.
		/// </summary>
		public virtual void ExitFunctionScope()
		{
			Scope = ScopeStack.Pop();
		}

		#region unsafe
		// Test for using unmanaged stuff...
		public unsafe virtual void AddVariable(int id, IntValue val, object dummyParam)
		{
			int v = val.Value;
			int* ptr = &v;
			IntPtr x = Marshal.AllocHGlobal(4);
			int* ptr2 = (int*)x.ToPointer();
			*ptr2 = v;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">The LSN type that will be stored.</param>
		/// <param name="number">The number of elements in the array.</param>
		/// <param name="id"  >An identifier, for other parts of the script to access this array.</param>
		public unsafe virtual void AllocArray(LSN_Type type, int number, int id)
		{
			var size = Marshal.SizeOf(type.CreateDefaultValue());
			var ptr = Marshal.AllocHGlobal(size * number + sizeof(int));
			Arrays[id] = ptr;
			*((int*)ptr.ToPointer()) = size;
		}

		public unsafe virtual void AssignArray(ILSN_Value[] collection, int id)
		{
			var count = collection.Length;
			var tmpPtr = (int*)(Arrays[id].ToPointer());
			var size = *(tmpPtr);
			var ptr = new IntPtr(tmpPtr + sizeof(int));
			for (int i = 0; i < count; i++)
			{
				Marshal.StructureToPtr(collection[i], ptr, false);
				ptr += size;
			}
		}

		public unsafe virtual ILSN_Value GetArrayValue(int id, int index)
		{
			var tmpPtr = (int*)(Arrays[id].ToPointer());
			var size = *(tmpPtr);
			var ptr = new IntPtr(tmpPtr + sizeof(int));
			ILSN_Value x = null;
			Marshal.PtrToStructure(ptr + index * size, x);
			return x;
		}

		public unsafe virtual void SetArrayValue(int id, int index, ILSN_Value value)
		{
			var tmpPtr = (int*)(Arrays[id].ToPointer());
			var size = *(tmpPtr);
			var ptr = new IntPtr(tmpPtr + sizeof(int));
			Marshal.StructureToPtr(value, ptr + index * size, true);
		}

		public unsafe virtual void DeallocArray(int id)
		{
			Marshal.FreeHGlobal(Arrays[id]);
		}

		#endregion

		public abstract void GiveItemTo(ILSN_Value id, int amount, ILSN_Value target);
		public abstract void GivArmorTo(ILSN_Value id, int amount, ILSN_Value target);
		public abstract void GiveWeaponTo(ILSN_Value id, int amount, ILSN_Value target);
		public abstract void GiveGoldTo(int amount, ILSN_Value target);

		public abstract IActor GetActor(LSN_Value id);

		public ILSN_Value Eval(string expression) => Engine.Execute<ILSN_Value>(expression, Scope.PyScope);

	}
}

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public abstract class Interpreter : IInterpreter
	{
		/// <summary>
		/// Should variables in compound expressions be parsed by name?
		/// </summary>
		public bool PassVariablesByName { get { return false; } }

		public LsnValue ReturnValue { get; set; }
		protected Scope Scope = new Scope(); // ToDo: Remove
		protected Stack<Scope> ScopeStack = new Stack<Scope>(); // ToDo: Remove


		protected List<ILsnValue> LSN_Objects = new List<ILsnValue>();


		private Dictionary<int, ConcurrentStack<LsnValue[]>> StackStore = new Dictionary<int, ConcurrentStack<LsnValue[]>>();


		private LsnValue[] CurrentStack;


		private Stack<LsnValue[]> StackOfStacks = new Stack<LsnValue[]>();





		// Where the current environment is pushed when a new function scope is entered.
		private Stack<LsnEnvironment> EnvStack = new Stack<LsnEnvironment>();

		// The current environment.
		private LsnEnvironment CurrentEnvironment;

		/// <summary>
		/// Run the script.
		/// </summary>
		/// <param name="script"></param>
		public virtual void Run(LsnScript script)
		{
			for(int i = 0; i < script.Components.Count; i++)
			{
				var c = script.Components[i];
				c.Interpret(this);
			}
		}

		/// <summary>
		/// Enters a new scope, that still has access to variables defined in the previuos scope.
		/// </summary>
		public virtual void EnterScope() // ToDo: Remove
		{
			Scope = Scope.Push();
		}

		/// <summary>
		/// Exits the current scope.
		/// </summary>
		public virtual void ExitScope()	{ Scope = Scope.Pop(); } // ToDo: Remove

		/// <summary>
		/// Creates a new variable with the provided name and value.
		/// </summary>
		/// <param name="name">The name of the variable to create.</param>
		/// <param name="val">The initial value to assign it.</param>
		public virtual void AddVariable(string name, LsnValue val) // ToDo: Remove
		{
			//Scope.AddVariable(name, val);			
		}
		

		/// <summary>
		/// Gets the value of the specified variable.
		/// </summary>
		/// <param name="name">The name of the variable whose value is requested.</param>
		/// <returns>The value of the variable.</returns>
		public virtual ILsnValue GetValue(string name) => Scope.GetValue(name);

		/// <summary>
		/// Enters a new scope for interpreting a function. Previously defined variables are inaccessable.
		/// </summary>
		public virtual void EnterFunctionScope(LsnEnvironment env, int scopeSize)
		{
			ScopeStack.Push(Scope);
			Scope = new Scope();
			EnvStack.Push(CurrentEnvironment);
			CurrentEnvironment = env;

			StackOfStacks.Push(CurrentStack);
			int i = NearestPower(scopeSize);
			if (StackStore.ContainsKey(i))
			{
				if (!StackStore[i].TryPop(out CurrentStack))
					CurrentStack = new LsnValue[i];
			}
			else
			{
				StackStore.Add(i, new ConcurrentStack<LsnValue[]>());
				CurrentStack = new LsnValue[i];
			}
		}

		/// <summary>
		/// Exits the scope of the current function.
		/// </summary>
		public virtual void ExitFunctionScope()
		{
			RecycleStack(CurrentStack);
			Scope = ScopeStack.Pop();
			CurrentStack = StackOfStacks.Pop();
			CurrentEnvironment = EnvStack.Pop();
		}

		/// <summary>
		/// Get a function.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual Function GetFunction(string name)
			=> CurrentEnvironment.Functions[name];


		public LsnValue GetValue(int index)
			=> CurrentStack[index];


		public void SetValue(int index, LsnValue value)
		{
			CurrentStack[index] = value;
		}



		private void RecycleStack(LsnValue[] stack)
		{
			Task.Run(() =>
			{
				for (int i = 0; i < stack.Length; i++)
					stack[i] = LsnValue.Nil;
				if(StackStore.ContainsKey(stack.Length))
					StackStore[stack.Length].Push(stack);
				else
				{
					StackStore.Add(stack.Length, new ConcurrentStack<LsnValue[]>());
					StackStore[stack.Length].Push(stack);
				}
			});
		}


		private static int NearestPower(int i)
			=> 1 << (int)Math.Ceiling(Math.Log(i, 2));

		/*
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

		#endregion*/

		public abstract void GiveItemTo(LsnValue id, int amount, LsnValue target);
		public abstract void GiveArmorTo(LsnValue id, int amount, LsnValue target);
		public abstract void GiveWeaponTo(LsnValue id, int amount, LsnValue target);
		public abstract void GiveGoldTo(int amount, LsnValue target);

		public abstract IActor GetActor(LsnValue id);

		public abstract void Say(string message, LsnValue graphic, string title);
		public abstract int Choice(List<string> choices);

		public abstract LsnValue GetGlobalVariable(string globalVarName/*, string fileName*/);
		public abstract void SetGlobalVariable(LsnValue value, string globalVarName/*, string fileName*/);
	}
}
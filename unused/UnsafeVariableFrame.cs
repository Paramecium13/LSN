using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LSN_Core
{
	public unsafe class UnsafeVariableFrame
	{
		protected Dictionary<int, IntPtr> Pointers = new Dictionary<int, IntPtr>();
		protected UnsafeVariableFrame Parent;

		public UnsafeVariableFrame(UnsafeVariableFrame parent)
		{
			Parent = parent;
		}

		public void AddVariable(int id, ILSN_Value val)
		{
			var size = Marshal.SizeOf(val);
			var ptr = Marshal.AllocHGlobal(size);
            Pointers.Add(id, ptr);
			Marshal.StructureToPtr(val, ptr, true);
		}

		public ILSN_Value GetValue(int id)
		{
			if(Pointers.ContainsKey(id))
			{
				ILSN_Value val = null;
				Marshal.PtrToStructure(Pointers[id], val);
				return val;
			}
			return Parent.GetValue(id);
		}

		public void ReassignVariable(int id, ILSN_Value val)
		{
			if(Pointers.ContainsKey(id))
			Marshal.StructureToPtr(val, Pointers[id], true);
			else
			{
				Parent.ReassignVariable(id, val);
			}
		}

		public void RemoveVariable(int id)
		{
			if (Pointers.ContainsKey(id))
			{
				Marshal.FreeHGlobal(Pointers[id]);
				Pointers.Remove(id);
			}else
			{
				Parent.RemoveVariable(id);
			}
		}

		public UnsafeVariableFrame Pop()
		{
			Dealloc();
			return Parent;
		}

		protected void Dealloc()
		{
			foreach(var ptr in  Pointers.Values)
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		~UnsafeVariableFrame()
		{
			Dealloc();
		}

	}
}

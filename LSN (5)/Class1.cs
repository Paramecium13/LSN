using System;
using System.Runtime.CompilerServices;

// Just testing stuff...

namespace LsnCore
{
	internal unsafe ref struct RegisterFile
	{
		internal Span<LsnValue> Values; 
	}
	public class Class1
	{
		internal static void Foo(ref RegisterFile registerFile)
		{
			registerFile.Values[0] = new LsnValue(1);
		}

		internal static unsafe void Bar()
		{
			var buffer = stackalloc IntPtr[16];
			var registerValues = new Span<LsnValue>(buffer, 8);
			var registerFile = new RegisterFile() {Values = registerValues};
		}
	}
}

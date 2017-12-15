﻿using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	public sealed class MethodCall : Expression
	{
		public IExpression[] Args;

		public override bool IsPure => false;

		public readonly Method Method;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(Method m, IExpression[] args)
		{
			Args = args ?? new IExpression[1];
			Method = m;
			Type = m.ReturnType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MethodCall(string name, IExpression[] args, LsnType callingType)
		{
			Args = args;
			Method = callingType.Methods[name];
			Type = Method.ReturnType;
		}

		public override LsnValue Eval(IInterpreter i)
		{
			var args = new LsnValue[Args.Length];
			for (int x = 0; x < Args.Length; x++)
				args[x] = Args[x].Eval(i);
			if (!Method.HandlesScope) i.EnterFunctionScope(Method.ResourceFilePath, Method.StackSize);
			var val = Method.Eval(args, i);
			if (!Method.HandlesScope) i.ExitFunctionScope();
			return val;
		}

		public override IExpression Fold() => this;

		public override bool IsReifyTimeConst() => false;

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.MethodCall);
			var typeName = Method.TypeId?.Name;
			if (string.IsNullOrEmpty(typeName))
				typeName = Method.Parameters[0].Type.Name;
			writer.Write(typeName);
			writer.Write(Method.Name);
			writer.Write((byte)Args.Length);
			for (int i = 0; i < Args.Length; i++)
				Args[i].Serialize(writer, resourceSerializer);
		}
	}
}

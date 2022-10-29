using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	/// <summary>
	/// A binary expression where one or both of the arguments is a string and that returns a string (or array of strings, with future updates).
	/// </summary>
	class StringBinaryExpression : BinaryExpressionBase
	{
		/// <inheritdoc />
		public StringBinaryExpression(IExpression left, IExpression right, BinaryOperation operation) : base(
			operation, GetArgTypes(left.Type, right.Type))
		{
			Left = left;
			Right = right;
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst()
		{
			return Left.IsReifyTimeConst() && Right.IsReifyTimeConst();
		}

		/// <inheritdoc />
		public override IExpression Fold()
		{
			Left.Fold();
			Right.Fold();
			if (Left is LsnValue leftValue && Right is LsnValue rightValue)
			{
				switch (ArgumentTypes)
				{
					case BinaryOperationArgsType.String_String:
						return Operation switch
						{
							BinaryOperation.Sum => new LsnValue(new StringValue(((StringValue) leftValue.Value).Value +
								((StringValue) rightValue.Value).Value)),
							BinaryOperation.Difference => throw new InvalidOperationException(
								"String subtraction is not currently supported"),
							BinaryOperation.Quotient => throw new InvalidOperationException(
								"String division is not currently supported"),
							BinaryOperation.Modulus => throw new InvalidOperationException(
								"String splitting via the '%' operator is not yet supported."),
							_ => throw new ArgumentOutOfRangeException()
						};
					case BinaryOperationArgsType.String_Int:
						switch (Operation)
						{
							case BinaryOperation.Sum:
								return new LsnValue(new StringValue(((StringValue)leftValue.Value).Value + rightValue.IntValue));
							case BinaryOperation.Difference:
								throw new InvalidOperationException(
									"Sub-string operations by subtracting by an int are not currently supported.");
							case BinaryOperation.Product:
								var strb = new StringBuilder();
								var str = ((StringValue)leftValue.Value).Value;
								if (rightValue.IntValue < 0)
								{
									throw new ArgumentException();
								}

								for (var i = 0; i < rightValue.IntValue; i++)
								{
									strb.Append(str);
								}

								return new LsnValue(new StringValue(str));
							case BinaryOperation.Quotient:
								// This would divide the string into n parts of equal length (the last part may be shorter).
								throw new InvalidOperationException("Splitting strings into equal parts via the '/' operator is not yet supported");
							case BinaryOperation.Modulus:
								// This would divide the string into parts of length n (the last part may be shorter).
								throw new InvalidOperationException("Splitting strings into equal parts via the '%' operator is not yet supported");
							default:
								throw new ArgumentOutOfRangeException();
						}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return this;
		}

		/// <inheritdoc />
		public override bool IsPure => Left.IsPure && Right.IsPure;

		/// <inheritdoc />
		public override void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		protected override void GetOperationInstruction(InstructionList instructions, InstructionGenerationContext context)
		{
			var subcontext = context.WithContext(ExpressionContext.SubExpression);
			Left.GetInstructions(instructions, subcontext);
			Right.GetInstructions(instructions, subcontext);
			switch (ArgumentTypes)
			{
				case BinaryOperationArgsType.String_String:
					switch (Operation)
					{
						case BinaryOperation.Sum:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Concat, 0));
							break;
						case BinaryOperation.Difference:
							throw new InvalidOperationException("String subtraction is not currently supported");
						case BinaryOperation.Quotient:
							throw new InvalidOperationException("String division is not currently supported");
						case BinaryOperation.Modulus:
							throw new InvalidOperationException("String splitting via the '%' operator is not yet supported.");
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				case BinaryOperationArgsType.String_Int:
					switch (Operation)
					{
						case BinaryOperation.Sum:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.IntToString, 0));
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Concat, 0));
							break;
						case BinaryOperation.Product:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.StrMult, 0));
							break;
						case BinaryOperation.Difference:
							throw new InvalidOperationException("Sub-string operations by subtracting by an int are not currently supported.");
						case BinaryOperation.Quotient:
							// This would divide the string into n parts of equal length (the last part may be shorter).
							throw new InvalidOperationException("Splitting strings into equal parts via the '/' operator is not yet supported");
						case BinaryOperation.Modulus:
							// This would divide the string into parts of length n (the last part may be shorter).
							throw new InvalidOperationException("Splitting strings into equal parts via the '%' operator is not yet supported");
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}

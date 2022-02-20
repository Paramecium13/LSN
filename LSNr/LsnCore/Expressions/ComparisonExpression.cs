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
	/// A comparison expression
	/// </summary>
	internal class ComparisonExpression : BinaryExpressionBase
	{
		/// <inheritdoc />
		public ComparisonExpression(IExpression left, IExpression right, BinaryOperation operation) : base(
			operation, GetArgTypes(left.Type, right.Type))
		{
			Left = left;
			Right = right;
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => Left.IsReifyTimeConst() && Right.IsReifyTimeConst();

		/// <inheritdoc />
		public override IExpression Fold()
		{
			Left = Left.Fold();
			Right = Right.Fold();
			if (Left is LsnValue leftVal)
			{
				if (Right is LsnValue rightValue)
				{
					switch (ArgumentTypes)
					{
						case BinaryOperationArgsType.Bool_Bool:
							switch (Operation)
							{
								case BinaryOperation.Equal:
									return LsnBoolValue.GetBoolValue(leftVal.BoolValue == rightValue.BoolValue);
								case BinaryOperation.NotEqual:
									return LsnBoolValue.GetBoolValue(leftVal.BoolValue != rightValue.BoolValue);
								default: throw new InvalidOperationException();
							}
						case BinaryOperationArgsType.Int_Int:
							switch (Operation)
							{
								case BinaryOperation.Equal:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue == rightValue.IntValue);
								case BinaryOperation.NotEqual:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue != rightValue.IntValue);
								case BinaryOperation.GreaterThan:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue > rightValue.IntValue);
								case BinaryOperation.GreaterThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue >= rightValue.IntValue);
								case BinaryOperation.LessThan:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue < rightValue.IntValue);
								case BinaryOperation.LessThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue <= rightValue.IntValue);
							}
							break;
						case BinaryOperationArgsType.Double_Int:
							switch (Operation)
							{
								case BinaryOperation.Equal:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue == rightValue.IntValue);
								case BinaryOperation.NotEqual:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue != rightValue.IntValue);
								case BinaryOperation.GreaterThan:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue > rightValue.IntValue);
								case BinaryOperation.GreaterThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue >= rightValue.IntValue);
								case BinaryOperation.LessThan:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue < rightValue.IntValue);
								case BinaryOperation.LessThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue <= rightValue.IntValue);
							}
							break;
						case BinaryOperationArgsType.Int_Double:
							switch (Operation)
							{
								case BinaryOperation.Equal:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue == rightValue.DoubleValue);
								case BinaryOperation.NotEqual:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue != rightValue.DoubleValue);
								case BinaryOperation.GreaterThan:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue > rightValue.DoubleValue);
								case BinaryOperation.GreaterThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue >= rightValue.DoubleValue);
								case BinaryOperation.LessThan:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue < rightValue.DoubleValue);
								case BinaryOperation.LessThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.IntValue <= rightValue.DoubleValue);
							}
							break;
						case BinaryOperationArgsType.Double_Double:
							switch (Operation)
							{
								case BinaryOperation.Equal:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue == rightValue.DoubleValue);
								case BinaryOperation.NotEqual:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue != rightValue.DoubleValue);
								case BinaryOperation.GreaterThan:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue > rightValue.DoubleValue);
								case BinaryOperation.GreaterThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue >= rightValue.DoubleValue);
								case BinaryOperation.LessThan:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue < rightValue.DoubleValue);
								case BinaryOperation.LessThanOrEqual:
									return LsnBoolValue.GetBoolValue(leftVal.DoubleValue <= rightValue.DoubleValue);
							}
							break;
						case BinaryOperationArgsType.String_String:
							switch (Operation)
							{
								case BinaryOperation.Equal:
									return LsnBoolValue.GetBoolValue(((StringValue)leftVal.Value).Value == ((StringValue)rightValue.Value).Value);
								case BinaryOperation.NotEqual:
									return LsnBoolValue.GetBoolValue(((StringValue)leftVal.Value).Value != ((StringValue)rightValue.Value).Value);
								default:
									throw new InvalidOperationException();
							}
					}
					throw new NotImplementedException();
				}

				if (ArgumentTypes != BinaryOperationArgsType.Bool_Bool) return this;
				switch (Operation)
				{
					case BinaryOperation.Equal:
						return leftVal.BoolValue ? Right : new NotExpression(Right);
					case BinaryOperation.NotEqual:
						return leftVal.BoolValue ? new NotExpression(Right) : Right;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else if (Right is LsnValue rightValue && ArgumentTypes == BinaryOperationArgsType.Bool_Bool)
			{
				switch (Operation)
				{
					case BinaryOperation.Equal:
						return rightValue.BoolValue ? Left : new NotExpression(Left);
					case BinaryOperation.NotEqual:
						return rightValue.BoolValue ? new NotExpression(Left) : Left;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return this;
		}

		/// <inheritdoc />
		public override bool IsPure => Left.IsPure && Right.IsPure;

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		protected override void GetOperationInstruction(InstructionList instructions, InstructionGenerationContext context)
		{
			switch (ArgumentTypes)
			{
				case BinaryOperationArgsType.Int_Int:
					switch (Operation)
					{
						case BinaryOperation.Equal:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Eq_I32, 0));
							break;
						case BinaryOperation.NotEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Neq_I32, 0));
							break;
						case BinaryOperation.GreaterThan:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Gt, 0));
							break;
						case BinaryOperation.GreaterThanOrEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Gte, 0));
							break;
						case BinaryOperation.LessThan:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Lt, 0));
							break;
						case BinaryOperation.LessThanOrEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Lte, 0));
							break;
					}
					break;
				case BinaryOperationArgsType.Int_Double:
				case BinaryOperationArgsType.Double_Double:
				case BinaryOperationArgsType.Double_Int:
					switch (Operation)
					{
						case BinaryOperation.Equal:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Eq_F64, 0));
							break;
						case BinaryOperation.NotEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Neq_F64, 0));
							break;
						case BinaryOperation.GreaterThan:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Gt, 0));
							break;
						case BinaryOperation.GreaterThanOrEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Gte, 0));
							break;
						case BinaryOperation.LessThan:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Lt, 0));
							break;
						case BinaryOperation.LessThanOrEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Lte, 0));
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				case BinaryOperationArgsType.String_String:
					switch (Operation)
					{
						case BinaryOperation.Equal:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Eq_Str, 0));
							break;
						case BinaryOperation.NotEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Neq_Str, 0));
							break;
						case BinaryOperation.GreaterThan:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Gt, 0));
							break;
						case BinaryOperation.GreaterThanOrEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Gte, 0));
							break;
						case BinaryOperation.LessThan:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Lt, 0));
							break;
						case BinaryOperation.LessThanOrEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Lte, 0));
							break;
					}
					break;
				case BinaryOperationArgsType.String_Int:
					instructions.AddInstruction(new SimplePreInstruction(OpCode.LoadConst_False, 0));
					break;
				case BinaryOperationArgsType.Bool_Bool:
					switch (Operation)
					{
						case BinaryOperation.Equal:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Eq_I32, 0));
							break;
						case BinaryOperation.NotEqual:
							instructions.AddInstruction(new SimplePreInstruction(OpCode.Neq_I32, 0));
							break;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			throw new InvalidOperationException();
		}
	}
}

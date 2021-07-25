using System;
using System.Collections.Generic;
using System.Linq;
using LSNr;
using LSNr.CodeGeneration;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	/// <summary>
	/// A binary expression for arithmetic.
	/// </summary>
	/// <seealso cref="LsnCore.Expressions.BinaryExpressionBase" />
	public sealed class BinaryArithmeticExpression : BinaryExpressionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryArithmeticExpression"/> class.
		/// </summary>
		/// <param name="left">The left expression.</param>
		/// <param name="right">The right expression.</param>
		/// <param name="operation">The operation.</param>
		public BinaryArithmeticExpression(IExpression left, IExpression right, BinaryOperation operation) : base(
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
			if (Left is LsnValue leftExpr)
			{
				if (Right is LsnValue rightExpr)
				{
					switch (ArgumentTypes)
					{
						case BinaryOperationArgsType.Int_Int:
						{
							switch (Operation)
							{
								case BinaryOperation.Sum:
									return new LsnValue(leftExpr.IntValue + rightExpr.IntValue);
								case BinaryOperation.Difference:
									return new LsnValue(leftExpr.IntValue - rightExpr.IntValue);
								case BinaryOperation.Product:
									return new LsnValue(leftExpr.IntValue * rightExpr.IntValue);
								case BinaryOperation.Quotient:
									return new LsnValue(leftExpr.IntValue / rightExpr.IntValue);
								case BinaryOperation.Modulus:
									return new LsnValue(leftExpr.IntValue % rightExpr.IntValue);
								case BinaryOperation.Power:
									return new LsnValue((int)Math.Pow(leftExpr.IntValue, rightExpr.IntValue));
								case BinaryOperation.And:
									return new LsnValue(leftExpr.IntValue & rightExpr.IntValue);
								case BinaryOperation.Or:
									return new LsnValue(leftExpr.IntValue | rightExpr.IntValue);
								case BinaryOperation.Xor:
									return new LsnValue(leftExpr.IntValue ^ rightExpr.IntValue);
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
						case BinaryOperationArgsType.Int_Double:
						case BinaryOperationArgsType.Double_Double:
						case BinaryOperationArgsType.Double_Int:

							switch (Operation)
							{
								case BinaryOperation.Sum:
									return new LsnValue(leftExpr.DoubleValue + rightExpr.DoubleValue);
								case BinaryOperation.Difference:
									return new LsnValue(leftExpr.DoubleValue - rightExpr.DoubleValue);
								case BinaryOperation.Product:
									return new LsnValue(leftExpr.DoubleValue * rightExpr.DoubleValue);
								case BinaryOperation.Quotient:
									return new LsnValue(leftExpr.DoubleValue / rightExpr.DoubleValue);
								case BinaryOperation.Modulus:
									return new LsnValue(leftExpr.DoubleValue % rightExpr.DoubleValue);
								case BinaryOperation.Power:
									return new LsnValue((int)Math.Pow(leftExpr.DoubleValue, rightExpr.DoubleValue));
								default:
									throw new ArgumentOutOfRangeException();
							}
						default:
							throw new ArgumentOutOfRangeException("");
					}
				}

				switch (ArgumentTypes)
				{
					case BinaryOperationArgsType.Int_Int:
					case BinaryOperationArgsType.Int_Double:
					{
						switch (leftExpr.IntValue)
						{
							case 0:
								switch (Operation)
								{
									case BinaryOperation.Sum:
									case BinaryOperation.Or:
									case BinaryOperation.Xor:
										return Right;
									case BinaryOperation.Difference:
										break;
									case BinaryOperation.Product:
									case BinaryOperation.Quotient:
									case BinaryOperation.Modulus:
									case BinaryOperation.Power:
									case BinaryOperation.And:
										return ArgumentTypes == BinaryOperationArgsType.Int_Int
											? new LsnValue(0)
											: new LsnValue(0.0);
									default:
										throw new ArgumentOutOfRangeException();
								}

								break;
							case 1:
								switch (Operation)
								{
									case BinaryOperation.Product:
										return Right;
									case BinaryOperation.Power:
										return ArgumentTypes == BinaryOperationArgsType.Int_Int
											? new LsnValue(1)
											: new LsnValue(1.0);
								}

								break;
						}
						break;
					}
					case BinaryOperationArgsType.Double_Double:
					case BinaryOperationArgsType.Double_Int:
					{
						if (Math.Abs(leftExpr.DoubleValue) <= double.Epsilon)
						{
							switch (Operation)
							{
								case BinaryOperation.Sum:
									return Right;
								case BinaryOperation.Difference:
									break;
								case BinaryOperation.Product:
								case BinaryOperation.Quotient:
								case BinaryOperation.Modulus:
								case BinaryOperation.Power:
									return new LsnValue(0.0);
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
						else if (Math.Abs(leftExpr.DoubleValue - 1) <= double.Epsilon)
						{
							switch (Operation)
							{
								case BinaryOperation.Product:
									return Right;
								case BinaryOperation.Power:
									return new LsnValue(1.0);
							}
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}

				return this;
			}

			if (!(Right is LsnValue rightValue)) {return this;}
			switch (ArgumentTypes)
			{
				case BinaryOperationArgsType.Int_Int:
				case BinaryOperationArgsType.Double_Int:
				{
					switch (rightValue.IntValue)
					{
						case 0:
						{
							switch (Operation)
							{
								case BinaryOperation.Sum:
								case BinaryOperation.Difference:
								case BinaryOperation.Or:
								case BinaryOperation.Xor:
									return Left;
								case BinaryOperation.Product:
								case BinaryOperation.And:
									return ArgumentTypes == BinaryOperationArgsType.Int_Int
										? new LsnValue(0)
										: new LsnValue(0.0);
								case BinaryOperation.Quotient:
								case BinaryOperation.Modulus:
									throw new DivideByZeroException();
								case BinaryOperation.Power:
									return ArgumentTypes == BinaryOperationArgsType.Int_Int
										? new LsnValue(1)
										: new LsnValue(1.0);
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
						case 1:
						{
							switch (Operation)
							{
								case BinaryOperation.Product:
								case BinaryOperation.Quotient:
								case BinaryOperation.Modulus:
								case BinaryOperation.Power:
									return Left;
							}
							break;
						}
					}
					break;
				}
				case BinaryOperationArgsType.Int_Double:
				case BinaryOperationArgsType.Double_Double:
				{
					if (Math.Abs(rightValue.DoubleValue) < double.Epsilon)
					{
						switch (Operation)
						{
							case BinaryOperation.Sum:
							case BinaryOperation.Difference:
								return Left;
							case BinaryOperation.Product:
								return new LsnValue(0.0);
							case BinaryOperation.Quotient:
							case BinaryOperation.Modulus:
								throw new DivideByZeroException();
							case BinaryOperation.Power:
								return new LsnValue(1.0);
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
					if (Math.Abs(rightValue.DoubleValue - 1) <= double.Epsilon)
					{
						switch (Operation)
						{
							case BinaryOperation.Product:
							case BinaryOperation.Quotient:
							case BinaryOperation.Modulus:
							case BinaryOperation.Power:
								return Left;
						}
					}

					if (Operation == BinaryOperation.Power)
					{
						if (Math.Abs(rightValue.DoubleValue - 0.5) <= double.Epsilon)
						{
							return new FunctionCall(ResourceManager.Sqrt, new[] {Left});
						}

						if (Math.Abs(rightValue.DoubleValue - -0.5) <= double.Epsilon)
						{
							return new FunctionCall(ResourceManager.InvSqrt, new[] { Left });
						}
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			return this;
		}

		/// <inheritdoc />
		public override bool IsPure => Left.IsPure && Right.IsPure;

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}

		/// <inheritdoc />
		protected override void GetOperationInstruction(InstructionList instructions, InstructionGenerationContext context)
		{
			var subcontext = context.WithContext(ExpressionContext.SubExpression);
			Left.GetInstructions(instructions, subcontext);
			Right.GetInstructions(instructions, subcontext);
			instructions.AddInstruction(new SimplePreInstruction(GetOpCode(), 0));
		}

		/// <summary>
		/// Gets the op code for the operation.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// </exception>
		/// <exception cref="System.NotImplementedException">
		/// Bitwise operations are currently unsupported.
		/// </exception>
		private OpCode GetOpCode()
		{
			switch (Operation)
			{
				case BinaryOperation.Sum:
					return OpCode.Add;
				case BinaryOperation.Difference:
					return OpCode.Sub;
				case BinaryOperation.Product:
					return OpCode.Mul;
				case BinaryOperation.Quotient:
					switch (ArgumentTypes)
					{
						case BinaryOperationArgsType.Int_Int:
							return OpCode.Div_I32;
						case BinaryOperationArgsType.Int_Double:
						case BinaryOperationArgsType.Double_Double:
						case BinaryOperationArgsType.Double_Int:
							return OpCode.Div_F64;
						default:
							throw new ArgumentOutOfRangeException();
					}
				case BinaryOperation.Modulus:
					switch (ArgumentTypes)
					{
						case BinaryOperationArgsType.Int_Int:
							return OpCode.Rem_I32;
						case BinaryOperationArgsType.Int_Double:
						case BinaryOperationArgsType.Double_Double:
						case BinaryOperationArgsType.Double_Int:
							return OpCode.Rem_F64;
						default:
							throw new ArgumentOutOfRangeException();
					}
				case BinaryOperation.Power:
					return OpCode.Pow;
				case BinaryOperation.And:
					throw new NotImplementedException("No bitwise AND instruction code.");
				case BinaryOperation.Or:
					throw new NotImplementedException("No bitwise OR instruction code.");
				case BinaryOperation.Xor:
					throw new NotImplementedException("No bitwise XOR instruction code.");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}

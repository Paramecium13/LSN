using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syroot.BinaryData;
using LSNr.CodeGeneration;
using LSNr;

namespace LsnCore.Expressions
{
	public enum BinaryOperation : byte { Sum, Difference, Product, Quotient, Modulus, Power, LessThan, LessThanOrEqual, GreaterThan,GreaterThanOrEqual,Equal,NotEqual,And,Or,Xor}

	public enum BinaryOperationArgsType : byte { Int_Int, Int_Double,Double_Double,Double_Int,String_String,String_Int,Bool_Bool}

	/// <summary>
	/// An expression that performs some mathematical operation on the results of its two sub-expressions.
	/// </summary>
	/// <seealso cref="LsnCore.Expressions.Expression" />
	public abstract class BinaryExpressionBase : Expression
	{
		/// <summary>
		/// The operation.
		/// </summary>
		public BinaryOperation Operation { get; }

		/// <summary>
		/// The types of the two sub-expressions.
		/// </summary>
		public BinaryOperationArgsType ArgumentTypes { get; }

		/// <summary>
		/// The left hand side of this expression.
		/// </summary>
		public IExpression Left { get; protected set; }

		/// <summary>
		/// The right hand side of this expression.
		/// </summary>
		public IExpression Right { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryExpressionBase"/> class.
		/// </summary>
		/// <param name="operation">The operation.</param>
		/// <param name="argumentTypes">The argument types.</param>
		protected BinaryExpressionBase(BinaryOperation operation, BinaryOperationArgsType argumentTypes)
		{
			Operation = operation;
			ArgumentTypes = argumentTypes;
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			var subContext = context.WithContext(ExpressionContext.SubExpression);
			Left.GetInstructions(instructions, subContext);
			Right.GetInstructions(instructions, subContext);
			GetOperationInstruction(instructions, context);
		}

		/// <summary>
		/// Gets the <see cref="BinaryArithmeticExpression"/> from the two provided <see cref="TypeId"/>s.
		/// </summary>
		/// <param name="left">The type of the left expression.</param>
		/// <param name="right">The type of the right expression.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public static BinaryOperationArgsType GetArgTypes(TypeId left, TypeId right)
		{
			switch (left.Name)
			{
				case "int":
					switch (right.Name)
					{
						case "int": return BinaryOperationArgsType.Int_Int;
						case "double": return BinaryOperationArgsType.Int_Double;
						default:
							break;
					}
					break;
				case "double":
					switch (right.Name)
					{
						case "int": return BinaryOperationArgsType.Double_Int;
						case "double": return BinaryOperationArgsType.Double_Double;
						default:
							break;
					}
					break;
				case "string":
					switch (right.Name)
					{
						case "string": return BinaryOperationArgsType.String_String;
						case "int": return BinaryOperationArgsType.String_Int;
						default:
							break;
					}
					break;
				case "bool":
					/*switch (right.Name)
					{
						case "bool": return BinaryOperationArgTypes.Bool_Bool;
						default:
							break;
					}
					break;*/
					return BinaryOperationArgsType.Bool_Bool;
				default:
					return BinaryOperationArgsType.Bool_Bool;
			}

			throw new InvalidOperationException();
		}

		/// <summary>
		/// Generate the instructions that perform the operation itself.
		/// </summary>
		/// <param name="instructions">The instructions.</param>
		/// <param name="context">The context.</param>
		protected abstract void GetOperationInstruction(InstructionList instructions, InstructionGenerationContext context);

		/// <inheritdoc/>
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Left;
			foreach (var expr in Left.SelectMany(e => e))
				yield return expr;
			yield return Right;
			foreach (var expr in Right.SelectMany(e => e))
				yield return expr;
		}
	}
}

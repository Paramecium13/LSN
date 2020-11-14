using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using LSNr.CodeGeneration;
using LSNr;

namespace LsnCore.Expressions
{
	/// <summary>
	/// A logical and expression.
	/// </summary>
	/// <remarks>
	/// This is an intermediate expression: calling <see cref="Expression.Fold"/> returns either a constant,
	/// a single side of this expression, or a <see cref="MultiAndExpression"/>.
	/// </remarks>
	/// <seealso cref="LsnCore.Expressions.BinaryExpressionBase" />
	public sealed class LogicalAndExpression : BinaryExpressionBase
	{
		/// <inheritdoc />
		public LogicalAndExpression(BinaryOperationArgsType argumentTypes, IExpression left, IExpression right) : base(
			BinaryOperation.And, argumentTypes)
		{
			Left = left;
			Right = right;
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => false;

		/// <inheritdoc />
		public override IExpression Fold()
		{
			Left = Left.Fold();
			Right = Right.Fold();
			var expressions = new List<IExpression>();
			switch (Left)
			{
				case LsnValue leftConst when !leftConst.BoolValue:
					return LsnBoolValue.GetBoolValue(false);
				case LsnValue _ when Right is LsnValue rConst:
					return LsnBoolValue.GetBoolValue(rConst.BoolValue);
				case LsnValue _:
					return Right;
				case LogicalAndExpression leftAnd:
					expressions.Add(leftAnd.Left);
					expressions.Add(leftAnd.Right);
					break;
				case MultiAndExpression leftMultiAnd:
					expressions.AddRange(leftMultiAnd.Expressions);
					break;
				default:
					expressions.Add(Left);
					break;
			}

			switch (Right)
			{
				case LsnValue rightConst when rightConst.BoolValue:
					return Left;
				case LsnValue _:
					return LsnBoolValue.GetBoolValue(false);
				case LogicalAndExpression rightAnd:
					expressions.Add(rightAnd.Left);
					expressions.Add(rightAnd.Right);
					break;
				case MultiAndExpression rightMultiAnd:
					expressions.AddRange(rightMultiAnd.Expressions);
					break;
				default:
					expressions.Add(Right);
					break;
			}

			if (expressions.Count < 2)
			{
				throw new InvalidOperationException("This probably can't happen...");
			}
			// ToDo: should I ever return this type?
			// if(expressions.Count == 2) { return new LogicalAndExpression(ArgumentTypes,expressions[0],expressions[1]);}
			return new MultiAndExpression(expressions.ToArray());
		}

		/// <inheritdoc />
		public override bool IsPure => Left.IsPure && Right.IsPure;

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Left;
			foreach (var expr in Left.SelectMany(e => e))
			{
				yield return expr;
			}
			yield return Right;
			foreach (var expr in Right.SelectMany(e => e))
			{
				yield return expr;
			}
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			var subContext = context.WithContext(ExpressionContext.ShortCircuitOnFalse);
			switch (context.Context)
			{
				case ExpressionContext.ShortCircuitOnFalse:
				{
					Left.GetInstructions(instructions, subContext);
					if (context.WantsBoolReturnValue)
					{
						instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False_NoPop,
							context.ShortCircuitLabelA));
					}
					else
					{
						instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False,
							context.ShortCircuitLabelA));
					}

					Right.GetInstructions(instructions, subContext);
					break;
				}
				case ExpressionContext.ShortCirtuitOnTrue:
				{
					var label = context.LabelFactory.CreateLabel();
					Left.GetInstructions(instructions, subContext);
					if (context.WantsBoolReturnValue)
					{
						instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False_NoPop, label));
					}
					else
					{
						instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False, label));
					}

					Right.GetInstructions(instructions, subContext);
					instructions.SetNextLabel(label);
					break;
				}
				case ExpressionContext.JumpTrueStatement:
				{
					subContext.WantsBoolReturnValue = false;
					Left.GetInstructions(instructions, subContext);
					instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False,
						context.ShortCircuitLabelB));
					Right.GetInstructions(instructions, subContext);
					break;
				}
				case ExpressionContext.JumpFalseStatement:
				{
					subContext.WantsBoolReturnValue = false;
					Left.GetInstructions(instructions, subContext);
					instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False,
						context.ShortCircuitLabelA));
					Right.GetInstructions(instructions, subContext);
					break;
				}
				default: // want the return value...
				{
					var label = context.LabelFactory.CreateLabel();
					subContext.WantsBoolReturnValue = true;
					subContext.ShortCircuitLabelA = label;
					Left.GetInstructions(instructions, subContext);
					instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False_NoPop, label));
					Right.GetInstructions(instructions, subContext);
					instructions.SetNextLabel(label);
					break;
				}

			}
		}

		/// <inheritdoc />
		protected override void GetOperationInstruction(InstructionList instructions, InstructionGenerationContext context)
		{
			throw new InvalidOperationException();
		}
	}

	/// <summary>
	/// An expression that consists of multiple boolean expressions AND`ed together.
	/// </summary>
	/// <seealso cref="LsnCore.Expressions.Expression" />
	public sealed class MultiAndExpression : Expression
	{
		/// <summary>
		/// The expressions.
		/// </summary>
		internal IExpression[] Expressions { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiAndExpression"/> class.
		/// </summary>
		/// <param name="expressions">The expressions.</param>
		public MultiAndExpression(IExpression[] expressions)
		{
			Expressions = expressions;
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => false;

		/// <inheritdoc />
		public override IExpression Fold()
		{
			var expr = Expressions.Select(e => e.Fold()).ToArray();
			if (!expr.Any(e => e is LsnValue)) return new MultiAndExpression(expr);
			
			if (expr.OfType<LsnValue>().Any(e => !e.BoolValue))
			{
				return LsnBoolValue.GetBoolValue(false);
			}

			if (expr.All(e => e is LsnValue))
			{
				return LsnBoolValue.GetBoolValue(true);
			}

			return new MultiAndExpression(expr.Where(e => !(e is LsnValue)).ToArray());
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			InstructionLabel trueLabel = null;
			InstructionLabel falseLabel;
			var addFalseLabel = false;
			switch (context.Context)
			{
				case ExpressionContext.JumpTrueStatement:
					falseLabel = context.ShortCircuitLabelB;
					trueLabel = context.ShortCircuitLabelA;
					break;
				case ExpressionContext.JumpFalseStatement:
					falseLabel = context.ShortCircuitLabelA;
					trueLabel = context.ShortCircuitLabelB;
					break;

				default: // Wants bool value
					falseLabel = context.LabelFactory.CreateLabel();
					addFalseLabel = true;
					break;
			}

			var subcontext = context.WithContext(ExpressionContext.ShortCircuitOnFalse);
			subcontext.WantsBoolReturnValue = addFalseLabel; 
				// If we have to add the false label, that means this isn't (directly) under a conditional jump statement.
			for (var index = 0; index < Expressions.Length - 1; index++)
			{
				var expression = Expressions[index];
				expression.GetInstructions(instructions, subcontext);
				var jmpOpCode = context.WantsBoolReturnValue || addFalseLabel ? OpCode.Jump_False : OpCode.Jump_False_NoPop;
				instructions.AddInstruction(new TargetedPreInstruction(jmpOpCode, falseLabel));
			}

			var subcontextEnd = context.WithContext(ExpressionContext.ShortCircuitOnFalse);
			subcontext.WantsBoolReturnValue = true;
			// Last expression (no short circuiting)
			{
				var expression = Expressions[Expressions.Length - 1];
				expression.GetInstructions(instructions, subcontextEnd);
				if (trueLabel != null)
				{
					instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_True, trueLabel));
				}

				if (!addFalseLabel)
				{
					instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump, falseLabel));
				}
			}

			if (addFalseLabel)
			{
				instructions.SetNextLabel(falseLabel);
			}
		}

		/// <inheritdoc />
		public override bool IsPure => Expressions.All(e => e.IsPure);

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			foreach (var expr in Expressions)
			{
				yield return expr;
				foreach (var nested in expr.SelectMany(e => e))
				{
					yield return nested;
				}
			}
		}
	}

	/// <summary>
	/// An expression that consists of multiple boolean expressions OR`ed together.
	/// </summary>
	/// <seealso cref="LsnCore.Expressions.Expression" />
	public sealed class MultiOrExpression : Expression
	{
		private readonly IExpression[] Expressions;

		public MultiOrExpression(IExpression[] expressions)
		{
			Expressions = expressions;
		}

		/// <inheritdoc />
		public override bool IsReifyTimeConst() => false;

		/// <inheritdoc />
		public override IExpression Fold()
		{
			var expr = Expressions.Select(e => e.Fold()).ToArray();
			if (!expr.Any(e => e is LsnValue)) return new MultiOrExpression(expr);

			if (expr.OfType<LsnValue>().Any(e => e.BoolValue))
			{
				return LsnBoolValue.GetBoolValue(true);
			}

			if (expr.All(e => e is LsnValue))
			{
				return LsnBoolValue.GetBoolValue(false);
			}

			return new MultiOrExpression(expr.Where(e => !(e is LsnValue)).ToArray());
		}

		/// <inheritdoc />
		public override void GetInstructions(InstructionList instructions, InstructionGenerationContext context)
		{
			InstructionLabel trueLabel;
			InstructionLabel falseLabel = null;
			var addTrueLabel = false;
			switch (context.Context)
			{
				case ExpressionContext.JumpTrueStatement:
					falseLabel = context.ShortCircuitLabelB;
					trueLabel = context.ShortCircuitLabelA;
					break;
				case ExpressionContext.JumpFalseStatement:
					falseLabel = context.ShortCircuitLabelA;
					trueLabel = context.ShortCircuitLabelB;
					break;

				default: // Wants bool value
					trueLabel = context.LabelFactory.CreateLabel();
					addTrueLabel = true;
					break;
			}

			var subcontext = context.WithContext(ExpressionContext.ShortCirtuitOnTrue);
			subcontext.WantsBoolReturnValue = addTrueLabel;
			// If we have to add the true label, that means this isn't (directly) under a conditional jump statement.
			for (var index = 0; index < Expressions.Length - 1; index++)
			{
				var expression = Expressions[index];
				expression.GetInstructions(instructions, subcontext);
				var jmpOpCode = context.WantsBoolReturnValue || addTrueLabel ? OpCode.Jump_True : OpCode.Jump_True_NoPop;
				instructions.AddInstruction(new TargetedPreInstruction(jmpOpCode, trueLabel));
			}

			var subcontextEnd = context.WithContext(ExpressionContext.ShortCirtuitOnTrue);
			subcontext.WantsBoolReturnValue = true;
			// Last expression (no short circuiting)
			{
				var expression = Expressions[Expressions.Length - 1];
				expression.GetInstructions(instructions, subcontextEnd);
				if (falseLabel != null)
				{
					instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump_False, falseLabel));
				}

				if (!addTrueLabel)
				{
					instructions.AddInstruction(new TargetedPreInstruction(OpCode.Jump, trueLabel));
				}
			}

			if (addTrueLabel)
			{
				instructions.SetNextLabel(trueLabel);
			}
		}

		/// <inheritdoc />
		public override bool IsPure => Expressions.All(e => e.IsPure);

		/// <inheritdoc />
		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			foreach (var expr in Expressions)
			{
				yield return expr;
				foreach (var nested in expr.SelectMany(e => e))
				{
					yield return nested;
				}
			}
		}
	}
}
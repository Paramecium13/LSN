using LsnCore.Expressions;
using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using LSNr.CodeGeneration;

namespace LsnCore.Statements
{
	/// <summary>
	/// A statement that constructs a <see cref="ScriptObject"/> and attaches it to an <see cref="IHostInterface"/>.
	/// </summary>
	/// <seealso cref="LsnCore.Statements.Statement" />
	public class AttachStatement : Statement
	{
		/// <summary>
		/// The script class
		/// </summary>
		private readonly TypeId ScriptClass;

		/// <summary>
		/// The constructor arguments
		/// </summary>
		private readonly IExpression[] ConstructorArguments;

		/// <summary>
		/// The expression that returns the host
		/// </summary>
		private IExpression HostExpression;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttachStatement"/> class.
		/// </summary>
		/// <param name="scriptClass">The script class.</param>
		/// <param name="args">The arguments.</param>
		/// <param name="host">The host.</param>
		public AttachStatement(TypeId scriptClass, IExpression[] args, IExpression host)
		{
			ScriptClass = scriptClass; ConstructorArguments = args; HostExpression = host;
		}

		/// <inheritdoc />
		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			for (int i = 0; i < ConstructorArguments.Length; i++)
			{
				if (ConstructorArguments[i].Equals(oldExpr))
					ConstructorArguments[i] = newExpr;
				else ConstructorArguments[i].Replace(oldExpr, newExpr);
			}
			if (HostExpression.Equals(oldExpr))
				HostExpression = newExpr;
			else HostExpression.Replace(oldExpr, newExpr);
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.AttachNewScriptObject);

			resourceSerializer.WriteTypeId(ScriptClass, writer);

			writer.Write((byte)ConstructorArguments.Length);
			for (int i = 0; i < ConstructorArguments.Length; i++)
				ConstructorArguments[i].Serialize(writer, resourceSerializer);

			HostExpression.Serialize(writer, resourceSerializer);
		}

		/// <inheritdoc />
		public override IEnumerator<IExpression> GetEnumerator()
		{
			foreach (var arg in ConstructorArguments)
			{
				yield return arg;
				foreach (var expr in arg.SelectMany(e => e))
					yield return expr;
			}
			yield return HostExpression;
			foreach (var expr in HostExpression.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		protected override void GetInstructions(InstructionList instructionList, string target, InstructionGenerationContext context)
		{
			foreach (var argument in ConstructorArguments)
			{
				argument.GetInstructions(instructionList, context.WithContext(ExpressionContext.Parameter_Default));
			}

			HostExpression.GetInstructions(instructionList, context.WithContext(ExpressionContext.SubExpression));
			instructionList.AddInstruction(new TypeTargetedInstruction(OpCode.ConstructAndAttachScriptClass, ScriptClass));
		}

		/// <inheritdoc />
		protected override IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context)
		{
			throw new NotImplementedException();
		}
	}
}

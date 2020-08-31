using Syroot.BinaryData;
using System.Collections.Generic;
using LsnCore.Expressions;
using System.Collections;
using System.Linq;
using LSNr;
using MoreLinq;
using LSNr.CodeGeneration;

namespace LsnCore.Statements
{
	/// <summary>
	/// An LSN statement.
	/// </summary>
	public abstract class Statement : Component, IEnumerable<IExpression>
	{
		/// <inheritdoc/>
		public abstract IEnumerator<IExpression> GetEnumerator();

		/// <summary>
		/// Serializes this statement with the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="resourceSerializer">The resource serializer.</param>
		internal abstract void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer);

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets the <see cref="PreInstruction"/>s that make up this statement.
		/// </summary>
		/// <param name="labels"> The labels applied to this statement. </param>
		/// <param name="target"> The label that this statement targets (if any). </param>
		/// <param name="context"></param>
		// ReSharper disable once LocalSuppression
		// ReSharper disable once UnusedMember.Global
		internal IEnumerable<PreInstruction> GetInstructions(IList<string> labels, string target, InstructionGenerationContext context)
		{
			return GetInstructions(target, context).Pipe(instuction =>
			{
				if (!(labels?.Any() ?? false)) return;
				instuction.Labels.AddRange(labels.Select(context.LabelFactory.GetLabel));
				labels = null;
			});
		}

		/// <summary>
		/// Gets the instructions.
		/// </summary>
		/// <param name="instructionList">The instruction list.</param>
		/// <param name="target">The label that this statement targets (if any).</param>
		/// <param name="context">The context.</param>
		protected abstract void GetInstructions(InstructionList instructionList, string target,
			InstructionGenerationContext context);

		protected abstract IEnumerable<PreInstruction> GetInstructions(string target, InstructionGenerationContext context);
	}
}

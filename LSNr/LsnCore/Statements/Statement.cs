using Syroot.BinaryData;
using System.Collections.Generic;
using LsnCore.Expressions;
using System.Collections;
using System.Linq;
using LSNr;
using MoreLinq;

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

		internal IEnumerable<PreInstruction> GetInstructions(IList<string> labels, string target)
		{
			return GetInstructions(target).Pipe(instuction =>
			{
				if (!(labels?.Any() ?? false)) return;
				instuction.Labels.AddRange(labels.Select(l => new InstructionLabel(l)));
				labels = null;
			});
		}

		protected abstract IEnumerable<PreInstruction> GetInstructions(string target);
	}
}

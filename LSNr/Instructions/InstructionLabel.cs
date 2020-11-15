using System.Collections.Generic;

namespace LSNr
{
	/// <summary>
	/// A label for <see cref="PreInstruction"/>s. Its <see cref="Index"/> will be resolved when all instructions are ready...
	/// </summary>
	public class InstructionLabel
	{
		/// <summary>
		/// The index of the instruction this label points to.
		/// </summary>
		public ushort Index { get; set; }

		/// <summary>
		/// The name (optional).
		/// </summary>
		public string Name { get; }
		
		private InstructionLabel(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Creates <see cref="InstructionLabel"/>s...
		/// </summary>
		public class Factory
		{
			private readonly Dictionary<string, InstructionLabel> LabelsLookup =
				new Dictionary<string, InstructionLabel>();

			// ReSharper disable once UnusedMember.Global
			public IEnumerable<InstructionLabel> Labels => LabelsLookup.Values;

			private uint Count;

			// ReSharper disable once EmptyConstructor
			public Factory() { }

			/// <summary>
			/// Gets the label with the given <paramref name="name"/> if it already exists, otherwise creates it.
			/// </summary>
			/// <param name="name">The name.</param>
			/// <returns></returns>
			public InstructionLabel GetLabel(string name)
			{
				if (LabelsLookup.TryGetValue(name, out var label))
				{
					return label;
				}

				label = new InstructionLabel(name);
				LabelsLookup.Add(name, label);
				return label;
			}

			public InstructionLabel CreateLabel() => new InstructionLabel($"L{Count++}");
		}
	}
}
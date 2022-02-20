using System;
using LsnCore;
using LsnCore.Types;

namespace LSNr
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class InstructionResolutionContext
	{
		private readonly TypeId[] DefinedTypes;

		/// <summary>
		/// Gets the file header factory.
		/// </summary>
		public ObjectFileHeaderFactory FileHeaderFactory { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="InstructionResolutionContext"/> class.
		/// </summary>
		/// <param name="fileHeaderFactory">The file header factory.</param>
		public InstructionResolutionContext(ObjectFileHeaderFactory fileHeaderFactory, TypeId[] definedTypes)
		{
			FileHeaderFactory = fileHeaderFactory;
			DefinedTypes = definedTypes;
		}

		/// <summary>
		/// Gets the index of the locally defined procedure.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public ushort GetLocalProcedureIndex(Function function) => throw new NotImplementedException();

		/// <summary>
		/// Get the index of <paramref name="type"/>, which may or may not be defined locally.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public short GetTypeIndex(TypeId type)
		{
			if (TryGetLocalTypeIndex(type, out var index))
			{
				return (short)-index;
			}

			return FileHeaderFactory.AddType(type);
		}

		private bool TryGetLocalTypeIndex(TypeId type, out short index)
		{
			index = checked((short)Array.IndexOf(DefinedTypes, type));
			if (index >= 0) return true;
			index = -1;
			return false;
		}
	}
}
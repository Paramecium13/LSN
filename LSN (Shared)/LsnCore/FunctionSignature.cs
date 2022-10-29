using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LsnCore
{
	[Serializable]
	public sealed class FunctionSignature : IEquatable<FunctionSignature>
	{
		/// <summary>
		/// The name of the function.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The function's parameters.
		/// </summary>
		public readonly IReadOnlyList<Parameter> Parameters;

		/// <summary>
		/// The return <see cref="LsnType"/> Identifier of the function,
		/// or <see langword="null"/> if it does not have a return type.
		/// </summary>
		public readonly TypeId ReturnType;

		/// <summary>Initializes a new instance of the <a onclick="return false;" href="FunctionSignature" originaltag="see">FunctionSignature</a> class.</summary>
		/// <param name="parameters">The function's parameters.</param>
		/// <param name="name">The name of the function.</param>
		/// <param name="returnType">The function's return type, or null if it does not have one.</param>
		public FunctionSignature(IReadOnlyList<Parameter> parameters,string name, TypeId returnType)
		{
			Parameters = parameters ?? new List<Parameter>(); Name = name; ReturnType = returnType;
		}

		/// <inheritdoc/>
		public bool Equals(FunctionSignature other)
			=> Name == other.Name && ReturnType == other.ReturnType && Parameters.Count == other.Parameters.Count &&
			Parameters.All(p => p.Equals(other.Parameters[p.Index]));

		// ToDo: Can this be used in parsing?
		/*public IExpression[] CreateArgsArrayForMethod(IList<Tuple<string, IExpression>> args)
		{
			var argsArray = new IExpression[Parameters.Count];

			if (args.Count > 0 && args.Any(a => !string.IsNullOrEmpty(a.Item1)))
			{
				var dict = new Dictionary<string, IExpression>(args.Count);//args.ToDictionary(t => t.Item1, t => t.Item2);

				for (int i = 0; i < args.Count; i++)
				{
					if (!string.IsNullOrEmpty(args[i].Item1))
						dict.Add(args[i].Item1, args[i].Item2);
					else dict.Add(Parameters[i].Name, args[i].Item2);
				}

				foreach (var param in Parameters.Where(p => p.Index != 0))
					argsArray[param.Index] = dict.ContainsKey(param.Name) ? dict[param.Name] : param.DefaultValue;
			}
			else
			{
				for (int i = 1; i < args.Count; i++)
					argsArray[i] = args[i - 1].Item2;

				for (int i = args.Count - 1; i < Parameters.Count; i++)
					argsArray[i] = Parameters[i].DefaultValue;
			}
			return argsArray;
		}*/

		/// <summary>
		/// Serializes this <see cref="FunctionSignature"/> using the specified <paramref name="writer"/> and <paramref name="resourceSerializer"/>.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="resourceSerializer">The resource serializer.</param>
		public void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(Name);
			writer.Write(ReturnType?.Name ?? "");
			writer.Write((ushort)Parameters.Count);
			foreach (var param in Parameters)
				param.Serialize(writer, resourceSerializer);
		}

		/// <summary>
		/// Reads a <see cref="FunctionSignature"/> using the specified <paramref name="reader"/> and <paramref name="typeContainer"/>.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="typeContainer">The type container.</param>
		/// <returns></returns>
		public static FunctionSignature Read(BinaryStream reader, ITypeIdContainer typeContainer)
		{
			var name = reader.ReadString();
			var retTName = reader.ReadString();
			if (retTName == "")
				retTName = null;
			var nParams = reader.ReadUInt16();
			var parameters = new List<Parameter>(nParams);
			for (ushort i = 0; i < nParams; i++)
			{
				parameters.Add(Parameter.Read(i, reader, typeContainer));
			}
			return new FunctionSignature(parameters, name, retTName == null ? null: typeContainer.GetTypeId(retTName));
		}
	}

}

using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public sealed class FunctionSignature : IEquatable<FunctionSignature>
	{
		public readonly string Name;
		public readonly IReadOnlyList<Parameter> Parameters;
		public readonly TypeId ReturnType;

		public FunctionSignature(IList<Parameter> parameters,string name, TypeId returnType)
		{
			Parameters = parameters.ToList(); Name = name; ReturnType = returnType;
		}

		public bool Equals(FunctionSignature other)
		{
			if (Name != other.Name)
				return false;
			if (ReturnType != other.ReturnType)
				return false;
			if (Parameters.Count != other.Parameters.Count)
				return false;
			for (int i = 0; i < Parameters.Count; i++)
				if (!Parameters[i].Equals(other.Parameters[i]))
					return false;
			return true;
		}

		public IExpression[] CreateArgsArray(IList<Tuple<string, IExpression>> args)
		{
			var argsArray = new IExpression[Parameters.Count];

			if (args.Count > 0 && args.Any(a => !string.IsNullOrEmpty(a.Item1)))
			{
				var dict = new Dictionary<string, IExpression>(args.Count);//args.ToDictionary(t => t.Item1, t => t.Item2);

				for (int i = 0; i < args.Count; i++)
				{
					if (!string.IsNullOrEmpty( args[i].Item1 ))
						dict.Add(args[i].Item1, args[i].Item2);
					else dict.Add(Parameters[i].Name, args[i].Item2);
				}


				foreach (var param in Parameters)
					argsArray[param.Index] = dict.ContainsKey(param.Name) ? dict[param.Name] : param.DefaultValue;
			}
			else
			{
				for (int i = 0; i < args.Count; i++)
					argsArray[i] = args[i].Item2;

				for (int i = args.Count; i < Parameters.Count; i++)
					argsArray[i] = Parameters[i].DefaultValue;
			}
			return argsArray;
		}

		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write(Name);
			writer.Write(ReturnType?.Name ?? "");
			writer.Write((ushort)Parameters.Count);
			foreach (var param in Parameters)
				param.Serialize(writer);
		}

		public static FunctionSignature Read(BinaryDataReader reader, ITypeIdContainer typeContainer)
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

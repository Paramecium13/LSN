using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public interface ITypeIdContainer
	{
		TypeId GetTypeId(string name);
	}

	public interface ITypeContainer : ITypeIdContainer
	{
		LsnType GetType(string name);
		bool TypeExists(string name);
		bool GenericTypeExists(string name);
		GenericType GetGenericType(string name);
	}

	/// <summary>
	/// Contains functions, structs, constants, and macros/inlines.
	/// </summary>
	[Serializable]
	public class LsnResourceThing : LsnScriptBase
	{
		private LsnEnvironment Environment = null;

		public LsnEnvironment GetEnvironment()
		{
			if (Environment == null) Environment = new LsnEnvironment(this);
			return Environment;
		}


		public void Serialize(Stream stream)
		{
			using (var writer = new BinaryDataWriter(stream))
			{
				writer.Write(0x5f3759df); // Signature.


				writer.Write((ushort)Includes.Count);
				foreach (var inc in Includes)
					writer.Write(inc);
				writer.Write((ushort)Usings.Count);
				foreach (var u in Usings)
					writer.Write(u);



			}
		}


		public static LsnResourceThing Read(Stream stream, Func<string,LsnResourceThing> resourceLoader)
		{

		}

	}
}

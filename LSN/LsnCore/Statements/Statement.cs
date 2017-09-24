using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Serialization;

namespace LsnCore.Statements
{
	[Serializable]
	public abstract class Statement : Component
	{

		internal abstract void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer);
	}
}

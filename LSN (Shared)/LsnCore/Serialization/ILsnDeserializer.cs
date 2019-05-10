using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	public interface ILsnDeserializer
	{
		string GetString(uint index);

		void LoadScriptClassReference(uint id, Action<LsnValue> setter);

		void LoadReference(uint id, Action<LsnValue> setter);
	}
}

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

		bool LoadScriptClassReference(uint id, Action<LsnValue> setter);

		bool LoadReference(uint id, Action<LsnValue> setter);

		bool LoadString(uint id, Action<LsnValue> setter);

		bool LoadHostInterface(uint id, Action<IHostInterface> setter);

		LsnValue[] GetArray(int size);
	}
}

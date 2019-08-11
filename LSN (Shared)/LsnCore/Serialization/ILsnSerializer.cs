using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	interface ILsnSerializer
	{
		uint SaveString(string value);

		uint SaveScriptObject(ScriptObject scriptObject);

		uint SaveList(ILsnValue list);

		uint SaveVector(ILsnValue vector);

		uint SaveRecord(ILsnValue record);

		void SaveHost(IHostInterface hostInterface);
	}
}

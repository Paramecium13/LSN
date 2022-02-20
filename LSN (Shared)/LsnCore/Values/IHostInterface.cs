using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Values;

namespace LsnCore.Runtime.Values
{
	public interface IHostInterface : ILsnValue
	{
		uint NumericId { get; }

		string TextId { get; }

		uint AttachScriptObject(ScriptObject scriptObject, out string strId);

		void DetachScriptObject(ScriptObject scriptObject);

		LsnValue CallMethod(string name, LsnValue[] arguments);

		void SubscribeToEvent(string eventName, ScriptObject eventListener, int priority);

		void UnsubscribeToEvent(string eventName, ScriptObject eventListener);
	}
}

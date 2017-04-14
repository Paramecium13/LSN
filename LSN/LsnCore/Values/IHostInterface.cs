using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Values
{
	public interface IHostInterface : ILsnValue
	{
		LsnValue CallMethod(string name, LsnValue[] arguments);

		void SubscribeToEvent(string eventName, ScriptObject eventListener);

		void UnsubscribeToEvent(string eventName, ScriptObject eventListener);
	}
}

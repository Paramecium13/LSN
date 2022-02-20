using System.Threading.Tasks;
using LsnCore.Values;

namespace LsnCore.Runtime.Values
{
	public interface IEventInstance
	{
		//Task Fire(LsnValue[] args);
		void Subscribe(ScriptObject obj);
		void Unsubscribe(ScriptObject obj);
	}
}
using System.Threading.Tasks;

namespace LsnCore.Values
{
	public interface IEventInstance
	{
		/// <summary>
		/// The task returned by this method is mainly for debugging purposes.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		Task Fire(LsnValue[] args);
		void Subscribe(ScriptObject obj);
		void Unsubscribe(ScriptObject obj);
	}
}
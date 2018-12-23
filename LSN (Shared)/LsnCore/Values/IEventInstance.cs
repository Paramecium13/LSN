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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		Task Fire(LsnValue[] args);
		void Subscribe(ScriptObject obj);
		void Unsubscribe(ScriptObject obj);
	}
}
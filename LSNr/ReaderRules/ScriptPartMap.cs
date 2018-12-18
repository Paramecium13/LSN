using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Utilities;

namespace LSNr.ReaderRules
{
	class ScriptPartMap<TPart, TSrc> : IEnumerable<(string name, TPart Part, TSrc Source)>
	{
		private class Entry
		{
			internal readonly TPart Part;
			internal readonly TSrc Source;
			internal Entry(TPart part, TSrc src) { Part = part; Source = src; }
		}

		private readonly Dictionary<string, Entry> Map = new Dictionary<string, Entry>();

		internal bool HasPart(string name) => Map.ContainsKey(name);
		internal TPart GetPart(string name) => Map[name].Part;

		internal void AddPart(string name, TPart part, TSrc src)
		{
			Map.Add(name, new Entry(part, src));
		}

		internal void ParseParts(Action<string, TSrc, TPart> action)
		{
			if (action == null) throw new ApplicationException();
			foreach (var kvp in Map)
				action(kvp.Key, kvp.Value.Source, kvp.Value.Part);
		}

		internal IEnumerable<TPart> GetParts()
		{
			foreach (var entry in Map.Values)
				yield return entry.Part;
		}

		public IEnumerator<(string name, TPart Part, TSrc Source)> GetEnumerator()
		{
			foreach (var pair in Map) yield return (pair.Key, pair.Value.Part, pair.Value.Source);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var pair in Map) yield return (pair.Key, pair.Value.Part, pair.Value.Source);
		}

		public IEnumerable<T> SelectFromParts<T>(Func<TPart, T> fn) 
			=> fn != null ? Map.Values.Select(e => fn(e.Part)) : throw new ArgumentNullException(nameof(fn));
	}
}

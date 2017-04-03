using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class HostInterfaceType : LsnType
	{
		// Method definitions
		private readonly IReadOnlyDictionary<string, MethodDefinition> MethodDefinitions;
		
		
		// Event definitions
		private readonly IReadOnlyDictionary<string, EventDefinition> EventDefinitions;


		public HostInterfaceType(Dictionary<string,MethodDefinition> methods, Dictionary<string, EventDefinition> events)
		{
			MethodDefinitions = methods; EventDefinitions = events;
		}

		public override LsnValue CreateDefaultValue() => LsnValue.Nil;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool HasMethod(string name) => MethodDefinitions.ContainsKey(name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public MethodDefinition GetMethodDefinition(string name)
		{
			if (MethodDefinitions.ContainsKey(name))
				return MethodDefinitions[name];
			throw new ArgumentException($"No method named {name} exists.", "name");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool HasEventDefinition(string name)
			=> EventDefinitions.ContainsKey(name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public EventDefinition GetEventDefinition(string name)
		{
			if (EventDefinitions.ContainsKey(name))
				return EventDefinitions[name];
			throw new ArgumentException($"No event named {name} exists.", "name");
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class HostInterfaceType : LsnType
	{
		// Method definitions
		private readonly IReadOnlyDictionary<string, FunctionSignature> MethodDefinitions;
		
		
		// Event definitions
		private readonly IReadOnlyDictionary<string, EventDefinition> EventDefinitions;


		public HostInterfaceType(TypeId id, Dictionary<string, FunctionSignature> methods, Dictionary<string, EventDefinition> events)
		{
			Name = id.Name; Id = id; MethodDefinitions = methods; EventDefinitions = events;
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
		public FunctionSignature GetMethodDefinition(string name)
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

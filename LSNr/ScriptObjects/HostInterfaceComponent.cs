using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace LSNr.ScriptObjects
{
	public sealed class HostInterfaceComponent : IPreHostInterface
	{
		readonly IFunctionContainer Resource;
		readonly TypeId Id;
		readonly ISlice<Token> Tokens;
		readonly List<EventDefinition> Events = new List<EventDefinition>();
		readonly List<FunctionSignature> Methods = new List<FunctionSignature>();
		public string Path { get; }
		public bool Valid { get => Resource.Valid; set => Resource.Valid = value; }

		public HostInterfaceComponent(IFunctionContainer typeContainer, TypeId id, ISlice<Token> tokens, string path)
		{
			Resource = typeContainer; Id = id; Tokens = tokens; Path = path;
		}

		public bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Resource.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Resource.GetGenericType(name);

		public LsnType GetType(string name) => name != Id.Name ? Resource.GetType(name)
			: throw new ApplicationException();
		public Function GetFunction(string name) => Resource.GetFunction(name);
		
		/// <inheritdoc/>
		public SymbolType CheckSymbol(string symbol) => Resource.CheckSymbol(symbol);

		/// <inheritdoc/>
		public TypeId GetTypeId(string name) => name == Id.Name ? Id : Resource.GetTypeId(name);
		
		/// <inheritdoc/>
		public bool TypeExists(string name) => Resource.TypeExists(name) || name == Id.Name;

		/// <inheritdoc/>
		public void RegisterEvent(EventDefinition eventDefinition) { Events.Add(eventDefinition); }

		/// <inheritdoc/>
		public void RegisterMethod(FunctionSignature methodSignature) { Methods.Add(methodSignature); }

		public void OnParsingSignatures(IPreResource resource)
		{
			var reader = new HostInterfaceReader(Tokens, this);
			reader.Read();
			var t = new HostInterfaceType(Id, Methods.ToDictionary(m => m.Name), Events.ToDictionary(e => e.Name));
			resource.RegisterHostInterface(t);
		}

		/// <summary>
		/// Parses <paramref name="tokens" /> into a list of <see cref="Parameter" />s.
		/// </summary>
		/// <param name="tokens">The tokens.</param>
		/// <param name="index">
		///     The index of the first parameter parsed. For methods, this will be 1 so a 'hostObject' parameter can be added.
		///     For events, this will be 0.
		/// </param>
		/// <returns></returns>
		public IReadOnlyList<Parameter> ParseParameters(IReadOnlyList<Token> tokens, ushort index = 0)
		{
			var parameters = Resource.ParseParameters(tokens, index);
			if (index != 0)
			{
				return parameters.Prepend(new Parameter("hostObject", Id, LsnValue.Nil, 0)).ToList();
			}

			return parameters;
		}

		/// <inheritdoc/>
		/// <remarks> Since, host interfaces cannot contain LSN procedures, this method always fails. </remarks>
		public IProcedure CreateFunction(IReadOnlyList<Parameter> args, TypeId retType, string name, bool isVirtual = false)
		{
			throw new InvalidOperationException();
		}
	}
}

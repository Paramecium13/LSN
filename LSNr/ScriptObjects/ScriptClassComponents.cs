using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Runtime.Types;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.Optimization;
using LSNr.ReaderRules;
using EventListener = LsnCore.Types.EventListener;
using ScriptClassMethod = LsnCore.Types.ScriptClassMethod;

namespace LSNr.ScriptObjects
{
	public sealed class ScriptClassMethodComponent
	{
		private readonly ScriptClassMethod Method;
		private readonly ISlice<Token> Body;

		public ScriptClassMethodComponent(ScriptClassMethod method, ISlice<Token> body)
		{
			Method = method; Body = body;
		}

		public void OnParsingProcBodies(IBasePreScriptClass pre)
		{
			try
			{
				var preFn = new PreScriptClassFunction(pre);
				foreach (var param in Method.Parameters)
					preFn.CurrentScope.CreateVariable(param);
				var parser = new Parser(Body, preFn);
				parser.Parse();
				preFn.CurrentScope.Pop(parser.Components);

				var components = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
				Method.Code = new ComponentFlattener().Flatten(components);
				Method.StackSize = (preFn.CurrentScope as VariableTable)?.MaxSize ?? -1;
			}
			catch (LsnrException e)
			{
				pre.Valid = false;
				//var st = this as IPreState;
				//var x = st != null ? $"state {st.StateName} of " : "";
				Logging.Log($"method '{Method.Name}' in {/*x*/""}script class {pre.Id.Name}", e);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
			{
				pre.Valid = false;
				var st = pre as IPreState;
				var x = "";//st != null ? $"state {st.StateName} of " : "";
				Logging.Log($"method '{Method.Name}' in {x}script class {pre.Id.Name}", e, pre.Path);
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}

	public sealed class ScriptClassEventListenerComponent
	{
		private readonly EventListener Event;
		private readonly ISlice<Token> Body;
		private readonly Token First;

		public ScriptClassEventListenerComponent(EventListener e, ISlice<Token> body, Token first)
		{
			Event = e; Body = body; First = first;
		}

		public void Validate(IBasePreScriptClass pre)
		{
			if (!pre.Host.EventDefinitions.ContainsKey(Event.Definition.Name))
				throw new LsnrParsingException(First, $"The HostInterface '{pre.HostId.Name}' does not define an event '{Event.Definition.Name}'.", pre.Path);

			var predef = new EventDefinition(Event.Definition.Name,
				Event.Definition.Parameters.Skip(1)
				.Select(p => new Parameter(p.Name, p.Type, p.DefaultValue, (ushort)(p.Index - 1)))
				.ToList());

			if (!pre.Host.EventDefinitions[Event.Definition.Name].Equivalent(predef))
				throw new LsnrParsingException(First, $"The event '{Event.Definition.Name}' does not match the event definition in the HostInterface '{pre.HostId.Name}'.", pre.Path);
		}

		public void OnParsingProcBodies(IBasePreScriptClass pre)
		{
			try
			{
				var preFn = new PreScriptClassFunction(pre);

				foreach (var param in Event.Definition.Parameters)
					preFn.CurrentScope.CreateVariable(param);
				var parser = new Parser(Body, preFn);
				parser.Parse();
				preFn.CurrentScope.Pop(parser.Components);
				Event.Code = new ComponentFlattener().Flatten(Parser.Consolidate(parser.Components).Where(c => c != null).ToList());
				Event.StackSize = (preFn.CurrentScope as VariableTable)?.MaxSize /*For the 'self' arg.*/?? -1;
			}
			catch (LsnrException e)
			{
				pre.Valid = false;
				//var st = this as PreState;
				var x = "";//st != null ? $"state {st.StateName} of " : "";
				Logging.Log($"event listener '{Event.Definition.Name}' in {x}script object {pre.Id.Name}", e);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
			{
				pre.Valid = false;
				//var st = this as PreState;
				var x = "";//st != null ? $"state {st.StateName} of " : "";
				Logging.Log($"event listener '{Event.Definition.Name}' in {x}script object {pre.Id.Name}", e, pre.Path);
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}

	public sealed class ScriptClassConstructorComponent
	{
		private readonly ScriptClassConstructor Constructor;
		private readonly ISlice<Token> Body;

		public ScriptClassConstructorComponent(ScriptClassConstructor c, ISlice<Token> body)
		{
			Constructor = c; Body = body;
		}

		public void OnParsingProcBodies(IBasePreScriptClass sc)
		{
			try
			{
				var pre = new PreScriptClassFunction(sc, true);
				foreach (var param in Constructor.Parameters)
					pre.CurrentScope.CreateVariable(param);
				var parser = new Parser(Body, pre);
				parser.Parse();
				pre.CurrentScope.Pop(parser.Components);

				var components = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
				var statements = new ComponentFlattener().Flatten(components);
				Constructor.Code = null; //ToDo: Statement[] to Instruction[] 
				Constructor.StackSize = (pre.CurrentScope as VariableTable)?.MaxSize + 1 /*For the 'self' arg.*/?? -1;
			}
			catch (LsnrException e)
			{
				sc.Valid = false;
				Logging.Log($"constructor for script class {sc.Id.Name}", e);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
			{
				sc.Valid = false;
				Logging.Log($"constructor for script class {sc.Id.Name}", e, sc.Path);
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}
	}
}

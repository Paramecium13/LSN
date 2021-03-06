﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore.Values
{
	public sealed class ScriptObject : ILsnValue, IHasMutableFieldsValue
	{
		private readonly LsnValue[] Fields;

		private readonly ScriptClass ScriptClass;

		private IHostInterface Host;

		private int CurrentStateIndex;
		private ScriptClassState CurrentState;

		public bool BoolValue => true;
		
		public TypeId Type { get; }
		
		public ILsnValue Clone() => this;

		public uint NumericId { get; }

		public string TextId { get; }

		public ScriptObject(LsnValue[] fields, ScriptClass type, int currentState, IHostInterface host = null)
		{
			Fields = fields; Type = type.Id; ScriptClass = type; CurrentStateIndex = currentState;
			if (type._States.Count > 0)
				CurrentState = ScriptClass.GetState(CurrentStateIndex);
			if (host != null)
			{
				// Check types
				if (ScriptClass.HostInterface == null)
					throw new ArgumentException("This type of ScriptObject does not have a host.", nameof(host));
				if (!ScriptClass.HostInterface.Equals(host.Type))
					throw new ArgumentException($"Invalid HostInterface type. Expected {ScriptClass.HostInterface.Name}. Recieved {host.Type.Name}.", nameof(host));

				Host = host;
				// Subscribe to events.
				foreach (var evName in ((HostInterfaceType) host.Type.Type).EventDefinitions.Keys)
				{
					if ((CurrentState?.HasEventListener(evName) ?? false))
						host.SubscribeToEvent(evName, this, CurrentState.GetEventListener(evName).Priority);
					else if (ScriptClass.HasEventListener(evName))
						host.SubscribeToEvent(evName, this, GetEventListener(evName).Priority);
				}

				if (Settings.ScriptObjectIdFormat == ScriptObjectIdFormat.Host_Self)
				{
					NumericId = host.AttachScriptObject(this, out var str);
					TextId = str;
				}
				else host.AttachScriptObject(this, out _);
			}
			else if (ScriptClass.HostInterface != null)
				throw new ArgumentException("This type of ScriptObject cannot survive without a host.", nameof(host));
		}

		public LsnValue GetFieldValue(int index)
			=> Fields[index];

		public void SetFieldValue(int index, LsnValue value)
		{
			Fields[index] = value;
		}

		internal LsnValue GetPropertyValue(int index)
			=> throw new InvalidOperationException();

		internal ScriptClassMethod GetMethod(string methodName)
		{
			//Is the method virtual?
			//	Does the current state override it?
			//		Return the current state's implementation
			//	Else Does the Type have an implementation?
			//		Return that
			//	Else
			//	Throw exception
			//Else
			//	Return the type's implementation.

			if (!ScriptClass.HasMethod(methodName))
				throw new ArgumentException(
					$"The ScriptObject type \"{ScriptClass.Name}\" does not have a method named \"{methodName}\".",
					nameof(methodName));
			var method = ScriptClass.GetMethod(methodName);
			if (method.IsVirtual)
			{
				if (CurrentState?.HasMethod(methodName) ?? false)
					return CurrentState.GetMethod(methodName);
				if (!method.IsAbstract)
					return method;
				throw new ApplicationException("...");
			}
			return method;
		}

		public LsnValue ExecuteHostInterfaceMethod(string name, LsnValue[] values)
		{
			return Host.CallMethod(name, values);
		}

		public LsnValue GetHost() => new LsnValue(Host);

		public EventListener GetEventListener(string name)
		{
			if (CurrentState?.HasEventListener(name) ?? false)
				return CurrentState.GetEventListener(name);
			if (ScriptClass.HasEventListener(name))
				return ScriptClass.GetEventListener(name);
			throw new ArgumentException("", nameof(name));
		}

		internal void SetState(int index)
		{
			var nextState = ScriptClass.GetState(index);
			var expiringSubscriptions = CurrentState.EventsListenedTo;//.Except(nextState.EventsListenedTo);
			var newSubscriptions = nextState.EventsListenedTo;//.Except(CurrentState.EventsListenedTo);

			// Unsubscribe from old state's event subscriptions (if valid). Run old state exit method.
			if (Host != null)
				foreach (var subscription in expiringSubscriptions)
					Host.UnsubscribeToEvent(subscription, this);

			CurrentStateIndex = index;
			CurrentState = nextState;

			// Subscribe to new state's event subscriptions (if valid). Run new state Start method.
			if (Host == null) return;
			
			foreach (var subscription in newSubscriptions)
				Host.SubscribeToEvent(subscription, this,CurrentState.GetEventListener(subscription).Priority);
			
		}

		// Serialization?
		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write((byte)ConstantCode.ScriptObject);
			if (ScriptClass.Unique)
			{
				writer.Write(true);
				writer.Write(ScriptClass.Name);
			}
			else
			{
				writer.Write(false);
				switch (Settings.ScriptObjectIdFormat)
				{
					case ScriptObjectIdFormat.Host_Self:
						switch (Settings.HostInterfaceIdType)
						{
							case IdentifierType.Numeric:
								writer.Write(Host.NumericId);
								break;
							case IdentifierType.Text:
								writer.Write(Host.TextId);
								break;
							default:
								throw new ApplicationException();
						}
						switch (Settings.ScriptObjectIdType)
						{
							case IdentifierType.Numeric:
								writer.Write(NumericId);
								break;
							case IdentifierType.Text:
								writer.Write(TextId);
								break;
							default:
								throw new ApplicationException();
						}
						break;
					case ScriptObjectIdFormat.Self:
						switch (Settings.ScriptObjectIdType)
						{
							case IdentifierType.Numeric:
								writer.Write(NumericId);
								break;
							case IdentifierType.Text:
								writer.Write(TextId);
								break;
							default:
								throw new ApplicationException();
						}
						break;
					default:
						throw new ApplicationException();
				}
			}
		}

		public void SerializeScriptObject(BinaryDataWriter writer, bool writeHostId)
		{
			if (writeHostId)
			{
				switch (Settings.HostInterfaceIdType)
				{
					case IdentifierType.Numeric:
						writer.Write(Host?.NumericId ?? 0);
						break;
					case IdentifierType.Text:
						writer.Write(Host?.TextId ?? "");
						break;
					default:
						break;
				}
			}
			writer.Write(Type.Name);
			writer.Write(CurrentStateIndex);
			for (int i = 0; i < Fields.Length; i++)
				Fields[i].Serialize(writer);
		}

		public void Detach()
		{
			foreach (var name in ScriptClass.EventListeners.Keys.Union(CurrentState.EventsListenedTo).Distinct())
				Host.UnsubscribeToEvent(name, this);
			Host.DetachScriptObject(this);
			Host = null;
		}
	}
}

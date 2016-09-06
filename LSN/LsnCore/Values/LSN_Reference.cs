﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// Wraps a .NET reference to an LsnObject
	/// </summary>
	[Serializable]
	public class LsnReference : LsnValue
	{
		public readonly LsnReferenceValue Value;
		public override bool BoolValue { get { return Value?.BoolValue ?? false; } }

		public LsnReference(LsnReferenceValue value, LsnReferenceType type)
		{
			Value = value;
			Type = type.Id;
		}

		public override ILsnValue Clone() => this;

	}
}

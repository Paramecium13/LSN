﻿using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore
{
	public interface ILsnValue
	{
		TypeId Type { get; }
		bool BoolValue { get; }

		ILsnValue Clone();
		//int GetSize();
		void Serialize(BinaryDataWriter writer);
	}
}
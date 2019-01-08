﻿using LsnCore.ControlStructures;
using LsnCore.Utilities;

namespace LSNr.ControlStructures
{
	interface IControlStructureRule
	{
		bool PreCheck(Token t);
		bool Check(ISlice<Token> tokens, IPreScript script);
		ControlStructure Apply(ISlice<Token> tokens, IPreScript script);
	}
}

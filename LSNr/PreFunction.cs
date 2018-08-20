﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;

namespace LSNr
{
	public class PreFunction : IPreScript
	{
		private readonly PreResource Resource;

		internal PreFunction(PreResource resource)
		{
			Resource = resource;
		}

		public IScope CurrentScope { get; set; } = new VariableTable(new List<Variable>());

		public bool Valid {get { return Resource.Valid; } set { Resource.Valid = value; } }

		public bool Mutable								=> Resource.Mutable;
		public bool GenericTypeExists(string name)		=> Resource.GenericTypeExists(name);
		public Function GetFunction(string name)		=> Resource.GetFunction(name);
		public GenericType GetGenericType(string name)	=> Resource.GetGenericType(name);
		public LsnType GetType(string name)				=> Resource.GetType(name);
		public bool TypeExists(string name)				=> Resource.TypeExists(name);
		public string Path								=> Resource.Path;
		public TypeId GetTypeId(string name)			=> Resource.GetTypeId(name);

		public void GenericTypeUsed(TypeId typeId)
		{
			Resource.GenericTypeUsed(typeId);
		}

		public SymbolType CheckSymbol(string name)
		{
			if (CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			return Resource.CheckSymbol(name);
		}
	}
}

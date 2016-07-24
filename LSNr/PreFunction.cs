using System;
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

		public Scope CurrentScope { get; set; } = new Scope();

		public bool Mutable => Resource.Mutable;

		public bool Valid {get { return Resource.Valid; } set { Resource.Valid = value; } }

		public bool FunctionExists(string name) => Resource.FunctionExists(name);

		public bool FunctionIsIncluded(string name) => Resource.FunctionIsIncluded(name);

		public bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);

		public Function GetFunction(string name) => Resource.GetFunction(name);

		public GenericType GetGenericType(string name) => Resource.GetGenericType(name);

		public LsnType GetType(string name) => Resource.GetType(name);

		public bool TypeExists(string name) => Resource.TypeExists(name);
	}
}

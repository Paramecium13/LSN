using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	class LsnrTypeNotFoundException : LsnrException
	{
		public LsnrTypeNotFoundException(string fileName, string typeName) : base($"Type '{typeName}' not found.", fileName) { }
	}

	class LsnrFunctionNotFoundException : LsnrException
	{
		public LsnrFunctionNotFoundException(string fileName, string fnName) : base($"Function '{fnName}' not found. Are you missing an include or using?", fileName) { }
	}
}

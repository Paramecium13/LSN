using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	// Loads files.
	// Implementation should use caching.
	public interface ILsnFileManager
	{
		LsnResourceThing LoadResourse(string name);
	}

}

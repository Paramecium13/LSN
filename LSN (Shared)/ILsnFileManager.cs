using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Interpretation;

namespace LsnCore
{
	// Loads files.
	// Implementation should use caching.
	public interface ILsnFileManager
	{
		LsnObjectFile LoadResourse(string name);
	}

}

using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	public abstract class Method : Function
	{
		/// <summary>
		/// The type this method is a member of.
		/// </summary>
		public readonly LSN_Type Type;

		public Method(LSN_Type type,LSN_Type returnType)
		{
			Type = type;
			ReturnType = returnType;
		}
	}
}

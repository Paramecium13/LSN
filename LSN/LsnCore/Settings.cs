using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public enum IdentifierType
	{
		Numeric, Text
	}

	public enum ScriptObjectIdFormat
	{
		Host_Self,
		Self
	}

	public static class Settings
	{
		public static IdentifierType HostInterfaceIdType { get; set; } = IdentifierType.Numeric;

		public static IdentifierType ScriptObjectIdType { get; set; } = IdentifierType.Numeric;

		public static ScriptObjectIdFormat ScriptObjectIdFormat { get; set; } = ScriptObjectIdFormat.Host_Self;
	}
}

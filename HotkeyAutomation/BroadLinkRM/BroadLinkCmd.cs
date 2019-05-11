using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Broadlink.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HotkeyAutomation.iTach
{
	public class BroadLinkCmd : NamedItem
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public RMCommandType type = RMCommandType.IR;
		public byte repeat = 0;
		public ushort[] codes;
	}
}

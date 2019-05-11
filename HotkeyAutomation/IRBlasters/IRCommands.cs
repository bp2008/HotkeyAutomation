using HotkeyAutomation.iTach;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyAutomation.IRBlasters
{
	public static class IRCommands
	{
		public static string[] GetIRCommandShortNames()
		{
			string[] iTachIR = iTachCommands.GetCommandShortNames();
			string[] broadLinkIR = BroadLinkCommands.GetCommandNames(c=>c.type == Broadlink.Net.RMCommandType.IR);
			return iTachIR.Concat(broadLinkIR).OrderBy(s => s).ToArray();
		}
	}
}

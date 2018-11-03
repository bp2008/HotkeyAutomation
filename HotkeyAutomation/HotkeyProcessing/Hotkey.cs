using System;
using BPUtil;

namespace HotkeyAutomation.HotkeyProcessing
{
	public class Hotkey : NamedItem
	{
		public int? key;
		public string keyName
		{
			get
			{
				if (key == null)
					return "unset";
				return ConsoleKeyHelper.GetKeyName(key.Value);
			}
		}
		public Effect[] effects;
	}
}
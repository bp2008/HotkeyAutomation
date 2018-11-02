using System;

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
				return ((ConsoleKey)key).ToString();
			}
		}
		public Effect[] effects;
	}
}
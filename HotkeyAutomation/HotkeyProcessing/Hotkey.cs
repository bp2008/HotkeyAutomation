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
		/// <summary>
		/// If true, the hotkey will only activate if pressed twice within a short time.
		/// </summary>
		public bool doublePress;
		public Effect[] effects;
	}
}
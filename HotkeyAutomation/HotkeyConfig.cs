using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BPUtil;
using HotkeyAutomation.BroadLinkRM;
using HotkeyAutomation.HomeAssistant;
using HotkeyAutomation.HotkeyProcessing;
using HotkeyAutomation.iTach;
using HotkeyAutomation.Vera;

namespace HotkeyAutomation
{
	public class HotkeyConfig : SerializableObjectBase
	{
		public string systemName = "HotkeyAutomation";
		public int httpPort = 80;
		public int httpsPort = -1;
		public bool devMode = false;
		/// <summary>
		/// If greater than 0, we'll try to operate an active buzzer on this GPIO number.
		/// </summary>
		public int buzzerGpioNumber = 0;
		/// <summary>
		/// If true, we set the GPIO output to "Low" to make the buzzer make noise.  If false, we set the GPIO output to "High" to make the buzzer make noise.
		/// </summary>
		public bool buzzerGpioOutputLowToBeep = true;

		public string GetWWWDirectoryBase()
		{
			if (devMode)
				return "../../www/";
			else
				return Globals.ApplicationDirectoryBase + "www/";
		}

		public NamedItemCollection<Hotkey> hotkeys = new NamedItemCollection<Hotkey>();
		public NamedItemCollection<VeraController> veras = new NamedItemCollection<VeraController>();
		public NamedItemCollection<iTachController> iTachs = new NamedItemCollection<iTachController>();
		public NamedItemCollection<BroadLinkController> broadLinks = new NamedItemCollection<BroadLinkController>();
		public NamedItemCollection<HomeAssistantServer> homeAssistantServers = new NamedItemCollection<HomeAssistantServer>();

		#region Hotkeys

		/// <summary>
		/// Gets all hotkeys bound to the specified key.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Hotkey> GetHotkeysByKey(int key)
		{
			return hotkeys.Where(h => h.key == key);
		}

		///// <summary>
		///// Renames and retrieves the specified hotkey. 
		///// The name of the hotkey is only changed if the new name is not already taken by another hotkey.
		///// Returns null if no hotkey is found with the id.
		///// </summary>
		///// <param name="id">The unique identifier of the hotkey.</param>
		///// <param name="newName">The new name to assign to the hotkey.</param>
		///// <returns></returns>
		//public Hotkey RenameHotkey(int id, string newName)
		//{
		//	lock (hotkeys)
		//	{
		//		Hotkey hotkey = hotkeys.Get(id);
		//		if (hotkey == null)
		//			return null;
		//		Hotkey conflict = hotkeys.Get(newName);
		//		if (conflict == null)
		//			hotkey.name = newName;
		//		return hotkey;
		//	}
		//}

		///// <summary>
		///// Sets the effect for the specified hotkey and retrieves the hotkey.
		///// Returns null if the hotkey could not be found.
		///// </summary>
		///// <param name="id">The unique identifier of the hotkey.</param>
		///// <param name="effect">The new effect to assign to the hotkey.</param>
		//public Hotkey SetHotkeyEffect(int id, Effect effect)
		//{
		//	lock (hotkeys)
		//	{
		//		Hotkey hotkey = hotkeys.Get(id);
		//		if (hotkey == null)
		//			return null;
		//		hotkey.effect = effect;
		//		return hotkey;
		//	}
		//}

		/// <summary>
		/// Binds the key for the specified hotkey, and retrieves the hotkey.
		/// Returns null if no hotkey is found with the id.
		/// </summary>
		/// <param name="id">The unique identifier of the hotkey.</param>
		/// <param name="key">The new key to assign to the hotkey.</param>
		/// <returns></returns>
		public Hotkey SetHotkeyKey(int id, int? key)
		{
			lock (hotkeys)
			{
				Hotkey hotkey = hotkeys.Get(id);
				if (hotkey != null)
				{
					hotkey.key = key;
					Save();
				}
				return hotkey;
			}
		}
		#endregion
	}
}

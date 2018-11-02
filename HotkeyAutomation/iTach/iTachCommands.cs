using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HotkeyAutomation.iTach
{
	public static class iTachCommands
	{
		public static ConcurrentDictionary<string, iTachCmd> commands = new ConcurrentDictionary<string, iTachCmd>();

		/// <summary>
		/// Loads the [commands] list from the specified json file.
		/// </summary>
		/// <param name="path">Path to a json file containing serialized iTachCmd instances.</param>
		public static void Load(string path)
		{
			string json = File.ReadAllText(path);
			iTachCmd[] arr = JsonConvert.DeserializeObject<iTachCmd[]>(json);
			ConcurrentDictionary<string, iTachCmd> cmds = new ConcurrentDictionary<string, iTachCmd>();
			foreach (iTachCmd cmd in arr)
			{
				if (!cmds.ContainsKey(cmd.ShortName))
					cmds[cmd.ShortName] = cmd;
			}
			commands = cmds;
		}
		public static string[] GetCommandShortNames()
		{
			return commands.Keys.OrderBy(s => s).ToArray();
		}
	}
}

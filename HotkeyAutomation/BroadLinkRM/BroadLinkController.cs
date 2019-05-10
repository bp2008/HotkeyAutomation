using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BPUtil;
using Broadlink.Net;
using HotkeyAutomation.iTach;
using Newtonsoft.Json;

namespace HotkeyAutomation.BroadLinkRM
{
	public class BroadLinkController : NamedItem
	{
		// public string name; // Inherited from NamedItem
		public string host;
		protected RMDevice device;
		protected object myLock = new object();
		protected Stopwatch timer = new Stopwatch();
		protected long lastLoadAttempt = long.MinValue;
		protected long loadAttemptInterval = 1000 * 60;
		public BroadLinkController()
		{
			timer.Start();
		}
		public BroadLinkController(dynamic item)
		{
			name = item.name;
			host = item.host;
			timer.Start();
			LoadDeviceInfo();
		}

		public BroadLinkController(string host)
		{
			this.host = host;
			timer.Start();
			LoadDeviceInfo();
		}

		/// <summary>
		/// Sends the specified RMCommand.  May throw an exception if there is a network error.
		/// </summary>
		/// <param name="cmd">The command to send.</param>
		/// <param name="repeat">(Optional, 0-255) Override the number of command repeats specified in the RMCommand. 0 = no repeat, 1 = send twice, etc.</param>
		/// <returns></returns>
		public void SendCommandSync(RMCommand cmd, byte? repeat = null)
		{
			if (device == null)
			{
				LoadDeviceInfo_Throttled();
				if (device == null)
					return;
			}
			lock (myLock)
			{
				if (repeat != null && cmd.RepeatCount != repeat.Value)
				{
					cmd = JsonConvert.DeserializeObject<RMCommand>(JsonConvert.SerializeObject(cmd));
					cmd.RepeatCount = repeat.Value;
				}
				device.SendRemoteCommandAsync(cmd).Wait();
			}
		}


		/// <summary>
		/// Converts and sends the specified iTachCmd to the BroadLink RM Device.
		/// </summary>
		/// <param name="commandShortName">Short name of the command to send.</param>
		/// <param name="repeat">(Optional, 0-255) Override the number of command repeats specified in the RMCommand. 0 = no repeat, 1 = send twice, etc.</param>
		public void SendCommandSync(string commandShortName, byte? repeat = null)
		{
			if (commandShortName == null)
			{
				Logger.Debug("BroadLink command null is not valid");
				return;
			}
			if (iTachCommands.commands.TryGetValue(commandShortName, out iTachCmd cmd))
			{
				RMCommand rm = new RMCommand();
				rm.Type = RMCommandType.IR;
				rm.RepeatCount = cmd.Repeat;
				rm.SetPulses(cmd.GetMicrosecondPulses());
				SendCommandSync(rm, repeat);
			}
			else
			{
				Logger.Debug("BroadLink command \"" + commandShortName + "\" not found");
			}
		}

		#region Helpers
		private void LoadDeviceInfo_Throttled()
		{
			if (lastLoadAttempt + loadAttemptInterval < timer.ElapsedMilliseconds)
				LoadDeviceInfo();
		}
		/// <summary>
		/// Loads or reloads the <see cref="device"/> object.
		/// </summary>
		public void LoadDeviceInfo()
		{
			lock (myLock)
			{
				lastLoadAttempt = timer.ElapsedMilliseconds;
				device = null;
				int tries = 0;
				int maxTries = 5;
				while (device == null && tries < maxTries)
				{
					if (tries > 0)
						Logger.Info("Unable to find BroadLink device \"" + name + "/" + host + "\" after " + tries + " scan" + (tries == 1 ? "" : "s") + ". " + (tries + 1 < maxTries ? "Trying again." : "Aborting BroadLinkController load."));
					device = ScanForDevice(host);
					tries++;
				}
			}
		}

		/// <summary>
		/// Scans the network for the specified RMDevice and returns it or null.
		/// </summary>
		/// <param name="host">An IP address to scan for.  If "0.0.0.0", the first responding device will be returned.</param>
		/// <returns></returns>
		private RMDevice ScanForDevice(string host)
		{
			lock (myLock)
			{
				Client broadlinkClient = new Client();
				if (host == "0.0.0.0")
				{
					Task<List<BroadlinkDevice>> discoveryTask = broadlinkClient.DiscoverAsync(3000, true);
					discoveryTask.Wait();
					if (discoveryTask.IsCompleted && discoveryTask.Result.Count > 0)
						return (RMDevice)discoveryTask.Result[0];
					return null;
				}
				if (IPAddress.TryParse(host, out IPAddress targetAddr))
				{
					Task<List<BroadlinkDevice>> discoveryTask = broadlinkClient.DiscoverAsync(3000, targetDeviceAddr: targetAddr);
					discoveryTask.Wait();
					if (discoveryTask.IsCompleted)
						return (RMDevice)discoveryTask.Result.FirstOrDefault(d => d.EndPoint.Address == targetAddr);
					return null;
				}
			}
			return null;
		}
		#endregion
	}
}

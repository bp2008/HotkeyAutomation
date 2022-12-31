using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BPUtil;
using Broadlink.Net;
using HotkeyAutomation.HotkeyProcessing;
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
		protected int consecutiveCommandFailures = 0;
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
			if (device == null || consecutiveCommandFailures >= 5)
			{
				LoadDeviceInfo_Throttled();
				if (device == null)
				{
					Logger.Debug("BroadLinkController \"" + host + "\" failed to send a command because the device is not responding.");
					return;
				}
			}
			lock (myLock)
			{
				if (repeat != null && cmd.RepeatCount != repeat.Value)
				{
					cmd = JsonConvert.DeserializeObject<RMCommand>(JsonConvert.SerializeObject(cmd));
					cmd.RepeatCount = repeat.Value;
				}
				bool success = false;
				try
				{
					Task<bool> sendTask = device.SendRemoteCommandAsync(cmd);
					sendTask.Wait();
					success = sendTask.Result;
				}
				catch (Exception) { }
				if (success)
				{
					consecutiveCommandFailures = 0;
				}
				else
				{
					consecutiveCommandFailures++;
					Logger.Debug("BroadLinkController \"" + host + "\" failed to send a command. Consecutive failure #" + consecutiveCommandFailures);
				}
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
			RMCommand rm = GetCommandFromShortName(commandShortName);
			if (rm != null)
			{
				SendCommandSync(rm, repeat);
			}
			else
			{
				Logger.Debug("BroadLink command \"" + commandShortName + "\" not found");
			}
		}
		private RMCommand GetCommandFromShortName(string name)
		{
			BroadLinkCmd broadLinkCmd = BroadLinkCommands.commands.Get(name);
			if (broadLinkCmd != null)
			{
				RMCommand rm = new RMCommand();
				rm.Type = broadLinkCmd.type;
				rm.RepeatCount = broadLinkCmd.repeat;
				rm.SetPulses(broadLinkCmd.codes);
				return rm;
			}
			if (iTachCommands.commands.TryGetValue(name, out iTachCmd cmd))
			{
				RMCommand rm = new RMCommand();
				rm.Type = RMCommandType.IR;
				rm.RepeatCount = cmd.Repeat;
				rm.SetPulses(cmd.GetMicrosecondPulses());
				return rm;
			}
			return null;
		}

		#region Command Learning
		/// <summary>
		/// Keeps track of the learning state on the server. This object is reused from Hotkey binding code.
		/// </summary>
		private BindState _learningState = null;
		public string BeginLearning(int commandId)
		{
			if (device == null || consecutiveCommandFailures >= 5)
				LoadDeviceInfo_Throttled();
			if (device == null)
				throw new Exception("Unable to load Broadlink device \"" + host + "\".");
			lock (myLock)
			{
				BroadLinkCmd cmd = BroadLinkCommands.commands.Get(commandId);
				if (cmd == null)
					return null;
				if (GetCurrentLearningState() != null)
					return "";
				_learningState = new BindState(commandId);
				Logger.Info("[" + _learningState.bindId + "] Command Learning - START \"" + cmd.name + "\"");
				device.EnterLearningModeAsync().Wait();
				return GetCurrentLessonId();
			}
		}

		public string GetCurrentLessonId()
		{
			return GetCurrentLearningState()?.bindId;
		}

		public void CancelLearning(string lessonId)
		{
			if (device == null || consecutiveCommandFailures >= 5)
				LoadDeviceInfo_Throttled();
			if (_learningState != null)
				lock (myLock)
				{
					if (_learningState != null)
					{
						Logger.Info("[" + _learningState.bindId + "] Command Learning - CANCEL");
						Try.Swallow(() =>
						{
							device.ReadLearningDataAsync().Wait();
						});
						_learningState = null;
					}
				}
		}

		public BroadLinkCmd UnlearnCommandCodes(string lessonId)
		{
			if (_learningState != null)
				lock (myLock)
				{
					if (_learningState != null)
					{
						BroadLinkCmd cmd = BroadLinkCommands.commands.Get(_learningState.hotkeyId);
						if (cmd != null)
							cmd.codes = null;
						BroadLinkCommands.Save();
						Logger.Info("[" + _learningState.bindId + "] Command Learning - UNLEARN" + (cmd != null ? "" : " FAILED: Command " + _learningState.hotkeyId + " did not exist"));
						_learningState = null;
						return cmd;
					}
				}
			return null;
		}
		/// <summary>
		/// Blocks until the specified learning operation is complete.
		/// </summary>
		/// <param name="lessonId">ID of the learning operation which was started.</param>
		public void AwaitLearningResult(string lessonId)
		{
			if (device == null || consecutiveCommandFailures >= 5)
				LoadDeviceInfo_Throttled();
			BindState state = GetCurrentLearningState();
			while (state?.bindId == lessonId && state.bindStart + 5000 > TimeUtil.GetTimeInMsSinceEpoch())
			{
				Thread.Sleep(100);
				state = GetCurrentLearningState();
			}
			lock (myLock)
			{
				if (_learningState?.bindId == lessonId)
				{
					BroadLinkCmd cmd = BroadLinkCommands.commands.Get(_learningState.hotkeyId);
					if (cmd == null)
						return;
					try
					{
						Task<RMCommand> finishLearningTask = device.ReadLearningDataAsync();
						finishLearningTask.Wait();
						if (finishLearningTask.Result != null)
						{
							cmd.type = finishLearningTask.Result.Type;
							cmd.codes = finishLearningTask.Result.GetPulses();
							BroadLinkCommands.Save();
						}
					}
					catch (Exception ex)
					{
						string exMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
						Logger.Info("BroadLink Controller \"" + name + "\" failed to learn command codes.  " + exMsg);
					}
					_learningState = null;
				}
			}
		}
		/// <summary>
		/// Returns the current learning state object which will be null if no learning operation is active.
		/// </summary>
		/// <returns></returns>
		private BindState GetCurrentLearningState()
		{
			int expireTime = 10000;
			BindState ls = _learningState;
			long now = TimeUtil.GetTimeInMsSinceEpoch();
			if (ls != null && ls.bindStart + expireTime < now)
				lock (myLock)
				{
					// We found the existing state was expired, so we entered the lock.  Now check again.
					if (_learningState != null && _learningState.bindStart + expireTime < now)
					{
						Logger.Info("[" + _learningState.bindId + "] Command Learning - TIMEOUT");
						_learningState = null;
					}
					return _learningState;
				}
			else
				return ls; // Not expired, but may be null
		}
		#endregion

		#region Helpers
		private void LoadDeviceInfo_Throttled()
		{
			if (lastLoadAttempt + loadAttemptInterval < timer.ElapsedMilliseconds)
				LoadDeviceInfo();
			else if (consecutiveCommandFailures > 0 && lastLoadAttempt + 5000 < timer.ElapsedMilliseconds)
				LoadDeviceInfo();
		}
		/// <summary>
		/// Loads or reloads the <see cref="device"/> object.
		/// </summary>
		public void LoadDeviceInfo()
		{
			lastLoadAttempt = timer.ElapsedMilliseconds;
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
				if (device != null)
				{
					try
					{
						device.AuthorizeAsync().Wait();
					}
					catch (Exception ex)
					{
						Logger.Debug(ex);
						device = null;
					}
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
						return (RMDevice)discoveryTask.Result.FirstOrDefault(d => targetAddr.Equals(d.EndPoint.Address));
					return null;
				}
			}
			return null;
		}
		#endregion
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BPUtil;
using HotkeyAutomation.BroadLinkRM;
using HotkeyAutomation.HomeAssistant;
using HotkeyAutomation.iTach;
using HotkeyAutomation.Vera;
using Newtonsoft.Json;

namespace HotkeyAutomation.HotkeyProcessing
{
	/// <summary>
	/// Hotkey binding works like this:
	/// Step 1) User begins bind operation via web interface.
	/// Step 2) A key is pressed on the server's keyboard/keypad, or the timeout expires, or the operation is canceled by the user.
	/// Step 3) User is notified of bind result.
	/// </summary>
	public class HotkeyManager
	{
		private static WebRequestUtility http_fast = new WebRequestUtility("HotkeyAutomation " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), 5000);

		/// <summary>
		/// Keeps track of the Hotkey Binding state on the server.
		/// </summary>
		private BindState _bindState = null;
		private object GetLock()
		{
			return ServiceWrapper.config.hotkeys;
		}

		#region Public API
		/// <summary>
		/// Call when any key is pressed.
		/// </summary>
		/// <param name="key">A ConsoleKey value.</param>
		/// <returns></returns>
		public void NotifyKeyPressed(int key)
		{
			try
			{
				if (HandlePossibleKeybind(key))
					return;
				TriggerHotkeys(key);
			}
			catch (Exception ex)
			{
				Logger.Debug(ex, "Handling key " + ConsoleKeyHelper.GetKeyName(key));
			}
		}
		public string ExecuteHotkeyById(int id)
		{
			Hotkey hotkey = ServiceWrapper.config.hotkeys.Get(id);
			if (hotkey == null)
				return "Hotkey with id " + id + " was not found";
			try
			{
				ExecuteHotkey(hotkey);
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
			return null;
		}

		/// <summary>
		/// Begins binding the specified hotkey and returns the bind ID which is a unique number assigned to this bind session.
		/// If the hotkey ID is invalid, returns null.
		/// If a bind operation is already in progress, returns empty string.
		/// </summary>
		/// <param name="id">The unique identifier of the hotkey to be bound.</param>
		/// <returns></returns>
		public string BeginHotkeyBind(int id)
		{
			lock (GetLock())
			{
				Hotkey hotkey = ServiceWrapper.config.hotkeys.Get(id);
				if (hotkey == null)
					return null;
				if (GetCurrentBindState() != null)
					return "";
				_bindState = new BindState(id);
				Logger.Info("[" + _bindState.bindId + "] Hotkey Bind - START \"" + hotkey.name + "\"");
				return GetCurrentBindId();
			}
		}
		/// <summary>
		/// Returns the current bind id or null if no bind operation is currently active.
		/// </summary>
		/// <returns></returns>
		public string GetCurrentBindId()
		{
			return GetCurrentBindState()?.bindId;
		}

		public void CancelHotkeyBind(string bindId)
		{
			if (_bindState != null)
				lock (GetLock())
				{
					if (_bindState != null)
					{
						Logger.Info("[" + _bindState.bindId + "] Hotkey Bind - CANCEL");
						_bindState = null;
					}
				}
		}
		/// <summary>
		/// Attempts to unbind and return the hotkey specified by the bindId. Returns null if we aren't in the bind state or if the hotkey does not exist.
		/// </summary>
		/// <param name="bindId"></param>
		/// <returns></returns>
		public Hotkey UnbindHotkey(string bindId)
		{
			if (_bindState != null)
				lock (GetLock())
				{
					if (_bindState != null)
					{
						Hotkey hotkey = ServiceWrapper.config.SetHotkeyKey(_bindState.hotkeyId, null);
						Logger.Info("[" + _bindState.bindId + "] Hotkey Bind - UNBIND" + (hotkey != null ? "" : " FAILED: Hotkey " + _bindState.hotkeyId + " did not exist"));
						_bindState = null;
						return hotkey;
					}
				}
			return null;
		}
		#endregion
		/// <summary>
		/// Triggers all hotkeys that are bound to the specified key.
		/// </summary>
		/// <param name="key"></param>
		private void TriggerHotkeys(int key)
		{
			ParallelOptions parallelOptions = new ParallelOptions();
			parallelOptions.MaxDegreeOfParallelism = 16;
			Parallel.ForEach(ServiceWrapper.config.GetHotkeysByKey(key), parallelOptions, hotkey =>
			{
				Logger.Info("Trigger: " + hotkey.name);
				if (hotkey.effects == null || hotkey.effects.Length == 0)
					return;
				ExecuteHotkey(hotkey);
			});
		}
		private static ConcurrentDictionary<int, long> hotkeyLastPressTimes = new ConcurrentDictionary<int, long>();
		private static Stopwatch hotkeyStopwatch = Stopwatch.StartNew();
		/// <summary>
		/// Hotkeys that require double press must be pressed twice within this number of milliseconds.
		/// </summary>
		private static long hotkeyDoublePressInterval = 1000;
		/// <summary>
		/// Gets the time elapsed since this class started measuring time.
		/// </summary>
		private static long hotkeyPressTimeNow
		{
			get
			{
				return hotkeyStopwatch.ElapsedMilliseconds;
			}
		}
		private static void ExecuteHotkey(Hotkey hotkey)
		{
			if (hotkey.doublePress)
			{
				if (!hotkeyLastPressTimes.TryGetValue(hotkey.id, out long lastPressTime))
				{
					// First press of sequence has not occurred
					//Logger.Info("Hotkey " + hotkey.id + " beginning double-press sequence.");
					hotkeyLastPressTimes[hotkey.id] = hotkeyPressTimeNow;
					return;
				}
				if (hotkeyPressTimeNow > lastPressTime + hotkeyDoublePressInterval)
				{
					// First press of sequence was too long ago. Begin new sequence.
					//Logger.Info("Hotkey " + hotkey.id + " pressed too late. Beginning new double-press sequence.");
					hotkeyLastPressTimes[hotkey.id] = hotkeyPressTimeNow;
					return;
				}
				// If we get here, the double press sequence has completed successfully.
				//Logger.Info("Hotkey " + hotkey.id + " was double-pressed and will now execute effects.");
				hotkeyLastPressTimes.TryRemove(hotkey.id, out lastPressTime);
			}
			ParallelOptions parallelOptions = new ParallelOptions();
			parallelOptions.MaxDegreeOfParallelism = 16;
			Parallel.ForEach(hotkey.effects, parallelOptions, effect =>
			{
				switch (effect.type)
				{
					case EffectType.HttpGet:
						{
							if (!string.IsNullOrWhiteSpace(effect.data.httpget_url) && Uri.TryCreate(effect.data.httpget_url, UriKind.Absolute, out Uri result))
								http_fast.GET(effect.data.httpget_url);
							else
								Logger.Info("Can't GET Invalid URL: " + effect.data.httpget_url);
							break;
						}
					case EffectType.HttpPost:
						{
							if (!string.IsNullOrWhiteSpace(effect.data.httppost_url) && Uri.TryCreate(effect.data.httppost_url, UriKind.Absolute, out Uri result))
							{
								string ContentType = string.IsNullOrWhiteSpace(effect.data.httppost_content_type) ? "application/x-www-form-urlencoded" : effect.data.httppost_content_type;
								byte[] body = effect.data.httppost_body == null ? new byte[0] : ByteUtil.Utf8NoBOM.GetBytes(effect.data.httppost_body);
								http_fast.POST(effect.data.httppost_url, body, ContentType);
							}
							else
								Logger.Info("Can't POST to Invalid URL: " + effect.data.httppost_url);
							break;
						}
					case EffectType.BroadLink:
						{
							BroadLinkController broadLink = ServiceWrapper.config.broadLinks.Get(effect.data.broadlink_name);
							if (broadLink == null)
							{
								Logger.Info("BroadLink \"" + effect.data.broadlink_name + "\" does not exist.");
								break;
							}
							broadLink.SendCommandSync(effect.data.broadlink_commandName, effect.data.broadlink_repeatCount);
							break;
						}
					case EffectType.iTach:
						{
							iTachController iTach = ServiceWrapper.config.iTachs.Get(effect.data.itach_name);
							if (iTach == null)
							{
								Logger.Info("iTach \"" + effect.data.itach_name + "\" does not exist.");
								break;
							}
							iTach.SendCommandSync(effect.data.itach_commandShortName, effect.data.itach_connectorAddress, effect.data.itach_repeatCount);
							break;
						}
					case EffectType.Vera:
						{
							VeraController vera = ServiceWrapper.config.veras.Get(effect.data.vera_name);
							if (vera == null)
							{
								Logger.Info("Vera \"" + effect.data.vera_name + "\" does not exist.");
								break;
							}
							vera.Send(effect.data);
							break;
						}
					case EffectType.HomeAssistant:
						{
							HomeAssistantServer hassServer = ServiceWrapper.config.homeAssistantServers.Get(effect.data.hass_servername);
							if (hassServer == null)
							{
								Logger.Info("HomeAssistant Server \"" + effect.data.hass_servername + "\" does not exist.");
								break;
							}
							hassServer.CallService(effect.data.hass_entityid, effect.data.hass_method.Value, effect.data.hass_value);
							break;
						}
					default:
						Logger.Info("Unhandled hotkey effect type: " + effect.type + " in hotkey with name \"" + hotkey.name + "\"");
						break;
				}
			});
		}

		/// <summary>
		/// Call when any key is pressed. Returns true if the key was consumed by a keybind event. If false, the key press may be used to trigger hotkeys.
		/// </summary>
		/// <param name="key">The key code.</param>
		/// <returns></returns>
		private bool HandlePossibleKeybind(int key)
		{
			if (GetCurrentBindState() == null)
				return false;
			lock (GetLock())
			{
				BindState state = GetCurrentBindState();
				if (state == null)
					return false;
				Logger.Info("[" + _bindState.bindId + "] Hotkey Bind - SET KEY " + ConsoleKeyHelper.GetKeyName(key));
				_bindState = null;
				return ServiceWrapper.config.SetHotkeyKey(state.hotkeyId, key) != null;
			}
		}
		/// <summary>
		/// Returns the current bind state object which will be null if no bind operation is active.
		/// </summary>
		/// <returns></returns>
		private BindState GetCurrentBindState()
		{
			BindState bs = _bindState;
			long now = TimeUtil.GetTimeInMsSinceEpoch();
			if (bs != null && bs.bindStart + 15000 < now)
				lock (GetLock())
				{
					// We found the existing state was expired, so we entered the lock.  Now check again.
					if (_bindState != null && _bindState.bindStart + 15000 < now)
					{
						Logger.Info("[" + _bindState.bindId + "] Hotkey Bind - TIMEOUT");
						_bindState = null;
					}
					return _bindState;
				}
			else
				return bs; // Not expired, but may be null
		}
	}
}

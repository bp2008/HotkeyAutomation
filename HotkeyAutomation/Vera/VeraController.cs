using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BPUtil;
using HotkeyAutomation.HotkeyProcessing;

namespace HotkeyAutomation.Vera
{
	public class VeraController : NamedItem
	{
		private static WebRequestUtility http_slow = new WebRequestUtility("HotkeyAutomation " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), 20000);
		private static WebRequestUtility http_fast = new WebRequestUtility("HotkeyAutomation " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), 5000);
		// public string name; // Inherited from NamedItem
		public string host;
		public int? port;
		private int GetPort()
		{
			return port == null ? 80 : port.Value;
		}
		private string BaseUrl()
		{
			return "http://" + host + ":" + GetPort();
		}
		public void Send(EffectData effect)
		{
			if (string.IsNullOrWhiteSpace(host))
				return;
			if (effect == null)
			{
				Logger.Debug("VeraController.Send was given null EffectData");
				return;
			}
			if (effect.vera_deviceNum == null)
			{
				Logger.Debug("VeraController.Send was given null EffectData.vera_deviceNum");
				return;
			}
			try
			{
				string args = null;
				if (effect.vera_service == VeraService.CurtainStop)
					args = "WindowCovering1&action=Stop";
				else if (effect.vera_service == VeraService.DimmerValue)
					args = "Dimming1&action=SetLoadLevelTarget&newLoadlevelTarget=" + ToInteger(effect.vera_value, 0, 100);
				else if (effect.vera_service == VeraService.SwitchSet)
					args = "SwitchPower1&action=SetTarget&newTargetValue=" + ToInteger(effect.vera_value, 0, 1);
				else
				{
					Logger.Debug("Unknown VeraService value \"" + effect.vera_service + "\"");
					return;
				}

				if (args == null)
				{
					Logger.Debug("VeraController does not know how to handle service type \"" + effect.vera_service + "\"");
					return;
				}
				http_fast.GET(BaseUrl() + "/port_3480/data_request?id=lu_action&output_format=json&DeviceNum=" + effect.vera_deviceNum + "&serviceId=urn:upnp-org:serviceId:" + args);
			}
			catch (ThreadAbortException) { }
			catch (Exception ex)
			{
				Logger.Debug(ex.ToString());
			}
		}

		private int ToInteger(string value, int min, int max)
		{
			if (int.TryParse(value, out int i))
				return BPUtil.BPMath.Clamp(i, min, max);
			Logger.Debug("VeraController expected an integer value but got \"" + value + "\"");
			return min;
		}

		private object displayNameLoadLock = new object();
		private ConcurrentDictionary<int, string> deviceIdToDisplayName;
		/// <summary>
		/// May be null if the names could not be loaded.  This method can take a little time if the names aren't cached or if they are currently being reloaded from cache.
		/// </summary>
		/// <returns></returns>
		public ConcurrentDictionary<int, string> GetDeviceIdToDisplayNameMap()
		{
			LoadDisplayNames();
			return deviceIdToDisplayName;
		}
		/// <summary>
		/// Loads display names for device IDs and returns true if successful.
		/// </summary>
		/// <param name="forceLoad">if true, the load will occur even if data is already loaded</param>
		/// <returns></returns>
		public bool LoadDisplayNames(bool forceLoad = false)
		{
			if (string.IsNullOrWhiteSpace(host))
				return false;
			if (deviceIdToDisplayName == null || forceLoad)
			{
				lock (displayNameLoadLock)
				{
					if (deviceIdToDisplayName == null || forceLoad)
					{
						ConcurrentDictionary<int, string> map = new ConcurrentDictionary<int, string>();
						try
						{
							BpWebResponse response = http_slow.GET(BaseUrl() + "/port_3480/data_request?id=user_data&output_format=xml");
							string xmlStr = response.str;
							XDocument doc = XDocument.Parse(xmlStr);
							var rooms = doc.Descendants("rooms").First().Descendants("room")
								.Select(room => new
								{
									id = int.Parse(room.Attribute("id").Value),
									name = room.Attribute("name").Value
								}).ToDictionary(room => room.id);

							var devices = doc.Descendants("devices").First().Descendants("device")
								.Select(device => new
								{
									id = int.Parse(device.Attribute("id").Value),
									roomId = device.Attribute("room") == null ? -1 : int.Parse(device.Attribute("room").Value),
									name = device.Attribute("name").Value
								}).ToDictionary(room => room.id);

							foreach (var device in devices.Values)
							{
								string deviceDisplayName = "";
								if (rooms.ContainsKey(device.roomId))
									deviceDisplayName = rooms[device.roomId].name + " - ";
								deviceDisplayName += device.name;
								map[device.id] = deviceDisplayName;
							}
							return true;
						}
						catch (ThreadAbortException) { }
						catch (Exception ex)
						{
							Logger.Debug(ex, "Error getting data from vera \"" + BaseUrl() + "\"");
						}
						finally
						{
							deviceIdToDisplayName = map;
						}
					}
				}
			}
			return false;
		}
	}
}

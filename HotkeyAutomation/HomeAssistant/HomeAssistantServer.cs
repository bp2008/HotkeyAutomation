using BPUtil;
using HADotNet.Core;
using HADotNet.Core.Clients;
using HADotNet.Core.Models;
using HotkeyAutomation.HotkeyProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyAutomation.HomeAssistant
{
	public class HomeAssistantServer : NamedItem
	{
		// public int id; // Inherited from NamedItem
		// public string name; // Inherited from NamedItem
		public string url;
		public string apiKey;
		protected bool initialized = false;
		protected static object initLock = new object();

		protected StatesClient statesClient;
		protected ServiceClient serviceClient;

		protected List<StateObject> states;

		public HomeAssistantServer()
		{
		}
		private void Initialize()
		{
			if (!initialized)
				lock (initLock)
				{
					if (!initialized)
					{
						ClientFactory.Initialize(url, apiKey);
						statesClient = ClientFactory.GetClient<StatesClient>();
						serviceClient = ClientFactory.GetClient<ServiceClient>();
						initialized = true;
					}
				}
		}
		/// <summary>
		/// Loads the entity states list, returning true if succesful.
		/// </summary>
		/// <returns></returns>
		public bool Load()
		{
			try
			{
				Initialize();
				Task<List<StateObject>> statesLoadTask = statesClient.GetStates();
				statesLoadTask.Wait();
				states = statesLoadTask.Result.Where(s =>
				{
					if (s.EntityId.StartsWith("light.", StringComparison.OrdinalIgnoreCase))
						return true;
					if (s.EntityId.StartsWith("switch.", StringComparison.OrdinalIgnoreCase))
						return true;
					if (s.EntityId.StartsWith("cover.", StringComparison.OrdinalIgnoreCase))
						return true;
					return false;
				}).ToList();
			}
			catch (Exception ex)
			{
				Logger.Debug(ex);
				return false;
			}
			return true;
		}
		public void CallService(string EntityId, HomeAssistantMethod method, string Value)
		{
			string domain = "unknown";
			string service = "unknown";
			if (string.IsNullOrWhiteSpace(EntityId))
			{
				Logger.Info("HomeAssistant effect does not have a device selected.");
				return;
			}
			if (EntityId.StartsWith("light.", StringComparison.OrdinalIgnoreCase))
			{
				domain = "light";
				service = "turn_on";
				if (!int.TryParse(Value, out int brightness))
				{
					Logger.Info("HomeAssistantServer received a non-integer value for " + EntityId + " " + method + ": " + Value);
					return;
				}
				if (brightness == 0)
					service = "turn_off";
				if (method == HomeAssistantMethod.DimmerValue)
					serviceClient.CallService(domain, service, new { entity_id = EntityId, brightness }).Wait();
				else if (method == HomeAssistantMethod.SwitchSet)
					serviceClient.CallService(domain, service, new { entity_id = EntityId }).Wait();
				else
				{
					Logger.Info("HomeAssistantServer rejects method " + method + " for entity " + EntityId);
					return;
				}
			}
			else if (EntityId.StartsWith("cover.", StringComparison.OrdinalIgnoreCase))
			{
				domain = "cover";
				service = "set_cover_position";
				if (method == HomeAssistantMethod.CoverStop)
				{
					service = "stop_cover";
					serviceClient.CallService(domain, service, new { entity_id = EntityId }).Wait();
				}
				else if (method == HomeAssistantMethod.CoverSet)
				{
					service = "set_cover_position";
					if (!int.TryParse(Value, out int position))
					{
						Logger.Info("HomeAssistantServer received a non-integer value for " + EntityId + " " + method + ": " + Value);
						return;
					}
					if (position == 0)
					{
						service = "close_cover";
						serviceClient.CallService(domain, service, new { entity_id = EntityId }).Wait();
					}
					else if (position == 100)
					{
						service = "open_cover";
						serviceClient.CallService(domain, service, new { entity_id = EntityId }).Wait();
					}
					else
						serviceClient.CallService(domain, service, new { entity_id = EntityId, position }).Wait();
				}
				else
				{
					Logger.Info("HomeAssistantServer rejects method " + method + " for entity " + EntityId);
					return;
				}
			}
			else if (EntityId.StartsWith("switch.", StringComparison.OrdinalIgnoreCase))
			{
				domain = "switch";
				service = "turn_on";
				if (!int.TryParse(Value, out int state))
				{
					Logger.Info("HomeAssistantServer received a non-integer value for " + EntityId + " " + method + ": " + Value);
					return;
				}
				if (state == 0)
					service = "turn_off";
				if (method == HomeAssistantMethod.SwitchSet)
					serviceClient.CallService(domain, service, new { entity_id = EntityId }).Wait();
				else
				{
					Logger.Info("HomeAssistantServer rejects method " + method + " for entity " + EntityId);
					return;
				}
			}
			else
			{
				Logger.Info("Home Assistant EntityId does not have a supported domain: " + EntityId);
				return;
			}
		}
		public List<object> GetCommandList()
		{
			if (states == null)
				Load();
			List<object> cmds = new List<object>();
			foreach (StateObject state in states)
			{
				string friendlyName;
				if (state.Attributes.TryGetValue("friendly_name", out object objFriendlyName))
					friendlyName = objFriendlyName.ToString();
				else
					friendlyName = state.EntityId;
				cmds.Add(new { ServerId = id, ServerName = name, EntityId = state.EntityId, FriendlyName = friendlyName });
			}
			return cmds;
		}
	}
}

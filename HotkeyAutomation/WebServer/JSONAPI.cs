using System;
using System.Linq;
using System.Collections.Generic;
using BPUtil;
using BPUtil.SimpleHttp;
using Newtonsoft.Json;
using HotkeyAutomation.HotkeyProcessing;
using System.Threading;
using HotkeyAutomation.iTach;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace HotkeyAutomation
{
	public static class JSONAPI
	{
		private const string jsonType = "application/json";
		public static void HandleRequest(HttpProcessor p, string jsonStr)
		{
			object response = null;
			try
			{
				dynamic requestObj = JsonConvert.DeserializeObject(jsonStr);
				string cmd = Try.Get(() => (string)requestObj.cmd);
				switch (cmd)
				{
					case "log_get":
						{
							int nextLine = (int)requestObj.nextLine;
							long logId = (long)requestObj.logId;
							if (logId == -1)
								logId = ServiceWrapper.logReader.readerId;
							else if (logId != ServiceWrapper.logReader.readerId)
							{
								response = new ResultLogGet("REFRESH", -1, ServiceWrapper.logReader.readerId, null);
								break;
							}
							List<string> lines = ServiceWrapper.logReader.GetLogUpdate(logId, ref nextLine);
							response = new ResultLogGet("OK", nextLine, ServiceWrapper.logReader.readerId, lines);
							break;
						}
					#region Hotkeys
					case "hotkey_reorder":
					case "hotkey_names":
					case "hotkey_list":
					case "hotkey_new":
					case "hotkey_get":
					case "hotkey_update":
					case "hotkey_delete":
						{
							response = NamedItemAPI(requestObj, ServiceWrapper.config.hotkeys);
							break;
						}
					case "beginHotkeyBind":
						{
							int hotkeyId = requestObj.hotkeyId;
							string bindId = ServiceWrapper.hotkeyManager.BeginHotkeyBind(hotkeyId);
							if (bindId == null)
								response = new ResultFailWithReason("hotkey not found");
							else if (bindId == "")
								response = new ResultFailWithReason("another bind is already in progress");
							else
								response = new ResultWithData(bindId);
							break;
						}
					case "endHotkeyBind":
						{
							int hotkeyId = requestObj.hotkeyId;
							Hotkey hotkey = ServiceWrapper.config.hotkeys.Get(hotkeyId);
							if (hotkey == null)
							{
								response = new ResultFailWithReason("hotkey not found");
								break;
							}
							string bindId = requestObj.bindId;
							if (string.IsNullOrWhiteSpace(bindId))
							{
								response = new ResultFailWithReason("invalid bindId");
								break;
							}
							while (bindId == ServiceWrapper.hotkeyManager.GetCurrentBindId())
								Thread.Sleep(100);
							response = new ResultWithData(hotkey);
							break;
						}
					case "cancelHotkeyBind":
						{
							string bindId = requestObj.bindId;
							if (string.IsNullOrWhiteSpace(bindId))
							{
								response = new ResultFailWithReason("invalid bindId");
								break;
							}
							ServiceWrapper.hotkeyManager.CancelHotkeyBind(bindId);
							response = new ResultSuccess();
							break;
						}
					case "unbindHotkey":
						{
							string bindId = requestObj.bindId;
							if (string.IsNullOrWhiteSpace(bindId))
							{
								response = new ResultFailWithReason("invalid bindId");
								break;
							}
							Hotkey hotkey = ServiceWrapper.hotkeyManager.UnbindHotkey(bindId);
							if (hotkey != null)
								response = new ResultWithData(hotkey);
							else
								response = new ResultFailWithReason("Unable to unbind hotkey. Please try again.");
							break;
						}
					#endregion
					#region BroadLink
					case "broadlink_reorder":
					case "broadlink_names":
					case "broadlink_list":
					case "broadlink_new":
					case "broadlink_get":
					case "broadlink_update":
					case "broadlink_delete":
						{
							response = NamedItemAPI(requestObj, ServiceWrapper.config.broadLinks);
							break;
						}
					case "broadlink_command_short_names":
						{
							response = new ResultWithData(IRBlasters.IRCommands.GetIRCommandShortNames());
							break;
						}
					#endregion
					#region BroadLink Commands
					case "broadlinkcmd_reorder":
					case "broadlinkcmd_names":
					case "broadlinkcmd_list":
					case "broadlinkcmd_new":
					case "broadlinkcmd_get":
					case "broadlinkcmd_update":
					case "broadlinkcmd_delete":
						{
							response = NamedItemAPI(requestObj, BroadLinkCommands.commands, (Action)(() =>
							{
								BroadLinkCommands.Save(ServiceWrapper.BroadLinkCommandsFile);
							}));
							break;
						}
					case "broadlinkcmd_reload_commands":
						{
							BroadLinkCommands.Load(ServiceWrapper.BroadLinkCommandsFile);
							response = new ResultSuccess();
							break;
						}
					case "beginBroadlinkCommandLearn":
						{
							int controllerId = requestObj.controllerId;
							int commandId = requestObj.commandId;
							string lessonId = BroadLinkCommands.BeginLearning(controllerId, commandId);
							if (lessonId == null)
								response = new ResultFailWithReason("hotkey not found");
							else if (lessonId == "")
								response = new ResultFailWithReason("another bind is already in progress");
							else
								response = new ResultWithData(lessonId);
							break;
						}
					case "endBroadlinkCommandLearn":
						{
							int controllerId = requestObj.controllerId;
							int commandId = requestObj.commandId;
							BroadLinkCmd command = BroadLinkCommands.commands.Get(commandId);
							if (command == null)
							{
								response = new ResultFailWithReason("command not found");
								break;
							}
							string lessonId = requestObj.lessonId;
							if (string.IsNullOrWhiteSpace(lessonId))
							{
								response = new ResultFailWithReason("invalid lessonId");
								break;
							}
							BroadLinkCommands.AwaitLearningResult(controllerId, lessonId);
							response = new ResultWithData(command);
							break;
						}
					case "cancelBroadlinkCommandLearn":
						{
							int controllerId = requestObj.controllerId;
							string lessonId = requestObj.lessonId;
							if (string.IsNullOrWhiteSpace(lessonId))
							{
								response = new ResultFailWithReason("invalid lessonId");
								break;
							}
							BroadLinkCommands.CancelLearning(controllerId, lessonId);
							response = new ResultSuccess();
							break;
						}
					case "unlearnBroadlinkCommand":
						{
							int controllerId = requestObj.controllerId;
							string lessonId = requestObj.lessonId;
							if (string.IsNullOrWhiteSpace(lessonId))
							{
								response = new ResultFailWithReason("invalid lessonId");
								break;
							}
							BroadLinkCmd command = BroadLinkCommands.UnlearnCommandCodes(controllerId, lessonId);
							if (command != null)
								response = new ResultWithData(command);
							else
								response = new ResultFailWithReason("Unable to unbind command. Please try again.");
							break;
						}
					#endregion
					#region iTach
					case "itach_reorder":
					case "itach_names":
					case "itach_list":
					case "itach_new":
					case "itach_get":
					case "itach_update":
					case "itach_delete":
						{
							response = NamedItemAPI(requestObj, ServiceWrapper.config.iTachs);
							break;
						}
					case "ir_command_short_names":
						{
							response = new ResultWithData(IRBlasters.IRCommands.GetIRCommandShortNames());
							break;
						}
					case "itach_reload_commands":
						{
							iTachCommands.Load(ServiceWrapper.iTachCommandsFile);
							response = new ResultSuccess();
							break;
						}
					#endregion
					#region Vera
					case "vera_reorder":
					case "vera_names":
					case "vera_list":
					case "vera_new":
					case "vera_get":
					case "vera_update":
					case "vera_delete":
						{
							response = NamedItemAPI(requestObj, ServiceWrapper.config.veras);
							break;
						}
					case "vera_command_list":
						{
							List<object> list = new List<object>();
							Parallel.ForEach(ServiceWrapper.config.veras.List(), vera =>
							{
								ConcurrentDictionary<int, string> map = vera.GetDeviceIdToDisplayNameMap();
								if (map != null)
								{
									int[] DeviceIds = map.Keys.ToArray();
									string[] Names = DeviceIds.Select(id => map[id]).ToArray();
									lock (list)
									{
										list.Add(new { Id = vera.id, Name = vera.name, DeviceIds = DeviceIds, Names = Names });
									}
								}
							});
							response = new ResultWithData(list);
							break;
						}
					case "vera_reload_commands":
						{
							int success = 0;
							int failure = 0;
							Parallel.ForEach(ServiceWrapper.config.veras.List(), vera =>
							{
								if (vera.LoadDisplayNames(true))
									Interlocked.Increment(ref success);
								else
									Interlocked.Increment(ref failure);
							});
							response = new ResultWithData("Vera Command Loading complete. Successful loads: " + success + ". Failed loads: " + failure);
							break;
						}
					#endregion
					#region Execute Command Manually
					case "execute":
						{
							int hotkeyId = requestObj.hotkeyId;
							if (ServiceWrapper.hotkeyManager.ExecuteHotkeyById(hotkeyId))
								response = new ResultFailWithReason("Hotkey manual execution failed");
							else
								response = new ResultSuccess();
							break;
						}
					#endregion
					default:
						response = new ResultFail() { error = "command \"" + cmd + "\" not supported" };
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.Debug(ex);
				response = new ResultFail() { error = "An unexpected error occurred. " + ex.ToString() };
			}
			finally
			{
				if (response == null)
					response = new ResultFail() { error = "Application Error: A response was not generated, so this response was generated as a fallback." };
				p.CompressResponseIfCompatible();
				p.writeSuccess(jsonType);
				p.outputStream.Write(JsonConvert.SerializeObject(response));
			}
		}
		private static object NamedItemAPI<T>(dynamic requestObj, NamedItemCollection<T> collection, Action saveCollection = null) where T : NamedItem, new()
		{
			if (saveCollection == null)
				saveCollection = (Action)(() => { ServiceWrapper.config.Save(); });
			string cmd = requestObj.cmd;
			int idxUnderscore = cmd.LastIndexOf('_');
			if (idxUnderscore > -1 && idxUnderscore + 1 < cmd.Length)
				cmd = cmd.Substring(idxUnderscore + 1);
			switch (cmd)
			{
				case "reorder":
					{
						return new ResultWithData(collection.Reorder(requestObj.ids.ToObject<List<int>>()));
					}
				case "names":
					{
						return new ResultWithData(collection.Names());
					}
				case "list":
					{
						return new ResultWithData(collection.List());
					}
				case "new":
					{
						object result = collection.New();
						if (result != null)
						{
							saveCollection();
							return new ResultWithData(result);
						}
						else
							return new ResultFailWithReason("unable to create new item");
					}
				case "get":
					{
						T item = collection.Get((int)requestObj.id);
						if (item != null)
							return new ResultWithData(item);
						else
							return new ResultFailWithReason("the item was not found");
					}
				case "update":
					{
						string itemReserialized = JsonConvert.SerializeObject(requestObj.item);
						T item = JsonConvert.DeserializeObject<T>(itemReserialized);
						if (collection.Update(item))
						{
							saveCollection();
							return new ResultWithData(item);
						}
						else
						{
							item = collection.Get(item.id);
							if (item != null)
								return new ResultFailWithData("the name was already taken", item);
							else
								return new ResultFailWithReason("the item was not found");
						}
					}
				case "delete":
					{
						collection.Delete((int)requestObj.id);
						saveCollection();
						return new ResultSuccess();
					}
				default:
					return null;
			}
		}

		private class ResultFail
		{
			public bool success = false;
			public string error;
		}

		private class ResultSuccess
		{
			public bool success = true;
		}

		private class ResultFailWithReason : ResultFail
		{
			/// <summary>
			/// Represents a failure response, providing a custom failure reason string.
			/// </summary>
			/// <param name="error">The reason for the failure.</param>
			public ResultFailWithReason(string reason)
			{
				this.error = reason;
			}
		}
		private class ResultWithData : ResultSuccess
		{
			public object data;
			public ResultWithData(object data) { this.data = data; }
		}
		private class ResultLogGet : ResultSuccess
		{
			public string status;
			public int nextLine;
			public long logId;
			public List<string> lines;
			public ResultLogGet(string status, int nextLine, long logId, List<string> lines)
			{
				this.status = status;
				this.nextLine = nextLine;
				this.logId = logId;
				this.lines = lines;
			}
		}
		private class ResultFailWithData : ResultFailWithReason
		{
			public object data;
			public ResultFailWithData(string reason, object data) : base(reason)
			{
				this.data = data;
			}
		}
	}
}
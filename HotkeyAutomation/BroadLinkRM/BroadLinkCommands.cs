using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Broadlink.Net;
using HotkeyAutomation.BroadLinkRM;
using Newtonsoft.Json;

namespace HotkeyAutomation.iTach
{
	public static class BroadLinkCommands
	{
		public static NamedItemCollection<BroadLinkCmd> commands = new NamedItemCollection<BroadLinkCmd>();

		/// <summary>
		/// Loads the [commands] list from the specified json file.
		/// </summary>
		/// <param name="path">Path to a json file containing serialized iTachCmd instances.</param>
		public static void Load(string path)
		{
			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				commands = JsonConvert.DeserializeObject<NamedItemCollection<BroadLinkCmd>>(json);
			}
		}
		public static string[] GetCommandNames(Func<BroadLinkCmd, bool> condition = null)
		{
			if (condition != null)
				return commands.Where(condition).Select(c => c.name).OrderBy(s => s).ToArray();
			else
				return commands.Names().OrderBy(s => s).ToArray();
		}

		public static void Save(string path = null)
		{
			if (path == null)
				path = ServiceWrapper.BroadLinkCommandsFile;
			JsonSerializerSettings jss = new JsonSerializerSettings();
			jss.Converters.Add(new OneLineArraySerializer());
			jss.Formatting = Formatting.Indented;
			string json = JsonConvert.SerializeObject(commands, jss);
			File.WriteAllText(path, json);
		}

		#region Command Learning
		/// <summary>
		/// Begins learning command codes for the specified command and returns the lesson ID which is a unique number assigned to this learning session.
		/// If the command ID is invalid, returns null.
		/// If a lesson operation is already in progress for the specified controller, returns empty string.
		/// </summary>
		/// <param name="controllerId">ID of the controller to do the learning.</param>
		/// <param name="commandId">Command ID to learn codes for.</param>
		/// <returns></returns>
		public static string BeginLearning(int controllerId, int commandId)
		{
			BroadLinkController controller = ServiceWrapper.config.broadLinks.Get(controllerId);
			if (controller == null)
				return null;
			return controller.BeginLearning(commandId);
		}
		/// <summary>
		/// Returns the current lesson id or null if no learning operation is currently active.
		/// </summary>
		/// <param name="controllerId">ID of the controller doing the learning.</param>
		/// <returns></returns>
		public static string GetCurrentLessonId(int controllerId)
		{
			BroadLinkController controller = ServiceWrapper.config.broadLinks.Get(controllerId);
			if (controller == null)
				return null;
			return controller.GetCurrentLessonId();
		}
		/// <summary>
		/// Cancels the current learning operation.
		/// </summary>
		/// <param name="controllerId">ID of the controller doing the learning.</param>
		/// <param name="lessonId">ID of the learning operation to cancel.</param>
		public static void CancelLearning(int controllerId, string lessonId)
		{
			BroadLinkController controller = ServiceWrapper.config.broadLinks.Get(controllerId);
			if (controller == null)
				return;
			controller.CancelLearning(lessonId);
		}
		/// <summary>
		///  Attempts to unlearn and return the command specified by the lessonId. Returns null if we aren't in the learning state or if the command does not exist.
		/// </summary>
		/// <param name="controllerId">ID of the controller doing the learning.</param>
		/// <param name="lessonId">ID of the learning operation which was started.</param>
		/// <returns></returns>
		public static BroadLinkCmd UnlearnCommandCodes(int controllerId, string lessonId)
		{
			BroadLinkController controller = ServiceWrapper.config.broadLinks.Get(controllerId);
			if (controller == null)
				return null;
			return controller.UnlearnCommandCodes(lessonId);
		}
		/// <summary>
		/// Blocks until the specified learning operation is complete.
		/// </summary>
		/// <param name="controllerId">ID of the controller doing the learning.</param>
		/// <param name="lessonId">ID of the learning operation which was started.</param>
		public static void AwaitLearningResult(int controllerId, string lessonId)
		{
			BroadLinkController controller = ServiceWrapper.config.broadLinks.Get(controllerId);
			if (controller == null)
				return;
			controller.AwaitLearningResult(lessonId);
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BPUtil;
using HotkeyAutomation.HotkeyProcessing;
using HotkeyAutomation.iTach;

namespace HotkeyAutomation
{
	public static class ServiceWrapper
	{
		public static HotkeyConfig config;
		public static HotkeyManager hotkeyManager;
		private static WebServer httpServer;
		public static StreamingLogReader logReader;
		public static string iTachCommandsFile = "";
		public static string BroadLinkCommandsFile = "";
		public static bool IsRunning { get; private set; }

		public static void Initialize()
		{
			Globals.Initialize(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Directory.SetCurrentDirectory(Globals.ApplicationDirectoryBase);
			Logger.logType = LoggingMode.Console | LoggingMode.File;

			Logger.CatchAll();

			BPUtil.SimpleHttp.SimpleHttpLogger.RegisterLogger(Logger.httpLogger);

			hotkeyManager = new HotkeyManager();

			config = new HotkeyConfig();
			config.Load();
			config.SaveIfNoExist();

			iTachCommandsFile = Globals.ApplicationDirectoryBase + "iTachCommands.json";
			iTachCommands.Load(iTachCommandsFile);

#if DEBUG
			BroadLinkCommandsFile = Globals.ApplicationDirectoryBase + "../../BroadLinkCommands.json";
#else
			BroadLinkCommandsFile = Globals.ApplicationDirectoryBase + "BroadLinkCommands.json";
#endif
			BroadLinkCommands.Load(BroadLinkCommandsFile);

			logReader = new StreamingLogReader(Globals.ErrorFilePath);
			logReader.Start();

			foreach (Vera.VeraController vera in config.veras.List())
			{
				Thread thr = new Thread(() =>
				{
					vera.LoadDisplayNames();
				});
				thr.Name = "VeraCommandLoad";
				thr.IsBackground = true;
				thr.Start();
			}

			foreach (HomeAssistant.HomeAssistantServer hass in config.homeAssistantServers.List())
			{
				Thread thr = new Thread(() =>
				{
					hass.Load();
				});
				thr.Name = "HomeAssistant Data Load";
				thr.IsBackground = true;
				thr.Start();
			}

			httpServer = new WebServer(config.httpPort);
			httpServer.SocketBound += HttpServer_SocketBound;
		}

		private static void HttpServer_SocketBound(object sender, string e)
		{
			Console.WriteLine(e);
			httpServer.SocketBound -= HttpServer_SocketBound;
		}

		public static void Start()
		{
			IsRunning = true;
			Logger.StartLoggingThreads();
			httpServer.Start();
		}
		public static void Stop()
		{
			IsRunning = false;
			Try.Catch(() => { httpServer?.Stop(); });
			Try.Catch(Logger.StopLoggingThreads);
		}
	}
}

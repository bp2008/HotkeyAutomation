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
using HotkeyAutomation.RpiGpio;

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

			httpServer = new WebServer();
			httpServer.EnableLogging(false);
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
			httpServer.SetBindings(config.httpPort, config.httpsPort < 0 ? -1 : config.httpsPort);
		}
		public static void Stop()
		{
			IsRunning = false;
			Try.Catch(() => { httpServer?.Stop(); });
		}

		#region Buzzer
		/// <summary>
		/// Buzzer may be null if not running on linux or if buzzer is not configured.  Always use null checking.
		/// </summary>
		private static Buzzer buzzer;
		private static object buzzerLock = new object();
		private static SetTimeout.TimeoutHandle buzzerTimeout;

		/// <summary>
		/// Activates the buzzer, if available, for the specified number of milliseconds.
		/// </summary>
		/// <param name="milliseconds">Milliseconds the buzzer should be activated for.</param>
		public static void ActivateBuzzer(int milliseconds)
		{
			if (!Platform.IsUnix())
				return;
			if (config.buzzerGpioNumber <= 0)
				return;
			lock (buzzerLock)
			{
				if (buzzer != null)
				{
					if (config.buzzerGpioNumber != buzzer.GpioNumber || config.buzzerGpioOutputLowToBeep != buzzer.outputHighToTurnOffSound)
					{
						buzzer.Dispose();
						buzzer = new Buzzer(config.buzzerGpioNumber, config.buzzerGpioOutputLowToBeep);
					}
				}
				else
					buzzer = new Buzzer(config.buzzerGpioNumber, config.buzzerGpioOutputLowToBeep);

				if (buzzerTimeout != null)
				{
					buzzerTimeout.Cancel();
					buzzerTimeout = null;
				}
				buzzer.Active = true;
				buzzerTimeout = SetTimeout.OnBackground(deactivateBuzzer, milliseconds, ex => Logger.Debug(ex));
			}
		}

		private static void deactivateBuzzer()
		{
			lock (buzzerLock)
			{
				if (buzzerTimeout != null && buzzer != null)
					buzzer.Active = false;
				buzzerTimeout = null;
			}
		}
		#endregion
	}
}

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
		public static WebRequestUtility http;
		public static StreamingLogReader logReader;
		public static string iTachCommandsFile = "";

		public static void Initialize()
		{
			Globals.Initialize(System.Reflection.Assembly.GetExecutingAssembly().Location);
			Directory.SetCurrentDirectory(Globals.ApplicationDirectoryBase);
			Logger.logType = LoggingMode.Console | LoggingMode.File;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			BPUtil.SimpleHttp.SimpleHttpLogger.RegisterLogger(Logger.httpLogger);

			hotkeyManager = new HotkeyManager();

			config = new HotkeyConfig();
			config.Load();
			config.SaveIfNoExist();

			iTachCommandsFile = Globals.ApplicationDirectoryBase + "iTachCommands.json";
			iTachCommands.Load(iTachCommandsFile);

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

			http = new WebRequestUtility("HotkeyAutomation " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), 5000);

			httpServer = new WebServer(config.httpPort);
			httpServer.SocketBound += HttpServer_SocketBound;
		}

		private static void HttpServer_SocketBound(object sender, string e)
		{
			Console.WriteLine(e);
			httpServer.SocketBound -= HttpServer_SocketBound;
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleUnhandledException((Exception)e.ExceptionObject, "Unhandled Exception");
		}

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			HandleUnhandledException(e.Exception, "Unhandled Thread Exception");
		}

		private static void HandleUnhandledException(Exception exception, string message)
		{
			Logger.Debug(exception, message);
		}
		public static void Start()
		{
			Logger.StartLoggingThreads();
			httpServer.Start();
		}
		public static void Stop()
		{
			Try.Catch(() => { httpServer?.Stop(); });
			Try.Catch(Logger.StopLoggingThreads);
		}

		public static void Write(string text)
		{
			Console.Write(text);
		}
		public static void WriteLine(string text)
		{
			Console.WriteLine(text);
		}
		public static void ErrorWrite(string text)
		{
			Console.Error.Write(text);
		}
		public static void ErrorWriteLine(string text)
		{
			Console.Error.WriteLine(text);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BPUtil;
using BPUtil.Linux.InputListener;

namespace HotkeyAutomation
{
	class Program
	{
		/// <summary>
		/// Set = true to cause a restart!
		/// </summary>
		public static bool restartNow;

		static void Main(string[] args)
		{
			do
			{
				restartNow = false;
				Console.WriteLine("HotkeyAutomation Server " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
				try
				{
					ServiceWrapper.Initialize();
					ServiceWrapper.Start();
					bool unix = Platform.IsUnix();
					AllKeyboardListener unixKeyListener = null;
					if (unix)
					{
						unixKeyListener = new AllKeyboardListener(5000);
						unixKeyListener.KeyDownEvent += UnixKeyListener_KeyDownEvent;
					}
					bool exit = false;
					Console.CancelKeyPress += (sender, e) =>
					{
						exit = true;
						e.Cancel = true;
					};
					while (true)
					{
						while (!Console.KeyAvailable && !exit && !restartNow)
							Thread.Sleep(1);
						if (restartNow)
						{
							Console.Write("Restarting application now! ");
							break;
						}
						if (exit)
						{
							Console.Write("Exiting");
							break;
						}
						ConsoleKeyInfo cki = Console.ReadKey(true);
						if (cki.Modifiers == ConsoleModifiers.Control && (cki.Key == ConsoleKey.C || cki.Key == ConsoleKey.Pause))
						{
							Console.Write("Exiting");
							break;
						}
						if (!unix)
							ServiceWrapper.hotkeyManager.NotifyKeyPressed((int)cki.Key);
					}
				}
				catch (ThreadAbortException) { }
				catch (Exception ex)
				{
					Logger.Debug(ex);
				}
				finally
				{
					ServiceWrapper.Stop();
					Console.WriteLine("...");
				}
			}
			while (restartNow);
		}

		private static void UnixKeyListener_KeyDownEvent(object sender, LinuxInputEventArgs e)
		{
			//Console.WriteLine(((LinuxInputListener)sender).inputDevicePath + ": " + e.Code);
			ServiceWrapper.hotkeyManager.NotifyKeyPressed(e.Code);
		}
	}
}

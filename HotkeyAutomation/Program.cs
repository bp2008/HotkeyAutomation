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
		static void Main(string[] args)
		{
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
				while (true)
				{
					ConsoleKeyInfo cki = Console.ReadKey(true);
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
			}
		}

		private static void UnixKeyListener_KeyDownEvent(object sender, LinuxInputEventArgs e)
		{
			//Console.WriteLine(((LinuxInputListener)sender).inputDevicePath + ": " + e.Code);
			ServiceWrapper.hotkeyManager.NotifyKeyPressed(e.Code);
		}
	}
}

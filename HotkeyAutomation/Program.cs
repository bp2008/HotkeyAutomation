using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BPUtil;

namespace HotkeyAutomation
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("HotkeyAutomation Server");
			try
			{
				ServiceWrapper.Initialize();
				ServiceWrapper.Start();
				while (true)
				{
					ConsoleKeyInfo cki = Console.ReadKey(true);
					ServiceWrapper.hotkeyManager.NotifyKeyPressed(cki.Key);
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
	}
}

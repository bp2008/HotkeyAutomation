using BPUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HotkeyAutomation.RpiGpio
{
	/// <summary>
	/// <para>Allows sound to be emitted from an active buzzer attached to this raspberry pi.</para>
	/// <para>Use in a <c>using</c> block.  This class implements IDisposable.</para>
	/// </summary>
	public class Buzzer : IDisposable
	{
		/// <summary>
		/// The /sys/class/gpio API was deprecated and is broken in latest Raspberry PI OS.  Not missing.  Not delivering a proper error message.  Just broken.  Completely.  That was several hours of my life I will never get back.
		/// </summary>
		private const string gpioPath = "/sys/class/gpio";
		public readonly int GpioNumber;
		public readonly bool outputHighToTurnOffSound;
		private Thread thrBackgroundBuzzer;
		private volatile bool abort = false;
		private volatile bool active = false;
		EventWaitHandle activeChangeWaiter = new EventWaitHandle(true, EventResetMode.AutoReset);
		/// <summary>
		/// Set to true to activate the buzzer.
		/// </summary>
		public bool Active
		{
			get { return active; }
			set
			{
				active = value;
				activeChangeWaiter.Set();
			}
		}

		public Buzzer(int gpioNumber, bool outputHighToTurnOffSound)
		{
			this.outputHighToTurnOffSound = outputHighToTurnOffSound;
			GpioNumber = gpioNumber;

			thrBackgroundBuzzer = new Thread(buzzerLoop);
			thrBackgroundBuzzer.IsBackground = true;
			thrBackgroundBuzzer.Name = "Buzzer Thread";
			thrBackgroundBuzzer.Start();
		}

		private void buzzerLoop()
		{
			try
			{
				while (!abort)
				{
					if (active)
						ConfigurePin(true, !outputHighToTurnOffSound);
					else
						ConfigurePin(true, outputHighToTurnOffSound);
					activeChangeWaiter.WaitOne();
				}
			}
			catch (Exception ex)
			{
				Logger.Debug(ex, "Buzzer loop for GPIO" + GpioNumber + " crashed.");
			}
			finally
			{
				try
				{
					ConfigurePin(true, outputHighToTurnOffSound);
				}
				catch { }
			}
		}

		void ConfigurePin(bool output, bool high)
		{
			if (output)
			{
				if (high)
					RunPinCtrl($"set {GpioNumber} op dh");
				else
					RunPinCtrl($"set {GpioNumber} op dl");
			}
			else
				RunPinCtrl($"set {GpioNumber} ip");
		}
		void RunPinCtrl(string args)
		{
			int retCode = ProcessRunner.RunProcessAndWait("pinctrl", args, out string std, out string err);
			if (!string.IsNullOrWhiteSpace(err) || retCode != 0)
				throw new Exception("pinctrl returned code " + retCode + ": " + err);
		}

		#region IDisposable
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
				}
				abort = true;
				activeChangeWaiter.Set();
				// free unmanaged resources (unmanaged objects) and override finalizer
				// set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~Buzzer()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
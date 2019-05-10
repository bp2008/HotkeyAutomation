using System;
using System.Linq;
using Newtonsoft.Json;

namespace HotkeyAutomation.iTach
{
	public class iTachCmd
	{
		/// <summary>
		/// Short name of the command. Should be unique.
		/// </summary>
		public string ShortName;
		/// <summary>
		/// Long name of the command, should use minimal abbreviations.
		/// </summary>
		public string LongName;
		/// <summary>
		/// Connector address, e.g. "1:1", "1:2", or "1:3". May be overridden by individual hotkey binds.
		/// </summary>
		public string ConnectorAddress;
		/// <summary>
		/// IR Frequency in hertz, e.g. 38000
		/// </summary>
		public int Frequency;
		/// <summary>
		/// IR codes (on, off, on, off, …). Must have an even size, minimum values based on the frequency, etc as noted in the iTach API specification.
		/// </summary>
		public ushort[] Codes = new ushort[0];
		/// <summary>
		/// Number of times to repeat the command. May be overridden by individual hotkey binds.
		/// </summary>
		public byte Repeat = 1;
		/// <summary>
		/// The repeat offset.  Must be an odd number.
		/// </summary>
		public ushort RepeatOffset = 1;

		/// <summary>
		/// Returns an array of pulse lengths with each value in microseconds, to aid in interoperability with other IR systems.
		/// </summary>
		/// <returns></returns>
		public double[] GetMicrosecondPulses()
		{
			return Codes.Select(p => IRBlasters.Converter.iTachPulseSizeToMicroSeconds(p)).ToArray();
		}
	}
}
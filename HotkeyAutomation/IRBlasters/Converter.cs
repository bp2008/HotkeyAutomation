using BPUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyAutomation.IRBlasters
{
	public static class Converter
	{
		// https://github.com/bp2008/broadlink-dotnet/blob/master/BroadlinkProtocol.md

		private const double BroadlinkPulseDurationInSeconds = 0.0000305175781; // a.k.a. (2 ^ -15).  a.k.a. (1 / 32768).  Why isn't this 1/38000?  Maybe it is and the guy who wrote the documentation is wrong?
		private const double BroadlinkPulseDurationInMicroSeconds = 30.5175781; // 1 second = 1,000,000 microseconds
		private const double iTachPulseDurationInSeconds = 0.00002631578; // 38000 KHz = (1 / 38000)
		private const double iTachPulseDurationInMicroSeconds = 26.31578;
		private const double OneMicrosecondInSeconds = 0.000001;
		public static ushort MicroSecondsToBroadlinkPulseSize(double microSeconds)
		{
			return BPMath.Clamp((ushort)Math.Round(microSeconds / BroadlinkPulseDurationInMicroSeconds), (ushort)1, ushort.MaxValue);
		}
		public static ushort MicroSecondsToiTachPulseSize(double microSeconds)
		{
			return (ushort)Math.Round(microSeconds / iTachPulseDurationInMicroSeconds);
		}
		public static double BroadlinkPulseSizeToMicroSeconds(ushort pulseSize)
		{
			return BPMath.Clamp(pulseSize * BroadlinkPulseDurationInMicroSeconds, (ushort)4, ushort.MaxValue); // iTach requires an 80uS minimum time, which at 38000 KHz means a minimum value of 4.
		}
		public static double iTachPulseSizeToMicroSeconds(ushort pulseSize)
		{
			return pulseSize * iTachPulseDurationInMicroSeconds;
		}
		public static ushort[] iTachPulseDataToBroadlinkPulseData(ushort[] iTachPulseData)
		{
			return iTachPulseData.Select(i => MicroSecondsToBroadlinkPulseSize(iTachPulseSizeToMicroSeconds(i))).ToArray();
		}
		public static ushort[] BroadlinkPulseDataToiTachPulseData(ushort[] broadlinkPulseData)
		{
			return broadlinkPulseData.Select(i => MicroSecondsToiTachPulseSize(BroadlinkPulseSizeToMicroSeconds(i))).ToArray();
		}
	}
}

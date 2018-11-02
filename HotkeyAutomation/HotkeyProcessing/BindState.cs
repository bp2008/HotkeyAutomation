using System.Threading;
using BPUtil;

namespace HotkeyAutomation.HotkeyProcessing
{
	public class BindState
	{
		private static long bindCounter = 0;
		public readonly string bindId = Interlocked.Increment(ref bindCounter) + "|" + TimeUtil.GetTimeInMsSinceEpoch();
		public readonly int hotkeyId;
		public readonly long bindStart = TimeUtil.GetTimeInMsSinceEpoch();
		public BindState(int hotkeyId)
		{
			this.hotkeyId = hotkeyId;
		}
	}
}
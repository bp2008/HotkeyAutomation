using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyAutomation.IRBlasters
{
	public class IRCommand
	{
		/// <summary>
		/// Array of pulses [on] [off] [on] [off], with each value measured in microseconds.
		/// </summary>
		public double[] pulses;
	}
}

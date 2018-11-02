using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HotkeyAutomation.HotkeyProcessing
{
	public class Effect
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public EffectType type;
		public EffectData data;
	}
}
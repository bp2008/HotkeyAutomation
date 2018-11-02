using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HotkeyAutomation.HotkeyProcessing
{
	/// <summary>
	/// Represents effect data.  Instead of using separate classed and/or inheritance, this is all in one class to make serialization and deserialization easier.
	/// </summary>
	public class EffectData
	{
		#region HttpGet
		public string httpget_url;
		#endregion
		#region iTach
		public string itach_name;
		public string itach_connectorAddress;
		public string itach_commandShortName;
		public byte itach_repeatCount;
		#endregion
		#region Vera
		public string vera_name;
		public int? vera_deviceNum;
		[JsonConverter(typeof(StringEnumConverter))]
		public VeraService? vera_service;
		public string vera_value;
		#endregion
	}
	public enum VeraService
	{
		DimmerValue,
		SwitchSet,
		CurtainStop
	}
}
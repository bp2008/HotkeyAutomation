using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HotkeyAutomation.iTach
{
	public class OneLineArraySerializer : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(string[])
				|| objectType == typeof(byte[])
				|| objectType == typeof(short[])
				|| objectType == typeof(ushort[])
				|| objectType == typeof(int[])
				|| objectType == typeof(uint[])
				|| objectType == typeof(long[])
				|| objectType == typeof(ulong[])
				|| objectType == typeof(float[])
				|| objectType == typeof(double[])
				|| objectType == typeof(decimal[]);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return reader.Value; // Untested
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteRawValue(JsonConvert.SerializeObject(value, Formatting.None));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BPUtil;

namespace HotkeyAutomation.iTach
{
	public class iTachController : NamedItem
	{
		// public string name; // Inherited from NamedItem
		public string host;
		public int? port;
		private int GetPort()
		{
			return port == null ? 4998 : port.Value;
		}
		public iTachController() { }
		public iTachController(dynamic item)
		{
			name = item.name;
			host = item.host;
			port = item.port;
		}
		public iTachController(string host, int port)
		{
			this.host = host;
			this.port = port;
		}
		private object myLock = new object();
		private ushort idCounter = 0;

		/// <summary>
		/// Sends the specified iTachCmd and returns null upon success, or the response string or other error message if the iTach returned an error response.  May throw an exception if there is a network error.
		/// </summary>
		/// <param name="cmd">The command to send.</param>
		/// <param name="connectorAddress">The connector address. If null, the address specified in the command will be used.</param>
		/// <param name="repeat">(Optional, 1-50) Number of times the iTach should repeat the command.</param>
		/// <returns></returns>
		public string SendCommandSync(iTachCmd cmd, string connectorAddress, byte? repeat = null)
		{
			if (string.IsNullOrWhiteSpace(host))
				return "This iTachController does not have the host specified.";
			lock (myLock)
			{
				ushort myId = idCounter++;
				if (repeat == null)
					repeat = cmd.Repeat;
				repeat = BPMath.Clamp(repeat.Value, (byte)1, (byte)50); // 50 is iTach's built-in limit
				if (connectorAddress == null)
					connectorAddress = cmd.ConnectorAddress;
				else if (!Regex.IsMatch(connectorAddress, "^\\d+:\\d+$"))
				{
					Logger.Debug("iTach effect specified invalid connector address  \"" + connectorAddress + "\"");
					connectorAddress = cmd.ConnectorAddress;
				}
				string commandText = "sendir," + connectorAddress + "," + myId + "," + cmd.Frequency + "," + repeat + "," + cmd.RepeatOffset + "," + string.Join(",", cmd.Codes) + "\r";
				string expectedResponse = "completeir," + connectorAddress + "," + myId + "\r";
				byte[] buff = Encoding.ASCII.GetBytes(commandText);
				using (TcpClient client = new TcpClient(host, GetPort()))
				{
					using (NetworkStream stream = client.GetStream())
					{
						stream.Write(buff, 0, buff.Length);

						StringBuilder sb = new StringBuilder();
						int i;
						while ((i = stream.ReadByte()) != -1)
						{
							char c = (char)i;
							sb.Append(c);
							if (c == '\r')
								break;
						}
						string response = sb.ToString();
						return response == expectedResponse ? null : response;
					}
				}
			}
		}
		/// <summary>
		/// Sends the specified iTachCmd.
		/// </summary>
		/// <param name="commandShortName">Short name of the command to send.</param>
		/// <param name="connectorAddress">The connector address. If null, the address specified in the command will be used.</param>
		/// <param name="repeat">(Optional, 1-50) Number of times the iTach should repeat the command.</param>
		public void SendCommandSync(string commandShortName, string connectorAddress, byte? repeat = null)
		{
			if (commandShortName == null)
			{
				Logger.Debug("iTach command null is not valid");
				return;
			}
			if (iTachCommands.commands.TryGetValue(commandShortName, out iTachCmd cmd))
			{
				string result = SendCommandSync(cmd, connectorAddress, repeat);
				if (result != null)
					Logger.Debug("iTach command \"" + commandShortName + "\" failed on connector address \"" + connectorAddress + "\" with repeat value " + repeat + ": " + result);
			}
			else
			{
				Logger.Debug("iTach command \"" + commandShortName + "\" not found");
			}
		}
	}
}

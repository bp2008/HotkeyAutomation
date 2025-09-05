using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using BPUtil.SimpleHttp;
using BPUtil;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using static BPUtil.ByteUtil;

namespace HotkeyAutomation
{
	public class WebServer : HttpServer
	{
		private WebpackProxy webpackProxy = null;
		public WebServer(X509Certificate2 cert = null) : base(SimpleCertificateSelector.FromCertificate(cert))
		{
			if (ServiceWrapper.config.devMode)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("Starting web server in dev mode. Webpack Proxy is enabled.");
				Console.ResetColor();
				webpackProxy = new WebpackProxy(9000, Globals.ApplicationDirectoryBase + "../../");
			}
		}

		public override void handleGETRequest(HttpProcessor p)
		{
			BasicEventTimer bet = new BasicEventTimer();
			bet.Start("GET " + p.Request.Page);
			try
			{
				string pageLower = p.Request.Page.ToLower();
				if (pageLower == "json")
				{
					p.Response.Simple("405 Method Not Allowed", "json API requests must use the POST method");
				}
				else if (pageLower == "broadlinkcommands.json")
				{
					if (File.Exists(ServiceWrapper.BroadLinkCommandsFile))
					{
						byte[] content = File.ReadAllBytes(ServiceWrapper.BroadLinkCommandsFile);
						p.Response.FullResponseUTF8(ByteUtil.Utf8NoBOM.GetString(content), "application/json");
					}
					else
						p.Response.Simple("404 Not Found");
				}
				else if (pageLower == "itachcommands.json")
				{
					if (File.Exists(ServiceWrapper.iTachCommandsFile))
					{
						byte[] content = File.ReadAllBytes(ServiceWrapper.iTachCommandsFile);
						p.Response.FullResponseUTF8(ByteUtil.Utf8NoBOM.GetString(content), "application/json");
					}
					else
						p.Response.Simple("404 Not Found");
				}
				else if (pageLower == "downloadconfiguration")
				{
					string filename = "HotkeyAutomationConfig_" + Environment.MachineName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".zip";
					byte[] fileData;
					using (MemoryStream ms = new MemoryStream())
					{
						ConfigurationIO.WriteToStream(ms);
						fileData = ms.ToArray();
					}
					p.Response.FullResponseBytes(fileData, "application/zip");
					p.Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + filename + "\"");
				}
				else
				{
					#region www
					DirectoryInfo WWWDirectory = new DirectoryInfo(ServiceWrapper.config.GetWWWDirectoryBase());
					string wwwDirectoryBase = WWWDirectory.FullName.Replace('\\', '/').TrimEnd('/') + '/';

					FileInfo fi = null;
					if (p.Request.Page == "")
						fi = GetDefaultFile(wwwDirectoryBase);
					else
					{
						try
						{
							fi = new FileInfo(wwwDirectoryBase + p.Request.Page);
						}
						catch
						{
							fi = GetDefaultFile(wwwDirectoryBase);
						}
					}
					string targetFilePath = fi.FullName.Replace('\\', '/');
					if (!targetFilePath.StartsWith(wwwDirectoryBase) || targetFilePath.Contains("../"))
					{
						p.Response.Simple("400 Bad Request");
						return;
					}
					if (webpackProxy != null)
					{
						// Handle hot module reload provided by webpack dev server.
						switch (fi.Extension.ToLower())
						{
							case ".js":
							case ".map":
							case ".css":
							case ".json":
								bet.Start("Proxy Start");
								webpackProxy.Proxy(p);
								bet.Start("Proxy End");
								return;
						}
					}
					if (!fi.Exists)
					{
						fi = GetDefaultFile(wwwDirectoryBase);
						if (!fi.Exists)
						{
							p.Response.Simple("404 Not Found");
							return;
						}
					}

					if ((fi.Extension == ".html" || fi.Extension == ".htm") && fi.Length < 256000)
					{
						bet.Start("Write HTML");
						string html = File.ReadAllText(fi.FullName);
						try
						{
							html = html.Replace("%%SYSTEM_NAME%%", ServiceWrapper.config.systemName);
							html = html.Replace("%%APP_VERSION%%", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
							html = html.Replace("%%APPPATH%%", "/");
						}
						catch (Exception ex)
						{
							Logger.Debug(ex);
						}
						p.Response.FullResponseUTF8(html, Mime.GetMimeType(fi.Extension));
					}
					else
					{
						bet.Start("Write Response");
						p.Response.StaticFile(fi.FullName);
						if (fi.LastWriteTimeUtc.ToString("R") == p.Request.Headers.Get("if-modified-since"))
						{
							p.Response.ContentType = Mime.GetMimeType(fi.Extension);
							p.Response.StatusString = "304 Not Modified";
							return;
						}
					}
					#endregion
				}
			}
			finally
			{
				bet.Stop();
				//Logger.Info(bet.ToString("\r\n"));
			}
		}

		private FileInfo GetDefaultFile(string wwwDirectoryBase)
		{
			return new FileInfo(wwwDirectoryBase + "Default.html");
		}

		public override void handlePOSTRequest(HttpProcessor p)
		{
			ReadToEndResult bodyReadResult = ByteUtil.ReadToEndWithMaxLength(p.Request.RequestBodyStream, 10 * 1024 * 1024);
			byte[] data = bodyReadResult.Data ?? new byte[0];

			p.GetTcpClient().NoDelay = true;

			string pageLower = p.Request.Page.ToLower();
			if (pageLower == "json")
			{
				JSONAPI.HandleRequest(p, ByteUtil.Utf8NoBOM.GetString(data));
			}
			else if (pageLower == "uploadconfiguration")
			{
				using (MemoryStream ms = new MemoryStream(data))
				{
					bool success = ConfigurationIO.ReadFromStream(ms);
					p.Response.FullResponseUTF8(success ? "1" : "0", "text/plain");
					if (success)
					{
						Thread thrRestartSelf = new Thread(() =>
						{
							try
							{
								Thread.Sleep(1000);
								Program.restartNow = true;
							}
							catch (Exception ex)
							{
								Logger.Debug(ex);
							}
						});
						thrRestartSelf.Name = "Restart Self";
						thrRestartSelf.IsBackground = true;
						thrRestartSelf.Start();
					}
				}
			}
		}
		protected override void stopServer()
		{
		}
	}
}

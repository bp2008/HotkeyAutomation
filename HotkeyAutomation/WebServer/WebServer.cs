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

namespace HotkeyAutomation
{
	public class WebServer : HttpServer
	{
		private static bool enableCaching = true;
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
			bet.Start("GET " + p.requestedPage);
			try
			{
				string pageLower = p.requestedPage.ToLower();
				if (pageLower == "json")
				{
					p.writeFailure("405 Method Not Allowed", "json API requests must use the POST method");
				}
				else if (pageLower == "broadlinkcommands.json")
				{
					if (File.Exists(ServiceWrapper.BroadLinkCommandsFile))
					{
						byte[] content = File.ReadAllBytes(ServiceWrapper.BroadLinkCommandsFile);
						p.writeSuccess("application/json", content.Length);
						p.outputStream.Flush();
						p.tcpStream.Write(content, 0, content.Length);
					}
					else
						p.writeFailure();
				}
				else if (pageLower == "itachcommands.json")
				{
					if (File.Exists(ServiceWrapper.iTachCommandsFile))
					{
						byte[] content = File.ReadAllBytes(ServiceWrapper.iTachCommandsFile);
						p.writeSuccess("application/json", content.Length);
						p.outputStream.Flush();
						p.tcpStream.Write(content, 0, content.Length);
					}
					else
						p.writeFailure();
				}
				else if (pageLower == "downloadconfiguration")
				{
					string filename = "HotkeyAutomationConfig_" + Environment.MachineName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".zip";
					HttpHeaderCollection additionalHeaders = new HttpHeaderCollection();
					additionalHeaders.Add("Content-Disposition", "attachment; filename=\"" + filename + "\"");
					p.writeSuccess("application/zip", additionalHeaders: additionalHeaders);
					p.outputStream.Flush();
					ConfigurationIO.WriteToStream(p.tcpStream);
				}
				else
				{
					#region www
					DirectoryInfo WWWDirectory = new DirectoryInfo(ServiceWrapper.config.GetWWWDirectoryBase());
					string wwwDirectoryBase = WWWDirectory.FullName.Replace('\\', '/').TrimEnd('/') + '/';

					FileInfo fi = null;
					if (p.requestedPage == "")
						fi = GetDefaultFile(wwwDirectoryBase);
					else
					{
						try
						{
							fi = new FileInfo(wwwDirectoryBase + p.requestedPage);
						}
						catch
						{
							fi = GetDefaultFile(wwwDirectoryBase);
						}
					}
					string targetFilePath = fi.FullName.Replace('\\', '/');
					if (!targetFilePath.StartsWith(wwwDirectoryBase) || targetFilePath.Contains("../"))
					{
						p.writeFailure("400 Bad Request");
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
							p.writeFailure();
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
						p.writeSuccess(Mime.GetMimeType(fi.Extension));
						p.outputStream.Write(html);
						p.outputStream.Flush();
					}
					else
					{
						bet.Start("Write Response");
						if (fi.LastWriteTimeUtc.ToString("R") == p.GetHeaderValue("if-modified-since"))
						{
							p.writeSuccess(Mime.GetMimeType(fi.Extension), -1, "304 Not Modified");
							return;
						}
						using (FileStream fs = fi.OpenRead())
						{
							p.writeSuccess(Mime.GetMimeType(fi.Extension), fi.Length, additionalHeaders: GetCacheLastModifiedHeaders(TimeSpan.FromHours(1), fi.LastWriteTimeUtc));
							p.outputStream.Flush();
							fs.CopyTo(p.tcpStream);
							p.tcpStream.Flush();
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

		private HttpHeaderCollection GetCacheEtagHeaders(TimeSpan maxAge, string etag)
		{
			HttpHeaderCollection additionalHeaders = new HttpHeaderCollection();
			if (enableCaching)
			{
				additionalHeaders.Add(new KeyValuePair<string, string>("Cache-Control", "max-age=" + (long)maxAge.TotalSeconds + ", public"));
				additionalHeaders.Add(new KeyValuePair<string, string>("ETag", etag));
			}
			return additionalHeaders;
		}
		private HttpHeaderCollection GetCacheLastModifiedHeaders(TimeSpan maxAge, DateTime lastModifiedUTC)
		{
			HttpHeaderCollection additionalHeaders = new HttpHeaderCollection();
			if (enableCaching)
			{
				additionalHeaders.Add(new KeyValuePair<string, string>("Cache-Control", "max-age=" + (long)maxAge.TotalSeconds + ", public"));
				additionalHeaders.Add(new KeyValuePair<string, string>("Last-Modified", lastModifiedUTC.ToString("R")));
			}
			return additionalHeaders;
		}

		public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
		{
			string pageLower = p.requestedPage.ToLower();
			p.tcpClient.NoDelay = true;
			if (pageLower == "json")
			{
				JSONAPI.HandleRequest(p, inputData.ReadToEnd());
			}
			else if (pageLower == "uploadconfiguration")
			{
				bool success = ConfigurationIO.ReadFromStream(p.RequestBodyStream);
				p.writeSuccess("text/plain", 1);
				p.outputStream.Write(success ? "1" : "0");
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
		protected override void stopServer()
		{
		}
	}
}

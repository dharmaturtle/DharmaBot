namespace IRCbot
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlServerCe;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using System.Threading.Tasks;
	using System.Threading.Tasks.Dataflow;

	public class MainClass
	{
		public static void Main()
		{
			try
			{
				// loaded into memory due to frequency of use
				var dbContext = new db(new SqlCeConnection("Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091")); // http://dotnet.dzone.com/articles/sql-server-compact-4-desktop
				MyGlobals.BanWords = new Dictionary<string, List<string>>();
				MyGlobals.BanWords["AutoBanList"] = dbContext.AutoBanList.Select(e => e.Word).ToList();
				MyGlobals.BanWords["AutoTempBanList"] = dbContext.AutoTempBanList.Select(e => e.Word).ToList();
				MyGlobals.ModVariables = new Dictionary<string, string>();
				MyGlobals.ModVariables = dbContext.ModVariables.Select(t => new { t.Variable, t.Value }) // http://stackoverflow.com/questions/953919/convert-linq-query-result-to-dictionary
																			.ToDictionary(t => t.Variable, t => t.Value);
				Connect();
				var modAsync = new ModAsync();
				var logSync = new LogSync();
				var rawIRCAsync = new RawIRCAsync();
				var rawIRCBuffer =
					new BufferBlock<string>(new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });
			
				// start consumers
				rawIRCAsync.Consumer(rawIRCBuffer);
				logSync.Consumer(Constants.LogBuffer);
				modAsync.Consumer(Constants.ModBuffer);
			
				// infinite raw producer
				while (true)
				{
					rawIRCBuffer.Post(MyGlobals.Input.ReadLine());
				}
			}
			catch (Exception e)
			{
				Log("An error occured!", ConsoleColor.Red);
				Log(e.Message, ConsoleColor.Red);
				Log(e.Source, ConsoleColor.Red);
				Log(e.StackTrace, ConsoleColor.Red);
			}
		}

		/****************************************************************************************************
		 *											COMMON METHODS											*
		 ****************************************************************************************************/

		// sends to channel
		public static void SendMessage(string msg)
		{
			WriteAndFlush("PRIVMSG " + Constants.Chan + " :" + msg + "\n");
		}

		// get website data
		public static async Task<string> DownloadData(string url)
		{
			try // http://stackoverflow.com/questions/13240915/converting-a-webclient-method-to-async-await
			{
				var client = new WebClient();
				var data = await client.DownloadStringTaskAsync(url);
				return data;
			}
			catch (Exception e)
			{
				Log("An error in DownloadData!", ConsoleColor.Red);
				Log(e.Message, ConsoleColor.Red);
				Log(e.Source, ConsoleColor.Red);
				Log(e.StackTrace, ConsoleColor.Red);
				return "Error! " + e;
			}
		}

		// connect to server
		public static void Connect()
		{
			var sock = new TcpClient();
			
			sock.Connect("irc.twitch.tv", 6667);

			if (!sock.Connected)
			{
				Log("Failed to connect!", ConsoleColor.Red);
				return;
			}

			MyGlobals.Input = new StreamReader(sock.GetStream());
			MyGlobals.Output = new StreamWriter(sock.GetStream());
			WriteAndFlush(
				"PASS " + PrivateConstants.Oauth + "\n" +
				"USER " + Constants.Nick + " 0 * :" + Constants.Owner + "\n" +
				"NICK " + Constants.Nick + "\n" +
				"JOIN " + Constants.Chan + "\n");  // could wait for 001 from server, but this works anyway
		}

		// Why write two lines when you can write one
		public static void WriteAndFlush(string data)
		{
			MyGlobals.Output.Write(data);
			MyGlobals.Output.Flush();
		}

		// colors console and prepends timestamp
		public static void Log(string text, ConsoleColor color = ConsoleColor.White)
		{
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write(DateTime.Now.ToString("t"));
			Console.ForegroundColor = color;
			Console.WriteLine(" " + text);
			Console.ResetColor();
		}

		// human readable time deltas
		public static string DeltaTimeFormat(TimeSpan span, string rough = "")
		{
			int day = Convert.ToInt16(span.ToString("%d")),
				hour = Convert.ToInt16(span.ToString("%h")),
				minute = Convert.ToInt16(span.ToString("%m"));

			if (span.CompareTo(TimeSpan.Zero) == -1)
			{
				Log("Time to sync the clock?" + span, ConsoleColor.Red);
				return "A few seconds";
			}

			if (day > 1)
			{
				if (hour == 0) return day + " days";
				return day + " days " + hour + "h";
			}

			if (day == 1)
			{
				if (hour == 0) return "a day";
				return "a day " + hour + "h";
			}

			if (hour == 0) return rough + minute + "m";
			if (minute == 0) return rough + hour + "h";

			return rough + hour + "h " + minute + "m";
		}

		// url unshorteners
		public static string UnTinyUrl(string link)
		{
			var redirectedto = link; // http://stackoverflow.com/questions/3175062/long-urls-from-short-ones-using-c-sharp
			var cycle = 3;
			while (cycle > 0)
			{
				// gives it 3 tries to untiny the URL before giving up
				try
				{
					var request = (HttpWebRequest)WebRequest.Create(link);
					request.AllowAutoRedirect = false;
					request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3 (.NET CLR 4.0.20506)";
					var response = (HttpWebResponse)request.GetResponse();
					if ((int)response.StatusCode == 301 || (int)response.StatusCode == 302)
					{
						redirectedto = response.Headers["Location"];
						//// Log("Redirecting " + url + " to " + redirectedto + " because " + response.StatusCode);
						cycle = 0;
					}
					else
					{
						Log("Not redirecting " + link + " because " + response.StatusCode);
						cycle--;
					}
				}
				catch (Exception ex)
				{
					ex.Data.Add("url", link);
					Log(ex.ToString());
					cycle--;
				}
			}

			return redirectedto;
		}
	}
}
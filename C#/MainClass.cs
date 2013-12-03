using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IRCbot
{
	public class MainClass
	{
		public static void Main()
		{
			string received = null, sender, message, MESSAGE;
			var parserawirc = new Regex(@":(.*)!.*(?:privmsg).*?:(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			MyGlobals.IsBanned = false;
			
			
			//frequently used  vars are loaded into memory for speed.) Loaded into memory because they're used frequently
			MyGlobals.Banwords = new Dictionary<string, List<string>>();
			MyGlobals.Banwords["AutoBanList"] = Constants.DBcontext.AutoBanList.Select(e => e.Word).ToList();
			MyGlobals.Banwords["AutoTempBanList"] = Constants.DBcontext.AutoTempBanList.Select(e => e.Word).ToList();
			MyGlobals.ModVariables = new Dictionary<string, string>();
			MyGlobals.ModVariables = Constants.DBcontext.ModVariables.Select(t => new { t.Variable, t.Value })//http://stackoverflow.com/questions/953919/convert-linq-query-result-to-dictionary
																	.ToDictionary(t => t.Variable, t => t.Value);

			Connect();
			ParallelWhile(delegate
			{
				try
				{
					var readLine = MyGlobals.Input.ReadLine();
					if (readLine != null) received = readLine.Trim(new[] {'\r', '\n', ' '});

					/* disconnected */
					if (received != null && received.Length == 0) Connect();

					/* get user & privmsg */
					if (received == null) return;
					var sendermessage = parserawirc.Match(received);
					sender = sendermessage.Groups[1].Value;
					MESSAGE = sendermessage.Groups[2].Value;
					message = MESSAGE.ToLower();
					if (sendermessage.Success)
					{
						/* log latest lines */
						var stalk = (from y in Constants.DBcontext.Stalk
									where y.User == sender
									select y).SingleOrDefault();
						if (stalk == null)
						{
							stalk = new Stalk();
							Constants.DBcontext.Stalk.InsertOnSubmit(stalk);
						}
						stalk.User = sender;
						stalk.Time = DateTime.Now;
						stalk.Message = MESSAGE;
						Constants.DBcontext.SubmitChanges();

						/* print to console */
						Log(sender + ": " + message, new[] {"dharm", "darm", "dhram"}.Any(received.Contains) ? ConsoleColor.Green : ConsoleColor.Gray);

						/* mod */
						if (PrivateConstants.Mods.Contains(sender))
						{
							// TODO: make sure you append longspamlist if over x lines
							if (message[0] != '!') return;
							ModClass.Parse(message.Substring(1));
							BasicCommandsClass.Parse(message.Substring(1));
						}
						
						/* pleb */
						else
						{
							BanLogicClass.Parse(message);
							if (((DateTime.Now - MyGlobals.Pleblag).TotalSeconds >= 15) && (MyGlobals.IsBanned == false) && (message[0] == '!'))
							{
								BasicCommandsClass.Parse(message.Substring(1));
							}
						}
					}
					
					/* pongs */
					else if (received.StartsWith("PING "))
					{
						WriteAndFlush(received.Replace("PING", "PONG") + "\n");
						Log(received, ConsoleColor.DarkGray);
					}
					
					/* print server text */
					else Log(received, ConsoleColor.DarkGray);
				}
				catch (Exception e)
				{
					Log("An error occured!", ConsoleColor.Red);
					Log(e.Message, ConsoleColor.Red);
					Log(e.Source, ConsoleColor.Red);
					Log(e.StackTrace, ConsoleColor.Red);
				}
			});
		}

		/****************************************************************************************************
		 *											COMMON METHODS											*
		 ****************************************************************************************************/

		/* Parallel while loop */
		public static void ParallelWhile(Action body)
		{
			Parallel.ForEach(ForeverTrue(), ignored => body());
		}

		private static IEnumerable<bool> ForeverTrue()
		{
			while (true) yield return true;
		}

		/* global vars */
		public static class MyGlobals {
			public static TextReader Input { get; set; }
			public static TextWriter Output { get; set; }
			public static DateTime Pleblag { get; set; }
			public static Boolean IsBanned { get; set; }
			public static Dictionary<string, string> ModVariables { get; set; }
			public static Dictionary<string, List<string>> Banwords { get; set; }
		}
		
		public static class Constants
		{
			public static readonly TcpClient Sock = new TcpClient();
			public static readonly db DBcontext = new db(new SqlCeConnection("Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091")); //http://dotnet.dzone.com/articles/sql-server-compact-4-desktop
			public const string Nick	= "dharmaturtle",
								Owner	= "dharmaturtle",
								Server	= "irc.twitch.tv",
								Chan	= "#dharmaturtle2";
		}

		/* sends to channel */
		public static void Sendmessage(string msg)
		{
			WriteAndFlush("PRIVMSG " + Constants.Chan + " :" + msg + "\n");
		}

		/* get website data */
		public static async Task<string> DLdata(string url)
		{
			try //http://stackoverflow.com/questions/13240915/converting-a-webclient-method-to-async-await
			{
				var client = new WebClient();
				var data = await client.DownloadStringTaskAsync(url);
				return data;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return "Error! " + ex;
			}
		}

		/* connect to server */
		public static void Connect()
		{
			Constants.Sock.Connect("irc.twitch.tv", 6667);
			if (!Constants.Sock.Connected)
			{
				Log("Failed to connect!", ConsoleColor.Red);
				return;
			}
			MyGlobals.Input = new StreamReader(Constants.Sock.GetStream());
			MyGlobals.Output = new StreamWriter(Constants.Sock.GetStream());
			WriteAndFlush(
				"PASS " + PrivateConstants.Oauth + "\n" +
				"USER " + Constants.Nick + " 0 * :" + Constants.Owner + "\n" +
				"NICK " + Constants.Nick + "\n" +
				"JOIN " + Constants.Chan + "\n" // could wait for 001 from server, but this works anyway
			);
		}

		/* Why write two lines when you can write one */
		public static void WriteAndFlush(string str)
		{
			MyGlobals.Output.Write(str);
			MyGlobals.Output.Flush();
		}

		/* colors console and prepends timestamp */
		public static void Log(string text, ConsoleColor color = ConsoleColor.White)
		{
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write(DateTime.Now.ToString("t"));
			Console.ForegroundColor = color;
			Console.WriteLine(" " + text);
			Console.ResetColor();
		}

		/* human readable time deltas */
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
				if (hour == 0)	return day + " days";
				return day + " days " + hour + "h";
			}
			if (day == 1)
			{
				if (hour == 0)	return "a day";
				return "a day " + hour + "h";
			}
			if (hour == 0)		return rough + minute + "m";
			if (minute == 0)	return rough + hour + "h";

			return rough + hour + "h " + minute + "m";
		}

		/* url unshorteners */
		public static string UnTinyUrl(string url)
		{
			var redirectedto = url; // http://stackoverflow.com/questions/3175062/long-urls-from-short-ones-using-c-sharp
			var cycle = 3;
			while (cycle > 0)
			{
				// gives it 3 tries to untiny the URL before giving up
				try
				{
					var request = (HttpWebRequest) WebRequest.Create(url);
					request.AllowAutoRedirect = false;
					request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3 (.NET CLR 4.0.20506)";
					var response = (HttpWebResponse) request.GetResponse();
					if ((int) response.StatusCode == 301 || (int) response.StatusCode == 302)
					{
						redirectedto = response.Headers["Location"];
						//log("Redirecting " + url + " to " + redirectedto + " because " + response.StatusCode);
						cycle = 0;
					}
					else
					{
						Console.WriteLine(response.Headers["Location"]);
						Log("Not redirecting " + url + " because " + response.StatusCode);
						cycle--;
					}
				}
				catch (Exception ex)
				{
					ex.Data.Add("url", url);
					Log(ex.ToString());
					cycle--;
				}
			}
			return redirectedto;
		}
	}
}
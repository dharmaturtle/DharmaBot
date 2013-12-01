using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Data.SqlServerCe;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace IRCbot {
	public class MainClass {
		public static void Main() {
			int i = 0;
			string received, sender, message, Message;
			Regex parserawirc = new Regex(@":(.*)!.*(?:privmsg).*?:(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			MyGlobals.banwords = MyGlobals.dbcontext.AutoBanList.Select(e => e.Word).ToList();				// Load these variables into memory because
			MyGlobals.tempbanwords = MyGlobals.dbcontext.AutoTempBanList.Select(e => e.Word).ToList();		// they're called very frequently
			MyGlobals.ModVariables = MyGlobals.dbcontext.ModVariables.Select(t => new { t.Variable, t.Value }) //http://stackoverflow.com/questions/953919/convert-linq-query-result-to-dictionary
															.ToDictionary(t => t.Variable, t => t.Value);
			
			connect();
			ParallelWhile(delegate() {
				try {
					i++;
					received = MyGlobals.input.ReadLine().Trim(new Char[] { '\r', '\n', ' ' });

					/* disconnected */
					if (received.Length == 0) connect();

					/* get user & privmsg */
					Match sendermessage = parserawirc.Match(received);
					sender = sendermessage.Groups[1].Value;
					Message = sendermessage.Groups[2].Value;
					message = Message.ToLower();
					if (sendermessage.Success) {

						/* log latest lines */
						var stalk = (from y in MyGlobals.dbcontext.Stalk
									 where y.User == sender
									 select y).SingleOrDefault();
						if (stalk == null) {
							stalk = new Stalk();
							MyGlobals.dbcontext.Stalk.InsertOnSubmit(stalk);
						}
						stalk.User = sender;
						stalk.Time = DateTime.Now;
						stalk.Message = Message;
						MyGlobals.dbcontext.SubmitChanges();

						/* print to console */
						if (new string[] { "dharm", "darm", "dhram" }.Any(received.Contains)) //http://stackoverflow.com/a/2912483/625919
							log(sender + ": " + message, ConsoleColor.Green);
						else
							log(sender + ": " + message, ConsoleColor.Gray);

						/* mod */
						if (Constants.mods.Contains(sender)) {								// TODO: make sure you append longspamlist if over x lines
							if (message[0] == '!') {
								ModClass.parse(message.Substring(1));
								BasicCommandsClass.parse(message.Substring(1));
							}
						}
						/* pleb */
						else {
							BanLogicClass.parse(message);
							if (((DateTime.Now - MyGlobals.pleblag).TotalSeconds >= 15) && (MyGlobals.isbanned == false) && (message[0] == '!')) {
								BasicCommandsClass.parse(message.Substring(1));
							}
						}
					}
					/* pongs */
					else if (received.StartsWith("PING ")) {
						WriteAndFlush(received.Replace("PING", "PONG") + "\n");
						log(received, ConsoleColor.DarkGray);
					}
					/* print server text */
					else
						log(received, ConsoleColor.DarkGray);
				}
				catch (Exception e) {
					log("An error occured!", ConsoleColor.Red);
					log(e.Message, ConsoleColor.Red);
					log(e.Source, ConsoleColor.Red);
					log(e.StackTrace, ConsoleColor.Red);
				}
			});
		}

		/****************************************************************************************************
		 *											COMMON METHODS											*
		 ****************************************************************************************************/

		/* Parallel while loop */
		public static void ParallelWhile(Action body) {
			Parallel.ForEach(foreverTrue(), ignored => body());
		}
		private static IEnumerable<bool> foreverTrue() {
			while (true) yield return true;
		}

		/* global vars */
		public static class MyGlobals {
			public static TcpClient sock = new TcpClient();
			public static TextReader input;
			public static TextWriter output;
			public static DateTime pleblag;
			public static Boolean isbanned = false;
			public static Regex untinyurl = new Regex(@"(http://t\.co/\w+)", RegexOptions.Compiled);
			public static List<string> banwords, tempbanwords;
			public static db dbcontext = new db(new SqlCeConnection("Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091"));
			public static Dictionary<string, string> ModVariables = new Dictionary<string, string>();
		}

		/* sends to channel */
		public static void sendmessage(string msg) {
			WriteAndFlush("PRIVMSG " + Constants.chan + " :" + msg + "\n");
		}

		/* Get website stuff */
		public static async Task<string> DLdata(string url) { //http://stackoverflow.com/questions/13240915/converting-a-webclient-method-to-async-await
			try {
				var client = new WebClient();
				string data = await client.DownloadStringTaskAsync(url);
				return data;
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
				return "Error! " + ex;
			}
		}

		/* connect to server */
		public static void connect() {
			MyGlobals.sock.Connect("irc.twitch.tv", 6667);
			if (!MyGlobals.sock.Connected) {
				log("Failed to connect!", ConsoleColor.Red);
				return;
			}
			MyGlobals.input = new System.IO.StreamReader(MyGlobals.sock.GetStream());
			MyGlobals.output = new System.IO.StreamWriter(MyGlobals.sock.GetStream());
			WriteAndFlush(
				"PASS " + Constants.oauth + "\n" +
				"USER " + Constants.nick + " 0 * :" + Constants.owner + "\n" +
				"NICK " + Constants.nick + "\n" +
				"JOIN " + Constants.chan + "\n"											// could wait for 001 from server, but this works anyway
			);
		}

		/* Why write two lines when you can write one */
		public static void WriteAndFlush(string str) {
			MyGlobals.output.Write(str);
			MyGlobals.output.Flush();
		}

		/* colors console and prepends timestamp */
		public static void log(string text, ConsoleColor color = ConsoleColor.White) {
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write(DateTime.Now.ToString("t"));
			Console.ForegroundColor = color;
			Console.WriteLine(" " + text);
			Console.ResetColor();
		}

		/* human readable time deltas */
		public static string deltatimeformat(TimeSpan span, string rough = "") {
			int day		= Convert.ToInt16(span.ToString("%d")),
				hour	= Convert.ToInt16(span.ToString("%h")),
				minute	= Convert.ToInt16(span.ToString("%m"));

			if (span.CompareTo(TimeSpan.Zero) == -1) {
				log("Time to sync the clock?" + span.ToString(), ConsoleColor.Red);
				return "A few seconds";
			}
			else if (day > 1) {
				if (hour == 0) return day + " days";
				else return day + " days " + hour + "h";
			}
			else if (day == 1) {
				if (hour == 0) return "a day";
				else return "a day " + hour + "h";
			}
			else { // when day == 0
				if (hour == 0)
					return rough + minute + "m";
				else {
					if (minute == 0) return rough + hour + "h";
					else return rough + hour + "h " + minute + "m";
				}
			}
		}

		/* url unshorteners */
		public static string untinyurl(string url) {
			string redirectedto = url;													// http://stackoverflow.com/questions/3175062/long-urls-from-short-ones-using-c-sharp
			int cycle = 3;
			while (cycle > 0) {															// gives it 3 tries to untiny the URL before giving up
				try {
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
					request.AllowAutoRedirect = false;
					request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3 (.NET CLR 4.0.20506)";
					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					if ((int)response.StatusCode == 301 || (int)response.StatusCode == 302) {
						redirectedto = response.Headers["Location"];
						//log("Redirecting " + url + " to " + redirectedto + " because " + response.StatusCode);
						cycle = 0;
					}
					else {
						log("Not redirecting " + url + " because " + response.StatusCode);
						cycle--;
					}
				}
				catch (Exception ex) {
					ex.Data.Add("url", url);
					log(ex.ToString());
					cycle--;
				}
			}
			return redirectedto;
		}
	}
}
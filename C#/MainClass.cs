using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;


namespace IRCbot{
	class MainClass{
		public static void Main(string[] args){
			int i = 0;
			string received, sender, message, Message;
			string[] mods = Constants.mods; //can't rely on +o because jtv servers can be very slow with applying modes
			Regex parserawirc = new Regex(@":(.*)!.*(?:privmsg).*?:(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled); // compiled for that lovely speed

			connect();
			while (true) { // TODO: Giant catch that logs all errors 'cept Keyboard Break and leaves bot running lovely
				i++;
				received = MyGlobals.input.ReadLine().Trim(new Char[] { '\r', '\n', ' ' });

				/* disconnected */
				if (received.Length == 0) connect();

				/* get user & privmsg */
				Match sendermessage = parserawirc.Match(received);
				sender	= sendermessage.Groups[1].Value;
				Message	= sendermessage.Groups[2].Value;
				message	= Message.ToLower();
				if (sendermessage.Success) {

					/* log their latest lines */
					object[] stalkinfo = { DateTime.Now, Message };
					MyGlobals.stalk[sender] = stalkinfo;
					SerializeObject(MyGlobals.stalk, "stalk.xml"); // TODO: move this to PONG to reduce writes, just here for now to test

					/* print to console */
					if (new string[] { "dharm", "darm", "dhram" }.Any(received.Contains)) //http://stackoverflow.com/a/2912483/625919
						log(sender + ": " + message, ConsoleColor.Green);
					else {
						log(sender + ": " + message, ConsoleColor.Gray);
					}

					/* mod */
					if (mods.Contains(sender)) { // TODO: make sure you append longspamlist if over x lines
						ModClass.parse(message);
						BasicCommandsClass.parse(message);
					}
					/* pleb */
					else {
						BanLogicClass.parse(message);
						if (((DateTime.Now - MyGlobals.pleblag).TotalSeconds > 15) && (MyGlobals.isbanned == false)) {
							BasicCommandsClass.parse(message);
						}
					}
				}
				/* pongs */
				else if (received.StartsWith("PING ")) {
					WriteAndFlush(received.Replace("PING", "PONG") + "\n");
					log(received, ConsoleColor.DarkGray);
				}
				/* print server text */
				else {
					log(received, ConsoleColor.DarkGray);
				}
			}
		}

		public static class MyGlobals{
			public static System.Net.Sockets.TcpClient sock = new System.Net.Sockets.TcpClient();
			public static System.IO.TextReader input;
			public static System.IO.TextWriter output;
			public static DateTime pleblag;
			public static Boolean isbanned = false;
			public static int									ninja		= DeSerializeObject<int>("ninja.xml"),
																modabuse	= DeSerializeObject<int>("modabuse.xml"),
																bancount	= DeSerializeObject<int>("bancount.xml");
			public static List<string>							banwords	= DeSerializeObject<List<string>>("banwords.xml"),
																tempbanwords= DeSerializeObject<List<string>>("tempbanwords.xml");
			public static SerializableDictionary<string, object[]> stalk	= DeSerializeObject<SerializableDictionary<string, object[]>>("stalk.xml");
		}

		public static void sendmessage(string msg){
			WriteAndFlush("PRIVMSG " + Constants.chan + " :" + msg + "\n");
		}

		/* connect to server */
		public static void connect() {
			int port = 6667;
			string	nick   = Constants.nick,
					owner  = Constants.owner,
					server = Constants.server,
					chan   = Constants.chan,
					oauth  = Constants.oauth;
			MyGlobals.sock.Connect(server, port);
			if (!MyGlobals.sock.Connected) {
				log("Failed to connect!", ConsoleColor.Red);
				return;
			}
			MyGlobals.input = new System.IO.StreamReader(MyGlobals.sock.GetStream());
			MyGlobals.output = new System.IO.StreamWriter(MyGlobals.sock.GetStream());
			WriteAndFlush(
				"PASS " + oauth + "\n" +
				"USER " + nick + " 0 * :" + owner + "\n" +
				"NICK " + nick + "\n" +
				"JOIN " + chan + "\n" // could wait for 001 from server, but this joins anyway
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

		/* store var into file */
		public static void SerializeObject<T>(T serializableObject, string fileName) { //http://stackoverflow.com/questions/6115721/how-to-save-restore-serializable-object-to-from-file
			try {
				XmlDocument xmlDocument = new XmlDocument();
				XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
				using (MemoryStream stream = new MemoryStream()) {
					serializer.Serialize(stream, serializableObject);
					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlDocument.Save(fileName);
					stream.Close();
				}
			}
			catch (Exception ex) {
				log(ex.ToString(), ConsoleColor.Red);
			}
		}

		/* load var from file */
		public static T DeSerializeObject<T>(string fileName) {
			T objectOut = default(T);
			try {
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(fileName);
				string xmlString = xmlDocument.OuterXml;
				using (StringReader read = new StringReader(xmlString)) {
					Type outType = typeof(T);
					XmlSerializer serializer = new XmlSerializer(outType);
					using (XmlReader reader = new XmlTextReader(read)) {
						objectOut = (T)serializer.Deserialize(reader);
						reader.Close();
					}
					read.Close();
				}
			}
			catch (Exception ex) {
				log(ex.ToString(), ConsoleColor.Red);
			}
			return objectOut;
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
				if (hour == 0) return rough + minute + "m";
				else {
					if (minute == 0) return rough + hour + "h";
					else return rough + hour + "h " + minute + "m";
				}
			}
		}
	}
}
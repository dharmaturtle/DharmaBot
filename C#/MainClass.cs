using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace IRCbot{
	class MainClass{
		public static void Main(string[] args){
			int i = 0;
			string received, sender, message;
			string[] mods = Constants.mods; //can't rely on +o because jtv servers can be very slow with applying modes
			Regex parserawirc = new Regex(@":(.*)!.*(?:privmsg).*?:(.*)", RegexOptions.Compiled); // compiled for that lovely speed

			connect();
			for (received = MyGlobals.input.ReadLine(); ; received = MyGlobals.input.ReadLine()) {
				i++;
				received = received.Trim(new Char[] {'\r','\n',' '});
				received = received.ToLower(); // TODO: Combine into one when you figure out what the bug is
				//Display received irc message
				//Console.WriteLine(received);  // raw irc
				//Console.WriteLine(i);

				if (received.Length == 0) { // Disconnected!
					connect();
				}

				//Get user & message if its a privmsg
				Match sendermessage = parserawirc.Match(received);
				sender = sendermessage.Groups[1].Value;
				message = sendermessage.Groups[2].Value;
				if (sendermessage.Success) {
					Console.WriteLine(sender);
					Console.WriteLine(message);

					if (mods.Contains(sender)) {
						ModClass.parse(message);
					}
					else {
						Console.WriteLine("Plebian");
					}

					/*if (i == 16) {
						sendmessage("tester");
					}*/
				}
				//Send pong to any pings
				else if (received.StartsWith("PING ")) {
					WriteAndFlush(received.Replace("PING", "PONG") + "\n");
				}
			}
		}
		public static class MyGlobals{
			public static System.Net.Sockets.TcpClient sock = new System.Net.Sockets.TcpClient();
			public static System.IO.TextReader input;
			public static System.IO.TextWriter output;
		}
		public static void sendmessage(string msg){
			WriteAndFlush("PRIVMSG " + Constants.chan + " :" + msg + "\n");
		}
		public static void connect() {
			int port = 6667;
			string	nick = Constants.nick,
					owner = Constants.owner,
					server = Constants.server,
					chan = Constants.chan,
					oauth = Constants.oauth;

			//Connect to server
			MyGlobals.sock.Connect(server, port);
			if (!MyGlobals.sock.Connected) {
				Console.WriteLine("Failed to connect!");
				return;
			}
			MyGlobals.input = new System.IO.StreamReader(MyGlobals.sock.GetStream());
			MyGlobals.output = new System.IO.StreamWriter(MyGlobals.sock.GetStream());

			//Login & join
			WriteAndFlush(
				"PASS " + oauth + "\n" +
				"USER " + nick + " 0 * :" + owner + "\n" +
				"NICK " + nick + "\n" +
				"JOIN " + chan + "\n" // could wait for 001 from server, but this joins anyway
			);
		}
		public static void WriteAndFlush(string str) {
			MyGlobals.output.Write(str);
			MyGlobals.output.Flush();
		}
	}
}
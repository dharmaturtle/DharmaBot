namespace IRCbot
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks.Dataflow;

	// http://blog.stephencleary.com/2012/11/async-producerconsumer-queue-using.html
	// http://stackoverflow.com/questions/14255655/tpl-dataflow-producerconsumer-pattern
	public class RawIRCAsync : MainClass
	{
		public void Consumer(ISourceBlock<string> source)
		{
			var ablock = new ActionBlock<string>(
				data => this.ParseRawIRC(data), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });
			
			source.LinkTo(ablock);
		}

		public void ParseRawIRC(string readLine)
		{
			string received = null;
			var parseRawIRC = new Regex(
				@":(.*)!.*(?:privmsg).*?:(.*)",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

			if (readLine != null) received = readLine.Trim(new[] { '\r', '\n', ' ' });

			// disconnected
			if (received != null && received.Length == 0) Connect();

			// get user & privmsg
			if (received == null) return;
			var sendermessage = parseRawIRC.Match(received);
			var sender = sendermessage.Groups[1].Value;
			var messageCaps = sendermessage.Groups[2].Value;
			var message = messageCaps.ToLowerInvariant();
			if (sendermessage.Success)
			{
				// logs chat (synchronously!)
				Constants.LogBuffer.Post(
					new Log
						{
							User = sender, 
							Message = messageCaps, 
							Timestamp = DateTime.Now
						});
					
				// print to console
				Log(sender + ": " + message, new[] { "dharm", "darm", "dhram" }.Any(received.Contains) ? ConsoleColor.Green : ConsoleColor.Gray);

				// mod
				if (PrivateConstants.Mods.Contains(sender))
				{
					// TODO: make sure you append longspamlist if over x lines
					if (message[0] != '!') return;
					ModClass.Parse(message.Substring(1));
					BasicCommandsClass.Parse(message.Substring(1));
				}

				// pleb
				else
				{
					MyGlobals.IsBanned = false;
					BanLogicClass.Parse(message);
					if (((DateTime.Now - MyGlobals.PlebianLag).TotalSeconds >= 15) && (MyGlobals.IsBanned == false) && (message[0] == '!'))
					{
						BasicCommandsClass.Parse(message.Substring(1));
					}
				}
			}

			// pongs
			else if (received.StartsWith("PING ", StringComparison.CurrentCulture))
			{
				WriteAndFlush(received.Replace("PING", "PONG") + "\n");
				Log(received, ConsoleColor.DarkGray);
			}

			// print server text
			else Log(received, ConsoleColor.DarkGray);
		}
	}
}
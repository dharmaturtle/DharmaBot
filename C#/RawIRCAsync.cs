namespace IRCbot
{
	using System;
	using System.Linq;
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

		public void ParseRawIRC(string received)
		{
			received = received.Trim(new[] { '\r', '\n', ' ' });

			// disconnected
			if (received.Length == 0)
			{
				Connect();
				return;
			}

			// get user & privmsg
			var sendermessage = Constants.ParseRawIRC.Match(received);
			if (sendermessage.Success)
			{
				var sender = sendermessage.Groups[1].Value;
				var messageCaps = sendermessage.Groups[2].Value;
				var message = messageCaps.ToLowerInvariant();
				
				// logs chat (synchronously!)
				Constants.LogBuffer.Post(
					new Tuple<string, string, DateTime>(sender, messageCaps, DateTime.Now));
					
				// people talking to/about me is green; otherwise gray
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
				else if ((BanLogicClass.Parse(message) == false) &&
						(message[0] == '!') &&
						((DateTime.Now - MyGlobals.PlebianLag).TotalSeconds >= 15))
							BasicCommandsClass.Parse(message.Substring(1));
			}
			else 
			{
				// pongs
				if (received.StartsWith("PING ")) WriteAndFlush(received.Replace("PING", "PONG") + "\n");
				
				// print all server text
				Log(received, ConsoleColor.DarkGray);
			}
		}
	}
}
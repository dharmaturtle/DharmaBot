namespace IRCbot
{
	using System.Data.SqlServerCe;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks.Dataflow;

	public class Constants
	{
		public const string Nick = "dharmaturtle",
							Owner = "dharmaturtle",
							Server = "irc.twitch.tv",
							Chan = "#dharmaturtle2";

		// Compiled, so initialize it once
		public static readonly Regex ParseRawIRC = new Regex(
				@":(.*)!.*(?:privmsg).*?:(.*)",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);
		
		// Sequential, so we don't need to connect on every call
		public static readonly db DBLogContext = new db(
			new SqlCeConnection(
				"Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091")); 

		public static readonly BufferBlock<ModCommands> ModBuffer =
			new BufferBlock<ModCommands>(new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });

		public static readonly BufferBlock<Log> LogBuffer =
			new BufferBlock<Log>(new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
	}
}
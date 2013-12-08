namespace IRCbot
{
	using System.Data.SqlServerCe;
	using System.Net.Sockets;
	using System.Text.RegularExpressions;

	public class Constants
	{
		public const string Nick = "dharmaturtle",
							Owner = "dharmaturtle",
							Server = "irc.twitch.tv",
							Chan = "#dharmaturtle2";

		public static readonly TcpClient Sock = new TcpClient();

		public static readonly db DBContext = new db(
			new SqlCeConnection(
				"Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091")); // http://dotnet.dzone.com/articles/sql-server-compact-4-desktop

		public static readonly Regex Parserawirc = new Regex(
			@":(.*)!.*(?:privmsg).*?:(.*)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}
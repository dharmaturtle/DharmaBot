namespace IRCbot
{
	using System.Data.SqlServerCe;
	using System.Net.Sockets;

	public class Constants
	{
		public const string Nick = "dharmaturtle",
							Owner = "dharmaturtle",
							Server = "irc.twitch.tv",
							Chan = "#dharmaturtle2";

		public static readonly TcpClient Sock = new TcpClient();

		public static readonly db DBcontext = new db(new SqlCeConnection("Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091")); // http://dotnet.dzone.com/articles/sql-server-compact-4-desktop
	}
}

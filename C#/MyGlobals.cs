namespace IRCbot
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class MyGlobals
	{
		public static TextReader Input { get; set; }

		public static TextWriter Output { get; set; }

		public static DateTime Pleblag { get; set; }

		public static bool IsBanned { get; set; }

		public static Dictionary<string, string> ModVariables { get; set; }

		public static Dictionary<string, List<string>> Banwords { get; set; }
	}
}

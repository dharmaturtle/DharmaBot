namespace IRCbot
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class MyGlobals
	{
		public static TextReader Input { get; set; }

		public static TextWriter Output { get; set; }

		public static DateTime PlebianLag { get; set; }

		public static Dictionary<string, string> ModVariables { get; set; }

		public static Dictionary<string, List<string>> BanWords { get; set; }
	}
}

namespace IRCbot
{
	using System;
	using System.Data.SqlServerCe;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks.Dataflow;

	public class ModClass : MainClass
	{
		public static void Parse(string message)
		{
			// command to test async, TODO: remove when done
			if (message.StartsWith("asynctest", StringComparison.CurrentCulture))
			{
				// http://stackoverflow.com/questions/100841/artificially-create-a-connection-timeout-error
				SendMessage("Timing out...");
				Console.WriteLine("Timing out...");
				var data = DownloadData("http://www.google.com:81/");
				SendMessage("Timeout text is " + data.Result);
				Console.WriteLine("Timeout text is " + data.Result);
				return;
			}
			
			var dbPrimaryModContext = new db(new SqlCeConnection("Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091"));
			var userParameter = Regex.Match(message, @"(\w*) +(\w.*)").Groups[2].Value.Trim(); // gets the optional word after the !command

			var primaryQuery = dbPrimaryModContext.ModCommands.Where(x => x.Command == message.Split(' ')[0]);

			// lazily grabs first result - although all results of primaryQuery are used in primary loop, first assigned here b/c used in narrowing the selection
			var primaryQueryResult = primaryQuery.FirstOrDefault();

			// ensures that query has entries
			if (primaryQueryResult == null) return;

			// narrows selection if we're modifying the ModVariables table (ninja and modabuse)
			if (primaryQueryResult.CommandParameter != "true" && primaryQueryResult.CommandParameter != null)
				primaryQuery = primaryQuery.Where(i => i.CommandParameter == userParameter);

			// loops through the entire table, producing events
			foreach (var x in primaryQuery)
			{
				x.UserParameter = userParameter;
				Constants.ModBuffer.Post(x);
			}
		}
	}
}
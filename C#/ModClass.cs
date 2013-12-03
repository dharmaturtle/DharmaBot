using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IRCbot
{
	public class ModClass : MainClass
	{
		public static void Parse(string message)
		{
			/* command to test async, TODO: remove when done */
			if (message.StartsWith("asynctest"))
			{
				//http://stackoverflow.com/questions/100841/artificially-create-a-connection-timeout-error
				Sendmessage("Timing out...");
				Console.WriteLine("Timing out...");
				var data = DLdata("http://www.google.com:81/");
				Sendmessage("Timeout text is " + data.Result);
				Console.WriteLine("Timeout text is " + data.Result);
				return;
			}
			
			var dbcontext = Constants.DBcontext;
			var userParameter = Regex.Match(message, @"(\w*) +(\w.*)").Groups[2].Value.Trim();// gets the optional word after the !command
			

			var primaryQuery =	from x in dbcontext.ModCommands
								where x.Command == message.Split(' ')[0]
								select x;

			//lazily grabs first result - although all results of primaryQuery are used in primary loop, first assigned here b/c used in narrowing the selection
			var primaryQueryResult = primaryQuery.FirstOrDefault();

			// ensures that query has entries
			if (primaryQueryResult == null) return;

			// narrows selection if we're modifying the ModVariables table (ninja and modabuse)
			if (primaryQueryResult.CommandParameter != "true" && primaryQueryResult.CommandParameter != null)
				primaryQuery = primaryQuery.Where(i => i.CommandParameter == userParameter);

			// primary loop, goes through entire table
			foreach (var x in primaryQuery)
			{
				if (x.Action == "message") Sendmessage(x.Result);

				if (x.CommandParameter == null) continue;
				
				if (userParameter == "") Sendmessage("A word or name must be provided.");

				else 
					switch (x.Action)
					{
						case "set":
						{
							var modVariable =	(from y in dbcontext.ModVariables
								where y.Variable == x.Result
								select y).First();
							modVariable.Value = x.ResultParameter;
							MyGlobals.ModVariables[x.Result] = x.ResultParameter;
							dbcontext.SubmitChanges();
						}
							break;
						case "stalk":
						{
							var stalk =	(from y in dbcontext.Stalk
								where y.User == userParameter
								select y).FirstOrDefault();

							//checks if user can be found in table
							if (stalk != null)
							{
								if (stalk.Time == null) continue;
								var deltatime = DeltaTimeFormat(DateTime.Now - (DateTime) stalk.Time);
								Sendmessage(stalk.User + " seen " + deltatime + " ago saying " + stalk.Message);
							}
							else Sendmessage("No records of " + userParameter);
						}
							break;
						case "database":
						{
							//initial setup, and checks to see if userParameter is already in the table
							var tableName = x.Result;
							var tableType = Assembly.GetExecutingAssembly().GetType("IRCbot." + tableName);
							var itable = dbcontext.GetTable(tableType);
							object found = false;
							foreach (var y in itable) //My answer! http://stackoverflow.com/questions/1820102/how-can-i-create-a-linq-to-sql-statement-when-i-have-table-name-as-string/20307529#20307529
							{
								var value = (string) y.GetType().GetProperty("Word").GetValue(y, null);
								Console.Write(value + ",");
								if (value == userParameter) found = y;
							}

							switch (x.ActionParameter)
							{
								case "add":
									if (found.Equals(false))
									{
										Console.WriteLine(userParameter + " added");
										dynamic tableClass = Activator.CreateInstance(tableType);
										tableClass.Word = userParameter;
										itable.InsertOnSubmit(tableClass);
										dbcontext.SubmitChanges();
										MyGlobals.Banwords[tableName].Add(userParameter);
										Sendmessage(userParameter + " added to the " + tableName);
									}
									else
									{
										Console.WriteLine(userParameter + " already in the " + tableName);
										Sendmessage(userParameter + " already in the " + tableName);
									}
									break;
								case "remove":
									if (!found.Equals(false))
									{
										Console.WriteLine(userParameter + " removed");
										itable.DeleteOnSubmit(found);
										dbcontext.SubmitChanges();
										MyGlobals.Banwords[tableName].Remove(userParameter);
										Sendmessage(userParameter + " removed from the " + tableName);
									}
									else
									{
										Console.WriteLine(userParameter + " not found in the " + tableName);
										Sendmessage(userParameter + " not found in the " + tableName);
									}
									break;
							}
						}
							break;
					}
			}
		}
	}
}
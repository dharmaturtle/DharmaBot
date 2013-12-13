namespace IRCbot
{
	using System;
	using System.Data.SqlServerCe;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks.Dataflow;

	public class ModAsync : MainClass
	{
		public void Consumer(ISourceBlock<ModCommands> source)
		{
			var ablock = new ActionBlock<ModCommands>(
				data => this.ParseMod(data),
				new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });

			source.LinkTo(ablock);
		}

		public void ParseMod(ModCommands x)
		{
			var userParameter = x.UserParameter;
			var dbModContext = new db(new SqlCeConnection("Data Source=|DataDirectory|IRCbotDB.sdf;Max Database Size=4091"));

			if (x.Action == "message") SendMessage(x.Result);
			else if (x.CommandParameter == null) return;
			else if (userParameter.Length == 0) SendMessage("A word or name must be provided.");
			else
			{
				switch (x.Action)
				{
					case "set":
						var modVariable = dbModContext.ModVariables.First(y => y.Variable == x.Result);
						modVariable.Value = x.ResultParameter;
						MyGlobals.ModVariables[x.Result] = x.ResultParameter;
						dbModContext.SubmitChanges();
						break;

					case "stalk":
						var stalk = dbModContext.Stalk.FirstOrDefault(y => y.User == userParameter);

						// checks if user can be found in table
						if (stalk != null)
						{
							var deltatime = DeltaTimeFormat(DateTime.Now - (DateTime)stalk.Time);
							SendMessage(stalk.User + " seen " + deltatime + " ago saying " + stalk.Message);
						}
						else SendMessage("No records of " + userParameter);
						break;

					case "database":
						// initial setup, and checks to see if userParameter is already in the table
						var tableName = x.Result;
						var tableType = Assembly.GetExecutingAssembly().GetType("IRCbot." + tableName);
						var itable = dbModContext.GetTable(tableType);
						object found = false;
						foreach (var y in itable) // My answer! http://stackoverflow.com/questions/1820102/how-can-i-create-a-linq-to-sql-statement-when-i-have-table-name-as-string/20307529#20307529
						{
							var value = (string)y.GetType().GetProperty("Word").GetValue(y, null);
							Console.Write(value + ",");
							if (value == userParameter) found = y;
						}

						if (x.ActionParameter == "add")
						{
							if (found.Equals(false))
							{
								Console.WriteLine(userParameter + " added");
								dynamic tableClass = Activator.CreateInstance(tableType);
								tableClass.Word = userParameter;
								itable.InsertOnSubmit(tableClass);
								dbModContext.SubmitChanges();
								MyGlobals.BanWords[tableName].Add(userParameter);
								SendMessage(userParameter + " added to the " + tableName);
							}
							else SendMessage(userParameter + " already in the " + tableName);
						}
						else
						{
							Debug.Assert(x.ActionParameter == "remove");
							if (!found.Equals(false))
							{
								Console.WriteLine(userParameter + " removed");
								itable.DeleteOnSubmit(found);
								dbModContext.SubmitChanges();
								MyGlobals.BanWords[tableName].Remove(userParameter);
								SendMessage(userParameter + " removed from the " + tableName);
							}
							else SendMessage(userParameter + " not found in the " + tableName);
						}

						break;
				}
			}
		}
	}
}
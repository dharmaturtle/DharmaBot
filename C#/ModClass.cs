using System;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Data.SqlServerCe;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IRCbot {
	public class ModClass:MainClass{
		public static void parse(string message) {
			db dbcontext = MyGlobals.dbcontext;
			var initialQuery = from x in dbcontext.ModCommands			//lazy!
								where x.Command == message.Split(' ')[0]
								select x;
			foreach (var x in initialQuery) {
				if (x.CommandParameter == "required") {
					string userParameter = Regex.Match(message, @"(\w*) +(\w.*)").Groups[2].Value.Trim();
					if (userParameter == "") {
						sendmessage("No word entered.");
					}
					else if (x.Command == "stalk") {
						try {
							var stalk = (from y in dbcontext.Stalk
										 where y.User == userParameter
										 select y).First();
							string deltatime = deltatimeformat(DateTime.Now - (DateTime)stalk.Time);
							sendmessage(stalk.User + " seen " + deltatime + " ago saying " + stalk.Message);
						}
						catch (System.InvalidOperationException) {
							sendmessage("No records of " + userParameter);
						}
					}
					else {
						var AutoBanListQuery = (from y in dbcontext.AutoBanList
												where y.Word == userParameter
												select y);
						var AutoTempBanListQuery = (from y in dbcontext.AutoTempBanList
													where y.Word == userParameter
													select y);
						switch (x.Command) {
							case "add":
								try {
									AutoBanListQuery.First();
									sendmessage(userParameter + " is already on the auto ban list.");
								}
								catch (System.InvalidOperationException) {
									dbcontext.AutoBanList.InsertOnSubmit(new AutoBanList { Word = userParameter });
									dbcontext.SubmitChanges();
									MyGlobals.banwords.Remove(userParameter);
									sendmessage(userParameter + " added to the auto ban list.");
								}
								break;
							case "tempadd":
								try {
									AutoTempBanListQuery.First();
									sendmessage(userParameter + " is already on the auto ban list.");
								}
								catch (System.InvalidOperationException) {
									dbcontext.AutoTempBanList.InsertOnSubmit(new AutoTempBanList { Word = userParameter });
									dbcontext.SubmitChanges();
									MyGlobals.tempbanwords.Add(userParameter);
									sendmessage(userParameter + " added to the temp ban list.");
								}
								break;
							case "del":
								try {
									dbcontext.AutoBanList.DeleteOnSubmit(AutoBanListQuery.First());
									dbcontext.SubmitChanges();
									MyGlobals.banwords.Remove(userParameter);
									sendmessage(userParameter + " removed from auto ban list");
								}
								catch (System.InvalidOperationException) {
									sendmessage(userParameter + " not in the auto ban list.");
								}
								break;
							case "tempdel":
								try {
									dbcontext.AutoTempBanList.DeleteOnSubmit(AutoTempBanListQuery.First());
									dbcontext.SubmitChanges();
									MyGlobals.tempbanwords.Remove(userParameter);
									sendmessage(userParameter + " removed from auto temp ban list");
								}
								catch (System.InvalidOperationException) {
									sendmessage(userParameter + " not in the auto temp ban list.");
								}
								break;
						}
					}
				}
				if (x.Action == "message") sendmessage(x.Result);
				if (x.Action == "set") {
					var ModVariable =	(from y in dbcontext.ModVariables
										where y.Variable == x.Result
										select y).First();
					ModVariable.Value = x.ResultParameter;
					MyGlobals.ModVariables[x.Result] = x.ResultParameter;
					dbcontext.SubmitChanges();
				}
				
				/* command to test async, TODO: remove when done */
				if (message.StartsWith("timeout")) { //http://stackoverflow.com/questions/100841/artificially-create-a-connection-timeout-error
					MyGlobals.pleblag = DateTime.Now;
					sendmessage("Timing out...");
					Console.WriteLine("Timing out...");
					Task<string> DLedData = DLdata("http://www.google.com:81/");
					sendmessage("Timeout text is " + DLedData.Result);
					Console.WriteLine("Timeout text is " + DLedData.Result);
				}
			}
		}
	}
}
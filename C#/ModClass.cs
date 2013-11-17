using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRCbot {
	public static class ModClass{
		public static void parse(string message) {

			/* Simple stuff */
			if (message.StartsWith("!sing"))	MainClass.sendmessage("/me sings the body electric ♫");
			if (message.StartsWith("!dance"))	MainClass.sendmessage("/me roboboogies ⌐[º-°⌐] [¬º-°]¬");
			if (message.StartsWith("!status"))	MainClass.sendmessage("v8.0.0 N:" + MainClass.MyGlobals.ninja + " MA:" + MainClass.MyGlobals.modabuse);

			/* get user's last words */
			if (message.StartsWith("!stalk ")) {
				string user = message.Substring(message.IndexOf(" ") + 1).ToLower().Trim();
				object[] stalkinfo;
				if (MainClass.MyGlobals.stalk.TryGetValue(user, out stalkinfo)) {
					string deltatime = MainClass.deltatimeformat(DateTime.Now - (DateTime)stalkinfo[0]);
					MainClass.sendmessage(user + " seen " + deltatime + " ago saying: " + stalkinfo[1]);
				}
				else MainClass.sendmessage("There are no records of " + user);
			}

			/* In times of spam, the bot doesn't speak. It just bans. */
			if (message.StartsWith("!ninja ")) {
				if (message.Contains("on")){
					MainClass.MyGlobals.ninja = 1;
					MainClass.sendmessage("I am the blade of Shakuras.");
				}
				else if (message.Contains("off")) {
					MainClass.MyGlobals.ninja = 0;
					MainClass.sendmessage("The void claims its own.");
				}
				MainClass.SerializeObject(MainClass.MyGlobals.ninja, "ninja.xml");
			}

			/* Banlogic is relaxed during certain periods */
			if (message.StartsWith("!modabuse ")) {
				if (message.Contains("on")) {
					MainClass.MyGlobals.modabuse = 2;
					MainClass.sendmessage("Justice has come!");
				}
				else if (message.Contains("semi")) {
					MainClass.MyGlobals.modabuse = 1;
					MainClass.sendmessage("Calibrating void lenses.");
				}
				else if (message.Contains("off")) {
					MainClass.MyGlobals.modabuse = 0;
					MainClass.sendmessage("Awaiting the call.");
				}
				MainClass.SerializeObject(MainClass.MyGlobals.modabuse, "modabuse.xml");
			}
			
			/* Add to autoban list */
			if (message.StartsWith("!add ")) {
				string banword = message.Substring(message.IndexOf(" ") + 1).ToLower().Trim();
				MainClass.MyGlobals.banwords.Add(banword);
				MainClass.SerializeObject(MainClass.MyGlobals.banwords, "banwords.xml");
				MainClass.sendmessage(banword + " added to the autoban list.");
			}

			/* Remove from autoban list */
			if (message.StartsWith("!del ") || message.StartsWith("!delete ") || message.StartsWith("!remove ")) {
				string banword = message.Substring(message.IndexOf(" ") + 1).ToLower().Trim();
				MainClass.MyGlobals.banwords.Remove(banword);
				if (MainClass.MyGlobals.banwords.Contains(banword)){
					MainClass.SerializeObject(MainClass.MyGlobals.banwords, "banwords.xml");
					MainClass.sendmessage(banword + " removed from autoban list.");
				}
				else{
					MainClass.sendmessage(banword + " not in banlist");
				}
			}

			/* Add to tempban list */
			if (message.StartsWith("!tempadd ")) {
				string banword = message.Substring(message.IndexOf(" ") + 1).ToLower().Trim();
				MainClass.MyGlobals.tempbanwords.Add(banword);
				MainClass.SerializeObject(MainClass.MyGlobals.tempbanwords, "tempbanwords.xml");
				MainClass.sendmessage(banword + " added to the autoban list.");
			}

			/* Remove from autoban list */
			if (message.StartsWith("!tempdel ") || message.StartsWith("!dempdelete ") || message.StartsWith("!tempremove ")) {
				string banword = message.Substring(message.IndexOf(" ") + 1).ToLower().Trim();
				MainClass.MyGlobals.tempbanwords.Remove(banword);
				if (MainClass.MyGlobals.tempbanwords.Contains(banword)) {
					MainClass.SerializeObject(MainClass.MyGlobals.tempbanwords, "tempbanwords.xml");
					MainClass.sendmessage(banword + " removed from tempban list.");
				}
				else {
					MainClass.sendmessage(banword + " not in tempbanlist");
				}
			}
		}
	}
}
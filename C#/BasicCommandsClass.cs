using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace IRCbot {
	class BasicCommandsClass {
		public static void parse(string message) {
			var client = new WebClient();
			JavaScriptSerializer serializer = new JavaScriptSerializer();

			/* live */
			if (message.StartsWith("!live")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				dynamic livejson = serializer.Deserialize<object>(client.DownloadString("https://api.twitch.tv/kraken/streams/destiny"));
				if (livejson["stream"] != null) {
					MainClass.sendmessage("Stream is live with " + livejson["stream"]["viewers"] + " viewers");
				}
				else {
					MainClass.sendmessage("Offline.");
				}
			}

			/* song */
			else if (message.StartsWith("!song")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				dynamic songjson = serializer.Deserialize<object>(client.DownloadString("http://ws.audioscrobbler.com/2.0/?method=user.getRecentTracks&user=StevenBonnellII&format=json&api_key=" + Constants.lastfmkey));
				string artist = songjson["recenttracks"]["track"][0]["artist"]["#text"],
						track = songjson["recenttracks"]["track"][0]["name"];
				if (songjson["recenttracks"]["track"][0].ContainsKey("date")) {
					DateTime timestamp = DateTime.ParseExact(songjson["recenttracks"]["track"][0]["date"]["#text"], "dd MMM yyyy, HH:mm", new CultureInfo("en-US"), DateTimeStyles.None);//http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
					MainClass.sendmessage("No song played/scrobbled. Played " + MainClass.deltatimeformat(DateTime.UtcNow - timestamp) + " ago: " + track + " - " + artist);
				}
				else {
					MainClass.sendmessage(track + " - " + artist);
				}
			}

			/* starcraft */
			else if (message.StartsWith("!sc") || message.StartsWith("!starcraft")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				dynamic json = serializer.Deserialize<object>(client.DownloadString("http://us.battle.net/api/sc2/profile/310150/1/Destiny/matches"));
				string decision = json["matches"][0]["decision"];
				var gametime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(json["matches"][0]["date"]); // API gives back epoch time
				string mainstring = json["matches"][0]["type"].ToLower() + " game on " + json["matches"][0]["map"] + " " + MainClass.deltatimeformat(DateTime.UtcNow - gametime) + " ago";
				if (decision == "WIN")
					MainClass.sendmessage("Destiny won a " + mainstring);
				else if (json["matches"][0]["decision"] == "LOSS")
					MainClass.sendmessage("Destiny lost a " + mainstring);
				else
					MainClass.sendmessage("Destiny played a " + mainstring);
			}

			/* twitter */
			else if (message.StartsWith("!twitter") || message.StartsWith("!tweet")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				var twit = new OAuthTwitterWrapper.OAuthTwitterWrapper();
				JArray twitterjson = JArray.Parse(@twit.GetMyTimeline());
				string tweet = (string)twitterjson.SelectToken("[0].text");
				DateTime timestamp = DateTime.ParseExact((string)twitterjson.SelectToken("[0].created_at"), "ddd MMM dd HH:mm:ss +0000 yyyy", new CultureInfo("en-US"), DateTimeStyles.None);//http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
				MatchCollection matches = MainClass.MyGlobals.untinyurl.Matches(tweet);
				foreach (Match match in matches) { //http://msdn.microsoft.com/en-us/library/system.text.regularexpressions.regexoptions(v=vs.110).aspx
					GroupCollection groups = match.Groups;
					tweet = tweet.Replace(groups[1].Value, MainClass.untinyurl(groups[1].Value)); // will try to untiny each http://t.co 3x, individually
				}
				MainClass.sendmessage(MainClass.deltatimeformat(DateTime.UtcNow - timestamp) + " ago: " + tweet);
			}

			/* bancount */
			else if (message.StartsWith("!bancount")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				MainClass.sendmessage(MainClass.MyGlobals.bancount + " victims reaped.");
			}

			/* time */
			else if (message.StartsWith("!time")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				MainClass.sendmessage(DateTime.Now.ToShortTimeString() + " Central Steven Time");
			}

			/* rules */
			else if (message.StartsWith("!rules")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				MainClass.sendmessage("Rules: reddit.com/1aufkc");
			}

			/* playlist */
			else if (message.StartsWith("!playlist")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				MainClass.sendmessage("Playlist at last.fm/user/StevenBonnellII");
			}

			/* referral links */
			else if (message.StartsWith("!refer")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				MainClass.sendmessage("amazon.com/?tag=des000-20 ☜(ﾟヮﾟ☜) Amazon referral, buy anything within 24hrs ☜(⌒▽⌒)☞ 25$ off cellphone, runs on Sprint (☞ﾟヮﾟ)☞ z78nhc1gsa3.ting.com/");
			}

			/* sponsors */
			else if (message.StartsWith("!sponsor")) {
				MainClass.MyGlobals.pleblag = DateTime.Now;
				MainClass.sendmessage("feenixcollection.com ༼ ◔◡◔༽ dollar-shave-club.7eer.net/c/72409/74122/1969");
			}
		}
	}
}
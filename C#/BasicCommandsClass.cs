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
	class BasicCommandsClass:MainClass {
		public static void parse(string message) {

			/* live */
			if (message.StartsWith("live")) {
				MyGlobals.pleblag = DateTime.Now;
				Task<string> DLedData = DLdata("https://api.twitch.tv/kraken/streams/destiny");
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				dynamic livejson = serializer.Deserialize<object>(DLedData.Result);
				if (livejson["stream"] != null)
					sendmessage("Stream is live with " + livejson["stream"]["viewers"] + " viewers");
				else 
					sendmessage("Offline.");
			}

			/* song */
			else if (message.StartsWith("song")) {
				MyGlobals.pleblag = DateTime.Now;
				Task<string> DLedData = DLdata("http://ws.audioscrobbler.com/2.0/?method=user.getRecentTracks&user=StevenBonnellII&format=json&api_key=" + Constants.lastfmkey);
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				dynamic songjson = serializer.Deserialize<object>(DLedData.Result);
				string artist = songjson["recenttracks"]["track"][0]["artist"]["#text"],
						track = songjson["recenttracks"]["track"][0]["name"];
				if (songjson["recenttracks"]["track"][0].ContainsKey("date")) {
					DateTime timestamp = DateTime.ParseExact(songjson["recenttracks"]["track"][0]["date"]["#text"], "dd MMM yyyy, HH:mm", new CultureInfo("en-US"), DateTimeStyles.None);//http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
					sendmessage("No song played/scrobbled. Played " + deltatimeformat(DateTime.UtcNow - timestamp) + " ago: " + track + " - " + artist);
				}
				else {
					sendmessage(track + " - " + artist);
				}
			}

			/* starcraft */
			else if (message.StartsWith("sc") || message.StartsWith("starcraft")) {
				MyGlobals.pleblag = DateTime.Now;
				Task<string> DLedData = DLdata("http://us.battle.net/api/sc2/profile/310150/1/Destiny/matches");
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				dynamic json = serializer.Deserialize<object>(DLedData.Result);
				string decision = json["matches"][0]["decision"];
				var gametime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(json["matches"][0]["date"]); // API gives back epoch time
				string mainstring = json["matches"][0]["type"].ToLower() + " game on " + json["matches"][0]["map"] + " " + deltatimeformat(DateTime.UtcNow - gametime) + " ago";
				if (decision == "WIN")
					sendmessage("Destiny won a " + mainstring);
				else if (json["matches"][0]["decision"] == "LOSS")
					sendmessage("Destiny lost a " + mainstring);
				else
					sendmessage("Destiny played a " + mainstring);
			}

			/* twitter */
			else if (message.StartsWith("twitter") || message.StartsWith("tweet")) {
				MyGlobals.pleblag = DateTime.Now;
				var twit = new OAuthTwitterWrapper.OAuthTwitterWrapper();
				JArray twitterjson = JArray.Parse(@twit.GetMyTimeline());
				string tweet = (string)twitterjson.SelectToken("[0].text");
				DateTime timestamp = DateTime.ParseExact((string)twitterjson.SelectToken("[0].created_at"), "ddd MMM dd HH:mm:ss +0000 yyyy", new CultureInfo("en-US"), DateTimeStyles.None);//http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
				MatchCollection matches = MyGlobals.untinyurl.Matches(tweet);
				foreach (Match match in matches) { //http://msdn.microsoft.com/en-us/library/system.text.regularexpressions.regexoptions(v=vs.110).aspx
					GroupCollection groups = match.Groups;
					tweet = tweet.Replace(groups[1].Value, untinyurl(groups[1].Value)); // will try to untiny each http://t.co 3x, individually
				}
				sendmessage(deltatimeformat(DateTime.UtcNow - timestamp) + " ago: " + tweet);
			}

			/* bancount */
			else if (message.StartsWith("bancount")) {
				MyGlobals.pleblag = DateTime.Now;
				sendmessage(MyGlobals.ModVariables["bancount"] + " victims reaped.");
			}

			/* time */
			else if (message.StartsWith("time")) {
				MyGlobals.pleblag = DateTime.Now;
				sendmessage(DateTime.Now.ToShortTimeString() + " Central Steven Time");
			}

			/* rules */
			else if (message.StartsWith("rules")) {
				MyGlobals.pleblag = DateTime.Now;
				sendmessage("Rules: reddit.com/1aufkc");
			}

			/* playlist */
			else if (message.StartsWith("playlist")) {
				MyGlobals.pleblag = DateTime.Now;
				sendmessage("Playlist at last.fm/user/StevenBonnellII");
			}

			/* referral links */
			else if (message.StartsWith("refer")) {
				MyGlobals.pleblag = DateTime.Now;
				sendmessage("amazon.com/?tag=des000-20 ☜(ﾟヮﾟ☜) Amazon referral, buy anything within 24hrs ☜(⌒▽⌒)☞ 25$ off cellphone, runs on Sprint (☞ﾟヮﾟ)☞ z78nhc1gsa3.ting.com/");
			}

			/* sponsors */
			else if (message.StartsWith("sponsor")) {
				MyGlobals.pleblag = DateTime.Now;
				sendmessage("feenixcollection.com ༼ ◔◡◔༽ dollar-shave-club.7eer.net/c/72409/74122/1969");
			}
		}
	}
}
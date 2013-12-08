namespace IRCbot
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using System.Web.Script.Serialization;

	using Newtonsoft.Json.Linq;

	internal class BasicCommandsClass : MainClass
	{
		public static void Parse(string message)
		{
			/* live */
			if (message.StartsWith("live", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				var data = DownloadData("https://api.twitch.tv/kraken/streams/destiny");
				var serializer = new JavaScriptSerializer();
				dynamic livejson = serializer.Deserialize<object>(data.Result);
				if (livejson["stream"] != null) SendMessage("Stream is live with " + livejson["stream"]["viewers"] + " viewers");
				else SendMessage("Offline.");
			}

			/* song */
			else if (message.StartsWith("song", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				var data = DownloadData("http://ws.audioscrobbler.com/2.0/?method=user.getRecentTracks&user=StevenBonnellII&format=json&api_key=" + PrivateConstants.LastFmKey);
				var serializer = new JavaScriptSerializer();
				dynamic songjson = serializer.Deserialize<object>(data.Result);
				string artist = songjson["recenttracks"]["track"][0]["artist"]["#text"],
						track = songjson["recenttracks"]["track"][0]["name"];
				if (songjson["recenttracks"]["track"][0].ContainsKey("date"))
				{
					DateTime timestamp = DateTime.ParseExact(
						songjson["recenttracks"]["track"][0]["date"]["#text"],
						"d MMM yyyy, HH:mm",
						new CultureInfo("en-US"),
						DateTimeStyles.None); // http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
					SendMessage("No song played/scrobbled. Played " + DeltaTimeFormat(DateTime.UtcNow - timestamp) + " ago: " + track + " - " + artist);
				}
				else
				{
					SendMessage(track + " - " + artist);
				}
			}

			/* starcraft */
			else if (message.StartsWith("sc", StringComparison.CurrentCulture) || message.StartsWith("starcraft", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				var data = DownloadData("http://us.battle.net/api/sc2/profile/310150/1/Destiny/matches");
				var serializer = new JavaScriptSerializer();
				dynamic json = serializer.Deserialize<object>(data.Result);
				string decision = json["matches"][0]["decision"];
				var gametime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(json["matches"][0]["date"]); // API gives back epoch time
				string mainstring = json["matches"][0]["type"].ToLower() + " game on " + json["matches"][0]["map"] + " " + DeltaTimeFormat(DateTime.UtcNow - gametime) + " ago";
				if (decision == "WIN")
					SendMessage("Destiny won a " + mainstring);
				else if (json["matches"][0]["decision"] == "LOSS")
					SendMessage("Destiny lost a " + mainstring);
				else
					SendMessage("Destiny played a " + mainstring);
			}

			/* twitter */
			else if (message.StartsWith("twitter", StringComparison.CurrentCulture) || message.StartsWith("tweet", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				var twit = new OAuthTwitterWrapper.OAuthTwitterWrapper();
				var twitterjson = JArray.Parse(@twit.GetMyTimeline());
				var tweet = (string)twitterjson.SelectToken("[0].text");
				var timestamp = DateTime.ParseExact((string)twitterjson.SelectToken("[0].created_at"), "ddd MMM dd HH:mm:ss +0000 yyyy", new CultureInfo("en-US"), DateTimeStyles.None); // http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
				var untinyurl = new Regex(@"(http://t\.co/\w+)", RegexOptions.Compiled);
				var matches = untinyurl.Matches(tweet);
				foreach (Match match in matches)
				{
					// http://msdn.microsoft.com/en-us/library/system.text.regularexpressions.regexoptions(v=vs.110).aspx
					var groups = match.Groups;
					tweet = tweet.Replace(groups[1].Value, UnTinyUrl(groups[1].Value));
					/* will try to untiny each http://t.co 3x, individually */
				}

				SendMessage(DeltaTimeFormat(DateTime.UtcNow - timestamp) + " ago: " + tweet);
			}

			/* bancount */
			else if (message.StartsWith("bancount", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				SendMessage(MyGlobals.ModVariables["bancount"] + " victims reaped.");
			}

			/* time */
			else if (message.StartsWith("time", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				SendMessage(DateTime.Now.ToShortTimeString() + " Central Steven Time");
			}

			/* rules */
			else if (message.StartsWith("rules", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				SendMessage("Rules: reddit.com/1aufkc");
			}

			/* playlist */
			else if (message.StartsWith("playlist", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				SendMessage("Playlist at last.fm/user/StevenBonnellII");
			}

			/* referral links */
			else if (message.StartsWith("refer", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				SendMessage("amazon.com/?tag=des000-20 ☜(ﾟヮﾟ☜) Amazon referral, buy anything within 24hrs ☜(⌒▽⌒)☞ 25$ off cellphone, runs on Sprint (☞ﾟヮﾟ)☞ z78nhc1gsa3.ting.com/");
			}

			/* sponsors */
			else if (message.StartsWith("sponsor", StringComparison.CurrentCulture))
			{
				MyGlobals.PlebianLag = DateTime.Now;
				SendMessage("feenixcollection.com ༼ ◔◡◔༽ dollar-shave-club.7eer.net/c/72409/74122/1969");
			}
		}
	}
}
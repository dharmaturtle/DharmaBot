namespace IRCBotTests
{
	using IRCbot;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class IRCBotTests
	{
		[TestMethod]
		public void TdotCOtoReddit_Success()
		{
			
			string url = "http://t.co/64i50DNlds";
			
			var result = MainClass.UnTinyUrl("http://t.co/64i50DNlds");

			Assert.AreEqual(result, "http://www.reddit.com/r/Destiny/comments/1sr99s/tier_euphoria_2_review_of_the_great_dust_storm_of/");
		}
	}
}
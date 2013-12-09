/* This recieves RawIRCAsync async events and makes them syncronous for logging purposes.
 * SQL Server CE is not safe for multithreading, so we reorder log writes into a queue,
 * as seen by 'MaxDegreeOfParallelism = 1'. We also use the global constant DBLogContext so
 * that we don't reconnect to the DB every time text is recieved.
 * 
 * http://social.msdn.microsoft.com/Forums/en-US/df341f51-b3f6-4cd4-a7ed-d2149c0c4ce2/multithreaded-programming-with-sql-server-compact
 * http://stackoverflow.com/questions/4607454/does-sql-server-ce-support-multithreading
 */

namespace IRCbot
{
	using System.Linq;
	using System.Threading.Tasks.Dataflow;

	public class LogSync : MainClass
	{
		public void Consumer(ISourceBlock<Log> source)
		{
			var ablock = new ActionBlock<Log>(
				data => this.LogChat(data), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 }); // sync!

			source.LinkTo(ablock);
		}

		public void LogChat(Log logObject)
		{
			var sender = logObject.User;

			// Search for user
			var stalk = (from y in Constants.DBLogContext.Stalk
						 where y.User == sender
						 select y).SingleOrDefault();

			// If user doesn't exist, make new
			if (stalk == null)
			{
				stalk = new Stalk();
				Constants.DBLogContext.Stalk.InsertOnSubmit(stalk);
			}

			// Update & save
			stalk.User = sender;
			stalk.Time = logObject.Timestamp;
			stalk.Message = logObject.Message;
			Constants.DBLogContext.SubmitChanges();
		}
	}
}
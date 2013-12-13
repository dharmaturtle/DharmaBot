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
	using System;
	using System.Linq;
	using System.Threading.Tasks.Dataflow;

	public class LogSync
	{
		public void Consumer(ISourceBlock<Tuple<string, string, DateTime>> source)
		{
			var ablock = new ActionBlock<Tuple<string, string, DateTime>>(
				data => this.LogChat(data), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 }); // sync!

			source.LinkTo(ablock);
		}

		public void LogChat(Tuple<string, string, DateTime> data)
		{
			var sender = data.Item1;

			// Search for user
			var stalk = Constants.DBLogContext.Stalk.SingleOrDefault(x => x.User == sender);

			// If user doesn't exist, make new
			if (stalk == null)
				Constants.DBLogContext.Stalk.InsertOnSubmit(new Stalk());
			
			// Update & save
			stalk.User = sender;
			stalk.Time = data.Item3;
			stalk.Message = data.Item2;
			Constants.DBLogContext.SubmitChanges();
		}
	}
}
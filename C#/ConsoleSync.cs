namespace IRCbot
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks.Dataflow;

	public class ConsoleSync : MainClass
	{
		public void Consumer(ISourceBlock<Tuple<string, ConsoleColor>> source)
		{
			var ablock = new ActionBlock<Tuple<string, ConsoleColor>>(
				data => this.ConsoleLog(data), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 }); // sync!

			source.LinkTo(ablock);
		}

		public void ConsoleLog(Tuple<string, ConsoleColor> data)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write(Process.GetCurrentProcess().Threads.Count + " ");
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write(DateTime.Now.ToString("t"));
			Console.ForegroundColor = data.Item2;
			Console.WriteLine(" " + data.Item1);
			Console.ResetColor();
		}
	}
}
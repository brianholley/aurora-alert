using System;
using System.Text;

namespace AuroraWatcher
{
	public delegate void LogCallback(string s, params object[] args);

	public static class Log
	{
		public static LogCallback Callback { get; set; }

		public static void Write(string format, params object[] objs)
		{
			if (Callback != null)
				Callback(format, objs);
			else
				Console.WriteLine(format, objs);			
		}
	}
}

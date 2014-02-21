using System;
using System.Text;

namespace AuroraWatcher
{
	static class Log
	{
		public static void Write(string format, params object[] objs)
		{
			System.Console.WriteLine(format, objs);			
		}
	}
}

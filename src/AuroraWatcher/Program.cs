using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;

namespace AuroraWatcher
{
	class Program
	{
		static void Main(string[] args)
		{
			Scheduler.Run();
		}
	}
}

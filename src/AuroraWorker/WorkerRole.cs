using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace WorkerRole1
{
	public class WorkerRole : RoleEntryPoint
	{
		public override void Run()
		{
			Trace.TraceInformation("Began AuroraWorker");

			AuroraWatcher.Log.Callback = OnLog;
			AuroraWatcher.Scheduler.Run();
		}

		public void OnLog(string s, params object[] args)
		{
			Trace.TraceInformation(s, args);
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			// For information on handling configuration changes
			// see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

			return base.OnStart();
		}
	}
}

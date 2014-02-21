using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraWatcher
{
	static class Scheduler
	{
		public static void Run()
		{
			int interval;
			string intervalSetting = ConfigurationManager.AppSettings["Scheduler.IntervalTimeInSeconds"];
			if (!int.TryParse(intervalSetting, out interval))
				interval = 60*60;

			Log.Write("Starting scheduler: interval is {0} seconds", interval);

			while (true)
			{
				Iteration();

				ConfigurationManager.AppSettings["Scheduler.LastRunTime"] = DateTime.UtcNow.ToString();
				Thread.Sleep(interval * 1000);
			}
		}

		private static void Iteration()
		{
			DateTime lastRunTime;
			string lastRunSetting = ConfigurationManager.AppSettings["Scheduler.LastRunTime"];
			if (!DateTime.TryParse(lastRunSetting, out lastRunTime))
			{
				lastRunTime = DateTime.UtcNow;
			}
			
			Log.Write("Scheduler woke up. Task start time: {0}, Last run time: {1}", DateTime.UtcNow, lastRunTime);
			
			string fromEmail = ConfigurationManager.AppSettings["TextMessage.FromEmailAddress"];
			if (string.IsNullOrEmpty(fromEmail))
			{
				Log.Write("TextMessage.FromEmailAddress not found");
				return;
			}

			string toSetting = ConfigurationManager.AppSettings["TextMessage.ToPhoneNumbers"];
			if (string.IsNullOrEmpty(toSetting))
			{
				Log.Write("TextMessage.ToPhoneNumbers not found");
				return;
			}

			string[] to = toSetting.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
						
			List<Alert> alerts = SpaceWeatherDataSource.FetchCurrentAlerts();
			Log.Write("Data source found {0} alerts", alerts.Count);
			
			bool alerted = false;
			foreach (var alert in alerts.OrderByDescending(a => a.IssueTime))
			{
				if (alert.IssueTime > lastRunTime)
				{
					if (alert.ShouldAlert())
					{
						foreach (var address in to)
						{
							var smsEmail = address.Trim();
							if (!TextMessage.Send(smsEmail, fromEmail, alert.Subject, alert.Body))
							{
								Log.Write("Alert sent to {0} failed", smsEmail);
							}
						}
						alerted = true;
						break;
					}
				}
			}

			if (!alerted)
				Log.Write("No alertable events found");

			Log.Write("Task complete. Current time: {0}", DateTime.UtcNow);
		}
	}
}

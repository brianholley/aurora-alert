using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraWatcher
{
	public static class Scheduler
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
				try
				{
					Iteration();
				}
				catch (Exception e)
				{
					Log.Write("Exception occurred: {0}", e);
				}

				ConfigurationManager.AppSettings["Scheduler.LastRunTime"] = DateTime.UtcNow.ToString();
				Thread.Sleep(interval * 1000);
			}
		}

		private static void Iteration()
		{
			DateTime lastRunTime;
			string lastRunSetting = ConfigurationManager.AppSettings["Scheduler.LastRunTime"];
			if (!DateTime.TryParse(lastRunSetting, out lastRunTime))
				lastRunTime = DateTime.UtcNow;
			
			Log.Write("Scheduler woke up. Task start time: {0}, Last run time: {1}", DateTime.UtcNow, lastRunTime);

			bool shouldAlert;
			if (!bool.TryParse(ConfigurationManager.AppSettings["Alert.ShouldAlert"], out shouldAlert))
				shouldAlert = false;
			Log.Write("Should alert: {0}", shouldAlert);

			string fromEmail = ConfigurationManager.AppSettings["TextMessage.FromEmailAddress"];
			Log.Write("Sending email from: {0}", fromEmail);

			string toSetting = ConfigurationManager.AppSettings["TextMessage.ToPhoneNumbers"];
			string[] to = toSetting != null ? toSetting.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
			Log.Write("Sending to: {0}", toSetting);
			
			List<Alert> alerts = SpaceWeatherDataSource.FetchCurrentAlertsSince(lastRunTime);
			Log.Write("Data source found {0} alerts", alerts.Count);
			
			bool alerted = false;
			foreach (var alert in alerts.OrderByDescending(a => a.IssueTime))
			{
				Log.Write("Alert Serial Number: {0}", alert.SerialNumber);
				if (shouldAlert && alert.ShouldAlert() && !string.IsNullOrEmpty(fromEmail) && to != null)
				{
					Log.Write("Alert should send");
					foreach (var address in to)
					{
						var smsEmail = address.Trim();
						Log.Write("Sending alert to {0}", smsEmail);
						if (!TextMessage.Send(smsEmail, fromEmail, alert.Subject, alert.Body))
						{
							Log.Write("Alert failed");
						}
					}
					alerted = true;
					break;
				}
			}

			if (!alerted)
				Log.Write("No alertable events found");

			Log.Write("Task complete. Current time: {0}", DateTime.UtcNow);
		}
	}
}

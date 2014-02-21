using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraWatcher
{
	class Program
	{
		private const string EmailAddress = "";
		private const string EmailPassword = "";
		private const string PhoneNumber = "";
		private const Provider PhoneProvider = Provider.Unknown;

		static void Main(string[] args)
		{
			DateTime lastAlertTime = DateTime.MinValue;
			while (true)
			{
				List<Alert> alerts = SpaceWeatherDataSource.FetchCurrentAlerts();

				foreach (var alert in alerts.OrderByDescending(a => a.IssueTime))
				{
					if (alert.IssueTime > lastAlertTime)
					{
						if (alert.ShouldAlert())
						{
							TextMessage.Send(PhoneNumber, PhoneProvider, EmailAddress, EmailPassword, alert.Subject(), alert.Body());
							break;
						}
					}
				}

				lastAlertTime = DateTime.UtcNow;
				Thread.Sleep(60 * 60 * 1000);
			}
		}
	}
}

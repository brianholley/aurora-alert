using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AuroraWatcher
{
	class SpaceWeatherDataSource
	{
		private const string DataUrl = "http://www.swpc.noaa.gov/alerts/archive/current_month.html";

		private const string MessageCodeField = "Space Weather Message Code";
		private const string SerialNumberField = "Serial Number";
		private const string IssueTimeField = "Issue Time";

		public static List<Alert> FetchCurrentAlertsSince(DateTime lastSyncTime)
		{
			List<Alert> alerts = new List<Alert>();
			try
			{
				HttpWebRequest request = HttpWebRequest.CreateHttp(DataUrl);
				request.Method = "GET";
				var response = request.GetResponse() as HttpWebResponse;

				using (var stream = response.GetResponseStream())
				using (var reader = new StreamReader(stream))
				{
					string page = reader.ReadToEnd();

					foreach (var group in page.Split(new[] { "<hr>" }, StringSplitOptions.RemoveEmptyEntries))
					{
						Alert alert = new Alert();
						string[] parts = group.Split(new[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length == 3)
						{
							foreach (var field in parts[0].Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries))
							{
								int index = field.IndexOf(':');
								if (index != -1)
								{
									string fieldName = field.Substring(0, index).Trim();
									string fieldValue = field.Substring(index + 1).Trim();
									switch (fieldName)
									{
										case MessageCodeField:
											alert.MessageCode = fieldValue;
											break;
										case SerialNumberField:
											alert.SerialNumber = fieldValue;
											break;
										case IssueTimeField:
											{
												alert.IssueTime = DateTime.ParseExact(fieldValue, "yyyy MMM dd HHmm UTC", CultureInfo.InvariantCulture);
												break;
											}
									}
								}
							}
							alert.Details = parts[2].Replace("<br>", "").Trim();
							if (alert.IssueTime > lastSyncTime)
								alerts.Add(alert);
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Write("Exception during SpaceWeather data source fetch: {0}", e);
			}
			return alerts;
		}
	}
}

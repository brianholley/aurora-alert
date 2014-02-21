using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraWatcher
{
	class Alert
	{
		public string MessageCode;
		public string SerialNumber;
		public DateTime IssueTime;
		public string Details;

		public bool ShouldAlert()
		{
			switch (MessageCode.Substring(0, 3))
			{
				case "ALT":
					if (MessageCode.Substring(3, 1) == "K")
					{
						int kLevel = 0;
						if (int.TryParse(MessageCode.Substring(4), out kLevel) && kLevel >= 6)
						{
							int latitude = 0;
							{
								Regex regex = new Regex(@"poleward of (\d+) degrees Geomagnetic Latitude");
								var match = regex.Match(Details);
								if (match.Success)
								{
									int.TryParse(match.Groups[1].Value, out latitude);
								}
							}

							bool washington = false;
							{
								Regex regex = new Regex(@"Aurora \- .* Washington state");
								washington = regex.Match(Details).Success;
							}

							if (latitude <= 58 || washington)
								return true;
						}
					}
					break;
				case "WAR":
					break;
			}
			return false;
		}

		public string Subject()
		{
			return MessageCode;
		}

		public string Body()
		{
			return Details;
		}
	}

	class Program
	{
		private const string EmailAddress = "";
		private const string EmailPassword = "";

		private const string DataUrl = "http://www.swpc.noaa.gov/alerts/archive/current_month.html";

		private const string MessageCodeField = "Space Weather Message Code";
		private const string SerialNumberField = "Serial Number";
		private const string IssueTimeField = "Issue Time";

		static void Main(string[] args)
		{
			DateTime lastAlertTime = DateTime.MinValue;
			while (true)
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

						foreach (var group in page.Split(new[] {"<hr>"}, StringSplitOptions.RemoveEmptyEntries))
						{
							Alert alert = new Alert();
							string[] parts = group.Split(new[] {"<p>", "</p>"}, StringSplitOptions.RemoveEmptyEntries);
							if (parts.Length == 3)
							{
								foreach (var field in parts[0].Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries))
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
								alerts.Add(alert);
							}
						}
					}
				}
				catch (Exception)
				{
					throw;
				}

				foreach (var alert in alerts.OrderByDescending(a => a.IssueTime))
				{
					if (alert.IssueTime > lastAlertTime)
					{
						if (alert.ShouldAlert())
						{
							SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
							smtp.UseDefaultCredentials = false;
							smtp.Credentials = new NetworkCredential(EmailAddress, EmailPassword);
							smtp.EnableSsl = true;

							string verizon = "vtext.com";
							MailMessage message = new MailMessage("bholley@gmail.com", "4252335014@txt.att.net", alert.Subject(), alert.Body());
							smtp.Send(message);
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

using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace AuroraWatcher
{
	static class TextMessage
	{
		public static bool Send(string toSmsEmail, string fromEmail, string subject, string body)
		{
			string host = ConfigurationManager.AppSettings["Smtp.ServerHost"];
			if (string.IsNullOrEmpty(host))
			{
				Log.Write("Can't send SMS - no host");
				return false;
			}

			int port;
			string portSetting = ConfigurationManager.AppSettings["Smtp.ServerPort"];
			if (!int.TryParse(portSetting, out port))
			{
				Log.Write("Can't send SMS - no port");
				return false;
			}

			string smtpEmail = ConfigurationManager.AppSettings["Smtp.EmailAddress"];
			if (string.IsNullOrEmpty(smtpEmail))
			{
				Log.Write("Can't send SMS - no smtp address credentials");
				return false;
			}

			string smtpPassword = ConfigurationManager.AppSettings["Smtp.EmailPassword"];
			if (string.IsNullOrEmpty(smtpPassword))
			{
				Log.Write("Can't send SMS - no smtp password credentials");
				return false;
			}

			var smtp = new SmtpClient(host, port);
			smtp.UseDefaultCredentials = false;
			smtp.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
			smtp.EnableSsl = true;

			try
			{
				var message = new MailMessage(fromEmail, toSmsEmail, subject, body);
				smtp.Send(message);
				return true;
			}
			catch (Exception e)
			{
				Log.Write("Failed to send SMS - {0}", e);
				return false;
			}
		}
	}
}

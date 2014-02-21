using System;
using System.Net;
using System.Net.Mail;

namespace AuroraWatcher
{
	enum Provider
	{
		Unknown,
		ATT,
		Verizon,
		TMobile
	}

	static class TextMessage
	{
		private static string ProviderToEmailHost(Provider provider)
		{
			string emailHost = "";
			switch (provider)
			{
				case Provider.ATT:
					emailHost = "@txt.att.net";
					break;
				case Provider.Verizon:
					emailHost = "@vtext.com";
					break;
				case Provider.TMobile:
					emailHost = "";
					break;
			}
			return emailHost;
		}

		public static bool Send(string toPhoneNumber, Provider provider, string fromEmailAddress, string emailPassword, string subject, string body)
		{
			var smtp = new SmtpClient("smtp.gmail.com", 587);
			smtp.UseDefaultCredentials = false;
			smtp.Credentials = new NetworkCredential(fromEmailAddress, emailPassword);
			smtp.EnableSsl = true;

			string textAddress = toPhoneNumber + ProviderToEmailHost(provider);
			var message = new MailMessage(fromEmailAddress, textAddress, subject, body);
			smtp.Send(message);
			return true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

		public string Subject
		{
			get { return MessageCode; }
		}

		public string Body
		{
			get { return Details; }
		}
	}

}

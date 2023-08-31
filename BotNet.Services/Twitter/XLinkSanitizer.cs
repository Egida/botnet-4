﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace BotNet.Services.Twitter {
	public class XLinkSanitizer {
		public static Uri Sanitize(Uri link) {
			string sanitizedUri = link.GetLeftPart(UriPartial.Path);
			return new Uri(sanitizedUri);
		}

		public static Uri? FindTrackedXLink(string message) {
			return Regex.Matches(message, "https://x.com/[0-9a-zA-Z_]+/status/[0-9]{18,20}\\?")
				.Select(match => new Uri(match.Value))
				.FirstOrDefault();
		}
	}
}

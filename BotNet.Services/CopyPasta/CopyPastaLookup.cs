﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace BotNet.Services.CopyPasta {
	public class CopyPastaLookup {
		private static readonly ImmutableDictionary<string, ImmutableList<string>> DICTIONARY;

		static CopyPastaLookup() {
			using Stream stream = Assembly.GetAssembly(typeof(CopyPastaLookup))!.GetManifestResourceStream("BotNet.Services.CopyPasta.Pasta.json")!;
			using StreamReader streamReader = new(stream);
			string json = streamReader.ReadToEnd();
			DICTIONARY = JsonSerializer.Deserialize<ImmutableDictionary<string, ImmutableList<string>>>(json)!;
		}

		public static bool TryGetAutoText(string key, [NotNullWhen(true)] out ImmutableList<string>? values) {
			return DICTIONARY.TryGetValue(key, out values);
		}
	}
}

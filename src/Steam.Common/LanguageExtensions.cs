using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Steam
{
    /// <summary>
    /// Provides extensions to get english, native, and API language code strings from a <see cref="Language"/> value
    /// </summary>
    public static class LanguageExtensions
    {
        // oh no what have I done
        private readonly static Dictionary<Language, (string english, string native, string api, string web, CultureInfo culture)> _names = new Dictionary<Language, (string, string, string, string, CultureInfo)>
        {
            { Language.Arabic, ("Arabic", "العربية", "arabic", "ar", CultureInfo.ReadOnly(new CultureInfo("ar"))) },
            { Language.Bulgarian, ("Bulgarian", "български език", "bulgarian", "bg", CultureInfo.ReadOnly(new CultureInfo("bg"))) },
            { Language.SimplifiedChinese, ("Chinese (Simplified)", "简体中文", "schinese", "zh-CN", CultureInfo.ReadOnly(new CultureInfo("zh-CN"))) },
            { Language.TraditionalChinese, ("Chinese (Traditional)", "繁體中文", "tchinese", "zh-TW", CultureInfo.ReadOnly(new CultureInfo("zh-TW"))) },
            { Language.Czech, ("Czech", "čeština", "czech", "cs", CultureInfo.ReadOnly(new CultureInfo("cs"))) },
            { Language.Danish, ("Danish", "Dansk", "danish", "da", CultureInfo.ReadOnly(new CultureInfo("da"))) },
            { Language.Dutch, ("Dutch", "Nederlands", "dutch", "nl", CultureInfo.ReadOnly(new CultureInfo("nl"))) },
            { Language.English, ("English", "English", "english", "en", CultureInfo.ReadOnly(new CultureInfo("en"))) },
            { Language.Finnish, ("Finnish", "Suomi", "finnish", "fi", CultureInfo.ReadOnly(new CultureInfo("fi"))) },
            { Language.French, ("French", "Français", "french", "fr", CultureInfo.ReadOnly(new CultureInfo("fr"))) },
            { Language.German, ("German", "Deutsch", "german", "de", CultureInfo.ReadOnly(new CultureInfo("de"))) },
            { Language.Greek, ("Greek", "Ελληνικά", "greek", "el", CultureInfo.ReadOnly(new CultureInfo("el"))) },
            { Language.Hungarian, ("Hungarian", "Magyar", "hungarian", "hu", CultureInfo.ReadOnly(new CultureInfo("hu"))) },
            { Language.Italian, ("Italian", "Italiano", "italian", "it", CultureInfo.ReadOnly(new CultureInfo("it"))) },
            { Language.Japanese, ("Japanese", "日本語", "japanese", "ja", CultureInfo.ReadOnly(new CultureInfo("ja"))) },
            { Language.Korean, ("Korean", "한국어", "koreana", "ko", CultureInfo.ReadOnly(new CultureInfo("ko"))) },
            { Language.Norwegian, ("Norwegian", "Norsk", "norwegian", "no", CultureInfo.ReadOnly(new CultureInfo("no"))) },
            { Language.Polish, ("Polish", "Polski", "polish", "pl", CultureInfo.ReadOnly(new CultureInfo("pl"))) },
            { Language.Portuguese, ("Portuguese", "Português", "portuguese", "pt", CultureInfo.ReadOnly(new CultureInfo("pt"))) },
            { Language.PortugueseBrazil, ("Portuguese-Brazil", "brazilian", "", "pt-BR", CultureInfo.ReadOnly(new CultureInfo("pt-BR"))) },
            { Language.Romanian, ("Romanian", "Română", "romanian", "ro", CultureInfo.ReadOnly(new CultureInfo("ro"))) },
            { Language.Russian, ("Russian", "Русский", "russian", "ru", CultureInfo.ReadOnly(new CultureInfo("ru"))) },
            { Language.Spanish, ("Spanish", "Español", "spanish", "es", CultureInfo.ReadOnly(new CultureInfo("es"))) },
            { Language.Swedish, ("Swedish", "Svenska", "swedish", "sv", CultureInfo.ReadOnly(new CultureInfo("sv"))) },
            { Language.Thai, ("Thai", "ไทย", "thai", "th", CultureInfo.ReadOnly(new CultureInfo("th"))) },
            { Language.Turkish, ("Turkish", "Türkçe", "turkish", "tr", CultureInfo.ReadOnly(new CultureInfo("tr"))) },
            { Language.Ukrainian, ("Ukrainian", "Українська", "ukrainian", "uk", CultureInfo.ReadOnly(new CultureInfo("uk"))) }
        };

        /// <summary>
        /// Gets the english name for a specified <see cref="Language"/> value
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetEnglishName(this Language lang)
        {
            if (!_names.TryGetValue(lang, out var value))
                throw new ArgumentOutOfRangeException(nameof(lang));

            return value.english;
        }

        /// <summary>
        /// Gets the native name for a specified <see cref="Language"/> value
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetNativeName(this Language lang)
        {
            if (!_names.TryGetValue(lang, out var value))
                throw new ArgumentOutOfRangeException(nameof(lang));

            return value.native;
        }

        /// <summary>
        /// Gets the API language code for a specified <see cref="Language"/> value
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetApiLanguageCode(this Language lang)
        {
            if (!_names.TryGetValue(lang, out var value))
                throw new ArgumentOutOfRangeException(nameof(lang));

            return value.api;
        }

        /// <summary>
        /// Gets the Web API language code for a specified <see cref="Language"/> value
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetWebApiLanguageCode(this Language lang)
        {
            if (!_names.TryGetValue(lang, out var value))
                throw new ArgumentOutOfRangeException(nameof(lang));

            return value.web;
        }

        /// <summary>
        /// Gets the culture that best matches the specified <see cref="Language"/> value
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static CultureInfo GetCulture(this Language lang)
        {
            if (!_names.TryGetValue(lang, out var value))
                throw new ArgumentOutOfRangeException(nameof(lang));

            return value.culture;
        }

        /// <summary>
        /// Gets the language that best matches the specified <see cref="CultureInfo"/>. If no language is found, this returns <see cref="Language.English"/>
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static Language GetSteamLanguage(this CultureInfo culture)
        {
            var value = _names.FirstOrDefault(v => v.Value.culture == culture);
            return value.Value.culture == null ? Language.English : value.Key;
        }
    }
}

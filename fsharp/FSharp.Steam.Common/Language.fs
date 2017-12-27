namespace FSharp.Steam

open System.Globalization

[<AutoOpen>]
module Language = 
    type Language = 
        | Arabic
        | Bulgarian
        | SimplifiedChinese
        | TraditionalChinese
        | Czech
        | Danish
        | Dutch
        | English
        | Finnish
        | French
        | German
        | Greek
        | Hungarian
        | Italian
        | Japanese
        | Korean
        | Norwegian
        | Polish
        | Portuguese
        | PortugueseBrazil
        | Romanian
        | Russian
        | Spanish
        | Swedish
        | Thai
        | Turkish
        | Ukrainian

    type LanguageInfo = { 
        EnglishName : string;
        NativeName : string;
        ApiCode : string;
        WebApiCode : string;
        Culture : CultureInfo;
    }

    let languageInfo = Map.ofList [ 
                        (Language.Arabic, {EnglishName = "Arabic"; NativeName = "العربية"; ApiCode = "arabic"; WebApiCode = "ar"; Culture = CultureInfo.ReadOnly(new CultureInfo("ar"))});
                        (Language.Bulgarian, {EnglishName = "Bulgarian"; NativeName = "български език"; ApiCode = "bulgarian";  WebApiCode = "bg"; Culture = CultureInfo.ReadOnly(new CultureInfo("bg"))});
                        (Language.SimplifiedChinese, {EnglishName = "Chinese (Simplified)"; NativeName = "简体中文"; ApiCode = "schinese";  WebApiCode = "zh-CN"; Culture = CultureInfo.ReadOnly(new CultureInfo("zh-CN"))});
                        (Language.TraditionalChinese, {EnglishName = "Chinese (Traditional)"; NativeName = "繁體中文"; ApiCode = "tchinese"; WebApiCode = "zh-TW"; Culture = CultureInfo.ReadOnly(new CultureInfo("zh-TW"))});
                        (Language.Czech, {EnglishName = "Czech"; NativeName = "čeština"; ApiCode = "czech"; WebApiCode = "cs"; Culture = CultureInfo.ReadOnly(new CultureInfo("cs"))});
                        (Language.Danish, {EnglishName = "Danish"; NativeName = "Dansk"; ApiCode = "danish"; WebApiCode = "da"; Culture = CultureInfo.ReadOnly(new CultureInfo("da"))});
                        (Language.Dutch, {EnglishName = "Dutch"; NativeName = "Nederlands"; ApiCode = "dutch"; WebApiCode = "nl"; Culture = CultureInfo.ReadOnly(new CultureInfo("nl"))});
                        (Language.English, {EnglishName = "English"; NativeName = "English"; ApiCode = "english"; WebApiCode = "en"; Culture = CultureInfo.ReadOnly(new CultureInfo("en"))});
                        (Language.Finnish, {EnglishName = "Finnish"; NativeName = "Suomi"; ApiCode = "finnish"; WebApiCode = "fi"; Culture = CultureInfo.ReadOnly(new CultureInfo("fi"))});
                        (Language.French, {EnglishName = "French"; NativeName = "Français"; ApiCode = "french"; WebApiCode = "fr"; Culture = CultureInfo.ReadOnly(new CultureInfo("fr"))});
                        (Language.German, {EnglishName = "German"; NativeName = "Deutsch"; ApiCode = "german"; WebApiCode = "de"; Culture = CultureInfo.ReadOnly(new CultureInfo("de"))});
                        (Language.Greek, {EnglishName = "Greek"; NativeName = "Ελληνικά"; ApiCode = "greek"; WebApiCode = "el"; Culture = CultureInfo.ReadOnly(new CultureInfo("el"))});
                        (Language.Hungarian, {EnglishName = "Hungarian"; NativeName = "Magyar"; ApiCode = "hungarian"; WebApiCode = "hu"; Culture = CultureInfo.ReadOnly(new CultureInfo("hu"))});
                        (Language.Italian, {EnglishName = "Italian"; NativeName = "Italiano"; ApiCode = "italian"; WebApiCode = "it"; Culture = CultureInfo.ReadOnly(new CultureInfo("it"))});
                        (Language.Japanese, {EnglishName = "Japanese"; NativeName = "日本語"; ApiCode = "japanese"; WebApiCode = "ja"; Culture = CultureInfo.ReadOnly(new CultureInfo("ja"))});
                        (Language.Korean, {EnglishName = "Korean"; NativeName = "한국어"; ApiCode = "koreana"; WebApiCode = "ko"; Culture = CultureInfo.ReadOnly(new CultureInfo("ko"))});
                        (Language.Norwegian, {EnglishName = "Norwegian"; NativeName = "Norsk"; ApiCode = "norwegian"; WebApiCode = "no"; Culture = CultureInfo.ReadOnly(new CultureInfo("no"))});
                        (Language.Polish, {EnglishName = "Polish"; NativeName = "Polski"; ApiCode = "polish"; WebApiCode = "pl"; Culture = CultureInfo.ReadOnly(new CultureInfo("pl"))});
                        (Language.Portuguese, {EnglishName = "Portuguese"; NativeName = "Português"; ApiCode = "portuguese"; WebApiCode = "pt"; Culture = CultureInfo.ReadOnly(new CultureInfo("pt"))});
                        (Language.PortugueseBrazil, {EnglishName = "Portuguese-Brazil"; NativeName = "Português-Brasil"; ApiCode = "brazilian"; WebApiCode = "pt-BR"; Culture = CultureInfo.ReadOnly(new CultureInfo("pt-BR"))});
                        (Language.Romanian, {EnglishName = "Romanian"; NativeName = "Română"; ApiCode = "romanian"; WebApiCode = "ro"; Culture = CultureInfo.ReadOnly(new CultureInfo("ro"))});
                        (Language.Russian, {EnglishName = "Russian"; NativeName = "Русский"; ApiCode = "russian"; WebApiCode = "ru"; Culture = CultureInfo.ReadOnly(new CultureInfo("ru"))});
                        (Language.Spanish, {EnglishName = "Spanish"; NativeName = "Español"; ApiCode = "spanish"; WebApiCode = "es"; Culture = CultureInfo.ReadOnly(new CultureInfo("es"))});
                        (Language.Swedish, {EnglishName = "Swedish"; NativeName = "Svenska"; ApiCode = "swedish"; WebApiCode = "sv"; Culture = CultureInfo.ReadOnly(new CultureInfo("sv"))});
                        (Language.Thai, {EnglishName = "Thai"; NativeName = "ไทย"; ApiCode = "thai"; WebApiCode = "th"; Culture = CultureInfo.ReadOnly(new CultureInfo("th"))});
                        (Language.Turkish, {EnglishName = "Turkish"; NativeName = "Türkçe"; ApiCode = "turkish"; WebApiCode = "tr"; Culture = CultureInfo.ReadOnly(new CultureInfo("tr"))});
                        (Language.Ukrainian, {EnglishName = "Ukrainian"; NativeName = "Українська"; ApiCode = "ukrainian"; WebApiCode = "uk"; Culture = CultureInfo.ReadOnly(new CultureInfo("uk"))})
                    ]

    type Language with
        member this.LanguageInfo = languageInfo.[this]


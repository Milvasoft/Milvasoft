using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;

namespace Milvasoft.Core.MultiLanguage.Manager;
public static class LanguagesSeed
{
    public static List<ILanguage> Seed =
    [
        new Language {
            Name = "Türkçe",
            Supported = true,
            Code = "tr-TR",
            Id = 1,
            IsDefault = true
        },
        new Language
        {
            Name = "English(US)",
            Supported = true,
            Code = "en-US",
            Id = 2
        },
        new Language
        {
            Name = "Azərbaycan Dili",
            Supported = false,
            Code = "az-AZ",
            Id = 3
        },
        new Language
        {
            Name = "Ελληνικά",
            Supported = false,
            Code = "el-GR",
            Id = 4
        },
        new Language
        {
            Name = "Deutsche",
            Supported = false,
            Code = "de-DE",
            Id = 5
        },
        new Language
        {
            Name = "Nederlands",
            Supported = false,
            Code = "nl-NL",
            Id = 6
        },
        new Language
        {
            Name = "English(UK)",
            Supported = false,
            Code = "en-GB",
            Id = 7
        },
        new Language
        {
            Name = "Español",
            Supported = false,
            Code = "es-ES",
            Id = 8
        },
        new Language
        {
            Name = "Français",
            Supported = false,
            Code = "fr-FR",
            Id = 9
        },
        new Language
        {
            Name = "Italiano",
            Supported = false,
            Code = "it-IT",
            Id = 10
        },
        new Language
        {
            Name = "русский",
            Supported = false,
            Code = "ru-RU",
            Id = 11
        },
        new Language
        {
            Name = "中文",
            Supported = false,
            Code = "zh-CHS",
            Id = 12
        },
        new Language
        {
            Name = "日本人",
            Supported = false,
            Code = "ja-JP",
            Id = 13
        },
        new Language
        {
            Name = "हिंदी",
            Supported = false,
            Code = "hi-IN",
            Id = 14
        },
        new Language
        {
            Name = "عربي",
            Supported = false,
            Code = "ar-AE",
            Id = 15
        }
    ];

    internal class Language : ILanguage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Supported { get; set; }
        public bool IsDefault { get; set; }
    }
}

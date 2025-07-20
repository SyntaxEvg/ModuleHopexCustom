using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestDetect
{
    //    Console.WriteLine(LanguageChecker.IsLikelyFrenchWord("maison"));    // True
    //Console.WriteLine(LanguageChecker.IsLikelyGermanWord("Buchhandlung")); // True (ung, ch)
    //Console.WriteLine(LanguageChecker.IsLikelySpanishWord("educación"));   // True (ción, á)
    //Console.WriteLine(LanguageChecker.IsLikelyPortugueseWord("comunicação")); // True (ção, ã)
    //Console.WriteLine(LanguageChecker.IsLikelyJapaneseWord("かわいい"));     // True (хирагана)
    //Console.WriteLine(LanguageChecker.IsLikelyJapaneseWord("大好き"));       // True (кандзи)
    //Console.WriteLine(LanguageChecker.IsLikelyJapaneseWord("yamada"));      // True (романизация)
    //Console.WriteLine(LanguageChecker.IsLikelyJapaneseWord("hello"));       // False
   

    public class LanguageChecker
    {
        bool IsLikelyWordOfLanguage(string word, string langCode)
        {
            return langCode switch
            {
                "fr" => IsLikelyFrenchWord(word),
                "de" => IsLikelyGermanWord(word),
                "es" => IsLikelySpanishWord(word),
                "pt" => IsLikelyPortugueseWord(word),
                "ja" => IsLikelyJapaneseWord(word),
                _ => false
            };
        }



        // 1. Французский (мы уже сделали)
        public static bool IsLikelyFrenchWord(string? word) => CheckWord(word,
            endings: new[] { "ée", "eau", "ette", "oir", "age", "ment", "ique", "eur", "ille", "tion" },
            patterns: new[] { "ch", "gn", "oi", "ou", "ill" },
            doubleConsonants: new[] { "ll", "tt", "nn", "ss", "mm", "pp", "rr", "cc", "ff" },
            //diacriticsRegex: @"[éèàùâêîôûçëïü]",
            nonPatterns: new[] { "w", "k", "th", "sh", "gh" },
            nonDoubleConsonants: new[] { "bb", "dd", "gg", "jj", "yy", "ww", "vv", "zz" }
        );

        // 2. Немецкий
        public static bool IsLikelyGermanWord(string? word) => CheckWord(word,
            endings: new[] { "ung", "lich", "keit", "heit", "chen", "lein", "tion", "tät", "schaft" },
            patterns: new[] { "sch", "ch", "ei", "ie", "au", "äu", "eu" },
            doubleConsonants: new[] { "ss", "tt", "nn", "mm", "ll", "ff", "ck", "pf", "tz" },
           // diacriticsRegex: @"[äöüß]",
            nonPatterns: new[] { "q", "x", "y" }, // Редко, в основном в заимствованиях
            nonDoubleConsonants: new[] { "bb", "dd", "gg", "jj", "vv", "ww" }
        );

        // 3. Испанский
        public static bool IsLikelySpanishWord(string? word) => CheckWord(word,
            endings: new[] { "ción", "dad", "ero", "ista", "mente", "ario", "ido", "ada", "ura" },
            patterns: new[] { "ll", "ñ", "ch", "gu", "qu", "ue", "ie" },
            doubleConsonants: new[] { "ll", "rr", "cc", "nn", "ss", "tt" },
          //  diacriticsRegex: @"[áéíóúüñ]",
            nonPatterns: new[] { "w", "k", "th", "sh" }, // w, k редки; th, sh не испанские
            nonDoubleConsonants: new[] { "bb", "dd", "gg", "jj", "yy", "ww", "hh" }
        );

        // 4. Португальский
        public static bool IsLikelyPortugueseWord(string? word) => CheckWord(word,
            endings: new[] { "ção", "mente", "eiro", "ista", "dade", "oso", "ada", "ido", "ura" },
            patterns: new[] { "ch", "lh", "nh", "ão", "õe", "ei", "ou" },
            doubleConsonants: new[] { "rr", "ss", "cc", "tt", "nn", "mm", "ll", "ff" },
            //diacriticsRegex: @"[áéíóúâêôãõç]",
            nonPatterns: new[] { "w", "k", "y", "th", "sh" }, // w, k почти не встречаются
            nonDoubleConsonants: new[] { "bb", "dd", "gg", "jj", "yy", "ww", "hh", "vv" }
        );

        // 5. Японский (совсем другая логика, без doubleConsonants и диакритиков)
        public static bool IsLikelyJapaneseWord(string? word)
        {
            if (string.IsNullOrWhiteSpace(word)) return false;

            string wordLower = word.ToLowerInvariant();

            // Проверка на наличие хотя бы одного иероглифа (кандзи) или символов хираганы/катаканы
            if (Regex.IsMatch(word, @"[\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FFF]")) // Hiragana, Katakana, Kanji
                return true;

            // Проверка на ромадзи (латинские японские слова) по характерным паттернам
            string[] japaneseRomajiPatterns = { "shi", "chi", "tsu", "kyo", "ryo", "nyo", "ryo", "gyo" };
            if (japaneseRomajiPatterns.Any(p => wordLower.Contains(p)))
                return true;

            // Отрицательные правила для ромадзи: если содержит нехарактерные для японского англ. сочетания
            string[] nonJapanesePatterns = { "w", "v", "x", "z", "th", "sh" };
            if (nonJapanesePatterns.Any(p => wordLower.Contains(p)))
                return false;

            // Если слово короткое и не содержит кандзи/кану — вряд ли японское
            return false;
        }

        // Универсальный метод проверки (для французского, немецкого, испанского, португальского)
        private static bool CheckWord(string? word,
            string[] endings,
            string[] patterns,
            string[] doubleConsonants,
            //string? diacriticsRegex,
            string[] nonPatterns,
            string[] nonDoubleConsonants)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            string wordLower = word.ToLowerInvariant();

            // 1. Вето (отрицательные правила)
            if (nonPatterns.Any(p => wordLower.Contains(p, StringComparison.Ordinal)))
                return false;
            if (nonDoubleConsonants.Any(dc => wordLower.Contains(dc, StringComparison.Ordinal)))
                return false;

            // 2. Положительные правила
            if (endings.Any(e => wordLower.EndsWith(e, StringComparison.Ordinal)))
                return true;
            if (patterns.Any(p => wordLower.Contains(p, StringComparison.Ordinal)))
                return true;
            if (doubleConsonants.Any(dc => wordLower.Contains(dc, StringComparison.Ordinal)))
                return true;
            //if (!string.IsNullOrEmpty(diacriticsRegex) && Regex.IsMatch(word, diacriticsRegex))
            //    return true;

            return false;
        }
    }
}

using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace WebApiNNcallMacro
{
    public class ProfessionalLanguageDetector
    {
        private static readonly Regex FrenchWordRegex = new Regex(@"
    (?:(?:[àâäéèêëîïôöùûüÿçœæ])|                # Любой французский спецсимвол
    (?:tion|ment|age|isme|able|ique|oire|eur|euse|ette|iste)|  # Суффиксы
    \b\w+[szx]e\b|                             # Окончания на -se, -ze, -xe
    ^(?:re|dé|pré|mé|sous|sur)\w+              # Префиксы
    ", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private bool IsLikelyFrenchWord(string word)
        {
            return FrenchWordRegex.IsMatch(word);
        }



        private const string LanguageProfilePath = "Core14.profile.xml";
        private static readonly RankedLanguageIdentifier Identifier;

        //bool isFrench = detector.IsLikelyWordOfLanguage("bibliothèque", "fr"); // true
        //bool isJapanese = detector.IsLikelyWordOfLanguage("こんにちは", "ja"); // true
        //bool isGibberish = detector.IsLikelyWordOfLanguage("asdf1234", "de"); // false
        public bool IsLikelyWordOfLanguage(string word, string langCode)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < 3)
                return false;

            if (IsGibberish(word))
                return false;

            // Сначала проверяем языковые специфичные паттерны
            var likelyByPattern = langCode switch
            {
                "fr" => IsLikelyFrenchWord(word),
                "de" => IsLikelyGermanWord(word),
                "es" => IsLikelySpanishWord(word),
                "pt" => IsLikelyPortugueseWord(word),
                "ja" => IsLikelyJapaneseWord(word),
                _ => false
            };

            if (!likelyByPattern)
                return false;

            // Затем проверяем через NTextCat для подтверждения
            var detectedLanguage = DetectLanguage(word);
            return detectedLanguage == langCode;
        }

        private string DetectLanguage(string text)
        {
            var languages = _identifier.Identify(text);
            var bestMatch = languages.FirstOrDefault();

            return bestMatch?.Item2 >= ConfidenceThreshold ? bestMatch.Item1.Iso639_3 : "unknown";
        }

        private bool IsGibberish(string text)
        {уау
            /// 1. Проверка на слишком много разных символов
            var uniqueChars = new HashSet<char>(text).Count;
            if (uniqueChars > text.Length * 0.5) // эмпирическое значение
                return true;

            // 2. Проверка на необычные комбинации символов
            var nonLetters = text.Count(c => !char.IsLetter(c));
            if (nonLetters > text.Length * 0.4) // слишком много не-букв
                return true;

            // 3. Проверка на повторяющиеся паттерны (например, "ыыыыы" или "asdfasdf")
            if (HasRepeatingPatterns(text))
                return true;

            if (text.Length < 3) return true;

            var uniqueChars = new HashSet<char>(text).Count;
            if (uniqueChars > text.Length * 0.7) return true;

            var nonLetters = text.Count(c => !char.IsLetter(c));
            if (nonLetters > text.Length * 0.3) return true;

            return HasRepeatingPatterns(text);
        }

        private bool HasRepeatingPatterns(string text)
        {
            if (text.Length < 6) return false;

            for (int i = 0; i < text.Length - 6; i++)
            {
                var substring = text.Substring(i, 3);
                if (text.IndexOf(substring, i + 3) != -1)
                    return true;
            }
            return false;
        }

        // Языковые специфичные проверки
        private bool IsLikelyFrenchWord(string word)
        {
            // Французские окончания и диакритические знаки
            return Regex.IsMatch(word, @"(?i)\b\w+(?:aison|ment|tion|ette|ique|eau|eur|[àâäéèêëîïôöùûüÿç])\b");
        }

        private bool IsLikelyGermanWord(string word)
        {
            // Немецкие умлауты и длинные составные слова
            return Regex.IsMatch(word, @"(?i)\b\w*(?:sch|ch|ei|ie|ung|heit|keit|[äöüß])\w*\b") &&
                   word.Length > 4; // Немецкие слова часто длинные
        }

        private bool IsLikelySpanishWord(string word)
        {
            // Испанские окончания и ñ
            return Regex.IsMatch(word, @"(?i)\b\w*(?:ción|dad|mente|ado|ada|ando|[áéíóúñ])\b");
        }

        private bool IsLikelyPortugueseWord(string word)
        {
            // Португальские окончания и ç
            return Regex.IsMatch(word, @"(?i)\b\w*(?:ção|mento|dade|nh|[áàâãéêíóôõúç])\b");
        }

        private bool IsLikelyJapaneseWord(string word)
        {
            // Проверка на наличие японских символов (хирагана, катакана, канжи)
            return Regex.IsMatch(word, @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]");
        }




        /// <summary>
        /// Проверяет, является ли текст "мусорным" (кракозябрами).
        /// </summary>
        public static bool IsLikelyGarbage(string text, double minLetterRatio = 0.7)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            int letterOrDigitCount = text.Count(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c));
            return (double)letterOrDigitCount / text.Length < minLetterRatio;
        }

        /// <summary>
        /// Определяет язык текста с проверкой на мусор и низкой уверенностью.
        /// </summary>
        /// <param name="text">Текст для анализа.</param>
        /// <param name="confidenceThreshold">Минимальный порог уверенности (от 0.0 до 1.0).</param>
        /// <returns>Название языка или сообщение об ошибке.</returns>
        public static string DetectLanguage(string text, double confidenceThreshold = 0.3)
        {
            // ШАГ 1: Предварительная фильтрация мусора
            if (IsLikelyGarbage(text))
            {
                return "Мусорный текст (кракозябры)";
            }

            if (Identifier == null || string.IsNullOrWhiteSpace(text))
            {
                return "Не определен";
            }

            var languages = Identifier.Identify(text);
            var mostLikelyLanguage = languages.FirstOrDefault();

            if (mostLikelyLanguage == null)
            {
                return "Не определен";
            }

            var langCode = mostLikelyLanguage.Item1.Iso639_3;
            var confidence = mostLikelyLanguage.Item2;

            // ШАГ 2: Проверка уверенности библиотеки
            if (confidence < confidenceThreshold)
            {
                return $"Неуверенное определение (похоже на {langCode}, но оценка {confidence:F2} слишком низкая)";
            }

            if (LanguageNames.TryGetValue(langCode, out var name))
            {
                return name;
            }

            return $"Неизвестный код языка: {langCode}";
        }









        // НАШ НОВЫЙ СЛОВАРЬ С НАЗВАНИЯМИ ЯЗЫКОВ
        private static readonly Dictionary<string, string> LanguageNames = new()
    {
        // Ваши языки в формате ISO 639-3
        { "fra", "Французский" },
        { "deu", "Немецкий" },
        { "spa", "Испанский" },
        { "jpn", "Японский" },
        { "por", "Португальский" },

        // Другие популярные языки
        { "eng", "Английский" },
        { "rus", "Русский" },
        { "ita", "Итальянский" },
        { "zho", "Китайский" },
        { "ukr", "Украинский" },
        { "pol", "Польский" },
        { "ara", "Арабский" }
    };

        static ProfessionalLanguageDetector()
        {
            try
            {
                var factory = new RankedLanguageIdentifierFactory();
                Identifier = factory.Load(LanguageProfilePath);
                Console.WriteLine("✅ Языковой профиль успешно загружен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ОШИБКА: Не удалось загрузить языковой профиль '{LanguageProfilePath}'.");
                Console.WriteLine($"Подробности: {ex.Message}");
                Identifier = null;
            }
        }

        public static string Detect(string text)
        {
            if (Identifier == null || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var languages = Identifier.Identify(text);
            var mostLikelyLanguage = languages.FirstOrDefault();
            return mostLikelyLanguage?.Item1.Iso639_3;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("--- Запуск профессионального детектора языка ---");

            // Добавим фразы на новых языках для теста
            string[] testPhrases = {
            "Bonjour, je voudrais un croissant.",          // Французский
            "Guten Tag, wie geht es Ihnen?",                // Немецкий
            "Hola, ¿cómo estás?",                           // Испанский
            "こんにちは世界",                               // Японский
            "Olá Mundo, tudo bem?",                         // Португальский
            "This is an English sentence.",                 // Английский
            "Это простое предложение для теста.",           // Русский
            "Buongiorno, come stai?",                       // Итальянский
            "Привіт, як справи?",                           // Украинский
            "Cześć, jak się masz?",                         // Польский
        };

            Console.WriteLine("\n--- Результаты анализа ---");
            foreach (var phrase in testPhrases)
            {
                // 1. Получаем ISO-код языка
                var detectedCode = Detect(phrase);

                // 2. Ищем название языка в нашем словаре
                string languageName;
                if (detectedCode != null && LanguageNames.TryGetValue(detectedCode, out var name))
                {
                    // Если код найден в словаре, используем его название
                    languageName = name;
                }
                else
                {
                    // Если код не найден, показываем сам код или сообщение
                    languageName = $"Неизвестный ({detectedCode ?? "не определен"})";
                }

                // 3. Выводим красивый результат
                Console.WriteLine($"Текст: \"{phrase,-45}\" -> Язык: {languageName}");
            }
        }
    }
}
private static readonly Regex FrenchWordRegex = new Regex(
    @"(?:[àâäéèêëîïôöùûüÿçœæ])|" +               // французские спецсимволы
    @"(?:\w*(?:tion|ment|age|isme|able|ique|oire|eur|euse|ette|iste)\b)|" + // суффиксы
    @"(?:\b\w*[szx]e\b)|" +                      // окончания на -se, -ze, -xe
    @"(?:^[re|dé|pré|mé|sous|sur]\w*)",          // префиксы
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

private bool IsLikelyFrenchWord(string word)
{
    if (string.IsNullOrWhiteSpace(word)) return false;
    return FrenchWordRegex.IsMatch(word);
}
.......
    private bool IsLikelyGermanWord(string word)
{
    if (string.IsNullOrWhiteSpace(word) || word.Length < 3)
        return false;

    // Немецкие слова обычно содержат:
    // 1. Умлауты (ä, ö, ü) и ß
    // 2. Характерные сочетания букв (sch, ch, ck, tz, pf, sp, st, ei, ie и др.)
    // 3. Типичные суффиксы (ung, heit, keit, chen, lein, nis, tion, ik, ig, lich)
    // 4. Типичные префиксы (ver, ge, be, ent, er, zer)
    // 5. Частые немецкие буквосочетания в начале/середине слов

    // Проверка на наличие умлаутов или ß
    if (Regex.IsMatch(word, @"[äöüß]", RegexOptions.IgnoreCase))
        return true;

    // Проверка характерных немецких сочетаний и морфем
    var res = Regex.IsMatch(word, @"
        ^                       # Начало слова
        (?:                     
        # Префиксы
        (ver|ge|be|ent|er|zer) 
        |
        # Характерные сочетания
        .*(sch|ch|ck|tz|pf|sp|st|ei|ie|qu|dt|th|ph|ng|nk).* 
        |
        # Суффиксы
        .*(ung|heit|keit|chen|lein|nis|tion|ik|ig|lich|bar|sam|haft|los)\b 
        |
        # Частые немецкие окончания
        .*[^aeiouy](en|er|el)\b 
        )
        $                       # Конец слова
        ", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
    return res;
}

private bool IsLikelySpanishWord(string word)
{
    if (string.IsNullOrWhiteSpace(word) return false;

    // Короткие исацспанские слова (артикли, предлоги, местоимения)
    if (word.Length <= 3)
    {
        return Regex.IsMatch(word,
            @"^(el|la|los|las|un|una|unos|unas|y|o|a|en|de|que|con|por|sin|al|del|se|lo|mi|tu|su|nos|vos)$",
            RegexOptions.IgnoreCase);
    }

    // 1. Проверка на испанскую диакритику (á, é, í, ó, ú, ü, ñ)
    bool hasSpanishDiacritics = Regex.IsMatch(word, @"[áéíóúüñ]", RegexOptions.IgnoreCase);

    // 2. Характерные испанские сочетания букв
    bool hasSpanishCombinations = Regex.IsMatch(word,
        @"(ll|rr|ch|qu|gu|gü|cu|ua|ue|ui|uo|ía|ió)",
        RegexOptions.IgnoreCase);

    // 3. Типичные испанские суффиксы и окончания
    bool hasSpanishSuffixes = Regex.IsMatch(word, @"
        (ción|miento|dad|tad|tud|anza|ario|ero|era|dor|dora|ista|able|ible|ismo|oso|osa|ito|ita|illo|illa)\b",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    // 4. Глагольные окончания (-ar, -er, -ir и их спряжения)
    bool isSpanishVerb = Regex.IsMatch(word, @"
        (ar|er|ir|ando|iendo|ado|ido|aba|ía|aré|eré|iré|arás|erás|irás)\b",
        RegexOptions.IgnoreCase);

    // 5. Частые испанские приставки
    bool hasSpanishPrefixes = Regex.IsMatch(word,
        @"^(des|re|pre|trans|super|anti|auto|extra|contra|bi|tri)",
        RegexOptions.IgnoreCase);

    // 6. Исключение португальских слов (ão, lh, nh, õ)
    bool isPortugueseWord = Regex.IsMatch(word, @"(ão|lh|nh|õ|ç)", RegexOptions.IgnoreCase);

    // 7. Исключение французских слов (eau, aux, tion)
    bool isFrenchWord = Regex.IsMatch(word, @"(eau|aux|tion|ienne|ille)\b", RegexOptions.IgnoreCase);

    // Исключаем слова, которые явно португальские или французские
    if (isPortugueseWord || isFrenchWord)
        return false;

    // Испанское слово, если:
    return hasSpanishDiacritics ||
           hasSpanishCombinations ||
           hasSpanishSuffixes ||
           isSpanishVerb ||
           hasSpanishPrefixes;
}

private bool IsLikelyPortugueseWord(string word)
{
    if (string.IsNullOrWhiteSpace(word) || word.Length < 3)
        return false;

    // 1. Проверка на диакритику (á, à, â, ã, é, ê, í, ó, ô, õ, ú, ç)
    bool hasDiacritics = Regex.IsMatch(word, @"[áàâãéêíóôõúç]", RegexOptions.IgnoreCase);

    // 2. Проверка на характерные португальские сочетания
    bool hasPortugueseCombinations = Regex.IsMatch(word, @"lh|nh|rr|ss|gu|qu|ch", RegexOptions.IgnoreCase);

    // 3. Проверка на суффиксы
    bool hasSuffixes = Regex.IsMatch(word, @"
        (ção|cões|dade|mente|inho|inha|ável|ível|ismo|ante|agem|ente|ista|izar|osos?|osas?|ados?|adas?|idos?|idas?)\b
        ", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    // 4. Проверка на глагольные окончания
    bool isVerbForm = Regex.IsMatch(word, @"
        (ar|er|ir|or|amos|emos|imos|aram|eram|iram|ando|endo|indo|ondo)\b
        ", RegexOptions.IgnoreCase);

    // 5. Проверка на частые предлоги/артикли (короткие слова)
    bool isShortPortugueseWord = word.Length <= 3 &&
        Regex.IsMatch(word, @"^(o|a|os|as|em|de|por|que|com|não|sem|num|numa)$", RegexOptions.IgnoreCase);

    // 6. Проверка на приставки
    bool hasPrefixes = Regex.IsMatch(word, @"^(des|re|trans|super|auto|extra)", RegexOptions.IgnoreCase);

    // 7. Исключение явно французских слов (например, -tion вместо -ção)
    bool isFrenchWord = Regex.IsMatch(word, @"(tion|eau|aux|eux|ille|ienne)\b", RegexOptions.IgnoreCase);

    // Если слово содержит французские паттерны, но нет португальских эквивалентов — отфильтровываем
    if (isFrenchWord && !hasDiacritics && !hasPortugueseCombinations)
        return false;

    // Португальское слово, если:
    // - Есть диакритика ИЛИ
    // - Есть характерные сочетания ИЛИ
    // - Есть суффиксы/глагольные формы ИЛИ
    // - Это короткое служебное слово
    return hasDiacritics || hasPortugueseCombinations || hasSuffixes || isVerbForm || isShortPortugueseWord || hasPrefixes;
}
private bool IsLikelyItalianWord(string word)
{
    if (string.IsNullOrWhiteSpace(word))
        return false;

    // Короткие итальянские слова (артикли, предлоги, местоимения)
    if (word.Length <= 3)
    {
        return Regex.IsMatch(word,
            @"^(il|lo|la|i|gli|le|un|uno|una|e|a|di|da|in|con|su|per|tra|fra|mi|ti|ci|vi|si)$",
            RegexOptions.IgnoreCase);
    }

    // 1. Проверка на итальянскую диакритику (à, è, é, ì, ò, ù)
    bool hasItalianDiacritics = Regex.IsMatch(word, @"[àèéìòù]", RegexOptions.IgnoreCase);

    // 2. Характерные итальянские сочетания букв
    bool hasItalianCombinations = Regex.IsMatch(word,
        @"(gli|gn|sc[i|e|h]|ch|gh|ci|ce|gi|ge|zz|tt|cc|pp|bb|dd|ff|gg|ll|mm|nn|rr|ss|vv)",
        RegexOptions.IgnoreCase);

    // 3. Типичные итальянские суффиксы и окончания
    bool hasItalianSuffixes = Regex.IsMatch(word, @"
        (zione|tore|trice|mento|anza|essa|ino|ina|etto|etta|uccio|uccia|astro|astra|ismo|ista|abile|ibile|oso|osa|ale|are|ire)\b",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

    // 4. Глагольные окончания (-are, -ere, -ire и их формы)
    bool isItalianVerb = Regex.IsMatch(word, @"
        (are|ere|ire|ando|endo|ato|ito|avo|evo|ivo|erò|irò|erai|irai|iamo|ite|ono)\b",
        RegexOptions.IgnoreCase);

    // 5. Частые итальянские приставки
    bool hasItalianPrefixes = Regex.IsMatch(word,
        @"^(ri|re|pre|dis|sotto|sovra|stra|anti|auto|contro|extra|inter|intra|iper|macro|micro|multi|neo|para|post|proto|semi|super|tele|ultra)",
        RegexOptions.IgnoreCase);

    // 6. Исключение испанских слов (ción, ll, ñ)
    bool isSpanishWord = Regex.IsMatch(word, @"(ción|ll|ñ)", RegexOptions.IgnoreCase);

    // 7. Исключение французских слов (eau, aux, tion)
    bool isFrenchWord = Regex.IsMatch(word, @"(eau|aux|tion|ienne|ille)\b", RegexOptions.IgnoreCase);

    // Исключаем слова, которые явно испанские или французские
    if (isSpanishWord || isFrenchWord)
        return false;

    // Итальянское слово, если:
    return hasItalianDiacritics ||
           hasItalianCombinations ||
           hasItalianSuffixes ||
           isItalianVerb ||
           hasItalianPrefixes;
}

====================
private Dictionary<string, Dictionary<string, int>> languageWordFrequencies = new Dictionary<string, Dictionary<string, int>>();

    public void LoadLanguageDictionaries(string directoryPath)
    {
        var languageFiles = new Dictionary<string, string>
        {
            {"es", "es_50k.txt"},
            {"fr", "fr_50k.txt"},
            {"de", "de_50k.txt"},
            {"pt", "pt_50k.txt"},
            {"it", "it_50k.txt"},
            {"ja", "ja_50k.txt"}
        };

        foreach (var lang in languageFiles)
        {
            string filePath = Path.Combine(directoryPath, lang.Value);
            if (File.Exists(filePath))
            {
                var wordFreq = new Dictionary<string, int>();
                foreach (string line in File.ReadLines(filePath))
                {
                    var parts = line.Split(' ');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int freq))
                    {
                        wordFreq[parts[0].ToLower()] = freq;
                    }
                }
                languageWordFrequencies[lang.Key] = wordFreq;
            }
        }
    }
    if (string.IsNullOrWhiteSpace(word)) return "unknown";
    
    word = word.ToLower();
    
    // Сначала проверяем по словарям
    var langScores = new Dictionary<string, double>();
    
    foreach (var lang in languageWordFrequencies)
    {
        if (lang.Value.ContainsKey(word))
        {
            // Чем выше частота слова, тем больше баллов
            double score = Math.Log(lang.Value[word] + 1);
            langScores[lang.Key] = score;
        }
    }
    
    // Если нашли в словарях
    if (langScores.Any())
    {
        var bestLang = langScores.OrderByDescending(x => x.Value).First();
        if (bestLang.Value >= Math.Log(1000)) // Порог частоты
            return bestLang.Key;
    }
https://github.com/hermitdave/FrequencyWords

https://github.com/oprogramador/most-common-words-by-language

https://github.com/aceimnorstuvwxz/top-words
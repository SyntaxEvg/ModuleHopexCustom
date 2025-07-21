using System.Text.RegularExpressions;

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
    if (string.IsNullOrWhiteSpace(word) || word.Length < 4)
        return false;

    // Немецкие слова обычно содержат:
    // - умлауты (ä, ö, ü) и ß
    // - характерные сочетания (sch, ch, ck, tz и др.)
    // - суффиксы (ung, heit, keit, chen)
    return Regex.IsMatch(word, @"
        ^                       # Начало слова
        (?:[a-zäöüß]+           # Основная часть слова
        (?:sch|ch|ck|tz|pf|sp|st|ei|ie)  # Немецкие сочетания
        |[äöüß]                 # Или содержит умлауты/эсцет
        )+                      # Повторяем
        (?:ung|heit|keit|chen|lein|nis)\b # Суффиксы
        $                       # Конец слова
        ", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
}

private bool IsLikelySpanishWord(string word)
{
    if (string.IsNullOrWhiteSpace(word) || word.Length < 4)
        return false;

    // Испанские слова обычно:
    // - оканчиваются на характерные суффиксы
    // - содержат ударения (á, é, í, ó, ú) или ñ
    // - не содержат редкие для испанского сочетания букв
    return Regex.IsMatch(word, @"
        ^                       # Начало слова
        (?:[a-záéíóúñ]+         # Основная часть слова
        (?:ll|rr|qu|gu|gü)      # Характерные сочетания
        )?                      # Опционально
        (?:ción|dad|miento|mente|ado|ada|ando|oso|osa)\b # Суффиксы
        $                       # Конец слова
        ", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase)
        && !Regex.IsMatch(word, @"[kwxy]", RegexOptions.IgnoreCase); // Редкие буквы
}

private bool IsLikelyPortugueseWord(string word)
{
    if (string.IsNullOrWhiteSpace(word) || word.Length < 4)
        return false;

    // Португальские слова обычно:
    // - содержат ç, ã, õ и другие специфические символы
    // - имеют характерные окончания
    // - содержат носовые гласные (ã, õ)
    return Regex.IsMatch(word, @"
        ^                       # Начало слова
        (?:[a-záàâãéêíóôõúç]+   # Основная часть слова
        (?:lh|nh|rr|ss|qu|gu)   # Характерные сочетания
        )?                      # Опционально
        (?:ção|mento|dade|agem|inho|inha)\b # Суффиксы
        $                       # Конец слова
        ", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase)
        && Regex.IsMatch(word, @"[ãõç]", RegexOptions.IgnoreCase); // Должны быть специфические символы
}
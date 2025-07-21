namespace WebApiNNcallMacro
{
    public class ProfessionalLanguageDetector
    {
        private const string LanguageProfilePath = "Core14.profile.xml";
        private static readonly RankedLanguageIdentifier Identifier;


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

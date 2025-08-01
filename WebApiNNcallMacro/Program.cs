﻿using FluentAssertions.Common;
using Mega.Has.Commons;
using Mega.Has.Instrumentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("windows7.0")]
[assembly: TargetPlatform("windows7.0")]

namespace Hopex.WebService.API
{
    public class Program
    {
        private static ModuleConfiguration _moduleConfiguration;

        public static async Task Main(string[] args)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // Получаем пути и конфигурируем Serilog
                string assemblyPath = Assembly.GetEntryAssembly()?.Location ?? "";
                string appFolder = Path.GetDirectoryName(assemblyPath) ?? "";
                //string loggerPath = Path.Combine(appFolder, "logs", "logfil881288e.log");
                //string loggerPath = Path.Combine("C:\\LogSerilog", "logwapmodulee.log");
                //Log.Error("start0");
                //Log.Logger = new LoggerConfiguration()
                //    .MinimumLevel.Debug()
                //    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //    .Enrich.FromLogContext()
                //    .WriteTo.File(loggerPath, rollingInterval: RollingInterval.Day)
                //    .CreateLogger();
                _moduleConfiguration = await ModuleConfiguration.CreateAsync(args, null);
                await Task.Delay(15000);
                var builder = WebApplication.CreateBuilder(args);

                // Настройка Kestrel
                builder.WebHost
                    .UseHASInstrumentation(_moduleConfiguration)
                    .UseUrls(_moduleConfiguration.ServerInstanceUrl)
                    .UseContentRoot(_moduleConfiguration.Folder)
                    .UseKestrel(options =>
                    {
                        options.AddServerHeader = false;
                    });

                // Настройка сервисов
                ConfigureServices(builder.Services);

                var app = builder.Build();

                // Настройка middleware
                ConfigureMiddleware(app, _moduleConfiguration);

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                // Log.Error("UAS - " + ex.Message);
                PreloadLogger.LogError("UAS - " + ex.Message);
                Log.CloseAndFlush();
                throw;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IFileSystem>(new FileSystem());

            services.AddHASModule(options =>
            {
                options.AuthenticationMode = AuthenticationMode.HopexSession;
            });

            services.AddControllersWithViews()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization()
                    .AddNewtonsoftJson();

            //services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddScoped<IHeaderCollector, HeaderCollector>();
        }

        private static void ConfigureMiddleware(WebApplication app, IModuleConfiguration moduleConfiguration)
        {
            // Получаем сервис трассировки
            var traceInstrumentation = app.Services.GetRequiredService<ITraceInstrumentation>();

            app.UseSwagger();
            app.UseSwaggerUI();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/nncall/swagger/v1/swagger.json", "My API V1");
            //    c.RoutePrefix = "swagger"; // Swagger будет доступен по /nncall/swagger
            //});
            app.UseHASModule(moduleConfiguration, traceInstrumentation);

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
        }
    }
}
//=====================
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

namespace FrenchWordChecker;

class Program
{
    

    
}
namespace FrenchWordChecker;

class Program
{
    private const string DictionaryUrl = "https://www.lexique.org/databases/Lexique383/Lexique383.csv";
    private const string LocalCsvFile = "Lexique383.csv";
    private const string LocalBinFile = "Lexique383.bin";
    private static HashSet<string> _frenchWords = [];
    private static List<string> _frenchWordsList = [];

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Загрузка словаря французских слов...");
        await LoadFrenchDictionaryAsync();

        Console.WriteLine($"Словарь загружен. Всего слов: {_frenchWords.Count:N0}");

        while (true)
        {
            Console.Write("\nВведите слово или часть слова (или 'exit' для выхода): ");
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input == "exit")
                break;

            // Точное совпадение
            if (_frenchWords.Contains(input))
            {
                Console.WriteLine($"✅ Слово \"{input}\" найдено в словаре (точное совпадение).");
                continue;
            }

            // Частичное совпадение
            var matches = _frenchWordsList
                .Where(w => w.Contains(input, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();

            if (matches.Count > 0)
            {
                Console.WriteLine($"🔍 Найдено {matches.Count} совпадений по подстроке \"{input}\":");
                foreach (var match in matches)
                    Console.WriteLine($" - {match}");
            }
            else
            {
                Console.WriteLine($"❌ Ничего не найдено по подстроке \"{input}\".");
            }
        }
    }

    /// <summary>
    /// Загружает словарь из бинарного кэша или CSV.
    /// </summary>
    private static async Task LoadFrenchDictionaryAsync()
    {
        if (File.Exists(LocalBinFile))
        {
            Console.WriteLine("Загрузка словаря из бинарного кэша...");
            var json = await File.ReadAllTextAsync(LocalBinFile);
            _frenchWords = JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
            _frenchWordsList = _frenchWords.ToList();
            return;
        }

        if (!File.Exists(LocalCsvFile))
        {
            Console.WriteLine("Словарь не найден локально. Скачиваем...");
            using HttpClient client = new();
            var data = await client.GetByteArrayAsync(DictionaryUrl);
            await File.WriteAllBytesAsync(LocalCsvFile, data);
            Console.WriteLine("Словарь успешно скачан.");
        }

        Console.WriteLine("Парсинг CSV-файла...");
        using var reader = new StreamReader(LocalCsvFile);
        string? line;
        bool isFirstLine = true;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue; // пропускаем заголовок
            }

            var columns = line.Split('\t'); // CSV с табуляцией
            if (columns.Length > 0)
            {
                string word = columns[0].Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(word))
                    _frenchWords.Add(word);
            }
        }

        _frenchWordsList = _frenchWords.ToList();

        Console.WriteLine("Сохранение словаря в бинарный кэш...");
        var jsonData = JsonSerializer.Serialize(_frenchWords);
        await File.WriteAllTextAsync(LocalBinFile, jsonData);
        Console.WriteLine("Кэш сохранён.");
    }
}
================
using System.Text.Json;

namespace FrenchWordChecker;

class Program
{
    private const string FrenchDictionaryUrl = "https://www.lexique.org/databases/Lexique383/Lexique383.csv";
    private const string GermanDictionaryUrl = "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/de/de_50k.txt";

    private const string FrenchCsvFile = "Lexique383.csv";
    private const string FrenchBinFile = "Lexique383.bin";

    private const string GermanTxtFile = "de_50k.txt";
    private const string GermanBinFile = "GermanWords.bin";

    private static HashSet<string> _frenchWords = [];
    private static List<string> _frenchWordsList = [];

    private static HashSet<string> _germanWords = [];
    private static List<string> _germanWordsList = [];

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Загрузка словарей...");
        await LoadFrenchDictionaryAsync();
        await LoadGermanDictionaryAsync();

        Console.WriteLine($"Французских слов: {_frenchWords.Count:N0}");
        Console.WriteLine($"Немецких слов: {_germanWords.Count:N0}");

        while (true)
        {
            Console.Write("\nВыберите язык (fr = французский, de = немецкий, exit = выход): ");
            string? lang = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (lang == "exit")
                break;

            if (lang != "fr" && lang != "de")
            {
                Console.WriteLine("❌ Неверный выбор языка.");
                continue;
            }

            Console.Write("Введите слово или часть слова: ");
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            HashSet<string> wordSet = lang == "fr" ? _frenchWords : _germanWords;
            List<string> wordList = lang == "fr" ? _frenchWordsList : _germanWordsList;

            // Точное совпадение
            if (wordSet.Contains(input))
            {
                Console.WriteLine($"✅ Слово \"{input}\" найдено в словаре ({(lang == "fr" ? "французский" : "немецкий")}, точное совпадение).");
                continue;
            }

            // Частичное совпадение
            var matches = wordList
                .Where(w => w.Contains(input, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();

            if (matches.Count > 0)
            {
                Console.WriteLine($"🔍 Найдено {matches.Count} совпадений по подстроке \"{input}\":");
                foreach (var match in matches)
                    Console.WriteLine($" - {match}");
            }
            else
            {
                Console.WriteLine($"❌ Ничего не найдено по подстроке \"{input}\".");
            }
        }
    }

    private static async Task LoadFrenchDictionaryAsync()
    {
        if (File.Exists(FrenchBinFile))
        {
            Console.WriteLine("Загрузка французского словаря из кэша...");
            var json = await File.ReadAllTextAsync(FrenchBinFile);
            _frenchWords = JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
            _frenchWordsList = _frenchWords.ToList();
            return;
        }

        if (!File.Exists(FrenchCsvFile))
        {
            Console.WriteLine("Скачивание французского словаря...");
            using HttpClient client = new();
            var data = await client.GetByteArrayAsync(FrenchDictionaryUrl);
            await File.WriteAllBytesAsync(FrenchCsvFile, data);
        }

        Console.WriteLine("Парсинг французского словаря...");
        using var reader = new StreamReader(FrenchCsvFile);
        string? line;
        bool isFirstLine = true;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            var columns = line.Split('\t');
            if (columns.Length > 0)
            {
                string word = columns[0].Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(word))
                    _frenchWords.Add(word);
            }
        }

        _frenchWordsList = _frenchWords.ToList();

        var jsonData = JsonSerializer.Serialize(_frenchWords);
        await File.WriteAllTextAsync(FrenchBinFile, jsonData);
    }

    private static async Task LoadGermanDictionaryAsync()
    {
        if (File.Exists(GermanBinFile))
        {
            Console.WriteLine("Загрузка немецкого словаря из кэша...");
            var json = await File.ReadAllTextAsync(GermanBinFile);
            _germanWords = JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
            _germanWordsList = _germanWords.ToList();
            return;
        }

        if (!File.Exists(GermanTxtFile))
        {
            Console.WriteLine("Скачивание немецкого словаря...");
            using HttpClient client = new();
            var data = await client.GetByteArrayAsync(GermanDictionaryUrl);
            await File.WriteAllBytesAsync(GermanTxtFile, data);
        }

        Console.WriteLine("Парсинг немецкого словаря...");
        using var reader = new StreamReader(GermanTxtFile);
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            var word = line.Split(' ')[0].Trim().ToLowerInvariant(); // Берём только слово, без частоты
            if (!string.IsNullOrWhiteSpace(word))
                _germanWords.Add(word);
        }

        _germanWordsList = _germanWords.ToList();

        var jsonData = JsonSerializer.Serialize(_germanWords);
        await File.WriteAllTextAsync(GermanBinFile, jsonData);
    }
}
//=================================
using System.Text.Json;

namespace MultilangWordChecker;

class Program
{
    private static readonly Dictionary<string, string> LanguageNames = new()
    {
        { "fr", "Французский" },
        { "de", "Немецкий" },
        { "es", "Испанский" },
        { "ja", "Японский" },
        { "pt", "Португальский" }
    };

    private static readonly Dictionary<string, string> DictionaryUrls = new()
    {
        { "fr", "https://www.lexique.org/databases/Lexique383/Lexique383.csv" },
        { "de", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/de/de_50k.txt" },
        { "es", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/es/es_50k.txt" },
        { "ja", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/ja/ja_50k.txt" },
        { "pt", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/pt/pt_50k.txt" }
    };

    private static readonly Dictionary<string, string> LocalFiles = new()
    {
        { "fr", "Lexique383.csv" },
        { "de", "de_50k.txt" },
        { "es", "es_50k.txt" },
        { "ja", "ja_50k.txt" },
        { "pt", "pt_50k.txt" }
    };

    private static readonly Dictionary<string, string> CacheFiles = new()
    {
        { "fr", "Lexique383.bin" },
        { "de", "GermanWords.bin" },
        { "es", "SpanishWords.bin" },
        { "ja", "JapaneseWords.bin" },
        { "pt", "PortugueseWords.bin" }
    };

    private static readonly Dictionary<string, HashSet<string>> WordSets = new();
    private static readonly Dictionary<string, List<string>> WordLists = new();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Загрузка словарей...");

        foreach (var lang in LanguageNames.Keys)
        {
            await LoadDictionaryAsync(lang);
            Console.WriteLine($"{LanguageNames[lang]}: {WordSets[lang].Count:N0} слов");
        }

        while (true)
        {
            Console.WriteLine("\nВыберите язык:");
            foreach (var lang in LanguageNames)
                Console.WriteLine($" - {lang.Key} = {lang.Value}");

            Console.Write("Введите код языка (или 'exit' для выхода): ");
            string? langCode = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (langCode == "exit")
                break;

            if (!LanguageNames.ContainsKey(langCode))
            {
                Console.WriteLine("❌ Неверный код языка.");
                continue;
            }

            Console.Write($"Введите слово или часть слова ({LanguageNames[langCode]}): ");
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            var wordSet = WordSets[langCode];
            var wordList = WordLists[langCode];

            // 1. Точное совпадение
if (wordSet.Contains(input))
{
    Console.WriteLine($"✅ Слово \"{input}\" найдено в словаре ({LanguageNames[langCode]}, точное совпадение).");
    continue;
}

// 2. Попробуем лемматизировать
string lemma = TryLemmatize(input, langCode);
if (lemma != input && wordSet.Contains(lemma))
{
    Console.WriteLine($"✅ Слово \"{input}\" приведено к форме \"{lemma}\" и найдено в словаре.");
    continue;
}

// 3. Частичное совпадение
var matches = wordList
    .Where(w => w.Contains(input, StringComparison.OrdinalIgnoreCase))
    .Take(20)
    .ToList();

if (matches.Count > 0)
{
    Console.WriteLine($"🔍 Найдено {matches.Count} совпадений по подстроке \"{input}\":");
    foreach (var match in matches)
        Console.WriteLine($" - {match}");
}
else
{
    Console.WriteLine($"❌ Ничего не найдено по подстроке \"{input}\".");
}
        }
    }

    private static async Task LoadDictionaryAsync(string lang)
    {
        if (File.Exists(CacheFiles[lang]))
        {
            var json = await File.ReadAllTextAsync(CacheFiles[lang]);
            var words = JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
            WordSets[lang] = words;
            WordLists[lang] = words.ToList();
            return;
        }

        if (!File.Exists(LocalFiles[lang]))
        {
            Console.WriteLine($"Скачивание словаря для {LanguageNames[lang]}...");
            using HttpClient client = new();
            var data = await client.GetByteArrayAsync(DictionaryUrls[lang]);
            await File.WriteAllBytesAsync(LocalFiles[lang], data);
        }

        Console.WriteLine($"Парсинг словаря для {LanguageNames[lang]}...");
        var wordSet = new HashSet<string>();

        using var reader = new StreamReader(LocalFiles[lang]);
        string? line;
        bool isFirstLine = true;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (lang == "fr")
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var columns = line.Split('\t');
                if (columns.Length > 0)
                {
                    string word = columns[0].Trim().ToLowerInvariant();
                    if (!string.IsNullOrWhiteSpace(word))
                        wordSet.Add(word);
                }
            }
            else
            {
                var word = line.Split(' ')[0].Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(word))
                    wordSet.Add(word);
            }
        }

        WordSets[lang] = wordSet;
        WordLists[lang] = wordSet.ToList();

        var jsonData = JsonSerializer.Serialize(wordSet);
        await File.WriteAllTextAsync(CacheFiles[lang], jsonData);
    }
    private static string TryLemmatize(string word, string lang)
{
    // Простейшие правила лемматизации
    if (lang == "fr")
    {
        if (word.EndsWith("ons") || word.EndsWith("ez") || word.EndsWith("ais") || word.EndsWith("ait") || word.EndsWith("aient"))
            return word[..^3] + "er";
        if (word.EndsWith("é"))
            return word[..^1] + "er";
    }
    else if (lang == "de")
    {
        if (word.EndsWith("st") || word.EndsWith("t"))
            return word[..^1] + "en";
    }
    else if (lang == "es" || lang == "pt")
    {
        if (word.EndsWith("o") || word.EndsWith("as") || word.EndsWith("a") || word.EndsWith("amos") || word.EndsWith("an"))
        {
            var stem = word[..^1];
            return stem + "ar"; // Пробуем вернуть инфинитив
        }
    }

    return word; // если не удалось — возвращаем как есть
}
}

private static async Task<Dictionary<string, string>> ParseConlluLemmasAsync(string url)
{
    var lemmaDict = new Dictionary<string, string>();

    using HttpClient client = new();
    var content = await client.GetStringAsync(url);

    using var reader = new StringReader(content);
    string? line;
    while ((line = reader.ReadLine()) != null)
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            continue;

        var parts = line.Split('\t');
        if (parts.Length >= 3)
        {
            string form = parts[1].Trim().ToLowerInvariant();
            string lemma = parts[2].Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(form) && !string.IsNullOrWhiteSpace(lemma))
            {
                if (!lemmaDict.ContainsKey(form))
                    lemmaDict[form] = lemma;
            }
        }
    }

    return lemmaDict;
}
//====================================
///==
///using System.Text;
using System.Text.Json;

namespace MultilangWordChecker;

class Program2
{
    private static readonly Dictionary<string, string> LanguageNames = new()
    {
        { "fr", "Французский" },
        { "de", "Немецкий" },
        { "es", "Испанский" },
        { "pt", "Португальский" },
        { "ja", "Японский" }
    };

    private static readonly Dictionary<string, string> LemmaUrls = new()
    {
        { "fr", "https://raw.githubusercontent.com/UniversalDependencies/UD_French-GSD/master/fr_gsd-ud-train.conllu" },
        { "de", "https://raw.githubusercontent.com/UniversalDependencies/UD_German-GSD/master/de_gsd-ud-train.conllu" },
        { "es", "https://raw.githubusercontent.com/UniversalDependencies/UD_Spanish-GSD/master/es_gsd-ud-train.conllu" },
        { "pt", "https://raw.githubusercontent.com/UniversalDependencies/UD_Portuguese-Bosque/master/pt_bosque-ud-train.conllu" }
        // Японский не имеет лемм-файла без NLP, поэтому не добавляем
    };

    private static readonly Dictionary<string, string> WordListUrls = new()
    {
        { "fr", "https://www.lexique.org/databases/Lexique383/Lexique383.csv" },
        { "de", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/de/de_50k.txt" },
        { "es", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/es/es_50k.txt" },
        { "pt", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/pt/pt_50k.txt" },
        { "ja", "https://raw.githubusercontent.com/hermitdave/FrequencyWords/master/content/2018/ja/ja_50k.txt" }
    };

    private static readonly Dictionary<string, HashSet<string>> WordSets = new();
    private static readonly Dictionary<string, List<string>> WordLists = new();
    private static readonly Dictionary<string, Dictionary<string, string>> LemmaDictionaries = new();

    public static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("📥 Загрузка лемматизационных словарей...");

        foreach (var lang in LemmaUrls.Keys)
        {
            await LoadLemmaDictionaryAsync(lang);
        }

        Console.WriteLine("📥 Загрузка словарей слов...");

        foreach (var lang in LanguageNames.Keys)
        {
            await LoadWordListAsync(lang);
            Console.WriteLine($"✅ {LanguageNames[lang]}: {WordSets[lang].Count:N0} слов");
        }

        while (true)
        {
            Console.Write("\nВведите слово (или 'exit' для выхода): ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.ToLowerInvariant() == "exit")
                break;

            var detectedLangs = DetectLanguages(input);

            if (detectedLangs.Count == 0)
            {
                Console.WriteLine("❌ Язык не определён. Слово не найдено ни в одном словаре.");
                continue;
            }

            foreach (var lang in detectedLangs)
            {
                Console.WriteLine($"\n🔎 Язык: {LanguageNames[lang]}");

                var wordSet = WordSets[lang];
                var wordList = WordLists[lang];

                if (wordSet.Contains(input))
                {
                    Console.WriteLine($"✅ Точное совпадение: \"{input}\" найдено.");
                    continue;
                }

                if (lang != "ja") // Японский без лемм
                {
                    string lemma = TryLemmatize(input.ToLowerInvariant(), lang);
                    if (lemma != input && wordSet.Contains(lemma))
                    {
                        Console.WriteLine($"✅ Лемма \"{lemma}\" найдена для формы \"{input}\".");
                        continue;
                    }
                }

                var matches = wordList
                    .Where(w => w.Contains(input, StringComparison.OrdinalIgnoreCase))
                    .Take(20)
                    .ToList();

                if (matches.Count > 0)
                {
                    Console.WriteLine($"🔍 Найдено {matches.Count} совпадений по подстроке:");
                    foreach (var match in matches)
                        Console.WriteLine($" - {match}");
                }
                else
                {
                    Console.WriteLine("❌ Ничего не найдено.");
                }
            }
        }
    }

    private static List<string> DetectLanguages(string word)
    {
        var langs = new List<string>();
        foreach (var lang in LanguageNames.Keys)
        {
            if (WordSets[lang].Contains(word.ToLowerInvariant()) ||
                (lang != "ja" && LemmaDictionaries.TryGetValue(lang, out var dict) && dict.ContainsKey(word.ToLowerInvariant())))
            {
                langs.Add(lang);
            }
        }
        return langs;
    }

    private static string TryLemmatize(string word, string lang)
    {
        if (LemmaDictionaries.TryGetValue(lang, out var dict))
        {
            if (dict.TryGetValue(word, out var lemma))
                return lemma;
        }
        return word;
    }

    private static async Task LoadLemmaDictionaryAsync(string lang)
    {
        string fileName = $"{lang}_lemmas.txt";
        var lemmaDict = new Dictionary<string, string>();

        if (!File.Exists(fileName))
        {
            if (LemmaUrls.TryGetValue(lang, out var url))
            {
                Console.WriteLine($"📥 Скачивание лемм для {LanguageNames[lang]}...");
                using HttpClient client = new();
                var content = await client.GetStringAsync(url);

                using var writer = new StreamWriter(fileName);
                using var reader = new StringReader(content);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split('\t');
                    if (parts.Length >= 3)
                    {
                        string form = parts[1].Trim().ToLowerInvariant();
                        string lemma = parts[2].Trim().ToLowerInvariant();
                        if (!string.IsNullOrWhiteSpace(form) && !string.IsNullOrWhiteSpace(lemma))
                        {
                            if (!lemmaDict.ContainsKey(form))
                                lemmaDict[form] = lemma;

                            await writer.WriteLineAsync($"{form}\t{lemma}");
                        }
                    }
                }
            }
        }
        else
        {
            using var reader = new StreamReader(fileName);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split('\t');
                if (parts.Length == 2)
                {
                    string form = parts[0].Trim().ToLowerInvariant();
                    string lemma = parts[1].Trim().ToLowerInvariant();
                    if (!string.IsNullOrWhiteSpace(form) && !string.IsNullOrWhiteSpace(lemma))
                        lemmaDict[form] = lemma;
                }
            }
        }

        LemmaDictionaries[lang] = lemmaDict;
    }

    private static async Task LoadWordListAsync(string lang)
    {
        string fileName = $"{lang}_words.txt";
        var wordSet = new HashSet<string>();

        if (!File.Exists(fileName))
        {
            if (WordListUrls.TryGetValue(lang, out var url))
            {
                Console.WriteLine($"📥 Скачивание словаря для {LanguageNames[lang]}...");
                using HttpClient client = new();
                var data = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(fileName, data);
            }
        }

        using var reader = new StreamReader(fileName);
        string? line;
        bool isFirstLine = true;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (lang == "fr")
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var columns = line.Split('\t');
                if (columns.Length > 0)
                {
                    string word = columns[0].Trim().ToLowerInvariant();
                    if (!string.IsNullOrWhiteSpace(word))
                        wordSet.Add(word);
                }
            }
            else
            {
                var word = line.Split(' ')[0].Trim();
                if (!string.IsNullOrWhiteSpace(word))
                    wordSet.Add(word);
            }
        }

        WordSets[lang] = wordSet;
        WordLists[lang] = wordSet.ToList();
    }
}
//==claude35
using System.Text.Json;
using System.Net.Http;
using NAudio.Wave;
using System.Speech.Synthesis;
using Google.Cloud.Speech.V1;

namespace MultilingualSpeechProcessor;

public class Program2
{
    private readonly Dictionary<string, LanguageInfo> _languages;
    private readonly Dictionary<string, Dictionary<string, string>> _lemmaDictionaries;
    private readonly HttpClient _httpClient;
    private readonly SpeechSynthesizer _synthesizer;
    private readonly string _cachePath = "cache/lemmas";

    public Program2()
    {
        _httpClient = new HttpClient();
        _lemmaDictionaries = new Dictionary<string, Dictionary<string, string>>();
        _synthesizer = new SpeechSynthesizer();

        // Инициализация поддерживаемых языков и их ресурсов
        _languages = new Dictionary<string, LanguageInfo>
        {
            ["fr"] = new LanguageInfo
            {
                Name = "French",
                Code = "fr-FR",
                Urls = new[]
                {
                    "https://raw.githubusercontent.com/SamuelLarkin/French-Lemmatizer/master/dict.csv",
                    "https://raw.githubusercontent.com/clips/pattern/master/pattern/text/fr/fr-lexicon.txt"
                }
            },
            ["es"] = new LanguageInfo
            {
                Name = "Spanish",
                Code = "es-ES",
                Urls = new[]
                {
                    "https://raw.githubusercontent.com/michmech/lemmatization-lists/master/spanish.txt",
                    "https://raw.githubusercontent.com/clips/pattern/master/pattern/text/es/es-lexicon.txt"
                }
            },
            ["de"] = new LanguageInfo
            {
                Name = "German",
                Code = "de-DE",
                Urls = new[]
                {
                    "https://raw.githubusercontent.com/michmech/lemmatization-lists/master/german.txt"
                }
            },
            ["ja"] = new LanguageInfo
            {
                Name = "Japanese",
                Code = "ja-JP",
                Urls = new[]
                {
                    "https://raw.githubusercontent.com/himkt/japanese-lemmatization/master/data/lemmatization.txt",
                    "https://raw.githubusercontent.com/WorksApplications/SudachiDict/main/src/main/text/small_lex.csv"
                },
                RequiresSpecialProcessing = true
            }
        };

        Initialize();
    }

    private record LanguageInfo
    {
        public required string Name { get; init; }
        public required string Code { get; init; }
        public required string[] Urls { get; init; }
        public bool RequiresSpecialProcessing { get; init; }
    }

    private async Task Initialize()
    {
        Console.WriteLine("Инициализация системы...");

        // Создаем директории для кэша
        Directory.CreateDirectory(_cachePath);

        // Загружаем словари лемм
        await LoadAllLemmaDictionariesAsync();

        Console.WriteLine("Система инициализирована и готова к работе");
    }

    private async Task LoadAllLemmaDictionariesAsync()
    {
        foreach (var (langCode, langInfo) in _languages)
        {
            await LoadLemmaDictionaryAsync(langCode, langInfo);
        }
    }

    private async Task LoadLemmaDictionaryAsync(string langCode, LanguageInfo langInfo)
    {
        var cacheFile = Path.Combine(_cachePath, $"{langCode}_combined.json");

        // Пробуем загрузить из кэша
        if (File.Exists(cacheFile))
        {
            try
            {
                var json = await File.ReadAllTextAsync(cacheFile);
                _lemmaDictionaries[langCode] = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
                Console.WriteLine($"✓ Загружен кэш для {langInfo.Name}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Ошибка загрузки кэша для {langCode}: {ex.Message}");
            }
        }

        var combinedLemmas = new Dictionary<string, string>();

        // Скачиваем и обрабатываем все ресурсы для языка
        for (int i = 0; i < langInfo.Urls.Length; i++)
        {
            var url = langInfo.Urls[i];
            var dictFile = Path.Combine(_cachePath, $"{langCode}_dict_{i}.txt");

            if (await DownloadLemmaDictionaryAsync(langCode, url, i, dictFile))
            {
                var newLemmas = await ProcessLemmaFileAsync(dictFile, langInfo.RequiresSpecialProcessing);
                foreach (var (word, lemma) in newLemmas)
                {
                    combinedLemmas[word] = lemma;
                }
                Console.WriteLine($"Добавлено {newLemmas.Count:N0} лемм из ресурса {i + 1}");
            }
        }

        _lemmaDictionaries[langCode] = combinedLemmas;

        // Сохраняем в кэш
        try
        {
            var json = JsonSerializer.Serialize(combinedLemmas, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(cacheFile, json);
            Console.WriteLine($"✓ Кэш создан для {langInfo.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Ошибка создания кэша для {langCode}: {ex.Message}");
        }
    }

    private async Task<bool> DownloadLemmaDictionaryAsync(string langCode, string url, int index, string outputFile)
    {
        try
        {
            if (File.Exists(outputFile))
            {
                return true;
            }

            Console.WriteLine($"\nСкачивание словаря для {_languages[langCode].Name}...");
            
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var buffer = new byte[8192];
            var bytesRead = 0L;

            using var sourceStream = await response.Content.ReadAsStreamAsync();
            using var targetStream = File.Create(outputFile);

            while (true)
            {
                var read = await sourceStream.ReadAsync(buffer);
                if (read == 0)
                    break;

                await targetStream.WriteAsync(buffer.AsMemory(0, read));
                bytesRead += read;

                if (totalBytes > 0)
                {
                    var percentage = (int)((bytesRead * 100) / totalBytes);
                    Console.Write($"\rПрогресс: {percentage}% ({bytesRead:N0}/{totalBytes:N0} байт)");
                }
            }

            Console.WriteLine($"\n✓ Словарь для {_languages[langCode].Name} успешно скачан");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✕ Ошибка при скачивании словаря {langCode}: {ex.Message}");
            return false;
        }
    }

    private async Task<Dictionary<string, string>> ProcessLemmaFileAsync(string filename, bool isJapanese)
    {
        var lemmas = new Dictionary<string, string>();

        try
        {
            var lines = await File.ReadAllLinesAsync(filename);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (isJapanese)
                {
                    ProcessJapaneseLine(line, lemmas);
                    continue;
                }

                var parts = line.Split(new[] { '\t', ',', ' ' }, 2);
                if (parts.Length >= 2)
                {
                    var word = parts[0].ToLowerInvariant();
                    var lemma = parts[1].ToLowerInvariant();
                    lemmas[word] = lemma;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке файла {filename}: {ex.Message}");
        }

        return lemmas;
    }

    private void ProcessJapaneseLine(string line, Dictionary<string, string> lemmas)
    {
        try
        {
            // Специальная обработка для японского языка
            var parts = line.Split('\t');
            if (parts.Length >= 2)
            {
                var word = parts[0].Trim();
                var lemma = parts[1].Trim();

                // Добавляем как оригинальное написание, так и возможные чтения
                lemmas[word] = lemma;

                // Если есть дополнительные чтения в других частях
                if (parts.Length > 2)
                {
                    var readings = parts[2].Split(',');
                    foreach (var reading in readings)
                    {
                        if (!string.IsNullOrWhiteSpace(reading))
                        {
                            lemmas[reading.Trim()] = lemma;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке японской строки: {ex.Message}");
        }
    }

   private record WordAnalysisResult
    {
        public required string Word { get; init; }
        public required string LanguageCode { get; init; }
        public required string Lemma { get; init; }
        public required double Confidence { get; init; }
        public required List<(string Word, double Similarity)> SimilarWords { get; init; }
    }

    private record SpeechRecognitionResult
    {
        public required string Text { get; init; }
        public required string LanguageCode { get; init; }
        public required double Confidence { get; init; }
        public required List<WordAnalysisResult> WordAnalysis { get; init; }
    }

    public async Task RunAsync()
    {
        Console.WriteLine("\n=== Многоязычная система распознавания и анализа речи ===");
        Console.WriteLine("Поддерживаемые языки:");
        foreach (var (code, info) in _languages)
        {
            Console.WriteLine($"• {info.Name} ({code})");
        }

        while (true)
        {
            try
            {
                await ProcessSpeechAsync();
                await Task.Delay(500); // Небольшая пауза между итерациями
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nЗавершение работы...");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                continue;
            }
        }
    }

    private async Task ProcessSpeechAsync()
    {
        Console.WriteLine("\nСлушаю... Говорите на любом поддерживаемом языке");
        
        try
        {
            using var waveIn = new WaveInEvent();
            using var waveFile = new WaveFileWriter("temp.wav", waveIn.WaveFormat);
            
            var recordingCompleted = new TaskCompletionSource<bool>();
            
            waveIn.DataAvailable += (s, e) =>
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            };

            waveIn.RecordingStopped += (s, e) =>
            {
                waveFile.Dispose();
                recordingCompleted.SetResult(true);
            };

            // Начинаем запись
            waveIn.StartRecording();

            // Ждем 5 секунд или пока не будет обнаружена тишина
            await Task.WhenAny(
                recordingCompleted.Task,
                Task.Delay(TimeSpan.FromSeconds(5))
            );

            waveIn.StopRecording();

            // Распознаем речь
            var recognitionResults = await RecognizeSpeechAsync("temp.wav");
            
            foreach (var result in recognitionResults)
            {
                await ProcessRecognitionResultAsync(result);
            }
        }
        finally
        {
            if (File.Exists("temp.wav"))
            {
                File.Delete("temp.wav");
            }
        }
    }

    private async Task<List<SpeechRecognitionResult>> RecognizeSpeechAsync(string audioFile)
    {
        var results = new List<SpeechRecognitionResult>();
        
        var speech = SpeechClient.Create();
        
        foreach (var (langCode, langInfo) in _languages)
        {
            try
            {
                var audio = await Google.Cloud.Speech.V1.RecognitionAudio.FromFileAsync(audioFile);
                var config = new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 16000,
                    LanguageCode = langInfo.Code,
                    EnableWordConfidence = true
                };

                var response = await speech.RecognizeAsync(config, audio);

                foreach (var result in response.Results)
                {
                    foreach (var alternative in result.Alternatives)
                    {
                        var wordAnalysis = await AnalyzeWordsAsync(alternative.Words, langCode);
                        
                        results.Add(new SpeechRecognitionResult
                        {
                            Text = alternative.Transcript,
                            LanguageCode = langCode,
                            Confidence = alternative.Confidence,
                            WordAnalysis = wordAnalysis
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при распознавании {langInfo.Name}: {ex.Message}");
            }
        }

        return results;
    }

    private async Task<List<WordAnalysisResult>> AnalyzeWordsAsync(
        IEnumerable<WordInfo> words, 
        string languageCode)
    {
        var results = new List<WordAnalysisResult>();

        foreach (var wordInfo in words)
        {
            var word = wordInfo.Word.ToLowerInvariant();
            var lemma = await GetLemmaAsync(word, languageCode);
            var similarWords = await FindSimilarWordsAsync(word, languageCode);

            results.Add(new WordAnalysisResult
            {
                Word = word,
                LanguageCode = languageCode,
                Lemma = lemma,
                Confidence = wordInfo.Confidence,
                SimilarWords = similarWords
            });
        }

        return results;
    }

    private async Task<string> GetLemmaAsync(string word, string languageCode)
    {
        if (_lemmaDictionaries.TryGetValue(languageCode, out var dictionary))
        {
            return dictionary.GetValueOrDefault(word, word);
        }
        return word;
    }

    private async Task<List<(string Word, double Similarity)>> FindSimilarWordsAsync(
        string word, 
        string languageCode)
    {
        var similarWords = new List<(string Word, double Similarity)>();

        if (_lemmaDictionaries.TryGetValue(languageCode, out var dictionary))
        {
            foreach (var dictWord in dictionary.Keys)
            {
                var similarity = CalculateSimilarity(word, dictWord);
                if (similarity > 0.8)
                {
                    similarWords.Add((dictWord, similarity));
                }
            }
        }

        return similarWords
            .OrderByDescending(x => x.Similarity)
            .Take(5)
            .ToList();
    }

    private double CalculateSimilarity(string word1, string word2)
    {
        if (string.IsNullOrEmpty(word1) || string.IsNullOrEmpty(word2))
            return 0;

        var distance = LevenshteinDistance(word1, word2);
        var maxLength = Math.Max(word1.Length, word2.Length);
        return 1 - ((double)distance / maxLength);
    }

    private int LevenshteinDistance(string word1, string word2)
    {
        var matrix = new int[word1.Length + 1, word2.Length + 1];

        for (int i = 0; i <= word1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= word2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= word1.Length; i++)
        {
            for (int j = 1; j <= word2.Length; j++)
            {
                var cost = word1[i - 1] == word2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        return matrix[word1.Length, word2.Length];
    }

    private async Task ProcessRecognitionResultAsync(SpeechRecognitionResult result)
    {
        var langInfo = _languages[result.LanguageCode];
        
        Console.WriteLine($"\nРаспознано на {langInfo.Name} (уверенность: {result.Confidence:P2}):");
        Console.WriteLine($"Текст: {result.Text}");

        foreach (var wordAnalysis in result.WordAnalysis)
        {
            Console.WriteLine($"\nАнализ слова: {wordAnalysis.Word}");
            Console.WriteLine($"Начальная форма: {wordAnalysis.Lemma}");
            Console.WriteLine($"Уверенность: {wordAnalysis.Confidence:P2}");

            if (wordAnalysis.SimilarWords.Any())
            {
                Console.WriteLine("Похожие слова:");
                foreach (var (similarWord, similarity) in wordAnalysis.SimilarWords)
                {
                    Console.WriteLine($"  • {similarWord} (схожесть: {similarity:P2})");
                }
            }
        }

        // Произносим распознанный текст
        _synthesizer.SelectVoiceByHints(
            VoiceGender.NotSet, 
            VoiceAge.NotSet, 
            0, 
            new CultureInfo(langInfo.Code)
        );
        _synthesizer.SpeakAsync(result.Text);
    }

    public static async Task Main(string[] args)
    {
        var program = new Program2();
        await program.RunAsync();
    }
}
//==================== bez ineta
Вы правы, давайте сделаем версию без зависимости от облачных сервисов. Можно использовать Vosk для офлайн-распознавания речи:

```csharp
using NAudio.Wave;
using System.Text.Json;
using Vosk;
using System.Speech.Synthesis;

namespace OfflineSpeechProcessor;

public class Program2
{
    private readonly Dictionary<string, LanguageInfo> _languages;
    private readonly Dictionary<string, Dictionary<string, string>> _lemmaDictionaries;
    private readonly Dictionary<string, Model> _speechModels;
    private readonly SpeechSynthesizer _synthesizer;
    private readonly string _cachePath = "cache/lemmas";
    private readonly string _modelsPath = "models";

    public Program2()
    {
        _lemmaDictionaries = new Dictionary<string, Dictionary<string, string>>();
        _speechModels = new Dictionary<string, Model>();
        _synthesizer = new SpeechSynthesizer();

        // Инициализация поддерживаемых языков
        _languages = new Dictionary<string, LanguageInfo>
        {
            ["fr"] = new LanguageInfo
            {
                Name = "French",
                Code = "fr-FR",
                ModelName = "vosk-model-fr-0.22",
                DictionaryFiles = new[] { "french_lemmas.txt" }
            },
            ["es"] = new LanguageInfo
            {
                Name = "Spanish",
                Code = "es-ES",
                ModelName = "vosk-model-es-0.42",
                DictionaryFiles = new[] { "spanish_lemmas.txt" }
            },
            ["de"] = new LanguageInfo
            {
                Name = "German",
                Code = "de-DE",
                ModelName = "vosk-model-de-0.21",
                DictionaryFiles = new[] { "german_lemmas.txt" }
            },
            ["ja"] = new LanguageInfo
            {
                Name = "Japanese",
                Code = "ja-JP",
                ModelName = "vosk-model-ja-0.22",
                DictionaryFiles = new[] { "japanese_lemmas.txt" },
                RequiresSpecialProcessing = true
            }
        };

        Initialize();
    }

    private record LanguageInfo
    {
        public required string Name { get; init; }
        public required string Code { get; init; }
        public required string ModelName { get; init; }
        public required string[] DictionaryFiles { get; init; }
        public bool RequiresSpecialProcessing { get; init; }
    }

    private void Initialize()
    {
        Console.WriteLine("Инициализация системы...");

        // Создаем необходимые директории
        Directory.CreateDirectory(_cachePath);
        Directory.CreateDirectory(_modelsPath);

        // Загружаем словари лемм
        LoadAllLemmaDictionaries();

        // Инициализируем модели распознавания речи
        InitializeSpeechModels();

        Console.WriteLine("Система инициализирована и готова к работе");
    }

    private void LoadAllLemmaDictionaries()
    {
        foreach (var (langCode, langInfo) in _languages)
        {
            LoadLemmaDictionary(langCode, langInfo);
        }
    }

    private void LoadLemmaDictionary(string langCode, LanguageInfo langInfo)
    {
        var cacheFile = Path.Combine(_cachePath, $"{langCode}_combined.json");

        // Пробуем загрузить из кэша
        if (File.Exists(cacheFile))
        {
            try
            {
                var json = File.ReadAllText(cacheFile);
                _lemmaDictionaries[langCode] = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
                Console.WriteLine($"✓ Загружен кэш для {langInfo.Name}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Ошибка загрузки кэша для {langCode}: {ex.Message}");
            }
        }

        var combinedLemmas = new Dictionary<string, string>();

        // Загружаем леммы из локальных файлов
        foreach (var dictionaryFile in langInfo.DictionaryFiles)
        {
            var filePath = Path.Combine(_cachePath, dictionaryFile);
            if (File.Exists(filePath))
            {
                var newLemmas = ProcessLemmaFile(filePath, langInfo.RequiresSpecialProcessing);
                foreach (var (word, lemma) in newLemmas)
                {
                    combinedLemmas[word] = lemma;
                }
                Console.WriteLine($"Добавлено {newLemmas.Count:N0} лемм из файла {dictionaryFile}");
            }
            else
            {
                Console.WriteLine($"⚠ Файл словаря {dictionaryFile} не найден");
            }
        }

        _lemmaDictionaries[langCode] = combinedLemmas;

        // Сохраняем в кэш
        try
        {
            var json = JsonSerializer.Serialize(combinedLemmas, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(cacheFile, json);
            Console.WriteLine($"✓ Кэш создан для {langInfo.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Ошибка создания кэша для {langCode}: {ex.Message}");
        }
    }

    private Dictionary<string, string> ProcessLemmaFile(string filename, bool isJapanese)
    {
        var lemmas = new Dictionary<string, string>();

        try
        {
            var lines = File.ReadAllLines(filename);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (isJapanese)
                {
                    ProcessJapaneseLine(line, lemmas);
                    continue;
                }

                var parts = line.Split(new[] { '\t', ',', ' ' }, 2);
                if (parts.Length >= 2)
                {
                    var word = parts[0].ToLowerInvariant();
                    var lemma = parts[1].ToLowerInvariant();
                    lemmas[word] = lemma;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке файла {filename}: {ex.Message}");
        }

        return lemmas;
    }

    private void ProcessJapaneseLine(string line, Dictionary<string, string> lemmas)
    {
        try
        {
            var parts = line.Split('\t');
            if (parts.Length >= 2)
            {
                var word = parts[0].Trim();
                var lemma = parts[1].Trim();
                lemmas[word] = lemma;

                if (parts.Length > 2)
                {
                    var readings = parts[2].Split(',');
                    foreach (var reading in readings)
                    {
                        if (!string.IsNullOrWhiteSpace(reading))
                        {
                            lemmas[reading.Trim()] = lemma;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке японской строки: {ex.Message}");
        }
    }
=============
static bool IsGarbage(string text)
    {
        // Разрешённые символы (латиница, кириллица, языковые символы, цифры, пробелы)
        string allowedChars = 
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
            "éèêëàâùûüÿçîïôœæÉÈÊËÀÂÙÛÜŸÇÎÏÔŒÆäöüßÄÖÜáéíóúñÁÉÍÓÚÑüÜáéíóúâêîôûãõàçÁÉÍÓÚÂÊÎÔÛÃÕÀÇāīūēōĀĪŪĒŌàèéìíîòóùúÀÈÉÌÍÎÒÓÙÚ" +
            "0123456789 ";

        // Запрещённые символы:
        // 1. Управляющие Unicode-символы (U+0000 - U+001F, U+007F - U+009F)
        // 2. Битые UTF-8 последовательности (например, \u0081, \u0090)
        // 3. Смешанная латиница + кириллица в одном слове (например, "Рu0090")
        bool hasForbiddenUnicode = text.Any(c => 
            (c >= '\u0000' && c <= '\u001F') ||  // Управляющие символы
            (c >= '\u007F' && c <= '\u009F') ||  // Доп. управляющие символы
            (c >= '\uFF00' && c <= '\uFFFF'));   // Спецсимволы из "Fullwidth" диапазона

        bool hasMixedScripts = Regex.IsMatch(text, @"\p{IsCyrillic}.*[a-zA-Z]|[a-zA-Z].*\p{IsCyrillic}");

        // Если есть запрещённые символы или смешение алфавитов — сразу мусор
        if (hasForbiddenUnicode || hasMixedScripts)
            return true;

        // Проверяем долю "нормальных" символов
        int normalCount = text.Count(c => allowedChars.Contains(c));
        return (normalCount * 2 < text.Length);
    }
    //==============
    using System;
using System.Text;
using System.Linq;

class Program
{
    static void Main()
    {
        string corruptedText = "РџСЂРёРІРµС‚"; // Пример искажённого текста (замени на свой)
        byte[] originalBytes = Encoding.UTF8.GetBytes(corruptedText); // Или другой исходной кодировки

        string[] encodings = new[]
        {
            "WINDOWS-1251", "KOI8-R", "KOI8-U", "ISO-8859-5", "CP855", "CP866", "GB2312", "UTF-16",
            "SJIS", "ISO-2022-JP", "CP437", "CP850", "CP852", "ISO-8859-1", "ISO-8859-2", "ISO-8859-3",
            "ISO-8859-4", "ISO-8859-6", "ISO-8859-7", "ISO-8859-8", "ISO-8859-9", "ISO-8859-13",
            "ISO-8859-15", "WINDOWS-1250", "WINDOWS-1252", "WINDOWS-1253", "WINDOWS-1254", "WINDOWS-1255",
            "WINDOWS-1256", "WINDOWS-1257", "WINDOWS-1258"
        };

        bool found = false;

        // Перебор 1-го поколения
        foreach (var enc1 in encodings)
        {
            try
            {
                string decoded = Encoding.GetEncoding(enc1).GetString(originalBytes);
                if (ContainsCyrillic(decoded))
                {
                    Console.WriteLine($"1 поколение: {enc1} -> {decoded}");
                    found = true;
                    break;
                }
            }
            catch { }

            if (found) break;

            // Перебор 2-го поколения
            foreach (var enc2 in encodings.Where(e => e != enc1))
            {
                try
                {
                    byte[] bytes2 = Encoding.GetEncoding(enc1).GetBytes(corruptedText);
                    string decoded2 = Encoding.GetEncoding(enc2).GetString(bytes2);
                    if (ContainsCyrillic(decoded2))
                    {
                        Console.WriteLine($"2 поколения: {enc1} -> {enc2} -> {decoded2}");
                        found = true;
                        break;
                    }
                }
                catch { }
            }

            if (found) break;

            // Перебор 3-го поколения
            foreach (var enc2 in encodings.Where(e => e != enc1))
            {
                foreach (var enc3 in encodings.Where(e => e != enc1 && e != enc2))
                {
                    try
                    {
                        byte[] bytes2 = Encoding.GetEncoding(enc1).GetBytes(corruptedText);
                        byte[] bytes3 = Encoding.GetEncoding(enc2).GetBytes(Encoding.GetEncoding(enc2).GetString(bytes2));
                        string decoded3 = Encoding.GetEncoding(enc3).GetString(bytes3);
                        if (ContainsCyrillic(decoded3))
                        {
                            Console.WriteLine($"3 поколения: {enc1} -> {enc2} -> {enc3} -> {decoded3}");
                            found = true;
                            break;
                        }
                    }
                    catch { }
                }
                if (found) break;
            }
            if (found) break;
        }

        if (!found)
            Console.WriteLine("Кириллица не найдена ни в одной комбинации.");
    }

    static bool ContainsCyrillic(string text)
    {
        return text.Any(c => c >= 'А' && c <= 'я');
    }
}
string input = "abcdefg"; // Пример строки для разбивки
        List<string> pairs = new List<string>();

        // Проверка на null или пустую строку
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Строка пуста или null.");
            return;
        }

        int i = 0;
        for (; i < input.Length - 1; i += 2)
        {
            string pair = input.Substring(i, 2);
            pairs.Add(pair);
        }

        // Проверка на нечётную длину и обработка последнего символа
        if (input.Length % 2 != 0)
        {
            string lastChar = input.Substring(input.Length - 1, 1);
            Console.WriteLine("Последний одиночный символ: " + lastChar);
        }

        // Вывод пар
        Console.WriteLine("Пары символов:");
        foreach (string pair in pairs)
        {
            Console.WriteLine(pair);
        }
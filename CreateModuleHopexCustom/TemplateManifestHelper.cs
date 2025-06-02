using HAS.Extensions.Package.DependenciesResolving.Domain.ValueObjects;
using Mega.Extensions.Packages;
using System.IO.Abstractions;
using System.Text.Json;

namespace CreateModuleHopexCustom
{
    public class TemplateManifestHelper
    {
        private string manifestFilePath;

        public bool isFindTemplateManifestFile()
        {
            try
            {
                // Получаем путь к папке, где находится исполняемый файл
                string basePath = AppContext.BaseDirectory;

                // Путь к папке TemplateCreateManifest
                string manifestFolderPath = Path.Combine(basePath, "TemplateCreateManifest");

                // Проверяем, существует ли папка
                if (!Directory.Exists(manifestFolderPath))
                {
                    Console.Error.WriteLine($"Папка не найдена: {manifestFolderPath}");
                    return false;
                }

                // Путь к файлу TemplateCreateManifest.json
                string manifestFilePath = Path.Combine(manifestFolderPath, "TemplateCreateManifest.json");

                // Проверяем, существует ли файл
                if (!File.Exists(manifestFilePath))
                {
                    Console.Error.WriteLine($"Файл не найден: {manifestFilePath}");
                    return false;
                }

                Console.WriteLine($"Файл найден: {manifestFilePath}");
                this.manifestFilePath = manifestFilePath;
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Ошибка при поиске файла: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Читает содержимое JSON-файла как строку
        /// </summary>
        public string ReadManifestAsString()
        {
            try
            {
                

                string content = File.ReadAllText(manifestFilePath);
                Console.WriteLine("✅ Файл успешно прочитан как строка.");
                return content;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Ошибка при чтении файла: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Десериализует JSON-файл в объект типа T
        /// </summary>
        public ModuleManifest ReadManifestAsObject()
        {
            try
            {
                ModuleManifest moduleManifest;
                using (var reader = File.OpenRead(manifestFilePath))
                {
                    moduleManifest = ModuleManifest.ReadFrom(reader);
                }
                Console.WriteLine("JSON успешно десериализован.");
                return moduleManifest;








                //string json = File.ReadAllText(manifestFilePath);

                //T result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                //{
                //    PropertyNameCaseInsensitive = true,
                //    WriteIndented = true
                //});

             
                //return result;
            }
            catch (JsonException jex)
            {
                Console.Error.WriteLine($"Ошибка десериализации JSON: {jex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ошибка при чтении/десериализации файла: {ex.Message}");
                return default;
            }
        }
    }
}

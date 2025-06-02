using CreateModuleHopexCustom;
using CSharpFunctionalExtensions;
using HAS.Extensions.Package.DependenciesResolving.Domain.ValueObjects;
using Mega.Extensions.Packages;
using Mega.Has.Commons;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

internal class Program
{
    private static string? inputFolder;
    private static string? outputFolder;
    private static string checksum;
    private static ModuleManifest templateManifestData;
    private static string destinationRootFolder;

    private static void Main(string[] args)
    {
        // начнем с простого, получаем папку с файлами для создания модуля
        //получаем выходной папку,
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Чтение значений из конфигурации
        inputFolder = configuration["InputFolder"];
        outputFolder = configuration["OutputFolder"];

        Console.WriteLine($"Input Folder: {inputFolder}");
        Console.WriteLine($"Output Folder: {outputFolder}");


        OpenManifistTemplate();
        CopyDirectoryPreserveStructure();
        CreateHasManifist();
        CompressDirectoriesAndFilesInPackage();

    }

    private static void CompressDirectoriesAndFilesInPackage()
    {
        string zipnew = string.Concat(destinationRootFolder, ".haspkg");
        try
        {
            //if (!Directory.Exists(destinationRootFolder))
            //{
            //    Console.Error.WriteLine($"Папка не найдена: {destinationRootFolder}");
            //    return;
            //}

            // Удаляем zip-файл, если он уже существует
            if (File.Exists(zipnew))
            {
                File.Delete(zipnew);
            }

            using (FileStream zipToOpen = new FileStream(zipnew, FileMode.CreateNew))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create, false))
            {
                var allFiles = Directory.GetFiles(destinationRootFolder, "*", SearchOption.AllDirectories);

                foreach (var filePath in allFiles)
                {
                    // Получаем относительный путь файла от исходной папки
                    string relativePath = Path.GetRelativePath(destinationRootFolder, filePath);

                    // Добавляем файл в архив с сохранением структуры
                    archive.CreateEntryFromFile(filePath, relativePath);
                    Console.WriteLine($"✅ Добавлено в архив: {relativePath}");
                }
            }

            Console.WriteLine($"Архив успешно создан: {zipnew}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Ошибка при создании архива: {ex.Message}");
        } 
    }
    
    /// <summary>
    /// Копирует все файлы и папки из исходной директории в новую директорию с сохранением структуры.
    /// </summary>
    /// <param name="sourceDirectory">Исходная директория</param>
    /// <param name="outputFolderName">Имя новой выходной папки (создаётся в той же директории, где sourceDirectory)</param>
    public static bool CopyDirectoryPreserveStructure()
    {
        try
        {
            string sourceDirectory = inputFolder;
            var file = string.Concat(templateManifestData.Id, "-", Path.GetRandomFileName());
            destinationRootFolder =  Path.Combine(outputFolder, file);
            if (!Directory.Exists(sourceDirectory))
            {
                Console.Error.WriteLine($"Исходная папка не найдена: {sourceDirectory}");
                return false;
            }
            if (!Directory.Exists(destinationRootFolder))
            {
                Console.Error.WriteLine($"Конечная папка не найдена: {sourceDirectory}");
                Directory.CreateDirectory(destinationRootFolder);
            }

            
            //// Путь к новой выходной папке
            //string destinationRoot = Path.Combine(parentDir, outputFolderName);

            //// Создаём корневую целевую папку, если её нет
            //Directory.CreateDirectory(destinationRoot);

            // Получаем все файлы из исходной папки и подпапок
            var allFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);

            foreach (var filePath in allFiles)
            {
                // Получаем относительный путь файла от исходной папки
                string relativePath = Path.GetRelativePath(sourceDirectory, filePath);

                // Строим путь назначения
                string destinationPath = Path.Combine(destinationRootFolder, relativePath);

                // Создаём директорию назначения, если её нет
                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                // Копируем файл
                File.Copy(filePath, destinationPath, overwrite: true);
                //Console.WriteLine($"Скопирован: {relativePath}");
            }

            Console.WriteLine($"Все файлы успешно скопированы в: {destinationRootFolder}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка при копировании: {ex.Message}");
            return false ;
        }
        return true;
    }


    private static void CreateHasManifist()
    {
        //считаем всю сумму файлов
        ChecksumCalculator helper = new ChecksumCalculator(IoHelper.DefaultFileSystem, Array.Empty<string>());
        checksum = helper.GetChecksum(destinationRootFolder);
            templateManifestData.Checksum = checksum;
           // templateManifestData.Version  = + "." + cs;
            //ModuleManifest
            templateManifestData.Save(destinationRootFolder);
        
    } 
    /// <summary>
    /// Открывает шаблон манифеста для создание модуля
    /// </summary>
    private static void OpenManifistTemplate()
    {
        var TemplateManifestHelper = new TemplateManifestHelper();
        if (TemplateManifestHelper.isFindTemplateManifestFile())
        {
            templateManifestData = TemplateManifestHelper.ReadManifestAsObject();
        }
    }
}
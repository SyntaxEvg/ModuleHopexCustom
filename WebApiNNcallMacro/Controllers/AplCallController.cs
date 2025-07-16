using Hopex.Common.JsonMessages;
using Mega.Api.Models;
using Mega.Has.Commons;
using Mega.Has.WebSite;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hopex.WebService.Controller
{

    [ApiController]
    public class MainQueryController : ControllerBase
    {
        private readonly IHeaderCollector _headerCollector;
        private string? un;
        private string ip;
        private readonly IHASClient _hopex;
        private readonly ILogger<HopexSessionController> _logger;

        public MainQueryController(IHASClient hopex, ILogger<HopexSessionController> logger, IHeaderCollector headerCollector)
        {
            this._hopex = hopex;
            this._logger = logger;
            _headerCollector = headerCollector;
            //un = base.User.Identity.Name;
            //un = HttpContext.User.Identity?.Name;
            //ip = base.Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            //ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        }


        [HttpPost]
        [Route("api1")]
        public IActionResult PostExecAll10()
        {
            // ���������� ������
            return Ok("api1");
        }

        [HttpPost]
        [Route("api_test")]
        public IActionResult PostExecAll10Test()
        {
            // ���������� ������
            return Ok("api_test");
        }

        [CollectHeadersAttribute]
        [HttpPost]
        [Route("api1")]
        public async Task<IActionResult> ProdExecuteMacro(oDataAll data)
        {
            string createSessionResponseJson = "error";
            try
            {
                await Task.Delay(100);
                string text = "";
                bool flag = data.MacrosID == null;
                if (flag)
                {
                    text += "MacrosID is null. ";
                }
                bool flag2 = data.Verb == null;
                if (flag2)
                {
                    text += "Verb is null. ";
                }
                bool flag3 = data.Class == null;
                if (flag3)
                {
                    text += "Class is null. ";
                }
                bool flag4 = text != "";
                IActionResult httpActionResult;
                if (flag4)
                {
                    //MainQueryController.Logger.Debug("errorValidData: " + text);
                    return this.ServerError500(new WebServiceResult
                    {
                        ErrorType = "BadRequest",
                        Content = text
                    });
                }

                data.ip = ip;
                data.un = un;
                var headers = _headerCollector.Headers; // HttpContext.Items["CollectedHeaders"] as Dictionary<string, string>;
                string idUploadSession = Guid.NewGuid().ToString();
                bool isSessionCreated = false;
                var oDataAlljsonstr = JsonConvert.SerializeObject(data);
                // await this._hopex.OpenSession();
                // HttpResponseMessage sess = await this._hopex.OpenSession();
                //Task<HttpResponseMessage> CallMacro(string data, string userData, IDictionary<string, string> headers, string path = "/api/generate");
                HttpResponseMessage httpResponseMessage = await this._hopex.CallMacro(data.MacrosID, oDataAlljsonstr, headers, "/api/generate");
                createSessionResponseJson = await httpResponseMessage.Content.ReadAsStringAsync();
                //    UploadCreateSessionResult createSessionResult = JsonConvert.DeserializeObject<UploadCreateSessionResult>(createSessionResponseJson);
                //    if (createSessionResult != null)
                //    {
                //        if (createSessionResult.Success && !string.IsNullOrWhiteSpace(createSessionResult.IdUploadSession))
                //        {
                //            idUploadSession = createSessionResult.IdUploadSession;
                //            isSessionCreated = true;
                //        }
                //        else
                //        {
                //            //if (createSessionResult.MaxUploadsReached)
                //            //{
                //            //    this._logger.LogError("UploadFile: MaxUploadsReached", Array.Empty<object>());
                //            //    return this.BadRequest("UploadFile: MaxUploadsReached");
                //            //}
                //            //this._logger.LogInformation("UploadFile: UnknownError (CreateSession API might not be available in this HOPEX version)", Array.Empty<object>());
                //        }
                //    }
            }
            catch (Exception)
            {
                this._logger.LogInformation("UploadFile: UnknownError (CreateSession API might not be available in this HOPEX version)", Array.Empty<object>());
            }


            //string res = "createSessionResponseJson";


            // ���������� ������
            return Ok(createSessionResponseJson);
        }

        private IActionResult ServerError500(WebServiceResult Result)
        {
            myResult myResult = new myResult();
            myResult.Result = "Fail";
            myResult.Info = Result.Content;
            return StatusCode(500,myResult);
        }

        [HttpGet]
        [CollectHeadersAttribute]
        [Route("testheaders")]
        public async Task<IActionResult> testheaders()
        {
            var headers = _headerCollector.Headers; // HttpContext.Items["CollectedHeaders"] as Dictionary<string, string>;

            // �������� ������� required ���������
            if (!headers.TryGetValue("x-api-key", out var apiKey))
            {
                return BadRequest("Missing x-api-key header");
            }
            return Ok(headers);
        }

        [HttpPost]
        [CollectHeadersAttribute]
        [Route("api0")]
        public async Task<IActionResult> UserInfo0(oDataAll oDataAll)
        {
            string createSessionResponseJson = "error";
            try
            {
                await Task.Delay(100);
                // string idUploadSession = Guid.NewGuid().ToString();
                // bool isSessionCreated = false;
                return await ProdExecuteMacro(oDataAll);
                // await this._hopex.OpenSession();
                // HttpResponseMessage sess = await this._hopex.OpenSession();
                //Task<HttpResponseMessage> CallMacro(string data, string userData, IDictionary<string, string> headers, string path = "/api/generate");

                //Dictionary<string, string> headers = new Dictionary<string, string>()
                //{
                //{ "x-api-key" , "6Y3BoAgo2tx4uyA2sCR7h3vPp24oM2GzRsLquJVKz3gCjfpSNmxcxdcJpkkGMCRrQ3" }
                //};
                //HttpResponseMessage httpResponseMessage = await this._hopex.CallMacro("E7CBF22C68211F92", "{\"ok\":\"ok\"}", headers, "/api/generate");
                //HttpResponseMessage createSessionResponse = httpResponseMessage;
                //createSessionResponseJson = await createSessionResponse.Content.ReadAsStringAsync();
                //    UploadCreateSessionResult createSessionResult = JsonConvert.DeserializeObject<UploadCreateSessionResult>(createSessionResponseJson);
                //    if (createSessionResult != null)
                //    {
                //        if (createSessionResult.Success && !string.IsNullOrWhiteSpace(createSessionResult.IdUploadSession))
                //        {
                //            idUploadSession = createSessionResult.IdUploadSession;
                //            isSessionCreated = true;
                //        }
                //        else
                //        {
                //            //if (createSessionResult.MaxUploadsReached)
                //            //{
                //            //    this._logger.LogError("UploadFile: MaxUploadsReached", Array.Empty<object>());
                //            //    return this.BadRequest("UploadFile: MaxUploadsReached");
                //            //}
                //            //this._logger.LogInformation("UploadFile: UnknownError (CreateSession API might not be available in this HOPEX version)", Array.Empty<object>());
                //        }
                //    }
            }
            catch (Exception)
            {
                this._logger.LogInformation("UploadFile: UnknownError (CreateSession API might not be available in this HOPEX version)", Array.Empty<object>());
            }


            //string res = "createSessionResponseJson";


            // ���������� ������
            return Ok(createSessionResponseJson);
        }
    }
}
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.Json;

//class Program
//{
//    static string filePath = "data.json";

//    static void Main()
//    {
//        // ������ ������
//        var dict = new Dictionary<string, string>
//        {
//            { "key1", "value1" },
//            { "key2", "value2" }
//        };

//        // 1. ������ �����
//        EnsureArrayStart(filePath);

//        // 2. ���������� ��������
//        foreach (var kvp in dict)
//        {
//            AppendJsonObject(filePath, kvp);
//        }

//        // 3. ���������� �����
//        EnsureArrayEnd(filePath);
//    }

//    // ? �����: �������� [ ���� ���� ������
//    static void EnsureArrayStart(string path)
//    {
//        if (!File.Exists(path) || new FileInfo(path).Length == 0)
//        {
//            File.AppendAllText(path, "[\n");
//        }
//    }

//    // ? �����: �������� JSON-������ ��� [ ] � � �������
//    static void AppendJsonObject(string path, KeyValuePair<string, string> kvp)
//    {
//        var obj = new Dictionary<string, string> { { kvp.Key, kvp.Value } };
//        string json = JsonSerializer.Serialize(obj);

//        // ������� ������� ������ { "key": "value" } => "key": "value"
//        ReadOnlySpan<char> span = json.AsSpan(1, json.Length - 2);
//        string trimmed = span.ToString();

//        // ��������� ��� { "key": "value" },
//        File.AppendAllText(path, "  {" + trimmed + "},\n");
//    }

//    // ? �����: �������� ] � �����, ������� ��������� �������
//    static void EnsureArrayEnd(string path)
//    {
//        if (!File.Exists(path)) return;

//        var lines = File.ReadAllLines(path);

//        if (lines.Length == 0) return;

//        // ������� ��������� ������� � ���������� �������
//        for (int i = lines.Length - 1; i >= 0; i--)
//        {
//            if (lines[i].TrimEnd().EndsWith(","))
//            {
//                lines[i] = lines[i].TrimEnd().TrimEnd(',');
//                break;
//            }
//        }

//        // ��������� ����������� ������
//        File.WriteAllLines(path, lines);
//        File.AppendAllText(path, "\n]");
//    }
//}
static void EnsureArrayEnd(string path)
{
    if (!File.Exists(path)) return;

    // ��������� ���� ��� ������ � ������
    using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    using var reader = new StreamReader(stream);

    long lastCommaPos = -1;
    long lastLineStart = 0;
    string? lastLine = null;

    // ������ ���������, ���������� ������� ������ ��������� ������
    while (!reader.EndOfStream)
    {
        lastLineStart = stream.Position;
        lastLine = reader.ReadLine();
    }

    if (lastLine == null) return;

    // ���������, ���� �� ������� � �����
    if (lastLine.TrimEnd().EndsWith(","))
    {
        // ������� �������: ������������ � ������ ��������� ������
        stream.Seek(lastLineStart, SeekOrigin.Begin);

        // ������ ������ ��� �����
        var encoding = reader.CurrentEncoding;
        byte[] buffer = new byte[encoding.GetByteCount(lastLine)];
        stream.Read(buffer, 0, buffer.Length);

        // ����������� � ������, ������� �������
        string lineWithoutComma = lastLine.TrimEnd().TrimEnd(',');

        // ������������ ����� � �������������� ������
        stream.Seek(lastLineStart, SeekOrigin.Begin);
        byte[] newBytes = encoding.GetBytes(lineWithoutComma + "\n");
        stream.Write(newBytes, 0, newBytes.Length);

        // �������� ����, ���� ����� ������ ������
        stream.SetLength(stream.Position);
    }

    // ��������� ����������� ������
    using var writer = new StreamWriter(stream, leaveOpen: true);
    writer.WriteLine("]");
}
static Dictionary<string, string> ReadJsonArrayToDictionary(string path)
{
    var dict = new Dictionary<string, string>();

    using FileStream fs = File.OpenRead(path);
    using var reader = new StreamReader(fs, Encoding.UTF8);
    string json = reader.ReadToEnd();

    var jsonBytes = Encoding.UTF8.GetBytes(json);
    var jsonSpan = new ReadOnlySpan<byte>(jsonBytes);

    var jsonReader = new Utf8JsonReader(jsonSpan, isFinalBlock: true, state: default);

    // ������� ������ �������
    if (!jsonReader.Read() || jsonReader.TokenType != JsonTokenType.StartArray)
        throw new InvalidDataException("�������� JSON-������");

    while (jsonReader.Read())
    {
        if (jsonReader.TokenType == JsonTokenType.EndArray)
            break;

        if (jsonReader.TokenType == JsonTokenType.StartObject)
        {
            string? key = null;
            string? value = null;

            while (jsonReader.Read() && jsonReader.TokenType != JsonTokenType.EndObject)
            {
                if (jsonReader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = jsonReader.GetString()!;
                    jsonReader.Read(); // ������� � ��������

                    if (key == null)
                    {
                        key = propertyName;
                        value = jsonReader.GetString();
                    }
                    else
                    {
                        // ���� ������ �������� ������ ������ ����� � ����� ���������
                        throw new InvalidDataException("�������� ������ � ����� ������");
                    }
                }
            }

            if (key != null && value != null)
            {
                dict[key] = value;
            }
        }
    }

    return dict;
}
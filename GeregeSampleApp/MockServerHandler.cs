using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace GeregeSampleApp
{
    /// <summary>
    /// Туршилтын зорилгоор ашиглах хуурамч сервер хандалт.
    /// </summary>
    public sealed class MockServerHandler : HttpMessageHandler
    {
        private readonly string tokenValue = "GEREGE_SUPER_SECRET_TOKEN";

        /// <inheritdoc />
        protected sealed override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {            
            try
            {
                Thread.Sleep(500); // Интернетээр хандаж буй мэт сэтгэгдэл төрүүлэх үүднээс хором хүлээлгэе

                string requestTarget = request.RequestUri?.ToString();
                if (requestTarget != this.AppRaiseEvent("get-server-address"))
                    throw new Exception("Unknown route pattern [" + requestTarget + "]");

                string message_code_header = string.Join("", request.Headers.GetValues("message_code"));
                if (string.IsNullOrEmpty(message_code_header)
                    || request.Method != HttpMethod.Post) throw new Exception("Invalid request");

                int message_code = Convert.ToInt32(message_code_header);
                string token = request.Headers.Authorization?.ToString();

                Task<string> input = request.Content?.ReadAsStringAsync();
                dynamic payload = JsonConvert.DeserializeObject(input?.Result);

                return HandleMessages(message_code, payload, token);
            }
            catch (Exception ex)
            {
                return Respond(new
                {
                    code = 404,
                    status = "Error 404",
                    message = ex.Message,
                    result = new { }
                },
                HttpStatusCode.NotFound);
            }
        }

        private Task<HttpResponseMessage> Respond(dynamic content, HttpStatusCode StatusCode = HttpStatusCode.OK)
        {
            return Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = StatusCode,
                Content = new StringContent(JsonConvert.SerializeObject(content))
            });
        }

        private Task<HttpResponseMessage> HandleMessages(int message_code, dynamic payload, string token = null)
        {
            if (message_code == 1)
                return UserLogin(payload);

            if (token != "Bearer " + tokenValue)
                throw new Exception("Unauthorized access!");

            switch (message_code)
            {
                case 101:
                    return Respond(new
                    {
                        code = 200,
                        status = "success",
                        message = "",
                        result = new
                        {
                            title = "Welcome to Gerege Systems"
                        }
                    });
                case 102:
                    return Respond(new
                    {
                        code = 200,
                        status = "success",
                        message = "",
                        result = LoadEmbeddedJson("gerege_partners.json")
                    });
                default:
                    throw new Exception(message_code + ": Message code not found");
            }
        }

        private Task<HttpResponseMessage> UserLogin(dynamic payload)
        {
            string username = Convert.ToString(payload?.username ?? "");
            string password = Convert.ToString(payload?.password ?? "");

            if (string.IsNullOrEmpty(username)
                || string.IsNullOrEmpty(password))
                throw new Exception("Please provide login information!");

            if (username != "Gerege" || password != "mongol")
                throw new Exception("Invalid user login!");

            return Respond(new {
                code = 200,
                status = "success",
                message = "fetching user token",
                result = new {
                    token = tokenValue,
                    expires_in = 300,
                    expires = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd H:mm:ss")
                }
            });
        }

        private dynamic LoadEmbeddedJson(string szFilePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(typeof(MockServerHandler).Namespace + "." + szFilePath);
            if (stream is null)
                throw new FileNotFoundException(assembly.GetName().Name + " дотор " + szFilePath + " гэсэн файл байхгүй л байна даа!");

            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject(reader.ReadToEnd());
            }
        }
    }
}

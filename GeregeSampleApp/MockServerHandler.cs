using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.IdentityModel.Tokens;

using Gerege.Framework.HttpClient;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace GeregeSampleApp;

/// <summary>
/// Туршилтын зорилгоор ашиглах хуурамч сервер хандалт.
/// </summary>
public sealed class MockServerHandler : HttpMessageHandler
{
    private string GenerateJwtToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS IS GEREGE CUSTOM SECRET KEY FOR AUTHENTICATION"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "Gerege",
            audience: "audience",
            claims: [],
            expires: DateTime.Now.AddDays(3),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    protected sealed override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            Thread.Sleep(500); // Интернетээр хандаж буй мэт сэтгэгдэл төрүүлэх үүднээс хором хүлээлгэе

            string? path = request.RequestUri?.AbsolutePath;

            if (request.Method == HttpMethod.Post
                && path == "/user/login")
            {
                Task<string>? input = request.Content?.ReadAsStringAsync(cancellationToken);
                var payload = JsonSerializer.Deserialize<JsonElement>(input?.Result ?? "{}");
                if (!payload.TryGetProperty("username", out JsonElement username)
                    || !payload.TryGetProperty("password", out JsonElement password))
                    throw new Exception("Please provide login information!");

                if (username.ToString() != "Gerege"
                    || password.ToString() != "mongol")
                    throw new Exception("Invalid user login!");

                return Respond(new
                {
                    token = GenerateJwtToken()
                });
            }
            else
            {
                string? tokenValue = request.Headers.Authorization?.ToString().Substring(7);
                GeregeToken token = new GeregeToken();
                token.Update(tokenValue ?? "null");
                if (!token.IsValid)
                    throw new Exception("Unauthorized access!");

                if (request.Method == HttpMethod.Get)
                    return path switch
                    {
                        "/get/title" => Respond(new
                        {
                            title = "Welcome to Gerege Systems"
                        }),
                        "/get/partners" => Respond(LoadEmbeddedJson("gerege_partners.json")),
                        _ => throw new Exception("Unknown route pattern")
                    };
            }

            throw new Exception("Not Found");
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

    private Task<HttpResponseMessage> Respond(object? content, HttpStatusCode StatusCode = HttpStatusCode.OK)
    {
        return Task.FromResult(new HttpResponseMessage()
        {
            StatusCode = StatusCode,
            Content = new StringContent(JsonSerializer.Serialize(content))
        });
    }

    private object? LoadEmbeddedJson(string szFilePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"{typeof(MockServerHandler).Namespace}.{szFilePath}")
            ?? throw new FileNotFoundException($"{assembly.GetName().Name} дотор {szFilePath} гэсэн файл байхгүй л байна даа!");
        using StreamReader reader = new StreamReader(stream);
        return JsonSerializer.Deserialize<object>(reader.ReadToEnd());
    }
}

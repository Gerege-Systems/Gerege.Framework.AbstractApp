using System;
using System.Net.Http;
using System.Text.Json;

using Gerege.Framework.HttpClient;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace GeregeSampleApp;

/// <inheritdoc />
public class SampleUserClient : GeregeClient
{
    /// <inheritdoc />
    public SampleUserClient(HttpMessageHandler handler) : base(handler)
    {
//        BaseAddress = new Uri((string)this.AppRaiseEvent("get-server-address")!);
    }

    /// <summary>
    /// Хэрэглэгч нэвтрэх.
    /// </summary>
    /// <param name="payload">Нэвтрэх мэдээлэл.</param>
    public void Login(object payload)
    {
        GeregeToken? token = FetchToken(payload);

        if (token is null || !token.IsValid)
            throw new Exception("Invalid token data!");

        this.AppRaiseEvent("client-login");
    }

    GeregeToken? currentToken = null;
    object? fetchTokenPayload = null;

    /// <inheritdoc />
    protected override GeregeToken? FetchToken(object? payload = null)
    {
        if (payload != null)
            fetchTokenPayload = payload;

        if (currentToken != null
            && currentToken.IsExpiring)
        {
            currentToken = null;
            payload = fetchTokenPayload;
        }

        if (payload != null)
        {
            JsonElement token = Request<JsonElement>(payload, HttpMethod.Post, Convert.ToString(this.AppRaiseEvent("get-token-server-address")));
            if (token.TryGetProperty("token", out JsonElement tokenValue))
            {
                currentToken = new GeregeToken();
                currentToken.Update(tokenValue.ToString());
            }
        }
        return currentToken;
    }
}

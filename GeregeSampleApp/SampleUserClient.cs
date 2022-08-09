using System.Net.Http;
using Gerege.Framework.HttpClient;

/////// date: 2022.02.09 //////////
///// author: Narankhuu ///////////
//// contact: codesaur@gmail.com //

namespace GeregeSampleApp;

/// <inheritdoc />
public class SampleUserClient : GeregeClient
{
    /// <inheritdoc />
    public class SampleToken : GeregeToken
    {
        /// <summary>
        /// Гэрэгэ токен авах мсж дугаар.
        /// </summary>
        public virtual int GeregeMessage() => 1;
    }

    /// <inheritdoc />
    public SampleUserClient(HttpMessageHandler handler) : base(handler)
    {
        BaseAddress = new(this.AppRaiseEvent("get-server-address"));
    }

    /// <summary>
    /// Терминал нэвтрэх.
    /// </summary>
    /// <param name="payload">Нэвтрэх мэдээлэл.</param>
    public void Login(dynamic payload)
    {
        GeregeToken? token = FetchToken(payload);

        if (token is null
            || string.IsNullOrEmpty(token.Value))
            throw new("Invalid token data!");

        this.AppRaiseEvent("client-login");
    }

    GeregeToken? currentToken = null;
    dynamic? fetchTokenPayload = null;

    /// <inheritdoc />
    protected override GeregeToken? FetchToken(object? payload = null)
    {
        if (payload is not null)
            fetchTokenPayload = payload;

        if (currentToken is not null
            && currentToken.IsExpiring)
        {
            currentToken = null;
            payload = fetchTokenPayload;
        }

        if (payload is not null)
            currentToken = Request<SampleToken>(payload);

        return currentToken;
    }
}

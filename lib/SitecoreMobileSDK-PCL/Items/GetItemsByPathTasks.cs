﻿namespace Sitecore.MobileSDK.Items
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Sitecore.MobileSDK.TaskFlow;
    using Sitecore.MobileSDK.UrlBuilder;

    public class GetItemsByPathTasks : IRestApiCallTasks<ReadItemByPathParameters, HttpRequestMessage, string, ScItemsResponse>
    {
        public GetItemsByPathTasks(ItemByPathUrlBuilder urlBuilder, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.urlBuilder = urlBuilder;
        }

        #region  IRestApiCallTasks

        public async Task<HttpRequestMessage> BuildRequestUrlForRequestAsync(ReadItemByPathParameters request)
        {
            string url = this.UrlToGetItemWithRequest(request);
            HttpRequestMessage result = new HttpRequestMessage(HttpMethod.Get, url);

            result = await request.CredentialsHeadersCryptor.AddEncryptedCredentialHeadersAsync(result);
            return result;
        }

        public async Task<string> SendRequestForUrlAsync(HttpRequestMessage requestUrl)
        {
            HttpResponseMessage httpResponse = await this.httpClient.SendAsync(requestUrl);
            return await httpResponse.Content.ReadAsStringAsync();
        }

        public async Task<ScItemsResponse> ParseResponseDataAsync(string data)
        {
            Func<ScItemsResponse> syncParseResponse = () =>
            {
                return ScItemsParser.Parse(data);
            };
            return await Task.Factory.StartNew(syncParseResponse);
        }

        #endregion IRestApiCallTasks

        private string UrlToGetItemWithRequest(ReadItemByPathParameters request)
        {
            return this.urlBuilder.GetUrlForRequest(request);
        }

        private readonly HttpClient httpClient;
        private readonly ItemByPathUrlBuilder urlBuilder;
    }
}

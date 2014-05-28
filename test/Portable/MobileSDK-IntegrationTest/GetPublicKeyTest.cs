﻿namespace MobileSDKIntegrationTest
{
    using NUnit.Framework;

    using System.Net.Http;
    
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Sitecore.MobileSDK;
    using Sitecore.MobileSDK.Items;

    [TestFixture]
    public class GetPublicKeyTest
    {
        private ScApiSession sessionWithAnonymousAccess;
        private ScApiSession sessionWithNoAnonymousAccess;

        private HttpClient httpClient;

        [SetUp]
        public void Setup()
        {
            SessionConfig config = new SessionConfig("http://mobiledev1ua1.dk.sitecore.net:7119", "sitecore\\admin", "b");
            this.sessionWithNoAnonymousAccess = new ScApiSession(config, ItemSource.DefaultSource());

            config = new SessionConfig("http://mobiledev1ua1.dk.sitecore.net:722", "sitecore\\admin", "b");
            this.sessionWithAnonymousAccess = new ScApiSession(config, ItemSource.DefaultSource());

            this.httpClient = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            this.sessionWithNoAnonymousAccess = null;
            this.sessionWithAnonymousAccess = null;
        }

        [Test]
        public async void TestRestrictedInstanceReturnsErrorByDefault()
        {
            string url = "http://mobiledev1ua1.dk.sitecore.net:7119/-/item/v1";
            string response = await this.httpClient.GetStringAsync(url);

            string expectedResponse = "{\"statusCode\":401,\"error\":{\"message\":\"Access to site is not granted.\"}}";
            Assert.AreEqual(response, expectedResponse);
        }

        [Test]
        public async void TestRestrictedInstanceReturnsItemsWhenAuthenticated()
        {
            PublicKeyX509Certificate publicKey = await this.sessionWithNoAnonymousAccess.GetPublicKey();
            string encryptedLogin = this.sessionWithNoAnonymousAccess.EncryptString("sitecore\\admin");
            string encryptedPassword = this.sessionWithNoAnonymousAccess.EncryptString("b");

            this.httpClient.DefaultRequestHeaders.Add("X-Scitemwebapi-Username", encryptedLogin);
            this.httpClient.DefaultRequestHeaders.Add("X-Scitemwebapi-Password", encryptedPassword);
            this.httpClient.DefaultRequestHeaders.Add("X-Scitemwebapi-Encrypted", "1");

            string url = "http://mobiledev1ua1.dk.sitecore.net:7119/-/item/v1";
            string response = await this.httpClient.GetStringAsync(url);


            JObject json = JObject.Parse(response);
            int statusCode = (int)json.SelectToken("$.statusCode");

            Assert.AreEqual(200, statusCode);
        }
    }
}
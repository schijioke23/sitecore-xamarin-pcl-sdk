﻿namespace Sitecore.MobileSDK
{
	using System;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using System.Collections.Generic;

    using Sitecore.MobileSDK.Items;

	public class ScItemsParser
	{
		private ScItemsParser ()
		{
		}

		public static ScItemsResponse Parse (string responseString)
		{
			if (string.IsNullOrEmpty (responseString))
			{
				throw new ArgumentException ("response cannot null or empty");
			}

			JObject response = JObject.Parse (responseString);

			int statusCode = ParseOrFail<int> (response, "$.statusCode");
			if (200 != statusCode)
			{
				var error = new ScErrorResponse (statusCode, ParseOrFail<string> (response, "$.error.message")); 
				throw new ScResponseException (error);
			}

			int totalCount = ParseOrFail<int> (response, "$.result.totalCount");
			int resultCount = ParseOrFail<int> (response, "$.result.resultCount");

			var responseItems = response.SelectToken ("$.result.items");
			var items = new List<ScItem> ();

			foreach (JObject item in responseItems)
			{
				var source = ParseItemSource (item);

				var displayName = (string)item.GetValue ("DisplayName");
				var hasChildren = (bool)item.GetValue ("HasChildren");
				var id = (string)item.GetValue ("ID");
				var longId = (string)item.GetValue ("LongID");
				var path = (string)item.GetValue ("Path");
				var template = (string)item.GetValue ("Template");

				items.Add (new ScItem (source, displayName, hasChildren, id, longId, path, template));
			}
			return new ScItemsResponse (totalCount, resultCount, items);
		}

		private static ItemSource ParseItemSource (JObject json)
		{
			var language = (string)json.GetValue ("Language");
			var database = (string)json.GetValue ("Database");
            var version = (string)json.GetValue ("Version");

			return new ItemSource (database, language, version);
		}

		private static T ParseOrFail<T> (JObject json, string path)
		{
			JToken objectToken = json.SelectToken (path);
			if (null == objectToken)
			{
				throw new JsonException ("Invalid json");
			}

			return objectToken.Value<T> ();
		}
	}
}
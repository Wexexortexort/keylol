﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Keylol
{
    public class Geetest
    {
        private const string BaseUrl = "http://api.geetest.com";
        private const string Key = "444dcf8693daa76733c7ad1c6e2655d7";

        private HttpClient Client { get; } = new HttpClient() {BaseAddress = new Uri(BaseUrl)};

        public async Task<bool> ValidateAsync(string challenge, string seccode, string validate)
        {
            if (validate.Length > 0 && CheckResultByPrivate(challenge, validate))
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("seccode", seccode));
                postData.Add(new KeyValuePair<string, string>("sdk", "csharp_2.15.7.23.1"));
                var result = await Client.PostAsync(GetApiEntry("/validate.php"), new FormUrlEncodedContent(postData));
                if (await result.Content.ReadAsStringAsync() == MD5Encode(seccode))
                    return true;
            }
            return false;
        }

        private string GetApiEntry(string apiPath)
        {
            return BaseUrl + apiPath;
        }

        private string MD5Encode(string text)
        {
            var md5 = new MD5CryptoServiceProvider();
            var t2 = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(text)));
            t2 = t2.Replace("-", "");
            t2 = t2.ToLower();
            return t2;
        }

        private bool CheckResultByPrivate(string origin, string validate)
        {
            var md5 = MD5Encode(Key + "geetest" + origin);
            return validate == md5;
        }
    }
}
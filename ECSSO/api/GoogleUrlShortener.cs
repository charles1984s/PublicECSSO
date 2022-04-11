using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace ECSSO.api
{
    public class GoogleUrlShortener
    {
        #region Const
        const String BASE_API_URL = @"https://www.googleapis.com/urlshortener/v1/url";
        const String SHORTENER_URL_PATTERN = BASE_API_URL + @"?key={0}";
        const String EXPAND_URL_PATTERN = BASE_API_URL + @"?shortUrl={0}";
        #endregion

        #region Var
        private String _apiKey;
        #endregion

        #region Private Property
        private String m_APIKey
        {
            get
            {
                if (_apiKey == null)
                    return string.Empty;
                return _apiKey;
            }
            set
            {
                _apiKey = value;
            }
        }
        #endregion

        #region Constructor
        public GoogleUrlShortener(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException("apiKey");

            this.m_APIKey = apiKey;
        }
        #endregion

        #region Private Method
        private string GetHTMLSourceCode(string url)
        {
            HttpWebRequest request = (WebRequest.Create(url)) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }
        #endregion

        #region Public Method
        public string Shorten(string url)
        {
            if (String.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            if (m_APIKey.Length == 0)
                throw new Exception("APIKey not set!");

            const string POST_PATTERN = @"{{""longUrl"": ""{0}""}}";
            const string MATCH_PATTERN = @"""id"": ?""(?<id>.+)""";

            var post = string.Format(POST_PATTERN, url);
            var request = (HttpWebRequest)WebRequest.Create(string.Format(SHORTENER_URL_PATTERN, m_APIKey));

            request.Method = "POST";
            request.ContentLength = post.Length;
            request.ContentType = "application/json";
            request.Headers.Add("Cache-Control", "no-cache");

            using (Stream requestStream = request.GetRequestStream())
            {
                var buffer = Encoding.ASCII.GetBytes(post);
                requestStream.Write(buffer, 0, buffer.Length);
            }

            using (var responseStream = request.GetResponse().GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(responseStream))
                {
                    return Regex.Match(sr.ReadToEnd(), MATCH_PATTERN).Groups["id"].Value;
                }
            }            
        }

        public String Expand(string url)
        {
            const string MATCH_PATTERN = @"""longUrl"": ?""(?<longUrl>.+)""";

            var expandUrl = string.Format(EXPAND_URL_PATTERN, url);
            var response = GetHTMLSourceCode(expandUrl);

            return Regex.Match(response, MATCH_PATTERN).Groups["longUrl"].Value;
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tmp.Link.Upload.Utils
{
    public class FormBuilder
    {
        private HttpClient HttpClient { get; init; }

        private readonly MultipartFormDataContent _data = new MultipartFormDataContent();

        public static FormBuilder Init(HttpClient client)
        {
            return new FormBuilder(client);
        }

        private FormBuilder(HttpClient client)
        {
            HttpClient = client;
        }

        public FormBuilder AddForm(string key, string value)
        {
            _data.Add(new StringContent(value), key);
            return this;
        }

        public FormBuilder AddForm(string key, int value)
        {
            _data.Add(new StringContent(value.ToString()), key);
            return this;
        }

        public FormBuilder AddForm(string key, string fileName, Stream value)
        {
            _data.Add(new StreamContent(value), key, fileName);
            return this;
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri)
            => await HttpClient.PostAsync(uri, _data);
    }
}

using Findx.DependencyInjection;
using Findx.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - Http请求
    /// </summary>
    public static partial class Extensions
    {
        private const string JsonMediaType = "application/json";

        /// <summary>
        /// 反序列化HttpClient返回json字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpContent"></param>
        /// <returns></returns>
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent httpContent)
        {
            var result = await httpContent.ReadAsStringAsync();
            return result.ToObject<T>();
        }

        /// <summary>
        /// 发起POST Json请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string requestUri, T parameter)
        {
            Check.NotNull(httpClient, nameof(httpClient));
            Check.NotNull(requestUri, nameof(requestUri));
            Check.NotNull(parameter, nameof(parameter));

            return httpClient.PostAsync(requestUri, new StringContent(parameter.ToJson<T>(), Encoding.UTF8, JsonMediaType));
        }

        /// <summary>
        /// 发起POST 表单请求
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="paramDic"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostAsFormAsync(this HttpClient httpClient, string requestUri, IEnumerable<KeyValuePair<string, string>> paramDic)
        {
            Check.NotNull(httpClient, nameof(httpClient));
            Check.NotNull(requestUri, nameof(requestUri));
            Check.NotNull(paramDic, nameof(paramDic));

            return httpClient.PostAsync(requestUri, new FormUrlEncodedContent(paramDic));
        }

        /// <summary>
        /// 发起文件上传请求
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUrl"></param>
        /// <param name="filePath"></param>
        /// <param name="fileKey"></param>
        /// <param name="formFields"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostFileAsync(this HttpClient httpClient, string requestUrl, string filePath, string fileKey = "file", IEnumerable<KeyValuePair<string, string>> formFields = null)
        {
            Check.NotNull(httpClient, nameof(httpClient));
            Check.NotNull(requestUrl, nameof(requestUrl));

            var content = new MultipartFormDataContent($"form--{DateTime.Now.Ticks:X}");

            if (formFields != null)
            {
                foreach (var kv in formFields)
                {
                    content.Add(new StringContent(kv.Value), kv.Key);
                }
            }

            content.Add(new StreamContent(File.OpenRead(filePath)), fileKey, Path.GetFileName(filePath));

            return httpClient.PostAsync(requestUrl, content);
        }

        /// <summary>
        /// 发起文件上传请求
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUrl"></param>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="fileKey"></param>
        /// <param name="formFields"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostFileAsync(this HttpClient httpClient, string requestUrl, Stream file, string fileName, string fileKey = "file", IEnumerable<KeyValuePair<string, string>> formFields = null)
        {
            if (file == null)
            {
                return await httpClient.PostAsFormAsync(requestUrl, formFields);
            }

            var content = new MultipartFormDataContent($"form--{DateTime.Now.Ticks:X}");

            if (formFields != null)
            {
                foreach (var kv in formFields)
                {
                    content.Add(new StringContent(kv.Value), kv.Key);
                }
            }

            content.Add(new StreamContent(file), fileKey, fileName);

            return await httpClient.PostAsync(requestUrl, content);
        }

        /// <summary>
        /// 发起文件上传请求
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="files"></param>
        /// <param name="formFields"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostFileAsync(this HttpClient httpClient, string requestUri, IEnumerable<KeyValuePair<string, Stream>> files, IEnumerable<KeyValuePair<string, string>> formFields = null)
        {
            if (files == null)
            {
                return await httpClient.PostAsFormAsync(requestUri, formFields);
            }

            var content = new MultipartFormDataContent($"form--{DateTime.Now.Ticks:X}");

            if (formFields != null)
            {
                foreach (var kv in formFields)
                {
                    content.Add(new StringContent(kv.Value), kv.Key);
                }
            }

            foreach (var file in files)
            {
                content.Add(new StreamContent(file.Value), Path.GetFileNameWithoutExtension(file.Key), Path.GetFileName(file.Key));
            }

            return await httpClient.PostAsync(requestUri, content);
        }

    }
}

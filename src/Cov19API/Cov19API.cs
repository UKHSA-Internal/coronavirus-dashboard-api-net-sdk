/**
 * Coronavirus (COVID-19) Dashboard - API Service
 * ==============================================
 *
 * Software Development Kit (SDK) for .NET
 * ---------------------------------------------
 *
 * This is a .NET SDK for the COVID-19 API, as published by
 * Public Health England on `Coronavirus (COVID-19) in the UK`_
 * dashboard.
 *
 * The endpoint for the data provided using this SDK is:
 *
 *     https://api.coronavirus.data.gov.uk/v1/data
 *
 * .. _`Coronavirus (COVID-19) in the UK`: http://coronavirus.data.gov.uk/
 */

namespace Cov19API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.OpenApi.Models;
    using Microsoft.OpenApi.Readers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines the entrypoint to the API
    /// </summary>
    public class Cov19Api
    {
        private readonly UkCovid19Props props;

        internal string Endpoint = "https://api.coronavirus.data.gov.uk/v1/data";

        internal APIParams ApiParams =>
            new APIParams
            {
                Filters = string.Join(";", this.props.FiltersType.Select(kv => kv.Key + "=" + kv.Value).ToArray()),
                Structure = JsonConvert.SerializeObject(this.props.StructureType),
                LatestBy = this.props.LatestBy
            };

        private DateTimeOffset LastUpdated { get; set; }

        private class APIJSONResponse<T>
        {
            public List<T> Data { get; set; }
        }

        /// <summary>
        /// Creates an instance of <see cref="Cov19Api"/>
        /// </summary>
        /// <param name="props">An instance of <see cref="UkCovid19Props"/></param>
        public Cov19Api(UkCovid19Props props)
        {
            this.props = props;
        }

        private async Task<(XDocument Xml, int TotalPages)> GetPagedXmlData(CancellationToken cancellationToken)
        {
            XDocument xDocResult = null;
            var currentPage = 1;

            while (true)
            {
                var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }) { Timeout = TimeSpan.FromSeconds(10) };
                var url = this.Endpoint + this.ApiParams + $"&page={currentPage}" + $"&format=xml";
                var response = await httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    break;
                }

                if ((int)response.StatusCode >= 400)
                {
                    throw new Exception(response.StatusCode.ToString());
                }

                this.LastUpdated = response.Content.Headers.LastModified ?? default;

                var stream = await response.Content.ReadAsStreamAsync();
                var xdoc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
                if (xDocResult == null)
                {
                    xDocResult = xdoc;
                }
                else
                {
                    xDocResult.Root.Add(xdoc.Root.Descendants("data"));
                }

                currentPage++;
            }

            return (xDocResult, currentPage - 1);
        }

        /// <summary>
        /// Get an XML representation of the API data
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task{XDocument}"/> instance</returns>
        public async Task<XDocument> GetXml(CancellationToken cancellationToken = default)
        {
            var data = await this.GetPagedXmlData(cancellationToken);
            data.Xml.Root.Element("length").Value = data.Xml.Descendants("data").Count().ToString();
            data.Xml.Root.Add(new XElement("totalPages", data.TotalPages));
            data.Xml.Root.Add(new XElement("lastUpdate", this.LastUpdated.ToString("O")));
            data.Xml.Root.Descendants("pagination").Remove();
            data.Xml.Root.Descendants("maxPageLimit").Remove();
            return data.Xml;
        }

        private async Task<(APIJSONResponse<T> Response, int TotalPages)> GetPagedJsonData<T>(CancellationToken cancellationToken)
        {
            var result = new APIJSONResponse<T> { Data = new List<T>() };

            var currentPage = 1;

            while (true)
            {
                var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }) { Timeout = TimeSpan.FromSeconds(10) };
                var url = this.Endpoint + this.ApiParams + $"&page={currentPage}" + $"&format=json";
                var response = await httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    break;
                }

                if ((int)response.StatusCode >= 400)
                {
                    throw new Exception(response.StatusCode.ToString());
                }

                this.LastUpdated = response.Content.Headers.LastModified ?? default;

                var body = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(body);
                var data = JsonConvert.DeserializeObject<List<T>>(jObject["data"].ToString());

                result.Data = result.Data.Concat(data).ToList();

                currentPage++;
            }

            return (result, currentPage - 1);
        }

        /// <summary>
        /// Get a strongly typed instance representing the API data
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <typeparam name="T">The <see cref="Type"/> of the value to return</typeparam>
        /// <returns>An instance of <typeparamref name="T"/></returns>
        public async Task<JSONResponse<T>> Get<T>(CancellationToken cancellationToken = default)
        {
            var data = await this.GetPagedJsonData<T>(cancellationToken);
            return new JSONResponse<T>
            {
                Length = data.Response.Data.Count,
                Data = data.Response.Data,
                TotalPages = data.TotalPages,
                LastUpdate = this.LastUpdated.ToString("O")
            };
        }

        /// <summary>
        /// Get an array of API headers
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="T:Task{IEnumerable{KeyValuePair{string,string}}}}" /></returns>
        public async Task<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> Head(CancellationToken cancellationToken = default)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = this.Endpoint + this.ApiParams;
            var response = await httpClient.GetAsync(url, cancellationToken);
            return response.Headers.Concat(response.Content.Headers);
        }

        /// <summary>
        /// Get an OpenAPI representation of the API
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task{OpenApiDocument}"/></returns>
        public async Task<OpenApiDocument> Options(CancellationToken cancellationToken = default)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var req = new HttpRequestMessage(HttpMethod.Options, this.Endpoint);
            var response = await httpClient.SendAsync(req, cancellationToken);

            var openapiBody = await response.Content.ReadAsStringAsync();
            var openApiStringReader = new OpenApiStringReader();
            var openApiDocument = openApiStringReader.Read(openapiBody, out var diagnostic);
            return openApiDocument;
        }

        /// <summary>
        /// Signals the last time the API was called
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Task{DateTimeOffset}"/></returns>
        public async Task<DateTimeOffset> LastUpdate(CancellationToken cancellationToken = default)
        {
            if (this.LastUpdated != default)
            {
                return this.LastUpdated;
            }

            var responseHeaders = await this.Head(cancellationToken);
            
            try {
                var modified = responseHeaders.First(x => x.Key == "Last-Modified");
                this.LastUpdated = DateTimeOffset.Parse(modified.Value.First());
            } catch (InvalidOperationException e) {}

            return this.LastUpdated;
        }
    }
}

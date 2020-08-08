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
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Cov19API
    {
        private readonly UkCovid19Props props;

        private string Endpoint = "https://api.coronavirus.data.gov.uk/v1/data";

        private APIParams ApiParams =>
            new APIParams
            {
                Filters = string.Join(";", this.props.FiltersType.Select(kv => kv.Key + "=" + kv.Value).ToArray()),
                Structure = JsonConvert.SerializeObject(this.props.StructureType),
                LatestBy = this.props.LatestBy
            };

        private class APIParams
        {
            public string Filters { get; set; }

            public string Structure { get; set; }

            public string LatestBy { get; set; }

            public override string ToString()
            {
                return $"?filters={Filters}&structure={Structure}&latestby={LatestBy}";
            }
        }

        private class APIJSONResponse<T>
        {
            public List<T> Data { get; set; }
        }

        public DateTimeOffset LastUpdate { get; set; }

        public Cov19API(UkCovid19Props props)
        {
            this.props = props;
        }

        private async Task<(APIJSONResponse<T> Response, int TotalPages)> GetPagedData<T>(Format format, CancellationToken cancellationToken)
        {
            var result = new APIJSONResponse<T> { Data = new List<T>() };

            var currentPage = 1;

            while (true)
            {
                var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }) { Timeout = TimeSpan.FromSeconds(10) };
                var url = this.Endpoint + this.ApiParams + $"&page={currentPage}" + $"&format={format.ToString().ToLower()}";
                var response = await httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    break;
                }

                if ((int)response.StatusCode >= 400)
                {
                    throw new Exception(response.StatusCode.ToString());
                }

                this.LastUpdate = response.Content.Headers.LastModified ?? DateTimeOffset.MinValue;

                var body = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(body);
                var data = JsonConvert.DeserializeObject<List<T>>(jObject["data"].ToString());

                result.Data = data;

                currentPage++;
            }

            return (result, currentPage - 1);
        }

        public async Task<JSONResponse<T>> Get<T>(Format format = Format.JSON, CancellationToken cancellationToken = default)
        {
            var data = await this.GetPagedData<T>(format, cancellationToken);
            return new JSONResponse<T>
            {
                Length = data.Response.Data.Count,
                Data = data.Response.Data,
                TotalPages = data.TotalPages,
                LastUpdate = this.LastUpdate.ToString("O")
            };
        }

        public async Task<HttpResponseHeaders> Head(CancellationToken cancellationToken = default)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = this.Endpoint + this.ApiParams;
            var response = await httpClient.GetAsync(url, cancellationToken);
            return response.Headers;
        }

        public async Task<string> Options(CancellationToken cancellationToken = default)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var req = new HttpRequestMessage(HttpMethod.Options, this.Endpoint);
            var response = await httpClient.SendAsync(req, cancellationToken);
            return await response.Content.ReadAsStringAsync();
        }
    }
}

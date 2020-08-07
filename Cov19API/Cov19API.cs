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
        private readonly UKCovid19Props props;
        
        private string Endpoint = "https://api.coronavirus.data.gov.uk/v1/data";

        private readonly Dictionary<string, string> structure;

        private readonly Dictionary<string, string> filters;

        private readonly string latestBy;

        private DateTimeOffset _lastUpdate;

        public Dictionary<string, string> HeadType;

        public Dictionary<string, object> OptionsType;

        public APIParams ApiParams =>
            new APIParams
            {
                Filters = string.Join(";", this.props.FiltersType.Select(kv => kv.Key + "=" + kv.Value).ToArray()),
                Structure = JsonConvert.SerializeObject(this.props.StructureType),
                LatestBy = this.props.LatestBy
            };

        public class UKCovid19Props
        {
            public Dictionary<string, string> FiltersType { get; set; }

            public Dictionary<string, string> StructureType { get; set; }

            public string LatestBy { get; set; }
        }

        public class APIParams
        {
            public string Filters { get; set; }

            public string Structure { get; set; }

            public string LatestBy { get; set; }

            public override string ToString()
            {
                return $"?filters={Filters}&structure={Structure}&latestby={LatestBy}";
            }
        }

        public class APIJSONResponse<T>
        {
            public List<T> Data { get; set; }
        }

        public class JSONResponse<T>
        {
            public List<T> Data { get; set; }

            public int Length { get; set; }

            public string LastUpdate { get; set; }

            public int TotalPages { get; set; }
        }

        public enum Format
        {
            JSON = 0,

            CSV = 1,

            XML = 2
        }

        public Cov19API(UKCovid19Props props)
        {
            this.props = props;
        }

        private async Task<(APIJSONResponse<T> Data, int TotalPages)> GetPagedData<T>(Format format, CancellationToken cancellationToken)
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

                this._lastUpdate = response.Content.Headers.LastModified ?? DateTimeOffset.MinValue;

                var body = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(body);
                var data = JsonConvert.DeserializeObject<List<T>>(jObject["data"].ToString());

                result.Data = data;

                currentPage++;
            }

            return (result, currentPage-1);
        }

        public async Task<JSONResponse<T>> Get<T>(Format format = Format.JSON, CancellationToken cancellationToken = default)
        {
            var data = await this.GetPagedData<T>(format, cancellationToken);
            return new JSONResponse<T>
            {
                Length = data.Data.Data.Count,
                Data = data.Data.Data,
                TotalPages = data.TotalPages,
                LastUpdate = this._lastUpdate.ToString("O")
            };
        }

        public async Task<HttpResponseHeaders> Head(CancellationToken cancellationToken = default)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = this.Endpoint + this.ApiParams;
            var response = await httpClient.GetAsync(url, cancellationToken);
            return response.Headers;
        }
    }
}

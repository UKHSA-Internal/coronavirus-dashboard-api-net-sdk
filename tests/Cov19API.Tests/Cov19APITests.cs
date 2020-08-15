namespace Cov19API.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class Cov19ApiTests
    {
        private readonly Cov19Api api;

        public Cov19ApiTests() =>
            this.api = new Cov19Api(new UkCovid19Props
            {
                FiltersType = new Dictionary<string, string> { { "areaType", "nation" }, { "areaName", "England" } },
                StructureType = new Dictionary<string, string> { { "MyDate", "date" }, { "newCases", "newCasesByPublishDate" } }
            });

        [Fact]
        public void API_Params_creates_correct_filter_format()
        {
            this.api.ApiParams.Filters.ShouldBe("areaType=nation;areaName=England");
            this.api.ApiParams.Structure.ShouldBe("{\"MyDate\":\"date\",\"newCases\":\"newCasesByPublishDate\"}");
        }

        [Fact]
        public async Task Options_Integrity()
        {
            var openApiDocument = await this.api.Options();
            openApiDocument.Servers.ShouldContain(server => server.Url == this.api.Endpoint);
        }

        [Fact]
        public async Task LastUpate_Integrity()
        {
            var lastUpdate = await this.api.LastUpdate();
            lastUpdate.ShouldNotBe(default);
        }

        [Fact]
        public async Task Head_Integrity()
        {
            var headers = await this.api.Head();
            headers.Any(x => x.Key == "Content-Location").ShouldBeTrue();
        }

        [Fact]
        public async Task Json_Integrity()
        {
            var data = await this.api.Get<TestCovidData>();
            data.Length.ShouldNotBe(default);
            data.Data.ShouldNotBeEmpty();
            data.LastUpdate.ShouldNotBe(default);
            data.TotalPages.ShouldNotBe(default);
        }

        [Fact]
        public async Task Xml_Integrity()
        {
            var data = await this.api.GetXml();
            data.Descendants("length").Count().ShouldBe(1);
            data.Descendants("data").Count().ShouldBeGreaterThan(0);
            data.Descendants("lastUpdate").Count().ShouldBe(1);
            data.Descendants("totalPages").Count().ShouldBe(1);
        }

        public class TestCovidData
        {
            public DateTime MyDate { get; set; }

            public int NewCases { get; set; }
        }
    }
}

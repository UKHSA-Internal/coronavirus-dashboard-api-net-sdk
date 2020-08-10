namespace Cov19API.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class Cov19ApiTests
    {
        private readonly Cov19Api api;

        public Cov19ApiTests()
        {
            this.api = new Cov19Api(new UkCovid19Props
            {
                FiltersType = new Dictionary<string, string> { { "areaType", "nation" }, { "areaName", "England" } },
                StructureType = new Dictionary<string, string> { { "MyDate", "date" }, { "newCases", "newCasesByPublishDate" } }
            });
        }

        [Fact]
        public void API_Params_creates_correct_filter_format()
        {
            api.ApiParams.Filters.ShouldBe("areaType=nation;areaName=England");
        }

        [Fact]
        public async Task Options_Integrity()
        {
            var openApiDocument = await api.Options();
            openApiDocument.Servers.ShouldContain(server => server.Url == api.Endpoint);
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
    }
}

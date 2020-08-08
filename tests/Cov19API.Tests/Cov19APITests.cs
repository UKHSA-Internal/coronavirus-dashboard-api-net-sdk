namespace Cov19API.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class Cov19ApiTests
    {
        [Fact]
        public void API_Params_creates_correct_filter_format()
        {
            var api = new Cov19Api(new UkCovid19Props { FiltersType = new Dictionary<string, string> { { "areaType", "nation" }, { "areaName", "England" } } });
            api.ApiParams.Filters.ShouldBe("areaType=nation;areaName=England");
        }
    }
}

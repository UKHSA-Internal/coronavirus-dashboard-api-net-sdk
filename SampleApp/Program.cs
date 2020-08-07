using System;

namespace SampleApp
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cov19API;

    public class CovidData
    {
        public DateTime MyDate { get; set; }

        public int NewCases { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var cov19api = new Cov19API(new Cov19API.UKCovid19Props
            {
                FiltersType = new Dictionary<string, string> { { "areaType", "nation" }, { "areaName", "England" } },
                StructureType = new Dictionary<string, string> { { "MyDate", "date" }, { "newCases", "newCasesByPublishDate" } }
            });

            var data = await cov19api.Get<CovidData>();

            foreach (var covidData in data.Data)
            {
                Console.WriteLine($"Date:{covidData.MyDate} No. of New Cases:{covidData.NewCases}");
            }
        }
    }
}

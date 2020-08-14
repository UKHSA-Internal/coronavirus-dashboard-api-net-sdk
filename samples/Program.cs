namespace SampleApp
{
    using System;
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
            var cov19api = new Cov19Api(new UkCovid19Props
            {
                FiltersType = new Dictionary<string, string> { { "areaType", "nation" }, { "areaName", "England" } },
                StructureType = new Dictionary<string, string> { { "MyDate", "date" }, { "newCases", "newCasesByPublishDate" } }
            });

            var data = await cov19api.Get<CovidData>();

            foreach (var covidData in data.Data)
            {
                Console.WriteLine($"Date:{covidData.MyDate} No. of New Cases:{covidData.NewCases}");
            }

            var openApi = await cov19api.Options();
            Console.WriteLine(openApi);

            var headers = await cov19api.Head();
            foreach (var httpResponseHeader in headers)
            {
                Console.WriteLine(httpResponseHeader.Key + " : " + string.Join(",", httpResponseHeader.Value));
            }

            var xml = await cov19api.GetXml();
            Console.WriteLine(xml.ToString());
        }
    }
}

namespace Cov19API
{
    using System.Collections.Generic;

    public class UkCovid19Props
    {
        public Dictionary<string, string> FiltersType { get; set; }

        public Dictionary<string, string> StructureType { get; set; }

        public string LatestBy { get; set; }
    }
}

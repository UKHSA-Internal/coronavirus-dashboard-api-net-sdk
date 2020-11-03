namespace Cov19API
{
    using System.Collections.Generic;

    /// <summary>
    /// Allows passing of arguments to the API
    /// </summary>
    public class UkCovid19Props
    {
        /// <summary>
        /// Specify which filters to apply to the API
        /// </summary>
        /// <remarks>See https://coronavirus.data.gov.uk/developers-guide#params-filters for specifics</remarks>
        public Dictionary<string, string> FiltersType { get; set; }

        /// <summary>
        /// Specify the return structure of the data from the API
        /// </summary>
        /// <remarks>See https://coronavirus.data.gov.uk/developers-guide#params-structure for specifics</remarks>
        public Dictionary<string, string> StructureType { get; set; }

        /// <summary>
        /// Specify the metric to see the latest data for
        /// </summary>
        /// <remarks>See https://coronavirus.data.gov.uk/developers-guide#params-latestby for specifics</remarks>
        public string LatestBy { get; set; }
    }
}

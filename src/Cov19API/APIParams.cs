namespace Cov19API
{
    internal class APIParams
    {
        public string Filters { get; set; }

        public string Structure { get; set; }

        public string LatestBy { get; set; }

        public override string ToString()
        {
            return $"?filters={Filters}&structure={Structure}&latestby={LatestBy}";
        }
    }

}

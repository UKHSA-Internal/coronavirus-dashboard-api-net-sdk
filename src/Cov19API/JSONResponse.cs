namespace Cov19API
{
    using System.Collections.Generic;

    public class JSONResponse<T>
    {
        public List<T> Data { get; set; }

        public int Length { get; set; }

        public string LastUpdate { get; set; }

        public int TotalPages { get; set; }
    }
}

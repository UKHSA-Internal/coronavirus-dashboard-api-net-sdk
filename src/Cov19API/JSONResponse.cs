namespace Cov19API
{
    using System.Collections.Generic;

    /// <summary>
    /// A response class with a strongly typed instance of data and API properties
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the value to return from the API</typeparam>
    public class JSONResponse<T>
    {
        /// <summary>
        /// The list of data returned from the API
        /// </summary>
        public List<T> Data { get; set; }

        /// <summary>
        /// The length of the data returned from the API
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// An ISO-8601 representation of the last time the API was called
        /// </summary>
        public string LastUpdate { get; set; }

        /// <summary>
        /// The number of pages in the API
        /// </summary>
        public int TotalPages { get; set; }
    }
}

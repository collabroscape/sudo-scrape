using System.Text.Json.Serialization;

namespace SudoScrape.Models
{
    /// <summary>
    /// Represents the result of a scraping operation
    /// </summary>
    public class ScrapeResponse
    {
        /// <summary>
        /// The URL that was scraped
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }
        /// <summary>
        /// Indicates whether navigation was successful
        /// </summary>
        [JsonPropertyName("navigation_successful")]
        public bool NavigationSuccessful { get; set; }
        /// <summary>
        /// The source document of the requested URL
        /// </summary>
        [JsonPropertyName("source_document")]
        public string SourceDocument { get; set; }
        /// <summary>
        /// The rendered document or markup from the requested URL
        /// </summary>
        [JsonPropertyName("rendered_document")]
        public string RenderedDocument { get; set; }
        /// <summary>
        /// A list of JSON responses received by the requested URL
        /// </summary>
        [JsonPropertyName("json_data")]
        public List<ScrapeDataItem> JsonData { get; set; }
        /// <summary>
        /// A list of XML responses received by the requested URL
        /// </summary>
        [JsonPropertyName("xml_data")]
        public List<ScrapeDataItem> XmlData { get; set; }
    }

    /// <summary>
    /// Represents a network response a page receives
    /// </summary>
    public class ScrapeDataItem
    {
        /// <summary>
        /// Content type of the response ("application/json" or "application/xml")
        /// </summary>
        [JsonPropertyName("content_type")]
        public string ContentType { get; set; }
        /// <summary>
        /// The authorization header for the response, if any
        /// </summary>
        [JsonPropertyName("auth_header")]
        public string AuthorizationHeader { get; set; }
        /// <summary>
        /// The network request issued by the page
        /// </summary>
        [JsonPropertyName("request_url")]
        public string RequestUrl { get; set; }
        /// <summary>
        /// The network response received by the page
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; }
        /// <summary>
        /// Whether response succeeded
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        /// <summary>
        /// Message regarding status of operation
        /// </summary>
        [JsonPropertyName("error_message")]
        public string ErrorMessage { get; set; }
    }
}

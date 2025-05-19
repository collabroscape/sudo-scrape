using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SudoScrape.Models
{
    /// <summary>
    /// Request to scrape a specified URL
    /// </summary>
    public class ScrapeRequest
    {
        /// <summary>
        /// The URL to scrape
        /// </summary>
        [JsonPropertyName("url")]
        [Required(ErrorMessage = "URL is required.")]
        [Url(ErrorMessage = "URL must be a valid absolute URL.")]
        public string Url { get; set; }
        /// <summary>
        /// Indicates whether to return the source document. In the case of single-page applications (SPAs), 
        /// this triggers the client side execution of code to render HTML or other markup
        /// </summary>
        [JsonPropertyName("return_source_document")]
        public bool ReturnSourceDocument { get; set; }
        /// <summary>
        /// Indicates whether to return the rendered document, for single-page applications (SPAs) that use
        /// client side execution of code to render HTML or other markup
        /// </summary>
        [JsonPropertyName("return_rendered_document")]
        public bool ReturnRenderedDocument { get; set; }
        /// <summary>
        /// Indicates whether to inspect all network responses when navigating to the URL, and return 
        /// any JSON responses the page receives
        /// </summary>
        [JsonPropertyName("return_json_data")]
        public bool ReturnJsonData { get; set; }
        /// <summary>
        /// Indicates whether to inspect all network responses when navigating to the URL, and return 
        /// any XML responses the page receives
        /// </summary>
        [JsonPropertyName("return_xml_data")]
        public bool ReturnXmlData { get; set; }
    }
}

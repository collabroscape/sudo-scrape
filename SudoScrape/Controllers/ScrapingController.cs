using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using SudoScrape.Models;

namespace SudoScrape.Controllers
{
    public class ScrapingController : Controller
    {
        /// <summary>
        /// Scrapes a web page using Playwright, optionally returning source HTML,
        /// rendered HTML, intercepted JSON responses, and intercepted XML responses,
        /// including Authorization headers from the requests.
        /// </summary>
        /// <param name="request">The scrape request details.</param>
        /// <returns>A ScrapeResponse containing the requested data.</returns>
        [HttpPost("/scrape")]
        [ProducesResponseType(typeof(ScrapeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Scrape([FromBody] ScrapeRequest request)
        {
            // Model validation will automatically trigger a 400 Bad Request
            // if [Required] or [Url] attributes fail, thanks to [ApiController]

            if (!request.ReturnSourceDocument && !request.ReturnRenderedDocument && !request.ReturnJsonData && !request.ReturnXmlData)
            {
                return BadRequest("At least one return type (return_source_document, return_rendered_document, return_json_data, return_xml_data) must be set to true.");
            }

            List<string> errorMessages = new List<string>();
            var response = new ScrapeResponse
            {
                Url = request.Url
            };
            var jsonDataItems = new List<ScrapeDataItem>();
            var xmlDataItems = new List<ScrapeDataItem>();
            var responseHandlerLock = new object();

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                if (request.ReturnJsonData || request.ReturnXmlData)
                {
                    page.Response += async (_, resp) =>
                    {
                        if (!resp.Ok) return;

                        string contentType = resp.Headers.TryGetValue("content-type", out var type) ? type.ToLowerInvariant() : string.Empty;
                        string requestUrl = resp.Request.Url;

                        // Extract Authorization header from the request
                        string authHeader = null;
                        if (resp.Request.Headers.TryGetValue("authorization", out var authValue)) // Header names are typically lowercase in Playwright's collection
                        {
                            authHeader = authValue;
                        }

                        if (request.ReturnJsonData && contentType.Contains("application/json"))
                        {
                            try
                            {
                                string body = await resp.TextAsync();
                                lock (responseHandlerLock)
                                {
                                    jsonDataItems.Add(new ScrapeDataItem
                                    {
                                        ContentType = "application/json",
                                        RequestUrl = requestUrl,
                                        Response = body,
                                        AuthorizationHeader = authHeader,
                                        Success = true
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                lock (responseHandlerLock)
                                {
                                    jsonDataItems.Add(new ScrapeDataItem
                                    {
                                        ContentType = "application/json",
                                        RequestUrl = requestUrl,
                                        AuthorizationHeader = authHeader,
                                        Success = false,
                                        ErrorMessage = ex.Message
                                    });
                                }
                            }
                        }
                        else if (request.ReturnXmlData && (contentType.Contains("application/xml") || contentType.Contains("text/xml")))
                        {
                            string actualContentType = contentType.Contains("application/xml") ? "application/xml" : "text/xml";
                            try
                            {
                                string body = await resp.TextAsync();
                                lock (responseHandlerLock)
                                {
                                    xmlDataItems.Add(new ScrapeDataItem
                                    {
                                        ContentType = actualContentType,
                                        RequestUrl = requestUrl,
                                        Response = body,
                                        AuthorizationHeader = authHeader,
                                        Success = true
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                lock (responseHandlerLock)
                                {
                                    xmlDataItems.Add(new ScrapeDataItem
                                    {
                                        ContentType = actualContentType,
                                        RequestUrl = requestUrl,
                                        AuthorizationHeader = authHeader,
                                        Success = false,
                                        ErrorMessage = ex.Message
                                    });
                                }
                            }
                        }
                    };
                }

                IResponse navigationResponse = null;
                try
                {
                    navigationResponse = await page.GotoAsync(request.Url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = 60000
                    });
                }
                catch (TimeoutException tex)
                {
                    if (navigationResponse == null)
                    {
                        errorMessages.Add("Navigation timed out before response.");
                    }
                }

                if (request.ReturnSourceDocument)
                {
                    if (navigationResponse != null && navigationResponse.Ok)
                    {
                        try
                        {
                            response.SourceDocument = await navigationResponse.TextAsync();
                            response.NavigationSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"Error getting source document: {ex.Message}.");
                        }
                    }
                    else if (navigationResponse != null)
                    {
                        errorMessages.Add($"Could not get source document, navigation status: {navigationResponse.Status}.");
                    }
                    else if (response.SourceDocument == null)
                    {
                        errorMessages.Add($"No navigation response received.");
                    }
                }

                if (request.ReturnRenderedDocument)
                {
                    try
                    {
                        response.RenderedDocument = await page.ContentAsync();
                        response.NavigationSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add($"Error getting rendered document: {ex.Message}.");
                    }
                }

                if (request.ReturnJsonData)
                {
                    response.JsonData = jsonDataItems;
                }
                if (request.ReturnXmlData)
                {
                    response.XmlData = xmlDataItems;
                }

                return Ok(response);
            }
            catch (PlaywrightException pex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Scraping failed due to a Playwright error: {pex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                // Clean up other resources
            }
        }
    }
}

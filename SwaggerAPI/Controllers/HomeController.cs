using Newtonsoft.Json;
using SwaggerAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwaggerAPI.Controllers
{
    public class HomeController : Controller
    {

        //Hosted web API REST Service base url  

        //https://badapi.iqvia.io/api/v1/Tweets?startDate=2016-03-20T04%3A07%3A56.271Z&endDate=2017-03-20T04%3A07%3A56.271Z

        //2018-03-20T04:07:56.271Z
        public string Baseurl = ConfigurationManager.AppSettings["baseUrl"];

        /// <summary>
        /// Please configure the logging path in Web.config.
        /// </summary>
        public static string LoggingFilePath = ConfigurationManager.AppSettings["path"];

        /// <summary>
        /// Method to get the tweets between two dates.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            string date1 = string.Empty;
            string date2 = string.Empty;           
            string FormattedURL = string.Empty;
            List<Tweet> tweets = new List<Tweet>(); 
            DateTime startDate = new DateTime(2016, 03, 01, 01, 01, 01, 0, DateTimeKind.Utc);

            List<Tweet> initialList = new List<Tweet>();
            List<Tweet> listToAdd = new List<Tweet>();

            //Pulling the data for 24 months by adding each month to base value of date;
            for (int i = 1; i < 25; i++)
            {
                date1 = startDate.AddMonths(i).ToString("o");//"2018-03-20T04:07:56.271Z";
                date2 = startDate.ToString("o");

                using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
                {
                    //Passing service base url  
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Clear();
                    //Define request data format  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = new HttpResponseMessage();

                    FormattedURL = String.Format("api/v1/Tweets?startDate={0}&endDate={1}", date2, date1);

                    //Sending request to find web api REST service resource GetAllTweets using HttpClient
                    try
                    {
                        response = await client.GetAsync(FormattedURL); 
                        //Checking the response is successful or not which is sent using HttpClient  
                        if (response.IsSuccessStatusCode)
                        {
                            var TweetsChunk = response.Content.ReadAsStringAsync().Result;
                            //Deserializing the response recieved from web api and storing into the Tweets list                                 
                            listToAdd = JsonConvert.DeserializeObject<List<Tweet>>(TweetsChunk);
                        }                         
                    }
                    catch (Exception ex)
                    {
                        //Logging activity goes here;
                        client.GetAsync(FormattedURL).GetAwaiter().GetResult();
                        continue;
                    }
                    //Adding to the initial list. 
                    initialList.AddRange(listToAdd);
                }
            }
            return View(initialList);
        }

        public class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // WriteAllLines creates a file, writes a collection of strings to the file,
                // and then closes the file.  You do NOT need to call Flush() or Close().
                System.IO.File.WriteAllText(LoggingFilePath, "Request");
                System.IO.File.WriteAllText(LoggingFilePath, request.ToString());
                 
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken); 
                 
                System.IO.File.WriteAllText(LoggingFilePath, "Response");
                System.IO.File.WriteAllText(LoggingFilePath, response.ToString());

                if (response.Content != null)
                {
                    System.IO.File.WriteAllText(LoggingFilePath, response.ReasonPhrase);
                } 

                return response;
            }
        }
    }    
}
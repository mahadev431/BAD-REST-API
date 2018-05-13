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

        //public const string readonly Baseurl =;
        //Hosted web API REST Service base url  

        //https://badapi.iqvia.io/api/v1/Tweets?startDate=2016-03-20T04%3A07%3A56.271Z&endDate=2017-03-20T04%3A07%3A56.271Z

        //2018-03-20T04:07:56.271Z

        /// <summary>
        /// Method to get the tweets between two dates.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            string date1 = string.Empty;
            string date2 = string.Empty;
            string Baseurl= ConfigurationManager.AppSettings["baseUrl"];
            List<Tweet> tweets = new List<Tweet>(); 
            DateTime startDate = new DateTime(2016, 03, 01, 01, 01, 01, 0, DateTimeKind.Utc);

            List<Tweet> initialList = new List<Tweet>();
            List<Tweet> listToAdd = new List<Tweet>();

            //Pulling the data for 24 months by adding each month to base value of date;
            for (int i = 1; i < 25; i++)
            {
                date1 = startDate.AddMonths(i).ToString("o");//"2018-03-20T04:07:56.271Z";
                date2 = startDate.ToString("o");

                using (var client = new HttpClient())
                {
                    //Passing service base url  
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Clear();
                    //Define request data format  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = new HttpResponseMessage();
                    //Sending request to find web api REST service resource GetAllTweets using HttpClient
                    try
                    {
                        response = await client.GetAsync("api/v1/Tweets?startDate=" + date2 + "&endDate=" + date1 + "");
                    }
                    catch (Exception ex)
                    {
                        //Logging activity goes here;
                        continue;
                    }
                    //Checking the response is successful or not which is sent using HttpClient  
                    if (response.IsSuccessStatusCode)
                    {
                        switch (response.StatusCode.GetHashCode())
                        {
                            case 200:
                                //Storing the response details recieved from web api   
                                var TweetsChunk = response.Content.ReadAsStringAsync().Result; 
                                //Deserializing the response recieved from web api and storing into the Tweets list                                 
                                listToAdd = JsonConvert.DeserializeObject<List<Tweet>>(TweetsChunk);
                                break;
                            case 400:
                                //return HttpResponseMessage
                                break;
                            case 500:
                                //return HttpResponseMessage
                                break;
                        }
                    }
                    //Adding to the initial list. 
                    initialList.AddRange(listToAdd);
                }
            }
            return View(initialList);
        }
    }    
}

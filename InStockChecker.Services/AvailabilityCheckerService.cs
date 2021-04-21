using InStockChecker.Models;
using InStockChecker.Utility;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.Json;

namespace InStockChecker.Services
{
    public class AvailabilityCheckerService
    {
        private readonly string urlToCheck = ConfigurationManager.AppSettings.Get(AppConfig.UrlToCheck);

        public bool CheckItemAvailability(string itemId)
        {
            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(urlToCheck + itemId);

            using (Stream objStream = wrGETURL.GetResponse().GetResponseStream())
            using (StreamReader objReader = new StreamReader(objStream))
            {
                OPNTCResponseModel response;

                try
                {
                    response = JsonSerializer.Deserialize<OPNTCResponseModel>(objReader.ReadToEnd());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not deserialize object! Message: \'{e.Message}\'");
                    throw;
                }

                Item jerryCase;

                try
                {
                    if (response.data.items.TryGetValue(itemId, out jerryCase))
                    {
                        if (jerryCase.stock == 1)
                        {
                            return true;
                        }
                    }
                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine($"Could not check availability! \'{nre.Message}\'");
                    throw;
                }

                return false;
            }
        }
    }
}
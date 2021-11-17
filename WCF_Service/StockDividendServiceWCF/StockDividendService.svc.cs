using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace StockDividendServiceWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "StockDividendService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select StockDividendService.svc or StockDividendService.svc.cs at the Solution Explorer and start debugging.
    public class StockDividendService : IStockDividendService
    {
        public string GetData()
        {
            string data = String.Empty;

            string url = "https://www.moneycontrol.com/stocks/marketinfo/dividends_declared/index.php";

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        data = content.ReadAsStringAsync().Result;
                    }
                }
            }
            return data;
        }
    }
}

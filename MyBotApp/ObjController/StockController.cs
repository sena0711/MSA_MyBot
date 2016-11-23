using System;
using System.Collections.Generic;
using System.Linq;
//using System.Web;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MyBotApp.Object1;

namespace MyBotApp.ObjController
{
    class StockController
    {
        public static async Task<string> GetStock(string strStock)
        {
            string replyString = string.Empty;
            double? stockValue = await StockController.GetStockPriceAsync(strStock);
            string stockName= strStock.ToUpper();
            if (null == stockValue)   // might be a company name rather than a stock ticker name
            {
                string strTicker = await GetStockTickerName(strStock);
                if (string.Empty != strTicker)
                {
                    stockValue = await StockController.GetStockPriceAsync(strTicker);
                    strStock = strTicker;
                    
                }
            }

            // return our reply to the user
            if (null == stockValue)
            {
                replyString = string.Format("Stock {0} is not valid", stockName);
            }
            else
            {
                replyString = string.Format("Stock: {0}, Value: {1}", stockName, stockValue);
            }

            return replyString;
        }

        private static async Task<double?> GetStockPriceAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            string url = $"http://finance.yahoo.com/d/quotes.csv?s={symbol}&f=sl1";
            string csv;
            using (WebClient client = new WebClient())
            {
                csv = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }
            string line = csv.Split('\n')[0];
            string price = line.Split(',')[1];
            double result;

            if (double.TryParse(price, out result))
                return result;

            return null;
        }

        private static async Task<string> GetStockTickerName(string strCompanyName)
        {
            string replyString = string.Empty;
            string url = $"http://d.yimg.com/autoc.finance.yahoo.com/autoc?query={strCompanyName}&region=1&lang=en&callback=YAHOO.Finance.SymbolSuggest.ssCallback";
            string sJson = string.Empty;
            using (WebClient client = new WebClient())
            {
                sJson = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }

            sJson = StripJsonString(sJson);
            YhooCompanyLookup lookup = null;
            try
            {
                lookup = JsonConvert.DeserializeObject<YhooCompanyLookup>(sJson);
            }
            catch (Exception e)
            {

            }

            if (null != lookup)
            {
                foreach (lResult r in lookup.ResultSet.Result)
                {
                    if (r.exch == "NAS")
                    {
                        replyString = r.symbol;
                        break;
                    }
                }
            }

            return replyString;
        }

        // String retrurned from StockController Company name lookup contains more than raw JSON
        // strip off the front/back to get to raw JSON
        private static string StripJsonString(string sJson)
        {
            int iPos = sJson.IndexOf('(');
            if (-1 != iPos)
            {
                sJson = sJson.Substring(iPos + 1);
            }

            iPos = sJson.LastIndexOf(')');
            if (-1 != iPos)
            {
                sJson = sJson.Substring(0, iPos);
            }

            return sJson;
        }
    }

}
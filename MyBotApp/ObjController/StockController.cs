using System;
using System.Collections.Generic;
using System.Linq;
//using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Text;
using Newtonsoft.Json;
using MyBotApp.Object1;


namespace MyBotApp.ObjController
{ 
    class StockController
    {
        public static async Task<string> GetStock(string strStock)
        {
            string replyString = string.Empty;

            string strsymbol;
             string strname;
            string exch;
          
            /////////////////////////////////////////
            string url = $"http://d.yimg.com/autoc.finance.yahoo.com/autoc?query={strStock}&region=1&lang=en&callback=YAHOO.Finance.SymbolSuggest.ssCallback";
            string x = string.Empty;
            double? stockValue = null;
            string stockName;
            using (WebClient client = new WebClient())
            {
                x = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }
            x = StripJsonString(x);
            StockObj.YhooCompanyLookup lookup = null;
            try
            {
                lookup = JsonConvert.DeserializeObject<StockObj.YhooCompanyLookup>(x);
            }
            catch (Exception e){}

            if (null != lookup)
            {

                foreach (StockObj.lResult r in lookup.ResultSet.Result)
                {
                    if (r.typeDisp == "Equity")
                    {
                       strsymbol      = r.symbol;
                       strname        =r.name;
                        exch = r.type;
                    
                       stockValue = await GetStockPriceAsync(strsymbol);
                       stockName = strname;


                        if (null == stockValue)
                        {
                            stockValue = await GetStockPriceAsync(strname);
                        }
                        else
                        {  //string logo = await GetCompanyLogoAsync(strname);
                            replyString = string.Format("Symbol: {0}, Name:{1},Value: {2}", strsymbol, strname, stockValue);
                            
                        }

                        break;
                    }
                }
            }
            //////////////////////////////////
            if (null == stockValue)   // might work with original  search
            {
                stockValue = await GetStockPriceAsync(strStock);
            }
            // return our reply to the user
            if (null == stockValue)   
            { 
                stockName = strStock.ToUpper();
                replyString = string.Format("Stock {0} is not valid", stockName);
            }
            //else
            //{
            //    stockName = strStock.ToUpper();
            //    replyString = string.Format("Stock: {0}{1}, Value: {1}", stockName, stockValue);
            //}

            return replyString;
        }

        private static async Task<string> GetCompanyLogoAsync(string name)
        {
            LogoObj.Class1 classobj;

            HttpClient client = new HttpClient();
            string url = await client.GetStringAsync(new Uri("https://autocomplete.clearbit.com/v1/companies/suggest?query=" + name + "&units=metric&APPID=sk_b0026f9a060c16eeea101871873ab952"));

            classobj = JsonConvert.DeserializeObject<LogoObj.Class1>(url);
            
            string strstockdomain = classobj.domain;
            string strstockname = classobj.name;
            string strstocklogo = classobj.logo;

            return strstocklogo;
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
            string x = string.Empty;
            using (WebClient client = new WebClient())
            {
                x = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }

            x = StripJsonString(x);
            StockObj.YhooCompanyLookup lookup = null;
            try
            {
                lookup = JsonConvert.DeserializeObject<StockObj.YhooCompanyLookup>(x);  
            }
            catch (Exception e)
            {

            }

            if (null != lookup)
            {
                
                foreach (StockObj.lResult r in lookup.ResultSet.Result)
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
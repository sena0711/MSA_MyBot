using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyBotApp.Object1
{
    public class StockObj
    {
        public class lResult
        {
            public string symbol { get; set; }
            public string name { get; set; }
            public string exch { get; set; }
            public string type { get; set; }
            public string exchDisp { get; set; }
            public string typeDisp { get; set; }
        }

        public class ResultSet
        {
            public string Query { get; set; }
            public lResult[] Result { get; set; }
        }

        public class YhooCompanyLookup
        {
            public ResultSet ResultSet { get; set; }
        }
    }
    public class LogoObj
    {
        public class RootObject
        {
            public Class1[] Property1 { get; set; }
        }

        public class Class1
        {
            public string domain { get; set; }
            public string logo { get; set; }
            public string name { get; set; }
        }
    }
  

    

}

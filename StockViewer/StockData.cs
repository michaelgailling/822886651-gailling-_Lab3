using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Lab 3 Question 1
//Michael Gailling
//822886651
//Comp 212
//Section 003

namespace StockViewer
{
    class StockData
    {
        public string Symbol { get; set; }
        public string Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }

        public StockData()
        {
            ;
        }

        public StockData(string symbol, string date, string open, string high, string low, string close)
        {
            this.Symbol = symbol;
            this.Date = date;
            this.Open = Convert.ToDecimal(open);
            this.High = Convert.ToDecimal(high);
            this.Low = Convert.ToDecimal(low);
            this.Close = Convert.ToDecimal(close);
        }

    }
}

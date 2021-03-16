using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//Lab 3 Question 1
//Michael Gailling
//822886651
//Comp 212
//Section 003

namespace StockViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<StockData> stockData = new List<StockData>();

        public MainWindow()
        {
            InitializeComponent();
            LoadCSV();
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            ShowAllRecords();
            lblAlert.Content = $"";
        }

        private async void LoadCSV()
        {
            stockData = await Task.Run(() => {
                //Uncomment the line below to induce artificial delay
                //System.Threading.Thread.Sleep(10000);
                List<StockData> fileData = new List<StockData>();

                if (File.Exists("stockData.csv"))
                {
                    using (StreamReader stream = new StreamReader("stockData.csv"))
                    {
                        string row;

                        if ((row = stream.ReadLine()) != null)
                        {
                            string[] columns;

                            StockData record;

                            while ((row = stream.ReadLine()) != null)
                            {
                                
                                bool containsNegative = (row.Contains("(") || row.Contains(")") || row.Contains("-"));

                                if (!containsNegative)
                                {
                                    row = CleanInput(row);

                                    columns = row.Split(',');

                                    record = new StockData(columns[0], columns[1], columns[2], columns[3], columns[4], columns[5]);

                                    fileData.Add(record);
                                }
                            }
                        }
                    }
                }

                return fileData;
            });

            ShowAllRecords();
        }

        private string CleanInput(string input)
        {
            string result = input;

            Regex badComma = new Regex(@"\d{1}\,{1}\d{1}");

            string[] invalidChars = { "$", "\"", " " };

            if (badComma.IsMatch(result))
            {
                MatchCollection matches = badComma.Matches(result);

                int badCommaIndex;

                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    badCommaIndex = matches[i].Index + 1;

                    result = result.Remove(badCommaIndex, 1);
                }

            }

            foreach (string invalid in invalidChars)
            {
                result = result.Replace(invalid, "");
            }

            return result;
        }


        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string symbol = txtStockSymbol.Text.ToUpper();

            if (symbol.Length != 0)
            {
                List<StockData> companyStockData = await Task.Run(() => {
                    //Uncomment the line below to induce artificial delay
                    //System.Threading.Thread.Sleep(1000);
                    return new List<StockData>(stockData.Where(x => x.Symbol == symbol));
                });

                dgStockRecords.ItemsSource = companyStockData;

                SortByDate();

                lblAlert.Content = $"{companyStockData.Count} Results Found";
            }
            else
            {
                lblAlert.Content = "Invalid Input";
            }
            
        }

        private async void btnCalcFactorial_Click(object sender, RoutedEventArgs e)
        {
            string userInput = txtFactorialInput.Text;

            tbFactorialOutput.Text = await Task.Run(() => {
                //Uncomment the line below to induce artificial delay
                //System.Threading.Thread.Sleep(10000);
                if (userInput != "0")
                {
                    ulong result = 0;

                    string output = "";

                    bool validInput = ulong.TryParse(userInput, out result);

                    if (validInput)
                    {
                        for (ulong i = result - 1; i > 0; i--)
                        {
                            result *= i;
                        }

                        output = result.ToString();
                    }
                    else
                    {
                        output = "Invalid Input";
                    }

                    return output;
                }

                return "1";
            });
        }

        private void SortByDate()
        {
            dgStockRecords.Items.SortDescriptions.Clear();

            dgStockRecords.Items.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));

            dgStockRecords.Items.Refresh();
        }

        private void ShowAllRecords()
        {
            dgStockRecords.ItemsSource = stockData;

            SortByDate();
        }



        
    }
}

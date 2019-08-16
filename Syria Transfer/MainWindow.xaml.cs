using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace Syria_Transfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// HK11712
    /// </summary>
    public partial class MainWindow : Window
    {

        string loginURL = "https://abili.syriatel.com.sy/Login.aspx";
        string rechrgeURL = "https://abili.syriatel.com.sy/Recharge.aspx";

        string viberLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Viber\\Viber.exe";

        string numberString;
        int transferAmount = 1000, price = 4000;

        int price200syp = 800;

        HttpClient client;
        HttpClientHandler handler;
        public MainWindow()
        {
            InitializeComponent();
            textBox_Amount.TextChanged += textBox_Amount_TextChanged;
            textBox_Number.Focus();
        }

        async private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateSentDay(DateTime.Now.AddHours(-2));

            handler = new HttpClientHandler();
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            //HttpResponseMessage response;
            //string responseString;
            //response = await client.GetAsync(loginURL);
            //responseString = await response.Content.ReadAsStringAsync();

            //var values = extractValues(responseString);
            //values.Add(new KeyValuePair<string, string>("UsernameTextBox", App.Username));
            //values.Add(new KeyValuePair<string, string>("PasswordTextBox", App.Password));
            //values.Add(new KeyValuePair<string, string>("SubmitButton", "Login"));
            //FormUrlEncodedContent postLoginContent = new FormUrlEncodedContent(values);

            //response = await client.PostAsync(loginURL, postLoginContent);

            //response = await client.GetAsync(rechrgeURL);
            //responseString = await response.Content.ReadAsStringAsync();
            //string balanceString = Regex.Match(responseString, "MainContentPlaceHolder_PointOfSalesMainContentPlaceHolder_BalanceText\"\\>(.*?)\\</span").Groups[1].Value;
            //label_Balance.Content = "Balance = " + balanceString;

            button_Transfer.IsEnabled = true;
        }

        List<KeyValuePair<string, string>> extractValues(string body)
        {
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            var ms = Regex.Matches(body, "type=\"hidden\" name=\"(.*?)\".*value=\"(.*)\"");
            foreach (Match m in ms)
            {
                string key = m.Groups[1].Value;
                string value = m.Groups[2].Value;
                values.Add(new KeyValuePair<string, string>(key, value));
            }
            return values;
        }

        async private void button_Transfer_Click(object sender, RoutedEventArgs e)
        {
            numberString = textBox_Number.Text;


            TelCompany com = (TelCompany)button_Transfer.Tag;
            if (com == TelCompany.MTN)
            {
                button_Transfer.IsEnabled = false;
                Clipboard.SetText(string.Format("{0}\r\n\r\n{1} وحدة", numberString, transferAmount));
                Process.Start(viberLocation);
                labelInfo.Content = string.Format("{0} SYP await to be sent by RIAD to {1}", transferAmount, numberString); ;
                labelInfo.Foreground = Brushes.Green;

                addTransfer();
            }
            else
            {
                labelOldBalance.Content = "old b" + (label_Balance.Content as string).TrimStart(new char[] { 'B' });
                WindowConfirmation win = new WindowConfirmation(numberString, transferAmount);
                if (win.ShowDialog() == true)
                {
                    int amount = transferAmount;
                    int amountToBeSent;

                    button_Transfer.IsEnabled = false;

                    while (amount > 0)
                    {
                        if (amount == 1100)
                            amountToBeSent = 1000;
                        else if (amount == 700)
                            amountToBeSent = 600;
                        else if (amount > 1500)
                            amountToBeSent = 1500;
                        else
                            amountToBeSent = amount;

                        labelInfo.Content = string.Format("transferring {0} to {1} ....", amountToBeSent, numberString); ;
                        labelInfo.Foreground = Brushes.Black;

                        try
                        {
                            if (!await transferCredits(numberString, amountToBeSent.ToString()))
                                return;
                        }
                        catch (Exception ex)
                        {
                            labelInfo.Content = ex.Message;
                            labelInfo.Foreground = Brushes.Red;
                        }
                        labelInfo.Content = string.Format("{0} transferred to {1}", amount, numberString); ;
                        labelInfo.Foreground = Brushes.Brown;
                        amount -= amountToBeSent;
                    }


                    labelInfo.Content = string.Format("{0} SYP transferred successfuly to {1}", transferAmount, numberString); ;
                    labelInfo.Foreground = Brushes.Green;
                    addTransfer();
                }
            }
        }

        private void addTransfer()
        {
            Transfer.GetTransfers();
            Transfer t = new Transfer(DateTime.Now, numberString, transferAmount, price);
            App.Transfers.Insert(0, t);
            Transfer.SaveTransfers();

            updateSentDay(DateTime.Now.AddHours(-2));
        }

        void updateSentDay(DateTime date)
        {
            labelDate.Content = date.ToString("dd-MM-yyyy");
            labelAmountSent.Content = "SYP sent = " + App.Transfers.Where(
                t => t.Date.AddHours(-2).ToShortDateString().Equals(date.ToShortDateString())).Sum(d => d.Amount).ToString();
            labelMoney.Content = "Money = " + App.Transfers.Where(
                t => t.Date.AddHours(-2).ToShortDateString().Equals(date.ToShortDateString())).Sum(d => d.Price).ToString();

            if (date.ToShortDateString().Equals(DateTime.Now.AddHours(-2).ToShortDateString()))
            {
                labelTotalAmountSent.Content = "";
                labelTotalMoney.Content = "";
            }
            else
            {
                DateTime date2 = new DateTime(date.Year, date.Month, date.Day, 2, 0, 0);
                labelTotalAmountSent.Content = "SYP sent = " + App.Transfers.Where(
                                t => t.Date < DateTime.Now && t.Date > date).Sum(d => d.Amount).ToString();
                labelTotalMoney.Content = "Money = " + App.Transfers.Where(
                                 t => t.Date < DateTime.Now && t.Date > date).Sum(d => d.Price).ToString();
            }
        }

        async Task<bool> transferCredits(string number, string transferAmount)
        {
            await Task.Delay(2000);
            HttpResponseMessage response;
            string responseString;

            response = await client.GetAsync(rechrgeURL);
            responseString = await response.Content.ReadAsStringAsync();

            var values = extractValues(responseString);

            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$SubscriberMSIDNTextBox", number));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$ConfirmSubscriberMSISDNTextBox", number));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$RechargeAmountTextBox", transferAmount));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$NotificationDropDownList", "1"));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$RechargeSubmitButton", "Recharge"));
            FormUrlEncodedContent postLoginContent = new FormUrlEncodedContent(values);

            response = await client.PostAsync(rechrgeURL, postLoginContent);
            responseString = await response.Content.ReadAsStringAsync();
            if (!responseString.Contains("successfully"))
            {
                string error = Regex.Match(responseString, "style=\"color:Red;\">(.*?)</span").Groups[1].Value;
                labelInfo.Content = error;
                labelInfo.Foreground = Brushes.Red;
                return false;
            }

            string balanceString = Regex.Match(responseString, "MainContentPlaceHolder_PointOfSalesMainContentPlaceHolder_BalanceText\"\\>(.*?)\\</span").Groups[1].Value;
            label_Balance.Content = "Balance = " + balanceString;
            return true;
        }

        private void textbox_Focus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.SelectAll();
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(App.transfersPath);
        }

        private void labelDate_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string dateString = labelDate.Content as string;
            DateTime date = DateTime.Parse(dateString);
            updateSentDay(date.AddDays(-1));
        }

        private void labelDate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string dateString = labelDate.Content as string;
            DateTime date = DateTime.Parse(dateString);

            if (date.ToShortDateString() == DateTime.Now.AddHours(-2).ToShortDateString())
                return;
            updateSentDay(date.AddDays(1));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(viberLocation);

        }

        private void textBox_Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox_Number.Text.Length > 0 && textBox_Number.Text[0] > 1600)
            {
                StringBuilder build = new StringBuilder();
                foreach (char c in textBox_Number.Text)
                {
                    if (c > 1600)
                        build.Append(char.ConvertFromUtf32(c - 1584));
                    else
                        build.Append(c);
                }
                if (build[0] != '0')
                    build.Insert(0, '0');
                textBox_Number.TextChanged -= textBox_Number_TextChanged;
                textBox_Number.Text = build.ToString();
                textBox_Number.TextChanged += textBox_Number_TextChanged;
            }
            //if (textBox_Number.Text.StartsWith("094") || textBox_Number.Text.StartsWith("095") || textBox_Number.Text.StartsWith("096"))
            {
                button_Transfer.Content = "Copy, Open Viber";
                button_Transfer.Tag = TelCompany.MTN;
            }
            //else
            //{
            //    button_Transfer.Content = "Transfer";
            //    button_Transfer.Tag = TelCompany.Syriatel;
            //}
        }

        private void textBox_Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            price200syp = 800;
            if (int.TryParse(textBox_Amount.Text, out transferAmount))
            {
                if (transferAmount > 5000)
                {
                    price200syp = 0;
                    button_Transfer.IsEnabled = false;
                    textBox_Amount.Background = Brushes.Red;
                }
                else if (transferAmount > 1000)
                {
                    //if (transferAmount == 2500)
                    //    price200syp = 801;
                    //else if (transferAmount > 3000)
                    //    price200syp = 933;
                    button_Transfer.IsEnabled = true;
                    textBox_Amount.Background = Brushes.LightYellow;
                }
                else
                {
                    button_Transfer.IsEnabled = true;
                    textBox_Amount.Background = Brushes.LightGreen;
                }

                double priceCorrection = transferAmount / 200.0 * price200syp;
                priceCorrection = priceCorrection / 500;
                price = (int)(Math.Ceiling(priceCorrection) * 500);

                labelPrice.Content = price.ToString();
            }
            else
            {
                button_Transfer.IsEnabled = false;
                textBox_Amount.Background = Brushes.Red;
            }
        }
    }

    public enum TelCompany
    {
        Syriatel,
        MTN
    }
    public class Transfer
    {
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }
        public TelCompany Company { get; set; }

        public Transfer(DateTime date, string number, int amount, int price)
        {
            Date = date;
            Number = number;
            Amount = amount;
            Price = price;
            if (Number[2] == '4' || Number[2] == '5' || Number[2] == '6') Company = TelCompany.MTN;
        }
        public Transfer(string s)
        {
            string[] data = s.Split(new char[] { ',' });

            //Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(data[0]), TimeZoneInfo.Local);
            Date = DateTime.ParseExact(data[0], "dd-MMM-yy hh:mm:ss tt", CultureInfo.CurrentCulture);
            Number = data[1];
            Amount = int.Parse(data[2]);
            Price = int.Parse(data[3]);
            if (Number[2] == '4' || Number[2] == '5' || Number[2] == '6') Company = TelCompany.MTN;
        }


        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", Date.ToString("dd-MMM-yy hh:mm:ss tt"), Number, Amount, Price);
        }
        public static void SaveTransfers()
        {
            File.WriteAllLines(App.transfersPath, App.Transfers.ConvertAll<string>(t => t.ToString()));

        }
        public static void GetTransfers()
        {
            if (File.Exists(App.transfersPath))
            {
                App.Transfers.Clear();
                foreach (string s in File.ReadAllLines(App.transfersPath))
                {
                    App.Transfers.Add(new Transfer(s));
                }
            }
            else
                File.Create(App.transfersPath);
        }
    }
}

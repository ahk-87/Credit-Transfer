using System;
using System.Collections.Generic;
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

        string loginURL = "https://newabili.syriatel.com.sy/Login.aspx";
        string rechrgeURL = "https://newabili.syriatel.com.sy/Recharge.aspx";

        string username = "HK11712";
        string password = "fnsn4575";

        int transferAmount, Price;

        double balance = 0;
        int price200syp = 1000;

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
            handler = new HttpClientHandler();
            client = new HttpClient(handler);

            HttpResponseMessage response;
            string responseString;
            response = await client.GetAsync(loginURL);
            responseString = await response.Content.ReadAsStringAsync();

            var values = extractValues(responseString);
            values.Add(new KeyValuePair<string, string>("UsernameTextBox", username));
            values.Add(new KeyValuePair<string, string>("PasswordTextBox", password));
            values.Add(new KeyValuePair<string, string>("SubmitButton", "Login"));
            FormUrlEncodedContent postLoginContent = new FormUrlEncodedContent(values);

            response = await client.PostAsync(loginURL, postLoginContent);

            response = await client.GetAsync(rechrgeURL);
            responseString = await response.Content.ReadAsStringAsync();
            string balanceString = Regex.Match(responseString, "MainContentPlaceHolder_PointOfSalesMainContentPlaceHolder_BalanceText\"\\>(.*?)\\</span").Groups[1].Value;
            label_Balance.Content = "Balance = " + balanceString;

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
            string number = textBox_Number.Text;
            string transferAmount = textBox_Amount.Text;
            WindowConfirmation win = new WindowConfirmation(number, transferAmount);
            if (win.ShowDialog() == true)
            {
                HttpResponseMessage response;
                string responseString;

                response = await client.GetAsync(rechrgeURL);
                responseString = await response.Content.ReadAsStringAsync();

                var values = extractValues(responseString);
                values.Add(new KeyValuePair<string, string>("ctl00%24ctl00%24MainContentPlaceHolder%24PointOfSalesMainContentPlaceHolder%24SubscriberMSIDNTextBox", number));
                values.Add(new KeyValuePair<string, string>("ctl00%24ctl00%24MainContentPlaceHolder%24PointOfSalesMainContentPlaceHolder%24ConfirmSubscriberMSISDNTextBox", number));
                values.Add(new KeyValuePair<string, string>("ctl00%24ctl00%24MainContentPlaceHolder%24PointOfSalesMainContentPlaceHolder%24RechargeAmountTextBox", transferAmount));
                values.Add(new KeyValuePair<string, string>("ctl00%24ctl00%24MainContentPlaceHolder%24PointOfSalesMainContentPlaceHolder%24NotificationDropDownList", "1"));
                values.Add(new KeyValuePair<string, string>("ctl00%24ctl00%24MainContentPlaceHolder%24PointOfSalesMainContentPlaceHolder%24RechargeSubmitButton", "Recharge"));
                FormUrlEncodedContent postLoginContent = new FormUrlEncodedContent(values);

                response = await client.PostAsync(rechrgeURL, postLoginContent);
                responseString = await response.Content.ReadAsStringAsync();
                //0936158477
            }

        }

         bool transferCredits()
        {
            return false;
        }

        private void textbox_Focus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.SelectAll();
        }

        private void textBox_Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            price200syp = 1000;
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
                    if (transferAmount > 4000)
                        price200syp = 920;
                    else if (transferAmount > 3000)
                        price200syp = 933;
                    button_Transfer.IsEnabled = true;
                    textBox_Amount.Background = Brushes.LightYellow;
                }
                else
                {
                    button_Transfer.IsEnabled = true;
                    textBox_Amount.Background = Brushes.LightGreen;
                }

                decimal priceCorrection = transferAmount / 200 * price200syp;
                priceCorrection = priceCorrection / 500;
                Price = (int)(Math.Ceiling(priceCorrection) * 500);

                labelPrice.Content = Price.ToString();
            }
            else
            {
                button_Transfer.IsEnabled = false;
                textBox_Amount.Background = Brushes.Red;
            }
        }
    }

    public class Transfer
    {

        public DateTime Date { get; set; }
        public string Number { get; set; }
        public DateTime Amount { get; set; }
        public DateTime Price { get; set; }
        public static void SaveTransfers()
        {

        }
        public static void GetTransfers()
        {

        }
    }
}

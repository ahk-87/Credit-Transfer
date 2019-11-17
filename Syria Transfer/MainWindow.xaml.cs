using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

        string Url1 = "https://services.mtn.com.sy:8443/agentportal/agentportal/Application.html?locale=en";
        string Url2 = "https://services.mtn.com.sy:8443/agentportal/agentportal/agentportal_service";
        string post1 = "7|0|5|https://services.mtn.com.sy:8443/agentportal/agentportal/|6B44B0C517CC73B42098F57E34D88759|com.seamless.ers.client.agentPortal.client.common.AgentPortalService|getLoggedinAgent|java.lang.String/2004016611|1|2|3|4|1|5|0|";

        //string viberLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Viber\\Viber.exe";
        string firefoxLocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Mozilla Firefox\\firefox.exe";

        bool balanceOK = false;
        string errorMessage = "";
        string numberString;

        int price200syp = 800;
        int transferAmount = 1000, price = 4000;
        int oldBalance, newBalance;

        HttpClient clientSyriatel, clientMTN;
        HttpClientHandler handlersyriatel, handlerMTN;
        public MainWindow()
        {
            InitializeComponent();

            labelPrice.Content = price200syp * 5;

            textBox_Amount.TextChanged += textBox_Amount_TextChanged;
            textBox_Number.TextChanged += textBox_Number_TextChanged;
            textBox_Number.Focus();
        }


        async private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateSentDay(DateTime.Now.AddHours(-2));

            //WebBrowser wb = new WebBrowser();
            //wb.Navigate(Url1);
            //wb.Navigated += Wb_Navigated;

            //handlerMTN = new HttpClientHandler();
            //handlerMTN.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            //clientMTN = new HttpClient(handlerMTN);
            //clientMTN.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
            //clientMTN.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            //clientMTN.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            //clientMTN.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            //clientMTN.DefaultRequestHeaders.Add("X-GWT-Permutation", "A4B98984F154B612806089AA25D3512E");
            //clientMTN.DefaultRequestHeaders.Add("X-GWT-Module-Base", "https://services.mtn.com.sy:8443/agentportal/agentportal/");

            //StringContent sc = new StringContent(post1, Encoding.UTF8, "text/x-gwt-rpc");
            //HttpResponseMessage res = await clientMTN.PostAsync(Url2, sc);
            //string ss = await res.Content.ReadAsStringAsync();


            //HttpResponseMessage response1 = await clientMTN.GetAsync(Url1);
            //string s = await response1.Content.ReadAsStringAsync();

            WebProxy proxy = new WebProxy(App.ProxyAddress, App.ProxyPort);
            proxy.BypassProxyOnLocal = false;
            handlersyriatel = new HttpClientHandler();
            handlersyriatel.Proxy = proxy;
            clientSyriatel = new HttpClient(handlersyriatel);
            clientSyriatel.Timeout = new TimeSpan(0, 2, 0);
            clientSyriatel.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");
            clientSyriatel.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            clientSyriatel.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            clientSyriatel.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            HttpResponseMessage response;
            string responseString;
            try
            {
                labelInfo.Content = string.Format("1st try, proxy = {0}:{1}", App.ProxyAddress, App.ProxyPort);
                response = await clientSyriatel.GetAsync(loginURL);
                responseString = await response.Content.ReadAsStringAsync();
                labelInfo.Content = "Success from 1st try";
                goto success;
            }
            catch (Exception ex)
            {
                responseString = "error";
            }

            if (responseString == "error")
            {
                labelInfo.Content = "2nd try, proxy = 82.137.244.73:8080";
                proxy = new WebProxy("82.137.244.73", 8080);
                proxy.BypassProxyOnLocal = false;
                handlersyriatel = new HttpClientHandler();
                handlersyriatel.Proxy = proxy;
            }

            try
            {
                response = await clientSyriatel.GetAsync(loginURL);
                responseString = await response.Content.ReadAsStringAsync();
                labelInfo.Content = "Success from 2nd try";
                goto success;
            }
            catch
            {
                responseString = "error";
            }

            if (responseString == "error")
            {
                labelInfo.Content = "3rd try, proxy = 82.137.244.73:8080";
                proxy = new WebProxy("82.137.244.73", 8080);
                proxy.BypassProxyOnLocal = false;
                handlersyriatel = new HttpClientHandler();
                handlersyriatel.Proxy = proxy;
            }

            try
            {
                response = await clientSyriatel.GetAsync(loginURL);
                responseString = await response.Content.ReadAsStringAsync();
                labelInfo.Content = "Success from 3rd try";
                goto success;
            }
            catch
            {
                responseString = "error";
            }

            if (responseString == "error")
            {
                labelInfo.Content = "4th try, proxy = 82.137.244.74:8080";
                proxy = new WebProxy("82.137.244.74", 8080);
                proxy.BypassProxyOnLocal = false;
                handlersyriatel = new HttpClientHandler();
                handlersyriatel.Proxy = proxy;
            }

            try
            {
                response = await clientSyriatel.GetAsync(loginURL);
                responseString = await response.Content.ReadAsStringAsync();
                labelInfo.Content = "Success from 4th try";
            }
            catch
            {
                labelInfo.Content = "error, you are loser!";
                return;
            }

        success:
            var values = extractValues(responseString);
            values.Add(new KeyValuePair<string, string>("UsernameTextBox", App.Username));
            values.Add(new KeyValuePair<string, string>("PasswordTextBox", App.Password));
            values.Add(new KeyValuePair<string, string>("SubmitButton", "Login"));
            FormUrlEncodedContent postLoginContent = new FormUrlEncodedContent(values);

            response = await clientSyriatel.PostAsync(loginURL, postLoginContent);

            response = await clientSyriatel.GetAsync(rechrgeURL);
            responseString = await response.Content.ReadAsStringAsync();
            string balanceString = Regex.Match(responseString, "MainContentPlaceHolder_PointOfSalesMainContentPlaceHolder_BalanceText\"\\>(.*?)\\</span").Groups[1].Value;
            label_Balance.Content = "Balance = " + balanceString;
            oldBalance = int.Parse(balanceString, NumberStyles.AllowThousands);
            newBalance = oldBalance;
            textOldBalance.Text = oldBalance.ToString();

            balanceOK = true;
            if (isSyriatel)
                button_Transfer.IsEnabled = true;
        }

        private void Wb_Navigated(object sender, NavigationEventArgs e)
        {
            throw new NotImplementedException();
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

        bool operationSuccess = false;
        async private void button_Transfer_Click(object sender, RoutedEventArgs e)
        {
            errorMessage = "";
            try
            {
                numberString = textBox_Number.Text;


                TelCompany com = (TelCompany)button_Transfer.Tag;
                if (com == TelCompany.MTN)
                {
                    button_Transfer.IsEnabled = false;
                    //Clipboard.SetText(string.Format("{0}\r\n\r\n{1} وحدة", numberString, transferAmount));
                    Clipboard.SetText(numberString);
                    //Process.Start(viberLocation);
                    labelInfo.Content = string.Format("{0} SYP await to be sent by MTN to {1}", transferAmount, numberString); ;
                    labelInfo.Foreground = Brushes.Green;

                    addTransfer();
                }
                else if (transferAmount >= 1500)
                {
                    button_Transfer.IsEnabled = false;
                    Clipboard.SetText(string.Format("{0}\r\n\r\n{1} وحدة", numberString, transferAmount));
                    labelInfo.Content = string.Format("{0} SYP await to be sent by riad to {1}", transferAmount, numberString); ;
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
                                amountToBeSent = 100;
                            else if (amount > 1500)
                                amountToBeSent = 1500;
                            else
                                amountToBeSent = amount;

                            labelInfo.Content = string.Format("transferring {0} to {1} ....", amountToBeSent, numberString); ;
                            labelInfo.Foreground = Brushes.Black;

                            try
                            {
                                operationSuccess = false;
                                await transferCredits(numberString, amountToBeSent.ToString());
                                if (!operationSuccess)
                                {
                                    labelInfo.Content = "something went wrong, (" + errorMessage + ")";
                                    labelInfo.Foreground = Brushes.Red;
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                labelInfo.Content = ex.Message;
                                labelInfo.Foreground = Brushes.Red;
                            }
                            //if (oldBalance != newBalance)
                            //{
                            labelInfo.Content = string.Format("{0} transferred to {1}", amount, numberString); ;
                            labelInfo.Foreground = Brushes.Brown;
                            amount -= amountToBeSent;
                            //}
                            //else
                            //{
                            //    labelInfo.Content = "not transferred, please retry";
                            //    labelInfo.Foreground = Brushes.Red;
                            //}
                        }

                        int amountTransferred = oldBalance - newBalance;
                        if (amountTransferred == 0)
                        {
                            labelInfo.Content = string.Format("Nothing transferred :( "); ;
                            labelInfo.Foreground = Brushes.Red;

                            button_Transfer.IsEnabled = true;
                        }
                        else if (amountTransferred < transferAmount)
                        {
                            int remainingAmount = transferAmount - amountTransferred;
                            transferAmount = amountTransferred;
                            double priceCorrection = transferAmount / 200.0 * price200syp;
                            priceCorrection = priceCorrection / 500;
                            price = (int)(Math.Ceiling(priceCorrection) * 500);
                            labelInfo.Content = string.Format("Only {0} SYP transferred to {1}, remaining {2}",
                                transferAmount, numberString, remainingAmount);
                            labelInfo.Foreground = Brushes.OrangeRed;
                            addTransfer();
                            textBox_Amount.Text = remainingAmount.ToString();
                            oldBalance = newBalance;
                        }
                        else
                        {
                            labelInfo.Content = string.Format("{0} SYP transferred successfuly to {1}", transferAmount, numberString); ;
                            labelInfo.Foreground = Brushes.Green;
                            addTransfer();
                            oldBalance = newBalance;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                labelInfo.Content = ex.Message;
                labelInfo.Foreground = Brushes.Red;
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
                                t => t.Date < DateTime.Now && t.Date > date2).Sum(d => d.Amount).ToString();
                labelTotalMoney.Content = "Money = " + App.Transfers.Where(
                                 t => t.Date < DateTime.Now && t.Date > date2).Sum(d => d.Price).ToString();
            }
        }

        async Task transferCredits(string number, string transferAmount)
        {
            await Task.Delay(2000);
            HttpResponseMessage response;
            string responseString;

            response = await clientSyriatel.GetAsync(rechrgeURL);
            responseString = await response.Content.ReadAsStringAsync();

            var values = extractValues(responseString);

            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$SubscriberMSIDNTextBox", number));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$ConfirmSubscriberMSISDNTextBox", number));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$RechargeAmountTextBox", transferAmount));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$NotificationDropDownList", "1"));
            values.Add(new KeyValuePair<string, string>("ctl00$ctl00$MainContentPlaceHolder$PointOfSalesMainContentPlaceHolder$RechargeSubmitButton", "Recharge"));
            FormUrlEncodedContent postLoginContent = new FormUrlEncodedContent(values);

            try
            {
                response = await clientSyriatel.PostAsync(rechrgeURL, postLoginContent);
                responseString = await response.Content.ReadAsStringAsync();
            }
            catch
            {

            }

            try
            {
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) +
                    string.Format("\\Syria Logs\\{2:00}-{1:00}-{0} {3}-{4}-{5}.html", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), responseString);
            }
            catch { }

            if (!responseString.Contains("successfully"))
            {
                errorMessage = Regex.Match(responseString, "style=\"color:Red;\">(.*?)</span").Groups[1].Value;
                return;
            }

            string balanceString = Regex.Match(responseString, "MainContentPlaceHolder_PointOfSalesMainContentPlaceHolder_BalanceText\"\\>(.*?)\\</span").Groups[1].Value;
            label_Balance.Content = "Balance = " + balanceString;
            newBalance = int.Parse(balanceString, NumberStyles.AllowThousands);
            textNewBalance.Text = newBalance.ToString();
            operationSuccess = true;
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


        bool isSyriatel = true;
        private void textBox_Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = textBox_Number.Text;
            if (query.Length > 0 && query[0] > 1600)
            {
                StringBuilder build = new StringBuilder();
                foreach (char c in query)
                {
                    if (c > 1600)
                        build.Append(char.ConvertFromUtf32(c - 1584));
                    else
                        build.Append(c);
                }
                if (build[0] != '0')
                    build.Insert(0, '0');
                query = build.ToString();
            }
            if (query.Contains(" "))
            {
                query = query.Replace(" ", "").TrimStart(new char[] { '+' });
                if (query.StartsWith("963"))
                    query = "0" + query.Remove(0, 3);
            }

            textBox_Number.TextChanged -= textBox_Number_TextChanged;
            textBox_Number.Text = query;
            textBox_Number.TextChanged += textBox_Number_TextChanged;

            if (query.StartsWith("094") || query.StartsWith("095") || query.StartsWith("096"))
            {
                button_Transfer.Content = "Copy Number MTN";
                button_Transfer.Tag = TelCompany.MTN;
                button_Transfer.IsEnabled = true;
                isSyriatel = false;
            }
            else
            {
                button_Transfer.Tag = TelCompany.Syriatel;
                isSyriatel = true;
                if (transferAmount >= 1500)
                {
                    button_Transfer.Content = "Copy, open firefox";
                    button_Transfer.IsEnabled = true;
                }
                else
                {
                    button_Transfer.Content = "Transfer";
                    if (balanceOK)
                        button_Transfer.IsEnabled = true;
                    else
                        button_Transfer.IsEnabled = false;
                }

            }
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
                else
                {
                    if (transferAmount > 1000)
                    {
                        textBox_Amount.Background = Brushes.LightYellow;
                        //if (transferAmount == 2500)
                        //    price200syp = 800;
                        //else if (transferAmount > 3000)
                        //    price200syp = 933;
                    }
                    else
                        textBox_Amount.Background = Brushes.LightGreen;

                    if (!isSyriatel)
                        button_Transfer.IsEnabled = true;
                    else if (transferAmount >= 1500)
                    {
                        button_Transfer.IsEnabled = true;
                        button_Transfer.Content = "Copy, open Firefox";
                    }
                    else
                    {
                        button_Transfer.Content = "Transfer";
                        if (balanceOK)
                            button_Transfer.IsEnabled = true;
                        else
                            button_Transfer.IsEnabled = false;
                    }
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

        void checkNumberAmount()
        {
            
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

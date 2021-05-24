using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
        string settingsLoc = "settings.txt";

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

        public static int RiadPrice = 4000;
        int price200sypDefault, price200sypDefaultMore2000, price = 5000, priceMore2000 = 4500;
        int transferAmount = 1000;
        int oldBalance, newBalance;
        int riadLimit = 1500;
        DateTime riadDateCalculation;

        int selectedIndex = -1;
        bool amountSentAboveRiadLimit = false;


        HttpClient clientSyriatel, clientMTN;
        HttpClientHandler handlersyriatel, handlerMTN;
        List<string> addresses = new List<string>();
        List<string> ranks = new List<string>() { "1st", "2nd", "3rd" };

        MediaPlayer successPlayer, failPlayer, countingPlayer;

        public MainWindow()
        {
            InitializeComponent();
            string[] settings;
            if (File.Exists(@"D:\Dropbox\Text Files\callingDollarPrice.txt"))
            {
                settings = File.ReadAllLines(@"D:\Dropbox\Text Files\callingDollarPrice.txt");
                //else
                //    lines = File.ReadAllLines(@"\\محل\Text Files\callingDollarPrice.txt");

                Dictionary<string, int> values = new Dictionary<string, int>();
                foreach (string l in settings)
                {
                    var keyValue = l.Split(new char[] { '=' });
                    values.Add(keyValue[0], int.Parse(keyValue[1]));
                }

                price = values["syriaPrice"];
                priceMore2000 = values["syriaPriceMore2000"];
                RiadPrice = values["syriaRiadPrice"];
            }

            successPlayer = new MediaPlayer();
            successPlayer.Open(new Uri("Win.mp3", UriKind.Relative));
            failPlayer = new MediaPlayer();
            failPlayer.Open(new Uri("Fail.mp3", UriKind.Relative));

            if (File.Exists(settingsLoc))
            {
                var lines = File.ReadAllLines(settingsLoc);
                riadLimit = int.Parse(lines[0]);
                riadDateCalculation = DateTime.Parse(lines[1]);
                string[] tempAddresses = lines[2].Split(new char[] { ',' });
                addresses.AddRange(tempAddresses);
            }
            else
            {
                riadDateCalculation = DateTime.Now.AddDays(-1);
                addresses.Add("82.137.244.151:8080");
                File.Create(settingsLoc).Close();
                saveData();
            };

            price200sypDefault = price / 5;
            price200sypDefaultMore2000 = priceMore2000 / 5;
            labelPrice.Text = (price200sypDefault * 5).ToString();

            datePicker.SelectedDate = riadDateCalculation;
            datePicker.SelectedDateChanged += datePicker_SelectedDateChanged;
            datePicker.DisplayDateEnd = DateTime.Today;
            updateRiadParameters();
            textRiadLimit.Text = riadLimit.ToString();
            textRiadLimit.TextChanged += textRiadLimit_TextChanged;

            textBox_Amount.TextChanged += textBox_Amount_TextChanged;
            textBox_Number.TextChanged += textBox_Number_TextChanged;
            textBox_Number.Focus();

            listBox.ItemsSource = App.NumbersNotTransferred;
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

            string responseString = "";
            HttpResponseMessage response;

            if (riadLimit > 100)
            {
                for (int i = 0; i < addresses.Count; i++)
                {
                    string[] addressSplit = addresses[i].Split(new char[] { ':' });
                    string proxyAddress = addressSplit[0];
                    int proxyPort = int.Parse(addressSplit[1]);
                    WebProxy proxy = new WebProxy(proxyAddress, proxyPort);
                    proxy.BypassProxyOnLocal = false;
                    handlersyriatel = new HttpClientHandler();
                    handlersyriatel.Proxy = proxy;

                    clientSyriatel = new HttpClient(handlersyriatel);
                    clientSyriatel.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");
                    clientSyriatel.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    clientSyriatel.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                    clientSyriatel.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

                    try
                    {
                        string rankString = i < 4 ? ranks[i] : i.ToString() + "th";

                        labelInfo.Foreground = Brushes.Black;
                        labelInfo.Text = string.Format("{0} try, proxy = {1}:{2}", rankString, proxyAddress, proxyPort);

                        checkNumberBalance();

                        response = await clientSyriatel.GetAsync(loginURL);
                        responseString = await response.Content.ReadAsStringAsync();
                        labelInfo.Foreground = Brushes.Black;
                        labelInfo.Text = "Success from " + rankString + " try";
                        successPlayer.Play();
                        //var ddd = Application.GetResourceStream(new Uri("Yess.wav", UriKind.Relative));
                        //SoundPlayer yesSound = new SoundPlayer(ddd.Stream);
                        //yesSound.Play();
                        break;
                    }
                    catch (Exception ex)
                    {
                        responseString = "error";
                    }
                }

                checkNumberBalance();
                if (responseString == "error")
                {
                    labelInfo.Foreground = Brushes.Red;
                    labelInfo.Text = "error, you are loser!";
                    failPlayer.Play();
                    return;
                }

                var values = extractValues(responseString);
                values.Add(new KeyValuePair<string, string>("UsernameTextBox", App.Username));
                values.Add(new KeyValuePair<string, string>("PasswordTextBox", App.Password));
                values.Add(new KeyValuePair<string, string>("SubmitButton", "Login"));
                FormUrlEncodedContent postLoginContent = new FormUrlEncodedContent(values);

                response = await clientSyriatel.PostAsync(loginURL, postLoginContent);

                response = await clientSyriatel.GetAsync(rechrgeURL);
                responseString = await response.Content.ReadAsStringAsync();


                if (responseString.Contains("newPasswordText"))
                {
                    riadLimit = 1;
                    saveData();
                    textRiadLimit.TextChanged -= textRiadLimit_TextChanged;
                    textRiadLimit.Text = "1";
                    textRiadLimit.TextChanged += textRiadLimit_TextChanged;

                    labelInfo.Text = "PASSWORD need to be changed";
                    labelInfo.Foreground = Brushes.Red;
                    return;
                }


                string balanceString = Regex.Match(responseString, "MainContentPlaceHolder_PointOfSalesMainContentPlaceHolder_BalanceText\"\\>(.*?)\\</span").Groups[1].Value;
                label_Balance.Content = "Balance = " + balanceString;
                oldBalance = int.Parse(balanceString, NumberStyles.AllowThousands);
                newBalance = oldBalance;
                textOldBalance.Text = oldBalance.ToString();

                balanceOK = true;

                if (isSyriatel && !amountSentAboveRiadLimit)
                    button_Transfer.IsEnabled = checkNumber();
            }
            else
            {
                labelInfo.Text = "Transfer to riad";
                labelInfo.Foreground = Brushes.Orange;
            }
        }

        void checkNumberBalance()
        {
            int balance = 0;
            int.TryParse(textBox_Amount.Text, out balance);
            if (checkNumber() && isSyriatel)
            {
                if (selectedIndex == -1)
                {
                    if (balance < riadLimit)
                    {
                        if (textBox_Number.Text.Length == 8 && textBox_Number.Text.StartsWith("09"))
                            return;

                        Transfer t = new Transfer(DateTime.Now, textBox_Number.Text, int.Parse(textBox_Amount.Text), 0);
                        App.NumbersNotTransferred.Insert(0, t);
                        selectedIndex = 0;
                        Transfer.SaveTransfers();
                    }
                }
                else
                {
                    if (App.NumbersNotTransferred[selectedIndex].Amount != transferAmount)
                    {
                        App.NumbersNotTransferred[selectedIndex].Amount = transferAmount;
                        Transfer.SaveTransfers();
                    }
                }
            }
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
            listBox.IsEnabled = false;
            textBox_Number.IsEnabled = false;
            textBox_Amount.IsEnabled = false;
            try
            {
                numberString = textBox_Number.Text;


                TelCompany com = (TelCompany)button_Transfer.Tag;
                if (com == TelCompany.MTN)
                {
                    button_Transfer.IsEnabled = false;
                    if (File.Exists("D:\\mtn.txt"))
                    {
                        Clipboard.SetText(string.Format("{0}\r\n\r\n{1} وحدة", numberString, transferAmount));
                        labelInfo.Text = string.Format("{0} SYP await to be sent by riad to {1}", transferAmount, numberString); ;
                    }
                    else
                    {
                        //Clipboard.SetText(string.Format("{0}\r\n\r\n{1} وحدة", numberString, transferAmount));
                        Clipboard.SetText(numberString);
                        //Process.Start(viberLocation);
                        labelInfo.Text = string.Format("{0} SYP await to be sent by MTN to {1}", transferAmount, numberString);
                    }

                    labelInfo.Foreground = Brushes.Green;
                    addTransfer();

                    if (transferAmount > 1000)
                    {
                        textBox_Amount.TextChanged -= textBox_Amount_TextChanged;
                        textBox_Amount.Text = "1000";
                        textBox_Amount.TextChanged += textBox_Amount_TextChanged;
                        labelPrice.Text = price.ToString();
                    }
                }
                else if (transferAmount >= riadLimit)
                {
                    button_Transfer.IsEnabled = false;
                    amountSentAboveRiadLimit = true;
                    Clipboard.SetText(string.Format("{0}\r\n\r\n{1} وحدة", numberString, transferAmount));
                    labelInfo.Text = string.Format("{0} SYP await to be sent by riad to {1}", transferAmount, numberString); ;
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
                                amountToBeSent = 100;
                            else if (amount == 1250)
                                amountToBeSent = 500;
                            else if (amount == 1300)
                                amountToBeSent = 300;
                            else if (amount == 1400)
                                amountToBeSent = 400;
                            else if (amount == 700)
                                amountToBeSent = 100;
                            else if (amount > 1500)
                                amountToBeSent = 1500;
                            else
                                amountToBeSent = amount;

                            labelInfo.Text = string.Format("transferring {0} to {1} ....", amountToBeSent, numberString); ;
                            labelInfo.Foreground = Brushes.Black;

                            try
                            {
                                operationSuccess = false;
                                await transferCredits(numberString, amountToBeSent.ToString());
                                if (!operationSuccess)
                                {
                                    labelInfo.Text = "something went wrong, (" + errorMessage + ")";
                                    labelInfo.Foreground = Brushes.Red;
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                labelInfo.Text = ex.Message;
                                labelInfo.Foreground = Brushes.Red;
                            }
                            //if (oldBalance != newBalance)
                            //{
                            labelInfo.Text = string.Format("{0} transferred to {1}", amount, numberString); ;
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
                            labelInfo.Text = string.Format("Nothing transferred :( "); ;
                            labelInfo.Foreground = Brushes.Red;

                            if (selectedIndex == -1)
                            {
                                App.NumbersNotTransferred.Insert(0, new Transfer(DateTime.Now, numberString, transferAmount, 0));
                                Transfer.SaveTransfers();
                                selectedIndex = 0;
                            }
                            else
                            {
                                if (App.NumbersNotTransferred[selectedIndex].Amount != transferAmount)
                                {
                                    App.NumbersNotTransferred[selectedIndex].Amount = transferAmount;
                                    Transfer.SaveTransfers();
                                }
                            }

                            button_Transfer.IsEnabled = true;
                        }
                        else if (amountTransferred < transferAmount)
                        {
                            int remainingAmount = transferAmount - amountTransferred;
                            transferAmount = amountTransferred;
                            double priceCorrection = transferAmount / 200.0 * price200sypDefault;
                            priceCorrection = priceCorrection / 500;
                            price = (int)(Math.Ceiling(priceCorrection) * 500);
                            labelInfo.Text = string.Format("Only {0} SYP transferred to {1}, remaining {2}",
                                transferAmount, numberString, remainingAmount);
                            labelInfo.Foreground = Brushes.OrangeRed;
                            addTransfer();
                            textBox_Amount.Text = remainingAmount.ToString();
                            oldBalance = newBalance;

                            if (selectedIndex != -1)
                            {
                                App.NumbersNotTransferred[selectedIndex].Amount = remainingAmount;
                                Transfer.SaveTransfers();
                            }
                            else
                            {
                                App.NumbersNotTransferred.Insert(0, new Transfer(DateTime.Now, numberString, remainingAmount, 0));
                                Transfer.SaveTransfers();
                                selectedIndex = 0;
                            }
                        }
                        else
                        {
                            labelInfo.Text = string.Format("{0} SYP transferred successfuly to {1}", transferAmount, numberString); ;
                            labelInfo.Foreground = Brushes.Green;
                            addTransfer();
                            oldBalance = newBalance;


                            if (transferAmount > 1000)
                            {
                                textBox_Amount.Text = "1000";
                                labelPrice.Text = price.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                labelInfo.Text = ex.Message;
                labelInfo.Foreground = Brushes.Red;
            }
            finally
            {
                listBox.IsEnabled = true;
                textBox_Number.IsEnabled = true;
                textBox_Amount.IsEnabled = true;
            }
        }

        private void addTransfer()
        {
            if (selectedIndex != -1)
            {
                App.NumbersNotTransferred.RemoveAt(selectedIndex);
                selectedIndex = -1;
            }

            //Transfer.GetTransfers();
            Transfer t = new Transfer(DateTime.Now, numberString, transferAmount, int.Parse(labelPrice.Text));
            t.Status = TransferStatus.Transferred;
            App.NumbersTransferred.Insert(0, t);
            Transfer.SaveTransfers();

            updateSentDay(DateTime.Now.AddHours(-2));
            updateRiadParameters();
        }

        void updateRiadParameters()
        {
            DateTime dd = DateTime.Today;
            if (riadDateCalculation == DateTime.Today || DateTime.Now.Hour > 20)
                dd = DateTime.Now;
            var transfers = App.NumbersTransferred.Where(
                t => t.Date.AddHours(-2) > riadDateCalculation && t.Date.AddHours(-2) < dd);
            labelRiadSYPSent.Content = transfers.Sum(d => d.Amount).ToString();
            labelRiadMoney1.Content = transfers.Sum(d => d.Price).ToString();
            labelRiadMoney2.Content = transfers.Sum(d => d.Cost).ToString(); //(Math.Ceiling((transfers.Sum(d => d.Amount) * RiadPrice / 250000.0)) * 250).ToString();
            labelRiadProfit.Content = (int.Parse(labelRiadMoney1.Content.ToString()) - int.Parse(labelRiadMoney2.Content.ToString())).ToString();
        }

        void updateSentDay(DateTime date)
        {
            labelDate.Content = date.ToString("dd-MM-yyyy");
            labelAmountSent.Content = "SYP sent = " + App.NumbersTransferred.Where(
                t => t.Date.AddHours(-2).ToShortDateString().Equals(date.ToShortDateString())).Sum(d => d.Amount).ToString();
            labelMoney.Content = "Money = " + App.NumbersTransferred.Where(
                t => t.Date.AddHours(-2).ToShortDateString().Equals(date.ToShortDateString())).Sum(d => d.Price).ToString();

            if (date.ToShortDateString().Equals(DateTime.Now.AddHours(-2).ToShortDateString()))
            {
                labelTotalAmountSent.Content = "";
                labelTotalMoney.Content = "";
            }
            else
            {
                DateTime date2 = new DateTime(date.Year, date.Month, date.Day, 2, 0, 0);
                labelTotalAmountSent.Content = "SYP sent = " + App.NumbersTransferred.Where(
                                t => t.Date < DateTime.Now && t.Date > date2).Sum(d => d.Amount).ToString();
                labelTotalMoney.Content = "Money = " + App.NumbersTransferred.Where(
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

            //try
            //{
            //    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) +
            //        string.Format("\\Syria Logs\\{2:00}-{1:00}-{0} {3}-{4}-{5}.html", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
            //        DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), responseString);
            //}
            //catch { }

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

        #region RiadLimit
        private void button_Click(object sender, RoutedEventArgs e)
        {
            textRiadLimit.TextChanged -= textRiadLimit_TextChanged;
            textRiadLimit.Text = riadLimit.ToString();
            textRiadLimit.TextChanged += textRiadLimit_TextChanged;
            button.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            riadLimit = int.Parse(textRiadLimit.Text);
            saveData();
            textBox_Amount_TextChanged(null, null);
            button.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;
        }

        private void textRiadLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            int limit = riadLimit;
            if (!int.TryParse(textRiadLimit.Text, out limit))
            {
                textRiadLimit.TextChanged -= textRiadLimit_TextChanged;
                textRiadLimit.Text = riadLimit.ToString();
                textRiadLimit.TextChanged += textRiadLimit_TextChanged;
                return;
            }

            button.Visibility = Visibility.Visible;
            button1.Visibility = Visibility.Visible;
        }

        void saveData()
        {
            string sss = "";
            foreach (string s in addresses)
            {
                sss += s + ",";
            }
            sss = sss.Remove(sss.Length - 1);
            File.WriteAllText(settingsLoc, textRiadLimit.Text + "\r\n" + riadDateCalculation.ToShortDateString() + "\r\n" + sss);
        }
        #endregion

        bool isSyriatel = true;



        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                Transfer t = listBox.SelectedItem as Transfer;
                textBox_Amount.Text = t.Amount.ToString();
                textBox_Number.Text = t.Number.ToString();
                selectedIndex = listBox.SelectedIndex;
            }
        }

        private void textBox_Number_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            listBox.SelectedItem = null;
            textBox_Number.SelectAll();
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            riadDateCalculation = datePicker.SelectedDate.Value;
            saveData();
            updateRiadParameters();
        }

        private void labelRiadSYPSent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText("بدك سعر " + int.Parse(labelRiadSYPSent.Content.ToString()) / 1000.0);
        }


        private void textBox_Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            amountSentAboveRiadLimit = false;
            selectedIndex = -1;
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
                if (transferAmount >= riadLimit)
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
            int price200syp = 0;
            if (int.TryParse(textBox_Amount.Text, out transferAmount))
            {
                //if (transferAmount > 5000)
                //{
                //    button_Transfer.IsEnabled = false;
                //    textBox_Amount.Background = Brushes.Red;
                //}
                //else
                //{
                price200syp = transferAmount >= 2000 ? price200sypDefaultMore2000 : price200sypDefault;
                if (transferAmount > 5000)
                {
                    textBox_Amount.Background = new SolidColorBrush(Color.FromArgb(0x80,0xFF,0x45,0x45));
                    //if (transferAmount == 2500)
                    //    price200syp = 800;
                    //else if (transferAmount > 3000)
                    //    price200syp = 933;
                }
                else if (transferAmount > 1000)
                {
                    textBox_Amount.Background = Brushes.LightYellow;
                }
                else
                    textBox_Amount.Background = Brushes.LightGreen;


                if (!isSyriatel)
                    button_Transfer.IsEnabled = true;
                else if (transferAmount >= riadLimit)
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
                //}

                double priceCorrection = transferAmount / 200.0 * price200syp;
                priceCorrection = priceCorrection / 500;
                price = (int)(Math.Ceiling(priceCorrection) * 500);

                labelPrice.Text = price.ToString();
            }
            else
            {
                button_Transfer.IsEnabled = false;
                textBox_Amount.Background = Brushes.Red;
            }
        }

        bool checkNumber()
        {
            string number = textBox_Number.Text;
            return Regex.IsMatch(number, "^(09\\d{8}|\\d{8})$");
        }
        bool checkNumber(string s)
        {
            return Regex.IsMatch(s, "^(09\\d{8}|\\d{8})$");
        }
    }

    public enum TelCompany
    {
        Syriatel,
        MTN
    }

    public enum TransferStatus
    {
        NotTrasnferred = 0,
        Transferred = 1
    }

    public class Transfer : INotifyPropertyChanged
    {
        private int amount;
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public int Amount
        {
            get { return amount; }
            set
            {
                if (value != amount)
                {
                    amount = value;
                    Cost = MainWindow.RiadPrice * amount / 1000;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Price { get; set; }
        public int Cost { get; set; }
        public TelCompany Company { get; set; }
        public TransferStatus Status { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public Transfer(DateTime date, string number, int amount, int price)
        {
            Date = date;
            Number = number;
            Amount = amount;
            Price = price;
            if (Number[2] == '4' || Number[2] == '5' || Number[2] == '6') Company = TelCompany.MTN;
            Status = TransferStatus.NotTrasnferred;
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
            Cost = int.Parse(data[4]);
            Status = (TransferStatus)int.Parse(data[5]);
        }


        public override string ToString()

        {
            return string.Format("{0},{1},{2},{3},{4},{5}", Date.ToString("dd-MMM-yy hh:mm:ss tt"), Number, Amount, Price, Cost, (int)Status);
        }
        public static void SaveTransfers()
        {
            File.WriteAllLines(App.transfersPath, App.NumbersNotTransferred.Concat(App.NumbersTransferred).ToList().ConvertAll<string>(t => t.ToString()));
        }
        public static void GetTransfers()
        {
            if (File.Exists(App.transfersPath))
            {
                App.NumbersNotTransferred.Clear();
                App.NumbersTransferred.Clear();
                foreach (string s in File.ReadAllLines(App.transfersPath))
                {
                    Transfer t = new Transfer(s);
                    if (t.Status == TransferStatus.Transferred)
                        App.NumbersTransferred.Add(t);
                    else
                        App.NumbersNotTransferred.Add(t);
                }
            }
            else
                File.Create(App.transfersPath);
        }
    }
}

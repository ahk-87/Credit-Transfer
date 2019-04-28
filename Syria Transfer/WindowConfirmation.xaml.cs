using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Syria_Transfer
{
    /// <summary>
    /// Interaction logic for WindowConfirmation.xaml
    /// </summary>
    public partial class WindowConfirmation : Window
    {

        public WindowConfirmation(string number, int amount)
        {
            InitializeComponent();
            if (number.StartsWith("09"))
            {
                textBlockCodeorNumber.Text = "to the number";
                number = number.Insert(7, " ");
                number = number.Insert(4, " ");
                textBlockNumber.Text = number;
            }
            else
            {
                textBlockCodeorNumber.Text = "to the code";
                number = number.Insert(4, " ");
                textBlockNumber.Text = number;
            }
            textBlockAmount.Text = amount.ToString();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == buttonYes)
            {
                DialogResult = true;
            }
            else
                DialogResult = false;

            Close();
        }
    }
}

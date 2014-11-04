using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LyncWpfApp
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
            WinLync.lyncCounter++;
            this.Closed += new EventHandler((sender, e)
                =>
                {
                    WinLync.lyncCounter--;
                });
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public void ShowDialog(string message, string title)
        {
            labContext.Content = message;
            this.Title = title == null ? "" : title;
            this.ShowDialog();
        }
    }
    public class Dialog
    {
        public static void Show(string message, string title = null)
        {
            DialogWindow win = new DialogWindow();
            win.ShowDialog(message, title);
        }
    }
}

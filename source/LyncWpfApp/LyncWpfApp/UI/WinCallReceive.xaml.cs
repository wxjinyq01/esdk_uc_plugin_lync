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
    /// Interaction logic for WinCallReceive.xaml
    /// </summary>
    public partial class WinCallReceive : Window
    {
        public WinCallReceiveViewModel model;
        public WinLync lync;
        public string callName;
        public bool isCloseButton = true;
        public WinCallReceive(WinLync lync, string name, bool isVide)
        {
            InitializeComponent();
            btnFinish.Visibility = Visibility.Hidden;
            imgOtherPhone.Visibility = Visibility.Hidden;
            txtOtherPhone.Visibility = Visibility.Hidden;

            callName = name;
            model = new WinCallReceiveViewModel(this, isVide);
            this.lync = lync;
            this.DataContext = model;
            this.Title = StringHelper.GetSubString(name);
            
            if (isVide)
            {
                this.Height = 120;
            }
            else
            {
                imgType.Visibility = Visibility.Collapsed;
            }

            WinLync.lyncCounter++;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Closing += new System.ComponentModel.CancelEventHandler(WinCallReceive_Closing);
            this.Closed += new EventHandler((sender, e)
                =>
            {
                WinLync.lyncCounter--;
            });
        }

        void WinCallReceive_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isCloseButton)
            {
                Visibility = Visibility.Hidden;
                model.DeclineCommand.Execute(isCloseButton);
                return;
            }
            if (model.isAnswerMessage)
            {
                string str = callName + ";" + SingletonObj.LoginInfo.LyncName;
                lync.winCall = new WinCall(lync, str); ;
                lync.winCall.callType = CallHistoryType.HISTORY_CALL_DIALED;
                lync.winCall.Show();
           }
        }

        private void expander_Expanded(object sender, RoutedEventArgs e)
        {
            imgType.Visibility = Visibility.Hidden;
            btnFinish.Visibility = Visibility.Visible;
            imgOtherPhone.Visibility = Visibility.Visible;
            txtOtherPhone.Visibility = Visibility.Visible;
            this.Height = 90;
        }

        private void txtOtherPhone_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtOtherPhone.Text == StringHelper.FindLanguageResource("transfernNumber"))
            {
                txtOtherPhone.Text = "";
            }
        }
    }
}

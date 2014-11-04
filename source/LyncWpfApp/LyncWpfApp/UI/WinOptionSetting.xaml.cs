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
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using System.Threading;

namespace LyncWpfApp
{
    /// <summary>
    /// Interaction logic for WinOptionSetting.xaml
    /// </summary>
    public partial class WinOptionSetting : Window
    {
        WinLync lync;
        public static bool isHave = false;
        public WinOptionSetting(WinLync lync)
        {
            this.lync = lync;
            InitializeComponent();
            tabOptionSetting.TabStripPlacement = Dock.Left;

            WinOptionSettingViewModel model = new WinOptionSettingViewModel(this);
            this.DataContext = model;
            model.StartLoadData();
            this.Closed += new EventHandler(WinOptionSetting_Closed);
            isHave = true;
            WinLync.lyncCounter++;
        }

        void WinOptionSetting_Closed(object sender, EventArgs e)
        {
            WinLync.lyncCounter--;
            isHave = false;
            lync.toolBar.Setting = null;
        }

        private void btnOpenPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = StringHelper.FindLanguageResource("SelectFile");
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
           (dataGrid1.DataContext as DataTable).Rows[dataGrid1.SelectedIndex][2]= openFileDialog.FileName;
        }

        private void OpenSelfMgr_Portal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserConfigBusiness conf = new UserConfigBusiness();
            UCServiceRetvCode iRet = (UCServiceRetvCode)conf.OpenPortal((int)PortalType.SelfMgr_Portal);
            if (iRet == UCServiceRetvCode.UC_SDK_NotLogin)
            {
                Dialog.Show(StringHelper.FindLanguageResource("NoLogin"), StringHelper.FindLanguageResource("error"));
            }
        }

        private void BookConf_Portal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SingletonObj.LoginInfo == null)
            {
                return;
            }
            UserConfigBusiness conf = new UserConfigBusiness();
            conf.OpenPortal((int)PortalType.BookConf_Portal);
        }

        private void OpenSelfMgr_Portal_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            OpenSelfMgr_Portal.FontWeight = FontWeights.Bold;
        }

        private void OpenSelfMgr_Portal_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            OpenSelfMgr_Portal.FontWeight = FontWeights.Normal;
        }

        private void BookConf_Portal_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BookConf_Portal.FontWeight = FontWeights.Bold;
        }

        private void BookConf_Portal_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BookConf_Portal.FontWeight = FontWeights.Normal;
        }

    }
}

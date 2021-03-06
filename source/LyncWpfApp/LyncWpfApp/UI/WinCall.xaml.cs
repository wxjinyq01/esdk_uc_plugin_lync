﻿using System;
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
using Microsoft.Lync.Model;
using System.Threading;
using System.Runtime.InteropServices;
using System.Globalization;

namespace LyncWpfApp
{
    /// <summary>
    /// Interaction logic for WinCall.xaml
    /// </summary>
    public partial class WinCall : Window
    {
        public WinCallViewModel model;
        public WinLync lync;
        public WinSlide slideMic;
        public WinSlide slideVol;
        public UCVideo video;
        public bool isMetting = false;
        public CallHistoryType callType;
        public int index;
        public static object lockObject = new object();
        string src;
        Thread threadGetSlideState = null;
        bool shouldStop = false;
        int iSleepTime = 500;//线程睡眠时间
        int winOrightHeight = 70;//窗体最初高度
        int win7OHeight = 70;//Win7窗体最初高度
        int win8OrigHeight = 90;//Win8窗体高度
        int singleItemHeight = 28;//单个与会人在列表中的高度
        int videoSize = 500;//视频画面的尺寸
        int maxMum = 10;//会议人数超过maxMum时显示滚动条
        int iSozeOff = 2;//尺寸偏移大小
        int iWinWidth = 352;//视频窗体宽度
        int iMicLeftOff = 85;//麦克风控件左偏移
        int iVolLeftOff = 115;//音量控件左偏移
        int iTopOff = 10;//控件top偏移
        int iSlideChangeValue = 10;//声音控件每次改变的值
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public WinCall(WinLync lync, string str)
        {
            InitializeComponent();
            OSInfo info = new OSInfo();
            if (info.GetOSVersion() == "Win 8")//只匹配Win7 Win8
            {
                winOrightHeight = win8OrigHeight;
            }
            src = str;
            this.lync = lync;
            model = new WinCallViewModel(this, str);
            this.DataContext = model;
            Closed += new EventHandler(WinCall_Closed);

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.btnCallSuspend.MouseEnter += new MouseEventHandler(hideWinSlide);
            this.btnSetVol.MouseEnter += new MouseEventHandler(hideWinSlide);
            this.btnDial.MouseEnter += new MouseEventHandler(hideWinSlide);
            this.btnHoldDown.MouseEnter += new MouseEventHandler(btnHoldDown_MouseEnter);
            this.GotMouseCapture += new MouseEventHandler(hideWinSlide);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(WinCall_MouseLeftButtonDown);
            this.MouseDown += new MouseButtonEventHandler(WinCall_MouseDown);

            if (str.Split(';')[0] == "VideoCall")
            {
                SetWinSize();
            }
            if (lync.toolBar.JointType == PhoneJointType.IPPhone_Device)
            {
                btnVideo.IsEnabled = false;
            }
            this.Loaded += new RoutedEventHandler(WinCall_Loaded);
            //被叫、主叫权限控制
            if (SingletonObj.LoginInfo.LyncName != str.Split(';')[0] && str.Split(';').Length != 1)
            {
                this.btnAddContact.IsEnabled = false;
            }
            if (SingletonObj.LoginInfo.LyncName != str.Split(';')[0] || str.Split(';').Length < 3)
            {
                listContact.ContextMenu = null;
            }
        }

        void GetSildeState()//获取音量控件可见状态
        {
            while (!shouldStop)
            {
                Thread.Sleep(iSleepTime);
                if (slideMic != null && slideMic.Visibility == Visibility.Visible)
                {
                    POINT point = new POINT();
                    GetCursorPos(out point);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        int xDis = (point.X - (int)slideMic.Left);
                        int yDis = (point.Y - (int)slideMic.Top);
                        if ((xDis > slideMic.Width || xDis < 0) || (yDis > slideMic.Height || -yDis > btnSetMicPhone.Height))
                        {
                            slideMic.Hide();
                        }
                    }));
                }
                else if (slideVol != null && slideVol.Visibility == Visibility.Visible)
                {
                    POINT point = new POINT();
                    GetCursorPos(out point);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        int xDis = (point.X - (int)slideVol.Left);
                        int yDis = (point.Y - (int)slideVol.Top);
                        if ((xDis > slideVol.Width || xDis < 0) || (yDis > slideVol.Height || -yDis > btnSetVol.Height))
                        {
                            slideVol.Hide();
                        }
                    }));
                }
            }
        }
        public void SetWinSize()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                this.Height = videoSize;
                this.Width = videoSize;
            }));
        }
        public void SetUCVideo()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                video = new UCVideo();
                video.Show();

                host.Child = video;
                btnAddContact.IsEnabled = false;
            }));
        }
        public void SetVideoImage()//设置视频按钮图标
        {
            if (imgVideo.Source.ToString().IndexOf("unvideo") != -1)
            {
                UpdateImage.UpdateData(imgVideo, "/Image/call/video_1.png");
            }
            else
            {
                UpdateImage.UpdateData(imgVideo, "/Image/call/unvideo_1.png");
            }
        }
        /// <summary>
        /// 获取视频播放窗体参数
        /// </summary>
        /// <param name="stLocalWnd"></param>
        /// <param name="stRemoteWnd"></param>
        public void GetVideoPlayPara(ref STVideoWindow stLocalWnd, ref STVideoWindow stRemoteWnd)
        {
            System.Drawing.Point phRemote = new System.Drawing.Point();
            System.Drawing.Point phLocal = new System.Drawing.Point();
            IntPtr intpRemote = new IntPtr();
            System.Drawing.Point plRemote = new System.Drawing.Point();
            IntPtr intpLocal = new IntPtr();
            System.Drawing.Point plLocal = new System.Drawing.Point();
            Dispatcher.Invoke(new Action(() =>
            {
                intpRemote = (host.Child as UCVideo).Remote.Handle;
                intpLocal = (host.Child as UCVideo).Local.Handle;
                plRemote.X = 0;
                plRemote.Y = 0;
                UCInterface.ClientToScreen(intpRemote, ref plRemote);

                phRemote.X = (int)(host.Child as UCVideo).Remote.Width;
                phRemote.Y = (int)(host.Child as UCVideo).Remote.Height;
                UCInterface.ClientToScreen(intpRemote, ref phRemote);

                plLocal.X = 0;
                plLocal.Y = 0;
                UCInterface.ClientToScreen(intpLocal, ref plLocal);

                phLocal.X = (int)(host.Child as UCVideo).Local.Width;
                phLocal.Y = (int)(host.Child as UCVideo).Local.Height;
                UCInterface.ClientToScreen(intpLocal, ref phLocal);
            }));
            stRemoteWnd.height = phRemote.Y;
            stRemoteWnd.hWnd = (int)(intpRemote);
            stRemoteWnd.left = plRemote.X;
            stRemoteWnd.top = plRemote.Y;
            stRemoteWnd.width = phRemote.X;

            stLocalWnd.height = phLocal.Y;
            stLocalWnd.hWnd = (int)(intpLocal);
            stLocalWnd.left = plLocal.X;
            stLocalWnd.top = plLocal.Y;
            stLocalWnd.width = phLocal.X;
            SetWinSize();//重新设置video尺寸
        }
        void btnHoldDown_MouseEnter(object sender, MouseEventArgs e)
        {
            if (slideVol != null)
            {
                slideVol.Hide();
            }
        }
        void WinCall_Loaded(object sender, RoutedEventArgs e)
        {
            threadGetSlideState = new Thread(new ThreadStart(GetSildeState));
            threadGetSlideState.Priority = ThreadPriority.BelowNormal;
            threadGetSlideState.Start();

            if (src.Split(';')[0] == "VideoCall")
            {
                SetUCVideo();
                if (src.Split(';')[1] == SingletonObj.LoginInfo.LyncName)//视频呼叫
                {
                    model.VideoCommand.Execute(null);
                }
                else//视频被呼叫
                {
                    model.AcceptVideoCall();
                }
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                lync.GetLyncUserState();
                lync.SetAvailability(ContactAvailability.Busy);
            }));
            WinLync.lyncCounter++;
            SetVideoLocalRemoteSize();
        }
        public void SetVideoLocalRemoteSize()
        {
            Dispatcher.BeginInvoke(new Action(() =>
               {
                   if (video != null)
                   {
                       video.Local.Left = video.Remote.Left + iSozeOff;//视频窗体左偏移大小
                       video.Local.Top = video.Remote.Top + video.Remote.Height - video.Local.Height - iSozeOff;//视频窗体top
                   }
               }));
        }
        public void ShowReceiveWin(STCallParam _callParam)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                WinCallReceive receive = new WinCallReceive(lync, _callParam.ucAcc + StringHelper.GetLyncDomainString(SingletonObj.LoginInfo.LyncName), true);
                receive.Show();
            }));
        }
        public void CloseWinVideo()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (video != null)
                {
                    video.Dispose();
                    video = null;
                }
                this.Height = win7OHeight;
                this.Width = iWinWidth;
                host.Child = null;
                model.IsVideo = false;
                if (callType == CallHistoryType.HISTORY_CALL_DIALED)
                {
                    btnAddContact.IsEnabled = true;
                }

                UpdateImage.UpdateData(imgVideo, "/Image/call/video_1.png");
                this.UpdateLayout();
            }));

        }
        void WinCall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (slideMic != null)
            {
                slideMic.Hide();
            }
            if (slideVol != null)
            {
                slideVol.Hide();
            }
        }

        void WinCall_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (slideMic != null)
            {
                slideMic.Hide();
            }
        }
        void hideWinSlide(object sender, MouseEventArgs e)
        {
            if (slideMic != null)
            {
                slideMic.Hide();
            }
        }

        public int GetAllSeconds()
        {
            string str = "";
            Dispatcher.Invoke(new Action(() =>
            {
                str = labTime.Content.ToString();
            }));
            DateTime dt = DateTime.Parse(str);
            TimeSpan sp = dt.Subtract(DateTime.Today);
            return (int)(sp.TotalSeconds);
        }
        void WinCall_Closed(object sender, EventArgs e)
        {
            try
            {
                if (slideMic != null)
                {
                    slideMic.Close();
                }
                if (slideVol != null)
                {
                    slideVol.Close();
                }
                if (lync.winTwoDail != null)
                {
                    lync.winTwoDail.Close();
                    lync.winTwoDail = null;
                }
                if (lync.winAllContact != null)
                {
                    lync.winAllContact.Close();
                    lync.winAllContact = null;
                }

                if (video == null)//呼叫
                {
                    model.HoldDownCommand.Execute(null);
                    List<UCContact> contactList = listContact.ItemsSource as List<UCContact>;
                    if (contactList == null || contactList.Count == 0)
                    {
                        return;
                    }
                    if (contactList.Count > 2)
                    {
                        model.InsertConfHistory();
                    }
                    else
                    {
                        model.InsertCallHistory();
                    }
                }
                else//视频呼叫
                {
                    model.HoldDownCommand.Execute(null);
                    model.InsertCallHistory();
                }

                lync.winCall = null;
                lync.SetAvailability(lync.userState);
                lync.UCAVSessionClosed(new STMsgAVSessionClosedParam());
            }
            finally
            {
                shouldStop = true;
                WinLync.lyncCounter--;
            }
        }

        private void btnSetMicPhone_MouseEnter(object sender, MouseEventArgs e)
        {
            if (slideVol != null)
            {
                slideVol.Hide();
            }
            if (slideMic == null)
            {
                slideMic = new WinSlide(this, "MicPhone");
                slideMic.Left = this.Left + iMicLeftOff;
                slideMic.Top = this.Top + this.Height - iTopOff;
                slideMic.Show();
            }
            if (slideMic.Visibility == Visibility.Hidden)
            {
                slideMic.Visibility = Visibility.Visible;
                Point pRelative = Mouse.GetPosition(e.Source as FrameworkElement);
                Point pScreen = (e.Source as FrameworkElement).PointToScreen(pRelative);
                slideMic.Left = this.Left + iMicLeftOff;
                slideMic.Top = this.Top + this.Height - iTopOff;
            }
            int newValue = model.Slider_GetValue("MicPhone");
            slideMic.slider.Value = newValue / iSlideChangeValue;

            if (imgMic.Source.ToString().IndexOf("Mic") > 0)
            {
                if (imgMic.Source.ToString().IndexOf("Mic_1") > 0)
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Mic_2.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Mic_1.png", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                if (imgMic.Source.ToString().IndexOf("Unmic_1") > 0)
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Unmic_2.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Unmic_1.png", UriKind.RelativeOrAbsolute));
                }
            }
        }

        private void btnSetVol_MouseEnter(object sender, MouseEventArgs e)
        {
            if (slideVol == null)
            {
                slideVol = new WinSlide(this, "SetVol");
                slideVol.Left = this.Left + iVolLeftOff;
                slideVol.Top = this.Top + this.Height - iTopOff;
                slideVol.Show();
            }
            if (slideVol.Visibility == Visibility.Hidden)
            {
                slideVol.Visibility = Visibility.Visible;
                Point pRelative = Mouse.GetPosition(e.Source as FrameworkElement);
                Point pScreen = (e.Source as FrameworkElement).PointToScreen(pRelative);
                slideVol.Left = this.Left + iVolLeftOff;
                slideVol.Top = this.Top + this.Height - iTopOff;
            }
            int newValue = model.Slider_GetValue("SetVol");
            slideVol.slider.Value = newValue / iSlideChangeValue;

            if (imgVol.Source.ToString().IndexOf("Spker") > 0)
            {

                imgVol.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Spker_2.png", UriKind.RelativeOrAbsolute));

            }
            else
            {
                imgVol.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Unspker_2.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void listContact_MouseEnter(object sender, MouseEventArgs e)
        {
            if (slideMic != null)
            {
                slideMic.Hide();
            }
            if (slideVol != null)
            {
                slideVol.Hide();
            }
        }

        private void btnDial_MouseEnter(object sender, MouseEventArgs e)
        {
            if (slideVol != null)
            {
                slideVol.Hide();
            }
            imgDial.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/DialPad_2.png", UriKind.RelativeOrAbsolute));
        }
        /// <summary>
        /// 联系人列表进行排序
        /// </summary>
        private Comparison<UCContact> sortUCContact = new Comparison<UCContact>((x, y)
        =>
        {
            LogManager.SystemLog.Debug("X:" + x.UserName + "Y:" + y.UserName);
            if (x.IsLeader && !y.IsLeader)//会议主席排第一，自己不能跟自己比较
            {
                return -1;// x 小
            }
            else if (y.IsLeader)
            {
                return 1;// y 小
            }
            else if (x.UserName == y.UserName)//本身比较相等
            {
                return 0;// 相等
            }
            else if (x.UCMemberType == MemberType.UC_IPPHONE && y.UCMemberType == MemberType.UC_IPPHONE)//都是ipphone，正常比较
            {
                return x.UserName.CompareTo(y.UserName);
            }
            else if (x.UCMemberType == MemberType.UC_IPPHONE)//x是ipphone 排在后面
            {
                return 1;
            }
            else if (y.UCMemberType == MemberType.UC_IPPHONE)//y是ipphone 排在前面
            {
                return -1;
            }
            else//其他情况按正常排序
            {
                return x.UserName.CompareTo(y.UserName);
            }
        });
        public void Render(List<UCContact> contactList,bool isConf = true)
        {
            try
            {
                Dispatcher.Invoke(new Action(()
                    =>
                {
                    LogManager.SystemLog.Debug(" Start UpdateDataSource");
                    contactList.Sort(sortUCContact);

                    listContact.ItemsSource = null;
                    listContact.ItemsSource = contactList;
                    if (contactList.Count >= 2 && isConf)
                    {
                        AddContactWinSize(contactList);
                    }
                    LogManager.SystemLog.Debug(" End UpdateDataSource");

                }));
            }
            catch (System.Exception ex)
            {
                LogManager.SystemLog.Error(ex.ToString());
            }
        }
        public int GetSelectListIndex()
        {
            int index = 0;
            Dispatcher.Invoke(new Action(()
                =>
            {
                index = listContact.SelectedIndex == -1 ? index : listContact.SelectedIndex;
            }));
            return index;
        }
        public void AddContactWinSize(List<UCContact> contactList)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
               {
                   Title = contactList[0].UserName + StringHelper.FindLanguageResource("TempGroup");
                   listContact.ContextMenu = contactList[0].UserName == SingletonObj.LoginInfo.LyncName ? listMenu : null;
                   btnVideo.IsEnabled = false;
                   btnCallSuspend.IsEnabled = false;
                   if (contactList.Count > maxMum)
                   {
                       Height = winOrightHeight + singleItemHeight * maxMum;
                       rowContact.Height = new System.Windows.GridLength(Height - win7OHeight);
                   }
                   else
                   {
                       Height = winOrightHeight + singleItemHeight * contactList.Count;
                       rowContact.Height = new System.Windows.GridLength(Height - win7OHeight);
                   }
                   UpdateLayout();
               }));
            }
            catch (System.Exception ex)
            {
                LogManager.SystemLog.Error(ex.ToString());
            }
        }
        public bool IsConf()
        {
            bool isConf = false;
            Dispatcher.Invoke(new Action(() =>
              {
                  if (Title.IndexOf(StringHelper.FindLanguageResource("TempGroup")) > 0)
                  {
                      isConf = true;
                  }
              }));
            return isConf;
        }
        public void DecreaseContactWinSize(List<UCContact> contactList)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (contactList.Count > maxMum)
                {
                    return;
                }
                else
                {
                    int iSize = winOrightHeight + singleItemHeight * contactList.Count;
                    rowContact.Height = new System.Windows.GridLength(iSize - win7OHeight);
                    Height = iSize;
                }
            }));
        }

        private void btnCallSuspend_MouseEnter(object sender, MouseEventArgs e)
        {
            if (btnCallSuspend.IsEnabled)
            {
                if (imgCallSuspend.Source.ToString().IndexOf("CallHold") > 0)
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/CallHold_2.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Resume_2.png", UriKind.RelativeOrAbsolute));
                }
            }
        }

        private void btnCallSuspend_MouseLeave(object sender, MouseEventArgs e)
        {
            if (btnCallSuspend.IsEnabled)
            {
                if (imgCallSuspend.Source.ToString().IndexOf("CallHold") > 0)
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/CallHold_1.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Resume_1.png", UriKind.RelativeOrAbsolute));
                }
            }
        }

        private void btnCallSuspend_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (btnCallSuspend.IsEnabled)
            {
                if (imgCallSuspend.Source.ToString().IndexOf("CallHold") > 0)
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/CallHold_1.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Resume_1.png", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                if (imgCallSuspend.Source.ToString().IndexOf("CallHold") > 0)
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/CallHold_3.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgCallSuspend.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Resume_3.png", UriKind.RelativeOrAbsolute));
                }
            }
        }

        private void btnSetMicPhone_MouseLeave(object sender, MouseEventArgs e)
        {
            if (imgMic.Source.ToString().IndexOf("Mic") > 0)
            {
                if (imgMic.Source.ToString().IndexOf("Mic_1") > 0)
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Mic_2.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Mic_1.png", UriKind.RelativeOrAbsolute));
                }

            }
            else
            {
                if (imgMic.Source.ToString().IndexOf("Unmic_1") > 0)
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Unmic_2.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgMic.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Unmic_1.png", UriKind.RelativeOrAbsolute));
                }
            }
        }

        private void btnSetVol_MouseLeave(object sender, MouseEventArgs e)
        {
            if (imgVol.Source.ToString().IndexOf("Spker") > 0)
            {

                imgVol.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Spker_1.png", UriKind.RelativeOrAbsolute));

            }
            else
            {
                imgVol.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/Unspker_1.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void btnDial_MouseLeave(object sender, MouseEventArgs e)
        {
            imgDial.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/DialPad_1.png", UriKind.RelativeOrAbsolute));
        }

        private void btnAddContact_MouseLeave(object sender, MouseEventArgs e)
        {
            if (btnAddContact.IsEnabled)
            {
                imgAdd.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/add_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgAdd.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/add_3.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void btnAddContact_MouseEnter(object sender, MouseEventArgs e)
        {
            if (btnAddContact.IsEnabled)
            {
                imgAdd.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/add_2.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgAdd.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/add_3.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void btnAddContact_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (btnAddContact.IsEnabled)
            {
                imgAdd.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/add_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgAdd.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/add_3.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void btnVideo_MouseEnter(object sender, MouseEventArgs e)
        {
            if (btnVideo.IsEnabled)
            {
                if (imgVideo.Source.ToString().IndexOf("unvideo") != -1)
                {
                    imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/unvideo_2.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/video_2.png", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/video_3.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void btnVideo_MouseLeave(object sender, MouseEventArgs e)
        {
            if (btnVideo.IsEnabled)
            {
                if (imgVideo.Source.ToString().IndexOf("unvideo") != -1)
                {
                    imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/unvideo_1.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/video_1.png", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/video_3.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void btnVideo_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (btnVideo.IsEnabled)
            {
                imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/video_1.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imgVideo.Source = new BitmapImage(new Uri("/LyncWpfApp;component/Image/call/video_3.png", UriKind.RelativeOrAbsolute));
            }
        }
        public void CloseCallInThread()
        {
            Dispatcher.Invoke(new Action(()
             =>
            {
                Close();
            }));
        }

        private void listContact_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            List<UCContact> contactList = listContact.ItemsSource as List<UCContact>;
            if (contactList == null || contactList.Count == 0)
            {
                return;
            }
            index = GetSelectListIndex();
            if (index < 0 || index >= contactList.Count || listContact.ContextMenu == null)//没选中数据时不显示菜单
            {
                return;
            }
            if (contactList[index].IsLeader)
            {
                HangUpMenuItem.Visibility = Visibility.Collapsed;
                ReInviteMenuItem.Visibility = Visibility.Collapsed;
                MuteUnmuteMenuItem.Visibility = Visibility.Visible;
                return;
            }
            if (MemStatusInCall.CONF_MEM_INVITING == contactList[index].Mute)//正在邀请时不显示静音、重新邀请菜单
            {
                MuteUnmuteMenuItem.Visibility = Visibility.Collapsed;
                ReInviteMenuItem.Visibility = Visibility.Collapsed;
                HangUpMenuItem.Visibility = Visibility.Visible;
                //RemoveMenuItem.Visibility = Visibility.Visible;
                return;
            }

            if (MemStatusInCall.CONF_MEM_HANGUP == contactList[index].Mute || MemStatusInCall.CONF_MEM_QUIT == contactList[index].Mute ||
                MemStatusInCall.CONF_MEM_DEL == contactList[index].Mute)//挂断、退出、删除时候重新邀请不可见
            {
                ReInviteMenuItem.Visibility = Visibility.Visible;
                MuteUnmuteMenuItem.Visibility = Visibility.Collapsed;
                HangUpMenuItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                ReInviteMenuItem.Visibility = Visibility.Collapsed;
                MuteUnmuteMenuItem.Visibility = Visibility.Visible;
                HangUpMenuItem.Visibility = Visibility.Visible;
            }
        }
        public void UpdateUCContactState(object ucInfo)
        {
            lock (WinCall.lockObject)
            {
                try
                {
                    LogManager.SystemLog.Info("UpdateUCContactState Monitor.Enter");
                    List<UCContact> contactList = listContact.ItemsSource as List<UCContact>;
                    if (contactList == null || contactList.Count == 0)
                    {
                        return;
                    }
                    UCContactInfo info = ucInfo as UCContactInfo;
                    foreach (UCContact uc in contactList)
                    {
                        string ucName = "";
                        if (uc.UCMemberType == MemberType.UC_ACCOUNT)
                        {
                            ucName = StringHelper.GetSubString(uc.UserName);
                        }
                        else
                        {
                            ucName = uc.UserName;
                        }
                        if (ucName == info.User.ucAcc_)
                        {
                            uc.Online = (UCContactAvailability)info.State;
                            break;
                        }
                    }
                }
                finally
                {
                    LogManager.SystemLog.Info("UpdateUCContactState Monitor.Exit");
                }
            }
        }
        public void SetButCallSuspEnable(bool isVideo)
        {
            Dispatcher.Invoke(new Action(()
            =>
            {
                btnCallSuspend.IsEnabled = !isVideo;
            }));
        }

        private void listContact_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            listContact_SelectionChanged(null, null);
        }  
    }
}
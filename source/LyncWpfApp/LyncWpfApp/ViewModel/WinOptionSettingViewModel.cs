using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Data;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace LyncWpfApp
{
   public class WinOptionSettingViewModel
   {
       public ICommand OKCommand { get; set; }
       public ICommand CancelCommand { get; set; }
       public ICommand ApplyCommand { get; set; }
       public ICommand PlayCommand { get; set; }
       public ICommand OpenPathCommand { get; set; }
       public WinOptionSetting winOptionSetting { get; set; }
       int count = 10;
       public WinOptionSettingViewModel(WinOptionSetting window)
       {
           winOptionSetting = window;

           OKCommand = new DelegateCommand(OKCommandProcess);
           CancelCommand = new DelegateCommand(CancelCommandProcess);
           ApplyCommand = new DelegateCommand(ApplyCommandProcess);

           LoadSystDve();//加载系统设备
        }
       public WinOptionSettingViewModel()
       {
       }
       /// <summary>
       /// 加载系统设备
       /// </summary>
       void LoadSystDve()
       {
           UserConfigBusiness conf = new UserConfigBusiness();

           int iSizeSTDeviceListParam = Marshal.SizeOf(typeof(STDeviceListParam));
           int iSizeSTDeviceParam = Marshal.SizeOf(typeof(STDeviceParam));
           int iBufSize = (iSizeSTDeviceListParam + iSizeSTDeviceParam * (count - 1));
           byte[] pSTDeviceList = new byte[iBufSize];

           UCServiceRetvCode iRet = (UCServiceRetvCode)conf.GetMicDevList(0, 9, iBufSize, pSTDeviceList);

           Dictionary<int, string> dicMicrophone = new Dictionary<int, string>();
           if (iRet == UCServiceRetvCode.UC_SDK_Success)
           {
               IntPtr tempInfoIntPtr = Marshal.AllocHGlobal((int)iSizeSTDeviceListParam);
               byte[] tempInfoByte = new byte[iSizeSTDeviceListParam];

               Marshal.Copy(pSTDeviceList, 0, tempInfoIntPtr, (int)iSizeSTDeviceListParam);
               STDeviceListParam head = (STDeviceListParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceListParam));

               for (int i = -1; i < head.iTotal - 1 && i < count - 1; i++)
               {
                   Marshal.Copy(pSTDeviceList, iSizeSTDeviceListParam + iSizeSTDeviceParam * i, tempInfoIntPtr, (int)iSizeSTDeviceParam);
                   STDeviceParam item = (STDeviceParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceParam));
                   dicMicrophone.Add(item.index, item.name);
               }
               Marshal.Release(tempInfoIntPtr);
           }
           winOptionSetting.comMicrophone.ItemsSource = dicMicrophone;
           winOptionSetting.comMicrophone.SelectedValuePath = "Key";
           winOptionSetting.comMicrophone.DisplayMemberPath = "Value";
           winOptionSetting.comMicrophone.SelectedIndex = 0;

           pSTDeviceList = new byte[iBufSize];
           iRet = (UCServiceRetvCode)conf.GetSpeakerDevList(0, 9, iBufSize, pSTDeviceList);
           Dictionary<int, string> dicSpeakerDev = new Dictionary<int, string>();
           if (iRet == UCServiceRetvCode.UC_SDK_Success)
           {
               IntPtr tempInfoIntPtr = Marshal.AllocHGlobal((int)iSizeSTDeviceListParam);
               byte[] tempInfoByte = new byte[iSizeSTDeviceListParam];

               Marshal.Copy(pSTDeviceList, 0, tempInfoIntPtr, (int)iSizeSTDeviceListParam);
               STDeviceListParam head = (STDeviceListParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceListParam));

               for (int i = -1; i < head.iTotal - 1 && i < count - 1; i++)
               {
                   Marshal.Copy(pSTDeviceList, iSizeSTDeviceListParam + iSizeSTDeviceParam * i, tempInfoIntPtr, (int)iSizeSTDeviceParam);
                   STDeviceParam item = (STDeviceParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceParam));
                   dicSpeakerDev.Add(item.index, item.name);
               }
               Marshal.Release(tempInfoIntPtr);
           }
           winOptionSetting.comSpeaker.ItemsSource = dicSpeakerDev;
           winOptionSetting.comSpeaker.SelectedValuePath = "Key";
           winOptionSetting.comSpeaker.DisplayMemberPath = "Value";
           winOptionSetting.comSpeaker.SelectedIndex = 0;

           pSTDeviceList = new byte[iBufSize];
           iRet = (UCServiceRetvCode)conf.GetVideoDevList(0, 9, iBufSize, pSTDeviceList);
           Dictionary<int, string> dicVideoDev = new Dictionary<int, string>();
           if (iRet == UCServiceRetvCode.UC_SDK_Success)
           {
               IntPtr tempInfoIntPtr = Marshal.AllocHGlobal((int)iSizeSTDeviceListParam);
               byte[] tempInfoByte = new byte[iSizeSTDeviceListParam];

               Marshal.Copy(pSTDeviceList, 0, tempInfoIntPtr, (int)iSizeSTDeviceListParam);
               STDeviceListParam head = (STDeviceListParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceListParam));

               for (int i = -1; i < head.iTotal - 1 && i < count - 1; i++)
               {
                   Marshal.Copy(pSTDeviceList, iSizeSTDeviceListParam + iSizeSTDeviceParam * i, tempInfoIntPtr, (int)iSizeSTDeviceParam);
                   STDeviceParam item = (STDeviceParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceParam));
                   dicVideoDev.Add(item.index, item.name);
               }
               Marshal.Release(tempInfoIntPtr);
           }
           winOptionSetting.comCamera.ItemsSource = dicVideoDev;
           winOptionSetting.comCamera.SelectedValuePath = "Key";
           winOptionSetting.comCamera.DisplayMemberPath = "Value";
           winOptionSetting.comCamera.SelectedIndex = 0;
       }

       private void OKCommandProcess()
       {
           winOptionSetting.Close();
           ApplyCommandProcess();
       }
       private void CancelCommandProcess()
       {
           winOptionSetting.Close();
       }
       private void ApplyCommandProcess()
       {
           try
           {
               UCUserInfo user = new UCUserInfo();
               user.Camera = winOptionSetting.comCamera.SelectedIndex.ToString();
               user.Lang = winOptionSetting.comLang.SelectedIndex.ToString();
               user.MicPhone = winOptionSetting.comMicrophone.SelectedIndex.ToString();
               user.Password = winOptionSetting.txtPassword.Password.ToString();
               user.Port = winOptionSetting.txtPort.Text.ToString();
               user.Server = winOptionSetting.txtServer1.Text.ToString();
               user.Speaker = winOptionSetting.comSpeaker.SelectedIndex.ToString();
               user.UserID = winOptionSetting.txtAccount.Text.ToString();
               user.AutoStart = winOptionSetting.chkStart.IsChecked == true ? "1" : "0";
               user.Available = (bool)winOptionSetting.chkAvailable.IsChecked;
               user.Unavailable = (bool)winOptionSetting.chkUnavailable.IsChecked;
               user.Busy = (bool)winOptionSetting.chkBusy.IsChecked;
               user.Voicemail = (bool)winOptionSetting.chkVoicemail.IsChecked;
               user.AvailableCallNumber = winOptionSetting.txtAvailable.Text;
               user.UnavailableCallNumber = winOptionSetting.txtUnavailable.Text;
               user.BusyCallNumber = winOptionSetting.txtBusy.Text;
               user.VoicemailCallNumber = winOptionSetting.txtVoicemail.Text;


               XmlHelper.SetUserConfig(user);

               UserConfigBusiness conf = new UserConfigBusiness();
               conf.SetCurrentMicDev(winOptionSetting.comMicrophone.SelectedIndex);
               conf.SetCurrentVideoDev(winOptionSetting.comCamera.SelectedIndex);
               conf.SetCurrentSpeakerDev(winOptionSetting.comSpeaker.SelectedIndex);

               if (winOptionSetting.chkStart.IsChecked == true)
               {
                   RegistryKey runKey = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                   runKey.SetValue("LyncWpfApp.exe", System.Windows.Forms.Application.StartupPath + "\\LyncWpfApp.exe");
               }
               else
               {
                  RegistryKey runKey= Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                  runKey.DeleteValue("LyncWpfApp.exe");
               }
           }
           catch
           {
               LogManager.SystemLog.Info("DeleteSubKey");
           }
           
       }
       public void SaveLyncAccountToConfig(string account)
       {
           try
           {
               UCUserInfo user = new UCUserInfo();
               user = XmlHelper.GetUserConfig();
               user.UserID = account;
               XmlHelper.SetUserConfig(user);
           }
           catch (System.Exception ex)
           {
               LogManager.SystemLog.Error(ex.ToString());
           }
       }
       public void StartLoadData()
       {
           try
           {
               UCUserInfo user = new UCUserInfo();
               user = XmlHelper.GetUserConfig();

               winOptionSetting.txtAccount.Text = user.UserID;
               winOptionSetting.comLang.SelectedIndex = Convert.ToInt32(user.Lang);
               winOptionSetting.comMicrophone.SelectedIndex = GetMicrophone();
               winOptionSetting.txtPassword.Password = user.Password;
               winOptionSetting.txtPort.Text = user.Port;
               winOptionSetting.txtServer1.Text = user.Server;
               winOptionSetting.comSpeaker.SelectedIndex = GetSpeaker();
               winOptionSetting.chkStart.IsChecked = user.AutoStart == "1" ? true : false;
               winOptionSetting.comCamera.SelectedIndex = GetCamera();

               winOptionSetting.chkAvailable.IsChecked = user.Available;
               winOptionSetting.chkUnavailable.IsChecked = user.Unavailable;
               winOptionSetting.chkBusy.IsChecked = user.Busy;
               winOptionSetting.chkVoicemail.IsChecked = user.Voicemail;
               winOptionSetting.txtAvailable.Text = user.AvailableCallNumber;
               winOptionSetting.txtUnavailable.Text = user.UnavailableCallNumber;
               winOptionSetting.txtBusy.Text = user.BusyCallNumber;
               winOptionSetting.txtVoicemail.Text = user.VoicemailCallNumber;
           }
           catch (System.Exception ex)
           {
               LogManager.SystemLog.Error(ex.ToString());
           }
       }
       private int GetMicrophone()
       {
           try
           {
               UserConfigBusiness conf = new UserConfigBusiness();

               int iSizeSTDeviceParam = Marshal.SizeOf(typeof(STDeviceParam));
               byte[] pSTDeviceParam = new byte[iSizeSTDeviceParam];

               UCServiceRetvCode iRet = (UCServiceRetvCode)conf.GetCurrentMicDev(pSTDeviceParam);
               if (iRet == UCServiceRetvCode.UC_SDK_Success)
               {
                   IntPtr tempInfoIntPtr = Marshal.AllocHGlobal(iSizeSTDeviceParam);

                   Marshal.Copy(pSTDeviceParam, 0, tempInfoIntPtr, (int)iSizeSTDeviceParam);
                   STDeviceParam head = (STDeviceParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceParam));

                   if (head.index<0)
                   {
                       return 0;
                   }
                   else
                   {
                       return head.index;
                   }
               }
               else
               {
                   return 0;
               }
           }
           catch (Exception ex)
           {
               LogManager.SystemLog.Error(ex.ToString());
               return 0;
           }
       }
       private int GetSpeaker()
       {
           try
           {
               UserConfigBusiness conf = new UserConfigBusiness();

               int iSizeSTDeviceParam = Marshal.SizeOf(typeof(STDeviceParam));
               byte[] pSTDeviceParam = new byte[iSizeSTDeviceParam];

               UCServiceRetvCode iRet = (UCServiceRetvCode)conf.GetCurrentSpeakerDev(pSTDeviceParam);
               if (iRet == UCServiceRetvCode.UC_SDK_Success)
               {
                   IntPtr tempInfoIntPtr = Marshal.AllocHGlobal(iSizeSTDeviceParam);

                   Marshal.Copy(pSTDeviceParam, 0, tempInfoIntPtr, (int)iSizeSTDeviceParam);
                   STDeviceParam head = (STDeviceParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceParam));

                   if (head.index < 0)
                   {
                       return 0;
                   }
                   else
                   {
                       return head.index;
                   }
               }
               else
               {
                   return 0;
               }
           }
           catch (Exception ex)
           {
               LogManager.SystemLog.Error(ex.ToString());
               return 0;
           }
       }
       private int GetCamera()
       {
           try
           {
               UserConfigBusiness conf = new UserConfigBusiness();

               int iSizeSTDeviceParam = Marshal.SizeOf(typeof(STDeviceParam));
               byte[] pSTDeviceParam = new byte[iSizeSTDeviceParam];

               UCServiceRetvCode iRet = (UCServiceRetvCode)conf.GetCurrentVideoDev(pSTDeviceParam);
               if (iRet == UCServiceRetvCode.UC_SDK_Success)
               {
                   IntPtr tempInfoIntPtr = Marshal.AllocHGlobal(iSizeSTDeviceParam);

                   Marshal.Copy(pSTDeviceParam, 0, tempInfoIntPtr, (int)iSizeSTDeviceParam);
                   STDeviceParam head = (STDeviceParam)Marshal.PtrToStructure(tempInfoIntPtr, typeof(STDeviceParam));

                   if (head.index < 0)
                   {
                       return 0;
                   }
                   else
                   {
                       return head.index;
                   }
               }
               else
               {
                   return 0;
               }
           }
           catch (Exception ex)
           {
               LogManager.SystemLog.Error(ex.ToString());
               return 0;
           }
       }
   }
}

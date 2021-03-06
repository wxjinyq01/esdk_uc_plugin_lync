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

namespace LyncWpfApp
{
    /// <summary>
    /// Interaction logic for WinDail.xaml
    /// </summary>
    public partial class WinTwoDail : Window
    {
        WinLync Lync;
        public WinTwoDail(WinLync Lync)
        {
            InitializeComponent();
            this.Lync = Lync;
            this.Closed += new EventHandler(WinTwoDail_Closed);
            WinLync.lyncCounter++;
            imgNum1.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum1.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum2.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum2.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum3.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum3.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum4.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum4.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum5.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum5.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum6.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum6.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum7.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum7.MouseLeave += new MouseEventHandler(img_MouseLeave);

            imgNum8.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum8.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum9.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum9.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNum0.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNum0.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNumS.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNumS.MouseLeave += new MouseEventHandler(img_MouseLeave);
            imgNumX.MouseEnter += new MouseEventHandler(img_MouseEnter);
            imgNumX.MouseLeave += new MouseEventHandler(img_MouseLeave);
        }
        void img_MouseLeave(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            string str = img.Source.ToString();
            img.Source = new BitmapImage(new Uri(str.Substring(0, str.LastIndexOf("/")) + "/2.png", UriKind.RelativeOrAbsolute));
        }

        void img_MouseEnter(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            string str = img.Source.ToString();
            img.Source = new BitmapImage(new Uri(str.Substring(0, str.LastIndexOf("/")) + "/1.png", UriKind.RelativeOrAbsolute));
        }
        void WinTwoDail_Closed(object sender, EventArgs e)
        {
            WinLync.lyncCounter--;
            Lync.winTwoDail = null;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {           
            //二次拨号
            MakeCallBusiness call = new MakeCallBusiness();
            string str =(((sender as Button).Content as StackPanel).Children[0] as Image).Tag.ToString();
            txtNumber.Text += str;
            call.SendDTMF(Convert.ToChar(str));
        }
     
        private void txtNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                string str = textBox.Text.Replace("*", "").Replace("#", "");
                if (!Double.TryParse(str, out num) && str != "")
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                }
            }
        }
    }
}

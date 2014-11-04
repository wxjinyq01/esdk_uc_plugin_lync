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
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;
using System.Data;

namespace LyncWpfApp
{
    /// <summary>
    /// Interaction logic for WinAllContact.xaml
    /// </summary>
    public partial class WinAllContact : Window
    {
        DataTable dtSource;
        DataTable dtContact = new DataTable();
        DataTable dtSelectedContact = new DataTable();
        public Action<DataTable> AddContactChanged;
        WinLync lync;
        /// <summary>
        /// 获取所有Lync联系人
        /// </summary>
        void GetAllLyncContacts()
        {
            dtContact.Clear();
            dtContact.Columns.Add("Name");
            dtContact.Columns.Add("Url");
            dtContact.Columns.Add("Phone");
            dtSelectedContact.Clear();
            dtSelectedContact.Columns.Add("Name");
            dtSelectedContact.Columns.Add("Url");
            dtSelectedContact.Columns.Add("Phone");

            foreach (Microsoft.Lync.Model.Group.Group group in WinLync.LyncContactGroups)
            {
                foreach (Contact contact in (ContactCollection)(group))
                {
                    int index = -1;
                    foreach (DataRow dr in dtContact.Rows)
                    {
                        if (dr["Url"].ToString() == contact.Uri)
                        {
                            index = 0;
                        }
                    }
                    if (index == -1)
                    {
                        string phone = "";
                        List<object> list = contact.GetContactInformation(ContactInformationType.ContactEndpoints) as List<object>;
                        foreach (object point in list)
                        {
                            if (((Microsoft.Lync.Model.ContactEndpoint)point).Type == ContactEndpointType.WorkPhone)
                            {
                                phone = ((Microsoft.Lync.Model.ContactEndpoint)point).DisplayName;
                            }
                        }
                        string name = contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
                        dtContact.Rows.Add(name, contact.Uri, phone);
                    }
                }
            }
        }

        public WinAllContact(WinLync lync)
        {
            InitializeComponent();

            GetAllLyncContacts();
            try
            {
                for (int i = 0; i < dtContact.Rows.Count; i++)//对数据按名称进行排序
                {
                    string iName = dtContact.Rows[i][0].ToString();
                    for (int j = i + 1; j < dtContact.Rows.Count; j++)
                    {
                        string jName = dtContact.Rows[j][0].ToString();
                        if (iName.CompareTo(jName) == 1)
                        {
                            object[] temp = new object[3];
                            temp = dtContact.Rows[j].ItemArray;
                            dtContact.Rows[j].ItemArray= dtContact.Rows[i].ItemArray;
                            dtContact.Rows[i].ItemArray = temp;
                            iName = jName;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.SystemLog.Error(ex.ToString());
            }
            dtSource = dtContact.Copy();
            this.listContact.DataContext = dtContact;
            WinLync.lyncCounter++;
            this.lync = lync;
            this.Closed += new EventHandler(WinAllContact_Closed);
            this.txtInput.Focus();//是输入文本获取焦点
        }

        void WinAllContact_Closed(object sender, EventArgs e)
        {
            WinLync.lyncCounter--;
            lync.winAllContact = null;
        }

        private void tabControl1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (DataRow d in dtSelectedContact.Rows)
            {
                if (d["Url"].ToString() == dtContact.Rows[listContact.SelectedIndex][1].ToString())
                {
                    return;
                }
            }
            DataRow dr = dtSelectedContact.NewRow();
            dr[0] = dtContact.Rows[listContact.SelectedIndex][0];
            dr[1] = dtContact.Rows[listContact.SelectedIndex][1];
            dr[2] = dtContact.Rows[listContact.SelectedIndex][2];

            if (dr[1].ToString().IndexOf("sip")<0)
            {
                dr[2] = dr[0];
                dr[0] = "";
                dr[1] = "";
            }
            dtSelectedContact.Rows.Add(dr);
            listSelectedContact.DataContext = null;
            listSelectedContact.DataContext = dtSelectedContact;
            listSelectedContact.ScrollIntoView(listSelectedContact.Items[dtSelectedContact.Rows.Count - 1], listSelectedContact.Columns[0]);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtNumber.Text.ToString() == "")
            {
                return;
            }
            foreach (DataRow d in dtSelectedContact.Rows)
            {
                if (d["Phone"].ToString() == txtNumber.Text.ToString())
                {
                    return;
                }
            }
            DataRow dr = dtSelectedContact.NewRow();
            dr[0] = txtName.Text.ToString();
            dr[2] = txtNumber.Text.ToString();
            dtSelectedContact.Rows.Add(dr);
            listSelectedContact.DataContext = null;
            listSelectedContact.DataContext = dtSelectedContact;
            listSelectedContact.ScrollIntoView(listSelectedContact.Items[dtSelectedContact.Rows.Count - 1], listSelectedContact.Columns[0]);

            txtName.Text = "";
            txtNumber.Text = "";
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listSelectedContact.SelectedIndex > -1)
            {
                dtSelectedContact.Rows.RemoveAt(listSelectedContact.SelectedIndex);
                listSelectedContact.DataContext = null;
                listSelectedContact.DataContext = dtSelectedContact;
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            listSelectedContact.DataContext = null;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (AddContactChanged != null)
            {
                AddContactChanged(dtSelectedContact);
            }
        }

        private void txtInput_KeyUp(object sender, KeyEventArgs e)
        {
            string str = txtInput.Text.ToString();
            listContact.DataContext = null;
            dtContact.Clear();
            if (str == "")
            {
                dtContact = dtSource.Copy();
            }
            else
            {
                foreach (DataRow dr in dtSource.Rows)
                {
                    if (dr[0].ToString().IndexOf(str) > -1 || dr[1].ToString().IndexOf(str) > -1 || dr[2].ToString().IndexOf(str) > -1)
                    {
                        dtContact.Rows.Add(dr.ItemArray);
                    }

                }
            }
            listContact.DataContext = dtContact;
        }

        private void txtNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)))
            {
                e.Handled = true;
            }
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
                if (!Double.TryParse(textBox.Text, out num))
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                }
            }
        }
    }
}

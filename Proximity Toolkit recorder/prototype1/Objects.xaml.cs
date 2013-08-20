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

namespace prototype1
{
    /// <summary>
    /// Interaction logic for Objects.xaml
    /// </summary>
    public partial class Objects : Window
    {
        private MainWindow parent;

        public Objects(MainWindow window)
        {
            parent = window;
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            tag tag = new tag(textBox1.Text, textBox2.Text, comboBox1.Text, comboBox1.SelectedIndex);

            if (listBox1.SelectedIndex != -1)
            {
                for (int x = 0; x < parent.listBox1.Items.Count; x++)
                {
                    if (parent.listBox1.Items[x].ToString() == ((ListBoxItem)listBox1.Items[listBox1.SelectedIndex]).Content.ToString())
                    {
                        parent.listBox1.Items[x] = textBox1.Text;
                        parent.listBox1.Items.Refresh();
                        break;
                    }
                }

                for (int x = 0; x < parent.listBox2.Items.Count; x++)
                {
                    if (parent.listBox2.Items[x].ToString() == ((ListBoxItem)listBox1.Items[listBox1.SelectedIndex]).Content.ToString())
                    {
                        parent.listBox2.Items[x] = textBox1.Text;
                        parent.listBox2.Items.Refresh();
                        break;
                    }
                }

                for (int x = 0; x < parent.events.Count; x++ )
                {
                    if (parent.events[x].obj1 == ((ListBoxItem)listBox1.Items[listBox1.SelectedIndex]).Content.ToString())
                    {
                        parent.events[x].obj1 = textBox1.Text;
                    }

                    if (parent.events[x].obj2 == ((ListBoxItem)listBox1.Items[listBox1.SelectedIndex]).Content.ToString())
                    {
                        parent.events[x].obj2 = textBox1.Text;
                    }
                }

                ListBoxItem item = (ListBoxItem)listBox1.SelectedItem;
                item.Content = textBox1.Text;
                item.Tag = tag;

                parent.updateObjectList();

                textBox1.Text = "";
                textBox2.Text = "";
                comboBox1.SelectedIndex = -1;

                listBox1.SelectedIndex = -1;
                listBox1.Items.Refresh();
            }
            else
            {
                foreach (ListBoxItem item in listBox1.Items)
                {
                    if ((String)item.Content == textBox1.Text.Trim() || ((tag)item.Tag).presence == textBox2.Text.Trim())
                    {
                        return;
                    }
                }

                if (textBox1.Text.Trim() != "" && textBox2.Text.Trim() != "")
                {
                    ListBoxItem myItem = new ListBoxItem();
                    myItem.Content = textBox1.Text;
                    myItem.Tag = tag;

                    listBox1.Items.Add(myItem);

                    textBox1.Text = "";
                    textBox2.Text = "";
                    comboBox1.SelectedIndex = -1;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true; // this cancels the close event.
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            listBox1.SelectedIndex = -1;
            comboBox1.SelectedIndex = -1;
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tag tag;

            if (listBox1.SelectedIndex != -1)
            {
                ListBoxItem item = (ListBoxItem)listBox1.SelectedItem;

                tag = (tag)item.Tag;

                textBox1.Text = item.Content.ToString();
                textBox2.Text = tag.presence;
                comboBox1.SelectedIndex = tag.index;
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && listBox1.SelectedIndex != -1)
            {
                foreach(relation item in parent.events)
                {
                    ListBoxItem temp = (ListBoxItem)listBox1.SelectedItem;

                    tag tag;
                    tag = (tag)temp.Tag;

                    if (item.obj1 == temp.Content.ToString() || item.obj2 == tag.presence)
                    {
                        return;
                    }
                }

                listBox1.Items.RemoveAt(listBox1.SelectedIndex);

                listBox1.SelectedIndex = -1;

                textBox1.Text = "";
                textBox2.Text = "";
                comboBox1.SelectedIndex = -1;
            }
        }
    }

    public class tag
    {
        public String name;
        public String presence;
        public String type;
        public int index;

        public tag(String name, String presence, String type, int index)
        {
            this.name = name;
            this.presence = presence;
            this.type = type;
            this.index = index;
        }
    }
}

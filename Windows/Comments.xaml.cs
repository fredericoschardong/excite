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

namespace TimelineSample
{
    /// <summary>
    /// Interaction logic for Comments.xaml
    /// </summary>
    public partial class Comments : Window
    {
        private bool pressOk = false;

        public Comments()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int numVal;

            try
            {
                numVal = Convert.ToInt32(textBox2.Text);

                this.pressOk = true;

                this.Close();
            }
            catch (FormatException ee)
            {
                e.Handled = true;
            }
        }

        public Comment show(Comment com){
            if (com != null)
            {
                this.textBox1.Text = com.comment;
                this.textBox2.Text = com.duration.ToString();
            }

            this.ShowDialog();

            if (this.pressOk == true)
            {
                try
                {
                    return new Comment(-1, Convert.ToInt32(textBox2.Text), this.textBox1.Text, -1);
                }
                catch (FormatException ee)
                {
                    return null;
                }
            }

            return null;
        }
    }
}

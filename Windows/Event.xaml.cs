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
    public partial class Event : Window
    {
        Parser validator;
        List <String> entities = new List <String>();
        MainWindow parent;
        ListBoxItem item;

        public Event(MainWindow parent, List<String> entities, ListBoxItem item)
        {
            InitializeComponent();
            textBox2.TextWrapping = TextWrapping.Wrap;

            this.parent = parent;
            this.entities = entities;
            this.item = item;

            this.textBlock1.Text = "Presences available:\n";
            this.textBlock1.Text += string.Join(", ", entities.ToArray());

            this.textBlock1.Text += "\n\nBoolean functions available:\n";
            this.textBlock1.Text += string.Join(", ", Parser.boolEvents);
            this.textBlock1.Text += "\nEx: {" + entities[0] + "." + Parser.boolEvents[0] + "(" + entities[1] + ")}";

            this.textBlock1.Text += "\n\nInteger functions available:\n";
            this.textBlock1.Text += string.Join(", ", Parser.valEvents);
            this.textBlock1.Text += "\nEx: {" + entities[0] + "." + Parser.valEvents[0] + "(" + entities[1] + ") > 100}";

            this.textBlock1.Text += "\n\nPrescences' properties available:\n";
            this.textBlock1.Text += string.Join(", ", Parser.valProperties);
            this.textBlock1.Text += "\nEx: {" + entities[0] + "." + Parser.valProperties[0] + " < 20}";

            this.textBlock1.Text += "\n\nNumerical operators available:\n";
            this.textBlock1.Text += string.Join(", ", Parser.valOperators);

            this.textBlock1.Text += "\n\nLogical operators available:\n&&, ||";
            this.textBlock1.Text += "\nEx: {{" + entities[0] + "." + Parser.boolEvents[0] + "(" + entities[1] + ")} && {" + entities[0] + "." + Parser.valProperties[0] + " < 20}}";

            validator = new Parser(entities);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            validator.setRule(textBox2.Text.Trim().ToLower());

            if (textBox2.Text.Trim() != "")
            {
                this.parent.load.updateText(textBox1.Text.Trim());

                List <Relations> relations = validator.splitConditions();

                if (relations != null)
                {
                    if (textBox1.Text.Trim() == "")
                    {
                        MessageBoxResult result = MessageBox.Show("The rule needs a name", "Error");
                        return;
                    }

                    if (textBox1.Text.Trim().IndexOf('(') != -1 || textBox1.Text.Trim().IndexOf(')') != -1)
                    {
                        MessageBoxResult result = MessageBox.Show("The character ( and ) are reserved", "Error");
                        return;
                    }

                    ListBoxItem temp = new ListBoxItem();
                    temp.Tag = relations;
                    temp.Content = textBox1.Text.Trim();

                    if (this.item != null)
                    {
                        parent.listBox1.Items[parent.listBox1.Items.IndexOf(this.item)] = temp;
                    }
                    else
                    {
                        parent.listBox1.Items.Add(temp);
                    }

                    parent.timeline(parent.listBox1.Items.IndexOf(temp));
                    this.Close();
                }
                else
                {
                    //MessageBoxResult result = MessageBox.Show("There is an error with your rule", "Error");
                }
            }
        }
    }
}

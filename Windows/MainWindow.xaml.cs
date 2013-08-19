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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using System.Windows.Threading;
using System.Text.RegularExpressions;

using TimelineSample.Windows;

namespace TimelineSample
{
    public partial class MainWindow : Window
    {
        private ReadProximityEvents info = null;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private int lastUpdate = -1;
        private int[] sync = new int[4];

        private List<int> totalEvents = new List<int>();
        private List<Comment> comments = new List<Comment>();
        private List<Csv> csv = new List<Csv>();

        public Loading load = new Loading("");

        private bool updatedColumns = false;

        private DateTime startTime;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region interpreter
        private void LoadColumns()
        {
            if (dgTimeline.Columns.Count != 0)
            {
                dgTimeline.Columns.Clear();
            }

            int sTime = 0;
            int eTime = (int)info.getPackages()[info.getPackages().Count() - 1].getTime() / 60;

            //buttons
            DataGridTemplateColumn dgc = new DataGridTemplateColumn();
            DataTemplate dtm = new DataTemplate();

            FrameworkElementFactory label = new FrameworkElementFactory(typeof(Label));
            label.SetValue(Label.ContentProperty, new Binding("Name"));

            FrameworkElementFactory button = new FrameworkElementFactory(typeof(Button));
            button.SetValue(Button.ContentProperty, "7");
            button.SetValue(Button.FontFamilyProperty, new FontFamily("Webdings"));
            button.SetValue(Button.HeightProperty, 18.0);
            button.AddHandler(Button.ClickEvent, new RoutedEventHandler(previous_event));

            FrameworkElementFactory button2 = new FrameworkElementFactory(typeof(Button));
            button2.SetValue(Button.ContentProperty, "8");
            button2.SetValue(Button.FontFamilyProperty, new FontFamily("Webdings"));
            button2.SetValue(Button.HeightProperty, 18.0);
            button2.AddHandler(Button.ClickEvent, new RoutedEventHandler(next_event));

            FrameworkElementFactory label2 = new FrameworkElementFactory(typeof(Label));
            label2.SetValue(Label.ContentProperty, "");

            FrameworkElementFactory btnReset = new FrameworkElementFactory(typeof(DockPanel));
            btnReset.AppendChild(label);
            btnReset.AppendChild(button);
            btnReset.AppendChild(button2);
            btnReset.AppendChild(label2);

            btnReset.SetValue(DockPanel.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            btnReset.SetValue(DockPanel.HeightProperty, 24.0);

            //set the visual tree of the data template  
            dtm.VisualTree = btnReset;
            dgc.Header = "Events / Time";
            dgc.CellTemplate = dtm;
            dgc.Width = 300;

            dgTimeline.Columns.Add(dgc);

            for (int i = sTime; i <= eTime; i++)
            {
                string header = (i/60).ToString("D2") + ":" + (i%60).ToString("D2");
                DataGridTemplateColumn dgTempCol = new DataGridTemplateColumn();
                dgTempCol.Header = header;
                dgTempCol.Width = 120;

                //Console.WriteLine("adding " + i);

                FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TimelineControl));
                //fef.SetValue(TimelineControl.IdProperty, i);

                Binding bTemp = new Binding("Time" + (i%60).ToString());
                bTemp.Mode = BindingMode.TwoWay;
                bTemp.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                fef.SetBinding(TimelineControl.MinutesProperty, bTemp);

                Binding bTemp2 = new Binding("Id");
                bTemp2.Mode = BindingMode.TwoWay;
                bTemp2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                fef.SetBinding(TimelineControl.IdProperty, bTemp2);

                DataTemplate dt = new DataTemplate();
                dt.VisualTree = fef;
                dgTempCol.CellTemplate = dt;

                dgTimeline.Columns.Add(dgTempCol);
            }
        }

        private void dtGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                ScrollViewerTime.ScrollToHorizontalOffset(e.HorizontalOffset);
            }

            this.time.Y2 = e.ViewportHeight * 25;
            this.ScrollViewerTime.Height = e.ViewportHeight * 25;
        }

        private int findInitialPosition(int position, int lastTime, List<Package> package, List<Relations> relations_list)
        {
            int initialPosition = 0;

            for (int y = position; y >= 0 && lastTime - Math.Floor((double)(relations_list[0].orderTime / 2)) < (int)package[y].getTime(); y--)
            {
                //Console.WriteLine(y + " " + tempTime + " " + relations_list[0].orderTime + " " + (int)package[y].getTime());
                initialPosition = y;
            }

            return initialPosition;
        }

        private int findFinalPosition(int position, int lastTime, List<Package> package, List<Relations> relations_list)
        {
            int finalPosition = 0;

            for (int y = position; y < package.Count() && lastTime + Math.Floor((double)(relations_list[0].orderTime / 2)) >= (int)package[y].getTime(); y++)
            {
                //Console.WriteLine(y + " " + tempTime + " " + relations_list[0].orderTime + " " + (int)package[y].getTime());
                finalPosition = y;
            }

            return finalPosition;
        }

        public void timeline(int index)
        {
            ListBoxItem item = (ListBoxItem)listBox1.Items[index];

            List<Relations> relations_list = (List<Relations>)item.Tag;

            if (relations_list.Count() == 0)
            {
                return;
            }

            TimeScale temp = new TimeScale { Name = item.Content.ToString() + " ", Id = index % 12 };

            if (index >= totalEvents.Count)
            {
                totalEvents.Add(0);
            }

            //if (index < totalEvents.Count && dgTimeline.Items.Count > 0 && dgTimeline.Items.Count > index && index != -1)
            //{
                totalEvents[index] = 0;
            //}

            List <Package> package = info.getPackages();
            int lastTime = 0;
            int lastTimePosition = 0;

            int initialPosition = -1;
            int finalPosition = -1;

            for (int x = 0; x < package.Count(); x++ )
            {
                int tempTime = (int)package[x].getTime();

                if (tempTime <= lastTime)
                {
                    continue;
                }

                if (relations_list[0].order != Relations.orderType.NULL)
                {
                    initialPosition = findInitialPosition(x, tempTime, package, relations_list);
                    finalPosition = findFinalPosition(x, tempTime, package, relations_list);

                    //Console.WriteLine("initial " + tempTime + " " + initialPosition + " " + finalPosition + " " + (int)package[initialPosition].getTime() + " " + (int)package[finalPosition].getTime());

                    int first = -1;
                    int last = -1;
                    bool strict = true;
                    int lastPositionTrue = -1;

                    for (int j = 0; j < relations_list.Count(); j++)
                    {
                        if (relations_list[j].rule != null)
                        {
                            for (int y = initialPosition; y < finalPosition; y++)
                            {
                                relations_list[j].result = false;
                                relations_list[j].result = (relations_list[j].result == true ? true : Parser.evaluate(relations_list[j].condition, package[y]));

                                if (relations_list[j].result == true)
                                {
                                    if (j == 0)
                                    {
                                        first = y;
                                    }

                                    last = y;

                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (relations_list[relations_list[j].relation1].result == null || relations_list[relations_list[j].relation2].result == null)
                            {
                                MainWindow.error("Some previous relation was not evalueated: " + relations_list[j].relation1 + " " + relations_list[j].relation2, true);
                            }
                            else
                            {
                                relations_list[j].result = Relations.Compare(relations_list[relations_list[j].relation1].result, relations_list[relations_list[j].relation2].result, relations_list[j].operation);
                            }
                        }

                        if (relations_list[0].order == Relations.orderType.STRICT)
                        {
                            if (relations_list[j].result == false || lastPositionTrue > last)
                            {
                                strict = false;
                                break;
                            }
                            else
                            {
                                lastPositionTrue = last;
                            }
                        }
                    }

                    if (relations_list.Last().result == true && first != -1 && last != -1 && strict)
                    {
                        //have to add 1 because of the int casts here and before
                        first = (int)package[first].getTime() + 1;
                        last = (int)package[last].getTime() + 1;

                        for (int y = Math.Min(first, last); y <= Math.Max(first, last); y++)
                        {
                            temp = this.addEvent(temp, y);
                        }

                        totalEvents[index] += 1;
                    }
                }
                else
                {
                    if (!this.hasEvent(temp, tempTime))
                    {
                        foreach (Relations relation in relations_list)
                        {
                            if (relation.rule != null)
                            {
                                for (int y = lastTimePosition; y <= x; y++)
                                {
                                    relation.result = false;
                                    relation.result = (relation.result == true ? true : Parser.evaluate(relation.condition, package[y]));

                                    if (relation.result == true)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (relations_list[relation.relation1].result == null || relations_list[relation.relation2].result == null)
                                {
                                    MainWindow.error("Some previous relation was not evalueated: " + relation.relation1 + " " + relation.relation2, true);
                                }
                                else
                                {
                                    relation.result = Relations.Compare(relations_list[relation.relation1].result, relations_list[relation.relation2].result, relation.operation);
                                }
                            }
                        }

                        if (relations_list.Last().result == true)
                        {
                            temp = this.addEvent(temp, (int)package[x].getTime());
                            totalEvents[index] += 1;
                        }
                    }
                }

                if (tempTime > lastTime)
                {
                    lastTime = tempTime;
                    lastTimePosition = x;
                }
            }

            if (index < totalEvents.Count)
            {
                temp.Name += "(" + totalEvents[index] + ")";
            }

            if (dgTimeline.Items.Count > 0 && dgTimeline.Items.Count > index && index != -1)
            {
                temp.Name = temp.Name.Remove(temp.Name.IndexOf('('));
                temp.Name += "(" + totalEvents[index] + ")";

                dgTimeline.Items[index] = temp;
                dgTimeline.Items.Refresh();
            }
            else
            {
                dgTimeline.Items.Add(temp);
            }

            dgTimeline.Columns[0].Width = DataGridLength.Auto;
            this.updatedColumns = false;
        }

        private TimeScale addEvent(TimeScale temp, int time)
        {
            PropertyInfo propertyInfo = temp.GetType().GetProperty("Time" + (time / 60));

            if (propertyInfo.GetValue(temp, null) != null)
            {
                if (propertyInfo.GetValue(temp, null).ToString().IndexOf((time % 60).ToString()) == -1)
                {
                    propertyInfo.SetValue(temp, Convert.ChangeType(propertyInfo.GetValue(temp, null) +
                        "," + (time % 60), propertyInfo.PropertyType), null);
                }
            }
            else
            {
                propertyInfo.SetValue(temp, Convert.ChangeType(time % 60, propertyInfo.PropertyType), null);
            }

            return temp;
        }

        private bool hasEvent(TimeScale temp, int time)
        {
            PropertyInfo propertyInfo = temp.GetType().GetProperty("Time" + (time / 60));

            if (propertyInfo.GetValue(temp, null) != null)
            {
                if (propertyInfo.GetValue(temp, null).ToString().IndexOf((time % 60).ToString()) != -1)
                {
                    return true;
                }
            }

            return false;
        }

        private void addTime(int time)
        {
            for (int x = 0; x < this.dgTimeline.Items.Count; x++)
            {
                PropertyInfo propertyInfo = ((TimeScale)this.dgTimeline.Items[x]).GetType().GetProperty("Time" + (time / 60));

                object tmp = propertyInfo.GetValue(((TimeScale)this.dgTimeline.Items[x]), null);

                if (tmp != null && tmp.ToString().Trim() != "")
                {
                    propertyInfo.SetValue(((TimeScale)this.dgTimeline.Items[x]), Convert.ChangeType(propertyInfo.GetValue(((TimeScale)this.dgTimeline.Items[x]), null) + "," + (time % 60) + "a", propertyInfo.PropertyType), null);
                }
                else
                {
                    propertyInfo.SetValue(((TimeScale)this.dgTimeline.Items[x]), Convert.ChangeType(time % 60 + "a", propertyInfo.PropertyType), null);
                }
            }
        }

        private void removeTime(int time)
        {
            for (int x = 0; x < this.dgTimeline.Items.Count; x++)
            {
                PropertyInfo propertyInfo = ((TimeScale)this.dgTimeline.Items[x]).GetType().GetProperty("Time" + (time / 60));

                if (propertyInfo.GetValue(((TimeScale)this.dgTimeline.Items[x]), null) != null)
                {
                    string array = propertyInfo.GetValue(((TimeScale)this.dgTimeline.Items[x]), null).ToString();

                    propertyInfo.SetValue(((TimeScale)this.dgTimeline.Items[x]), Convert.ChangeType(array.Replace((time % 60).ToString() + "a", "").Replace(",,", ","), propertyInfo.PropertyType), null);
                }
            }
        }

        private void removeAllEvents(char type)
        {
            for (int y = 0; y < this.dgTimeline.Items.Count; y++)
            {
                for (int x = 0; x < dgTimeline.Columns.Count; x++)
                {
                    PropertyInfo propertyInfo = ((TimeScale)this.dgTimeline.Items[y]).GetType().GetProperty("Time" + (x));

                    if (propertyInfo.GetValue(((TimeScale)this.dgTimeline.Items[y]), null) != null)
                    {
                        string array = propertyInfo.GetValue(((TimeScale)this.dgTimeline.Items[y]), null).ToString();

                        if (array.IndexOf(type) != -1)
                        {
                            string newArray = "";

                            foreach (string tmp in array.Split(','))
                            {
                                if (tmp.IndexOf(type) == -1)
                                {
                                    newArray += tmp + ",";
                                }
                            }

                            if (newArray != "")
                            {
                                newArray = newArray.Remove(newArray.Length - 1);
                            }

                            propertyInfo.SetValue(((TimeScale)this.dgTimeline.Items[y]), Convert.ChangeType(newArray, propertyInfo.PropertyType), null);
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (info == null)
            {
                MessageBoxResult result = MessageBox.Show("Open a recorded section first", "Error");
                return;
            }

            Event window = new Event(this, info.getEntities(), null);
            window.ShowDialog();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                if (info == null)
                {
                    MessageBoxResult result = MessageBox.Show("Open a recorded section first", "Error");
                    return;
                }

                ListBoxItem item = (ListBoxItem)listBox1.SelectedItem;

                if (item.Tag != null)
                {
                    List<Relations> cond = (List<Relations>)item.Tag;

                    Event window = new Event(this, info.getEntities(), item);

                    window.textBox2.Text = cond[0].displayRule;
                    window.textBox1.Text = item.Content.ToString();

                    window.ShowDialog();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        { 
            if (e.Key == Key.Delete && listBox1.SelectedIndex != -1)
            {
                dgTimeline.Items.RemoveAt(listBox1.SelectedIndex);
                totalEvents.RemoveAt(listBox1.SelectedIndex);

                if (((ListBoxItem)listBox1.Items[listBox1.SelectedIndex]).Tag == null)
                {
                    for (int x = 0; x < this.csv.Count; x++)
                    {
                        //Console.WriteLine(x + " " + this.csv[x].name + " " + ((ListBoxItem)listBox1.Items[listBox1.SelectedIndex]).Content.ToString());

                        if (this.csv[x].name == ((ListBoxItem)listBox1.Items[listBox1.SelectedIndex]).Content.ToString())
                        {
                            //Console.WriteLine("remove csv at " + x);

                            this.csv.RemoveAt(x);

                            break;
                        }
                    }
                }

                for (int x = 0; x < this.comments.Count; x++)
                {
                    if (this.comments[x].row == listBox1.SelectedIndex)
                    {
                        //Console.WriteLine("remove comment row " + this.comments[x].row);
                        this.comments.RemoveAt(x);
                        x--;
                    }
                    else
                    {
                        if (this.comments[x].row > listBox1.SelectedIndex)
                        {
                            this.comments[x].row--;
                        }
                    }
                }

                //for (int x = 0; x < this.comments.Count; x++)
                //{
                //    Console.WriteLine("comments row " + this.comments[x].row);
                //}

                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }

            if (e.Key == Key.Space)
            {
                if (btnPlay.Visibility == Visibility.Visible)
                {
                    this.btnPlay_Click(null, null);
                }
                else
                {
                    this.btnPause_Click(null, null);
                }
            }
        }

        #endregion

        #region load/save/interface stuff

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        public static void error(String err, bool exit)
        {
            MessageBoxResult result = MessageBox.Show(err, "Error");

            if (exit)
            {
                Environment.Exit(-1);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".ptr";
            dlg.Filter = "Proximity Toolkit Recorded Session Files (*.ptr)|*.ptr";

            // Get the selected file name and display in a TextBox 
            if (dlg.ShowDialog() == true)
            {
                info = new ReadProximityEvents(dlg.FileName);
                info.readAll();
                LoadColumns();

                dgTimeline.Columns[0].Width = DataGridLength.Auto;
                this.updatedColumns = false;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".pta";
            dlg.Filter = "Proximity Toolkit Analysis Files (*.pta)|*.pta";

            // Get the selected file name and display in a TextBox 
            if (dlg.ShowDialog() == true)
            {
                MenuItem_Click_3(null, null);

                String[] temp = System.IO.File.ReadAllText(@dlg.FileName).Split('`');

                if (temp.Count() > 1)
                {
                    //this.load.updateText("");
                    //Console.WriteLine("Creating ");
                    this.load.Show();

                    this.info = new ReadProximityEvents(temp[0]);
                    info.readAll();
                    LoadColumns();

                    int total = Convert.ToInt32(temp[1]);

                    for (int x = 2; x < (2 + total); x++)
                    {
                        String[] item = temp[x].Split('^');

                        ListBoxItem myItem = new ListBoxItem();
                        myItem.Content = item[0];

                        //Console.WriteLine("updating");
                        this.load.updateText(myItem.Content.ToString());

                        //Console.WriteLine(item[1]);

                        if (item.Count() == 2)
                        {
                            Parser parser = new Parser(info.getEntities());
                            parser.setRule(item[1]);

                            myItem.Tag = parser.splitConditions();

                            listBox1.Items.Add(myItem);
                            //listBox1.Items.Refresh();

                            timeline(listBox1.Items.Count - 1);
                        }
                        else
                        {
                            this.add_csv(item[0], item[1]);
                        }
                    }

                    listBox1.Items.Refresh();

                    //this.lastUpdate = Convert.ToInt32(temp[10 + total]);

                    sync[0] = Convert.ToInt32(temp[6 + total]);
                    sync[1] = Convert.ToInt32(temp[7 + total]);
                    sync[2] = Convert.ToInt32(temp[8 + total]);
                    sync[3] = Convert.ToInt32(temp[9 + total]);

                    if (temp[2 + total] != "")
                    {
                        mediaPlayerMain0.Source = new Uri(@temp[2 + total]);
                        btnMediaPlayerMain0.Background = Brushes.Transparent;
                        mediaPlayerMain0.Position = TimeSpan.FromMilliseconds(this.lastUpdate * 1000 + sync[0]);
                    }

                    if (temp[3 + total] != "")
                    {
                        mediaPlayerMain1.Source = new Uri(@temp[3 + total]);
                        btnMediaPlayerMain1.Background = Brushes.Transparent;
                        mediaPlayerMain1.Position = TimeSpan.FromMilliseconds(this.lastUpdate * 1000 + sync[1]);
                    }

                    if (temp[4 + total] != "")
                    {
                        mediaPlayerMain2.Source = new Uri(@temp[4 + total]);
                        btnMediaPlayerMain2.Background = Brushes.Transparent;
                        mediaPlayerMain2.Position = TimeSpan.FromMilliseconds(this.lastUpdate * 1000 + sync[2]);
                    }

                    if (temp[5 + total] != "")
                    {
                        mediaPlayerMain3.Source = new Uri(@temp[5 + total]);
                        btnMediaPlayerMain3.Background = Brushes.Transparent;
                        mediaPlayerMain3.Position = TimeSpan.FromMilliseconds(this.lastUpdate * 1000 + sync[3]);
                    }

                    total = 11 + total;

                    int size = Convert.ToInt32(temp[total]);

                    for (int x = total + 1; x < total + size + 1; x++)
                    {
                        String[] item = temp[x].Split('^');

                        //Console.WriteLine("openning comment " + item[0] + " " + item[3] + " " + item[1]);

                        this.comments.Add(new Comment(Convert.ToInt32(item[0]), Convert.ToInt32(item[2]), item[3], Convert.ToInt32(item[1])));
                    }

                    /*total = total + size + 1;

                    size = Convert.ToInt32(temp[total]);

                    for (int x = total + 1; x < total + size + 1; x++)
                    {
                        String[] item = temp[x].Split('^');

                        Console.WriteLine("opening track "  + item[0] + " " + item[1]);

                        this.add_csv(item[0], item[1]);
                    }*/

                    this.showCommentsTimeline();
                    this.btnPlay_Click(null, null);

                    dgTimeline.Items.Refresh();

                    this.load.Hide();
                }
                else
                {
                    MainWindow.error("This file is corrupted or has no information in it", true);
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (listBox1.Items.Count == 0 || this.info.getFile() == "")
            {
                MessageBoxResult result = MessageBox.Show("You haven't done anything, there is nothing to be saved", "Error");
                return;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".pta";
            dlg.Filter = "Proximity Toolkit Analysis Files (*.pta)|*.pta";

            if (dlg.ShowDialog() == true)
            {
                string s = info.getFile() + "`" + listBox1.Items.Count + "`";

                foreach (ListBoxItem item in listBox1.Items)
                {
                    if (item.Tag != null)
                    {
                        s += item.Content + "^" + ((List<Relations>)item.Tag)[0].displayRule + "`";
                    }
                    else
                    {
                        foreach (Csv item1 in this.csv)
                        {
                            if (item1.name == item.Content)
                            {
                                s += item1.file + "^" + item1.name + "^`";
                            }
                        }
                    }
                }

                s += (mediaPlayerMain0.Source != null ? mediaPlayerMain0.Source.ToString() : "") + "`";
                s += (mediaPlayerMain1.Source != null ? mediaPlayerMain1.Source.ToString() : "") + "`";
                s += (mediaPlayerMain2.Source != null ? mediaPlayerMain2.Source.ToString() : "") + "`";
                s += (mediaPlayerMain3.Source != null ? mediaPlayerMain3.Source.ToString() : "") + "`";

                s += sync[0] + "`";
                s += sync[1] + "`";
                s += sync[2] + "`";
                s += sync[3] + "`";

                s += this.lastUpdate + "`";

                s += this.comments.Count + "`";

                foreach (Comment item in this.comments)
                {
                    s += item.time + "^" + item.row + "^" + item.duration + "^" + item.comment + "`";
                }

                /*s += this.csv.Count() + "`";

                foreach (Csv item in this.csv)
                {
                    s += item.file + "^" + item.name + "`";
                }*/

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@dlg.FileName))
                {
                    file.WriteLine(s);
                }
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            this.info = null;
            listBox1.Items.Clear();
            dgTimeline.Items.Clear();
            dgTimeline.Columns.Clear();

            mediaPlayerMain0.Source = null;
            mediaPlayerMain1.Source = null;
            mediaPlayerMain2.Source = null;
            mediaPlayerMain3.Source = null;

            sync[0] = 0;
            sync[1] = 0;
            sync[2] = 0;
            sync[3] = 0;

            this.comments.Clear();
            this.csv.Clear();
        }

        private void add_csv(string file, string track = null)
        {
            List<ExternalTrack> listData = new List<ExternalTrack>();

            string[] lines = System.IO.File.ReadAllLines(@file);

            foreach (string line in lines)
            {
                string[] data = line.Split(',');

                if (data.Count() != 4)
                {
                    MessageBox.Show("The selected CSV file is not in the following format: time<int>,name<string>,comment<string>,comment_length<int>", "Error");
                    return;
                }

                ExternalTrack temp = new ExternalTrack();

                try
                {
                    //Console.WriteLine(this.info.getFirstDate() + " " + DateTime.Parse(data[0]) + " " + (DateTime.Parse(data[0]) - this.info.getFirstDate()).TotalSeconds);

                    temp.time = Convert.ToInt32((DateTime.Parse(data[0]) - this.info.getFirstDate()).TotalSeconds);
                }
                catch (FormatException ee)
                {
                    MessageBox.Show("The time (" + data[0] + ") is not a datetime valid format", "Error");
                    return;
                }

                try
                {
                    temp.duration = Convert.ToInt32(data[3]);
                }
                catch (FormatException ee)
                {
                    MessageBox.Show("The duration (" + data[3] + ") is not an integer", "Error");
                    return;
                }

                if (data[1].Trim() == "")
                {
                    MessageBox.Show("The column name (the second of each line) can not be null, it is on time " + temp.time, "Error");
                    return;
                }

                temp.name = data[1];
                temp.comment = data[2];

                listData.Add(temp);
            }

            List<String> listTracksName = new List<String>();

            foreach (ExternalTrack list in listData)
            {
                if (track != null && list.name != track)
                {
                    continue;
                }

                //Console.WriteLine("b " + track);

                if (listTracksName.IndexOf(list.name) == -1)
                {
                    listTracksName.Add(list.name);

                    Csv tmp = new Csv();
                    tmp.name = list.name;
                    tmp.file = file;

                    this.csv.Add(tmp);

                    TimeScale temp = new TimeScale { Name = list.name + " " };

                    temp = this.addEvent(temp, list.time);

                    totalEvents.Add(1);

                    temp.Name += "(" + 1 + ")";
                    dgTimeline.Items.Add(temp);

                    listBox1.Items.Add(list.name);
                    //Console.WriteLine("adding " + list.name + " " + listBox1.Items.Count + " " + track);

                    if (list.comment.Trim() != "")
                    {
                        this.comments.Add(new Comment(list.time, list.duration, list.comment, this.listBox1.Items.Count - 1));
                    }
                }
                else
                {
                   // Console.WriteLine(totalEvents.Count + " " + listBox1.Items.IndexOf(list.name));
                    totalEvents[listBox1.Items.IndexOf(list.name)]++;

                    TimeScale temp = (TimeScale)dgTimeline.Items[listBox1.Items.IndexOf(list.name)];
                    temp = this.addEvent(temp, list.time);

                    temp.Name = temp.Name.Remove(temp.Name.IndexOf('('));
                    temp.Name += "(" + totalEvents[listBox1.Items.IndexOf(list.name)] + ")";

                    dgTimeline.Items[listBox1.Items.IndexOf(list.name)] = temp;

                    if (list.comment.Trim() != "")
                    {
                        this.comments.Add(new Comment(list.time, list.duration, list.comment, listBox1.Items.IndexOf(list.name)));
                    }
                }
            }

            dgTimeline.Items.Refresh();

            for (int x = 0; x < listBox1.Items.Count; x++)
            {
                if (listBox1.Items[x] is String)
                {
                    ListBoxItem myItem = new ListBoxItem();
                    myItem.Content = listBox1.Items[x];

                    listBox1.Items[x] = myItem;
                }
            }

            dgTimeline.Columns[0].Width = DataGridLength.Auto;
            this.updatedColumns = false;

            if (track == null)
            {
                this.showCommentsTimeline();
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (this.info == null)
            {
                MessageBox.Show("You must load a recorded session first", "Error");
                return;
            }

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Comma-Separated Values (*.csv)|*.csv";

            if (dlg.ShowDialog() == true)
            {
                this.add_csv(dlg.FileName);
            }
        }

        #endregion

        #region video loading/control

        private void btnMediaPlayerMain_Click(object sender, RoutedEventArgs e)
        {
            Button temp = (Button)this.FindName(((Button)sender).Name);

            if (temp.Background != Brushes.Transparent)
            {
                MediaElement tmp = (MediaElement)this.FindName(((Button)sender).Name.Substring(3).Replace("Media", "media"));

                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.DefaultExt = ".avi";
                dlg.Filter = "Video files (*.avi, *.wmv, *.mts, *.mpeg, *.mpg, *.3gp, *.mp4)|*.avi;*.wmv;*.mts;*.mpeg;*.mpg;*.3gp;*.mp4";

                if (dlg.ShowDialog() == true)
                {
                    tmp.Source = new Uri(@dlg.FileName);
                    temp.Background = Brushes.Transparent;

                    btnStop_Click(null, null);
                }
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("play ");

            if (dgTimeline.Columns.Count == 0)
            {
                MessageBoxResult result = MessageBox.Show("Open a recorded section first", "Error");
                return;
            }

            if (mediaPlayerMain0.Source != null)
            {
                mediaPlayerMain0.Play();
            }

            if (mediaPlayerMain1.Source != null)
            {
                mediaPlayerMain1.Play();
            }

            if (mediaPlayerMain2.Source != null)
            {
                mediaPlayerMain2.Play();
            }

            if (mediaPlayerMain3.Source != null)
            {
                mediaPlayerMain3.Play();
            }

            /*if (mediaPlayerMain4.Source != null)
            {
                mediaPlayerMain4.Play();
            }

            if (mediaPlayerMain5.Source != null)
            {
                mediaPlayerMain5.Play();
            }*/

            //this.removeAllEvents('a');

            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible;
            
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dispatcherTimer.Start();

            this.startTime = DateTime.Now;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("pause ");

            dispatcherTimer.Stop();

            if (mediaPlayerMain0.Source != null)
            {
                mediaPlayerMain0.Pause();
            }

            if (mediaPlayerMain1.Source != null)
            {
                mediaPlayerMain1.Pause();
            }

            if (mediaPlayerMain2.Source != null)
            {
                mediaPlayerMain2.Pause();
            }

            if (mediaPlayerMain3.Source != null)
            {
                mediaPlayerMain3.Pause();
            }

            /*if (mediaPlayerMain4.Source != null)
            {
                mediaPlayerMain4.Pause();
            }

            if (mediaPlayerMain5.Source != null)
            {
                mediaPlayerMain5.Pause();
            }*/

            btnPause.Visibility = Visibility.Hidden;
            btnPlay.Visibility = Visibility.Visible;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();

            if (mediaPlayerMain0.Source != null)
            {
                mediaPlayerMain0.Stop();
            }

            if (mediaPlayerMain1.Source != null)
            {
                mediaPlayerMain1.Stop();
            }

            if (mediaPlayerMain2.Source != null)
            {
                mediaPlayerMain2.Stop();
            }

            if (mediaPlayerMain3.Source != null)
            {
                mediaPlayerMain3.Stop();
            }

            /*if (mediaPlayerMain4.Source != null)
            {
                mediaPlayerMain4.Stop();
            }

            if (mediaPlayerMain5.Source != null)
            {
                mediaPlayerMain5.Stop();
            }*/

            btnPause.Visibility = Visibility.Hidden;
            btnPlay.Visibility = Visibility.Visible;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            int time = 0;

            MediaElement[] temp = new MediaElement[4];

            /*temp[0] = mediaPlayerMain0;
            temp[1] = mediaPlayerMain1;
            temp[2] = mediaPlayerMain2;
            temp[3] = mediaPlayerMain3;

            for (int x = 0; x < 4; x++)
            {
                if (temp[x].Source != null)
                {
                    int tmp = (int)temp[x].Position.TotalSeconds;

                    if (tmp > time)
                    {
                        time = tmp;
                    }
                }
            }*/

            //Console.WriteLine(DateTime.Now.ToString("hh:mm:ss"));

            time = Convert.ToInt32((DateTime.Now - startTime).TotalSeconds);

            if (time != this.lastUpdate)
            {
                if (this.updatedColumns == false)
                {
                    Thickness tmp = ScrollViewerTime.Margin;

                    tmp.Left = dgTimeline.Columns[0].ActualWidth + 9;

                    ScrollViewerTime.Margin = tmp;

                    Double width = 0.0;

                    foreach (DataGridColumn column in dgTimeline.Columns)
                    {
                        width += column.ActualWidth; 
                    }

                    CanvasTime.Width = width;

                    this.updatedColumns = true;
                }

                this.time.X1 = lastUpdate * 2;
                this.time.X2 = lastUpdate * 2;

                lastUpdate = time;

                this.scroll();

                this.showCommentsTextbox();
            }
        }

        private void scroll(Boolean recursive = false)
        {
            ScrollViewer scrollView = GetScrollbar(dgTimeline);

            //Console.WriteLine(this.time.X1 + " " + scrollView.ActualWidth + " " + scrollView.HorizontalOffset + " " + (scrollView.ActualWidth - (this.time.X1 - scrollView.HorizontalOffset)));

            double temp = scrollView.ActualWidth - (this.time.X1 - scrollView.HorizontalOffset);

            //have to go further away
            if (temp < 150)
            {
                //if (recursive)
                //{
                    scrollView.ScrollToHorizontalOffset(scrollView.HorizontalOffset + Math.Abs(300 - temp));
                //}
                //else
                //{
                  //  scrollView.ScrollToHorizontalOffset(scrollView.HorizontalOffset + 100);
                //}
            }

            //have to go back a bit
            if (temp > (scrollView.ActualWidth - 50))
            {
                //if (recursive)
                //{
                    scrollView.ScrollToHorizontalOffset(scrollView.HorizontalOffset - Math.Abs(300 - temp));
                //}
            }
        }

        private static ScrollViewer GetScrollbar(DependencyObject dep)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dep); i++)
            {
                var child = VisualTreeHelper.GetChild(dep, i);
                if (child != null && child is ScrollViewer)
                    return child as ScrollViewer;
                else
                {
                    ScrollViewer sub = GetScrollbar(child);
                    if (sub != null)
                        return sub;
                }
            }
            return null;
        }

        public void previous_event(object sender, RoutedEventArgs e)
        {
            TimeScale temp = ((TimeScale)this.dgTimeline.Items[dgTimeline.SelectedIndex]);

            for (int x = this.lastUpdate / 60; x >= 0; x--)
            {
                PropertyInfo propertyInfo = temp.GetType().GetProperty("Time" + (x));

                if (propertyInfo.GetValue(temp, null) != null)
                {
                    String[] array = propertyInfo.GetValue(temp, null).ToString().Split(',');

                    int[] numbers = new int[array.Count()];

                    int y = 0;

                    foreach (string tmp in array)
                    {
                        if (tmp.Trim() != "" && tmp.Trim().IndexOf('b') == -1)
                        {
                            numbers[y] = Convert.ToInt32(tmp.Trim());
                            y++;
                        }
                    }

                    Array.Sort(numbers);
                    Array.Reverse(numbers);

                    int last;

                    if (x == this.lastUpdate / 60)
                    {
                        last = numbers[0];
                    }
                    else
                    {
                        last = 59;
                    }

                    foreach (int tmp in numbers)
                    {
                        if (numbers.Last() == tmp && tmp != 0 && (x * 60) + tmp < this.lastUpdate)
                        {
                            //Console.WriteLine("last " + x + " " + numbers.Count() + " " + tmp);

                            goTo(x * 60 + tmp - 1);
                            return;
                        }


                        if ((60 * x) + tmp < this.lastUpdate && Math.Abs(tmp - last) > 1 && numbers.Contains(last + 1))
                        {
                            //Console.WriteLine(tmp + " " + last + " " + x + " " + Math.Abs(tmp - last) + " " + numbers.Contains(last + 1));

                            goTo(x * 60 + (tmp == 0 ? last : tmp) - 1);
                            return;
                        }

                        last = tmp;
                    }
                }
            }
        }

        public void next_event(object sender, RoutedEventArgs e)
        {
            TimeScale temp = ((TimeScale)this.dgTimeline.Items[dgTimeline.SelectedIndex]);

            this.lastUpdate++;

            //Console.WriteLine("aaaa " + this.lastUpdate + " " + this.lastUpdate / 60);

            for (int x = this.lastUpdate / 60; x < dgTimeline.Columns.Count; x++)
            {
                //Console.WriteLine("ddd " + x);

                PropertyInfo propertyInfo = temp.GetType().GetProperty("Time" + (x));

                if (propertyInfo.GetValue(temp, null) != null)
                {
                    String[] array = propertyInfo.GetValue(temp, null).ToString().Split(',');

                    int[] numbers = new int[array.Count()];

                    int y = 0;

                    foreach (string tmp in array)
                    {
                        if (tmp.Trim() != "" && tmp.Trim().IndexOf('b') == -1)
                        {
                            numbers[y] = Convert.ToInt32(tmp.Trim());
                            y++;
                        }
                    }

                    Array.Sort(numbers);

                    int last;

                    if (x == this.lastUpdate / 60)
                    {
                       last = numbers[0];
                    }
                    else
                    {
                        last = -1;
                    }

                    foreach (int tmp in numbers)
                    {
                        if (tmp - last > 1 && (x == this.lastUpdate / 60 ? this.lastUpdate % 60 < tmp : true))
                        {
                            goTo(x * 60 + tmp - 1);
                            return;
                        }

                        last = tmp;
                    }
                }
            }
        }

        #endregion

        #region video buttons
        private void mediaPlayerBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            Button load = (Button)this.FindName("btn" + ((Border)sender).Name.Replace("Border", "Main").Replace("media", "Media"));

            if (load.Background == Brushes.Transparent)
            {
                Button temp = (Button)this.FindName("btnRemove" + ((Border)sender).Name.Replace("media", "Media"));
                temp.Visibility = Visibility.Visible;

                Button window = (Button)this.FindName("btnWindow" + ((Border)sender).Name.Replace("media", "Media"));
                window.Visibility = Visibility.Visible;

                Button forward = (Button)this.FindName("btnForward" + ((Border)sender).Name.Replace("media", "Media"));
                forward.Visibility = Visibility.Visible;

                Button back = (Button)this.FindName("btnBack" + ((Border)sender).Name.Replace("media", "Media"));
                back.Visibility = Visibility.Visible;

                Button resize = (Button)this.FindName("btnResize" + ((Border)sender).Name.Replace("media", "Media"));
                resize.Visibility = Visibility.Visible;
            }
        }

        private void mediaPlayerBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Button load = (Button)this.FindName("btn" + ((Border)sender).Name.Replace("Border", "Main").Replace("media", "Media"));

            if (load.Background == Brushes.Transparent)
            {
                Button temp = (Button)this.FindName("btnRemove" + ((Border)sender).Name.Replace("media", "Media"));
                temp.Visibility = Visibility.Hidden;

                Button window = (Button)this.FindName("btnWindow" + ((Border)sender).Name.Replace("media", "Media"));
                window.Visibility = Visibility.Hidden;

                Button forward = (Button)this.FindName("btnForward" + ((Border)sender).Name.Replace("media", "Media"));
                forward.Visibility = Visibility.Hidden;

                Button back = (Button)this.FindName("btnBack" + ((Border)sender).Name.Replace("media", "Media"));
                back.Visibility = Visibility.Hidden;

                Button resize = (Button)this.FindName("btnResize" + ((Border)sender).Name.Replace("media", "Media"));
                resize.Visibility = Visibility.Hidden;
            }
        }

        private void btnRemoveMediaPlayerBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            Button temp = (Button)sender;
            temp.Visibility = Visibility.Visible;
        }

        private void btnRemoveMediaPlayerBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Button temp = (Button)sender;
            temp.Visibility = Visibility.Hidden;
        }

        private void btnRemoveMediaPlayerBorder_Click(object sender, RoutedEventArgs e)
        {
            MediaElement tmp = (MediaElement)this.FindName(((Button)sender).Name.Replace("btnRemoveMedia", "media").Replace("Border", "Main"));
            tmp.Source = null;

            Button border = (Button)this.FindName(tmp.Name.Replace("media", "btnMedia"));
            border.Background = Brushes.White;

            ((Button)sender).Visibility = Visibility.Hidden;

            sync[Convert.ToInt32(((Button)sender).Name.ToString().Last()) - 48] = 0;

            Grid temp = (Grid)this.FindName(((Button)sender).Name.Replace("btnRemove", "grid").Replace("Border", "Main"));
            temp.Height = 126;
            temp.Width = 180;
        }

        private void btnWindowMediaPlayerBorder_Click(object sender, RoutedEventArgs e)
        {
            MediaElement tmp = (MediaElement)this.FindName(((Button)sender).Name.Replace("btnWindowMedia", "media").Replace("Border", "Main"));
           
            Window video = new Window();
            video.Height = tmp.NaturalVideoHeight;
            video.Width = tmp.NaturalVideoWidth;

            video.Content = new Rectangle();

            ((Rectangle)video.Content).Fill = new VisualBrush(tmp);

            video.Show();
        }

        private void goTo(int time)
        {
            this.startTime = DateTime.Now - TimeSpan.FromSeconds(time);

            //this.removeAllEvents('b');
            //this.addTime(time);

            //dgTimeline.Items.Refresh();

            if (mediaPlayerMain0.Source != null)
            {
                mediaPlayerMain0.Position = TimeSpan.FromMilliseconds(time * 1000 + sync[0]);
            }

            if (mediaPlayerMain1.Source != null)
            {
                mediaPlayerMain1.Position = TimeSpan.FromMilliseconds(time * 1000 + sync[1]);
            }

            if (mediaPlayerMain2.Source != null)
            {
                mediaPlayerMain2.Position = TimeSpan.FromMilliseconds(time * 1000 + sync[2]);
            }

            if (mediaPlayerMain3.Source != null)
            {
                mediaPlayerMain3.Position = TimeSpan.FromMilliseconds(time * 1000 + sync[3]);
            }

            this.time.X1 = time * 2;
            this.time.X2 = time * 2;

            lastUpdate = time;

            this.scroll(true);
             
            listBox1.Focus();
        }

        private void dgTimeline_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Mouse.DirectlyOver is Line)
            {
                Line rec = ((Line)Mouse.DirectlyOver);

                if (rec.Name != "" && Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    try
                    {
                        int time = (((DataGridCell)e.OriginalSource).Column.DisplayIndex - 1) * 60 +
                            Convert.ToInt32(rec.Name.Replace("Minute", ""));

                        goTo(time);
                    }
                    catch (FormatException ee)
                    {
                        
                    }
                }
            }
        }

        private void btnSyncMediaPlayerBorder_Click(object sender, RoutedEventArgs e)
        {
            int index = Convert.ToInt32(((Button)sender).Name.ToString().Last()) - 48;
            int value;

            if (((Button)sender).Name.IndexOf("Forward") != -1)
            {
                value = 500;
                sync[index] += 500;
            }
            else
            {
                value = -500;
                sync[index] -= 500;
            }

            MediaElement tmp = (MediaElement)this.FindName("mediaPlayerMain" + index);
            tmp.Position = TimeSpan.FromMilliseconds(tmp.Position.TotalMilliseconds + value);
        }

        #endregion

        #region comment
        private int LineToTime(Line rec, DependencyObject dep)
        {
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                return -1;
            }

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                cell.Focus();

                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                DataGridRow row = dep as DataGridRow;
                dgTimeline.SelectedItem = row.DataContext;

                Regex rgx = new Regex("[^0-9]");

                return (dgTimeline.CurrentCell.Column.DisplayIndex - 1) * 60 + Convert.ToInt32(rgx.Replace(rec.Name.Replace("Minute", ""), ""));
            }

            return -1;
        }

        private void dgTimeline_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.DirectlyOver is Line && ((Line)Mouse.DirectlyOver).Name != "")
            {
                Line rec = Mouse.DirectlyOver as Line;

                btnPause_Click(null, null);

                int time = this.LineToTime(rec, (DependencyObject)e.OriginalSource);

                if (rec.ToolTip != null)
                {
                    for (int x = 0; x < this.comments.Count; x++)
                    {
                        for (int y = 0; y < this.comments[x].duration; y++)
                        {
                            if (time == this.comments[x].time + y)
                            {
                                Comments com = new Comments();
                                Comment temp = com.show(this.comments[x]);

                                if (temp != null)
                                {
                                    this.comments[x].comment = temp.comment;
                                    this.comments[x].duration = temp.duration;
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    Comments com = new Comments();
                    Comment temp = com.show(null);

                    if (time != -1 && temp != null)
                    {
                        temp.time = time;
                        temp.row = dgTimeline.Items.IndexOf(((TimeScale)dgTimeline.CurrentCell.Item));

                        this.comments.Add(temp);
                    }
                }

                showCommentsTimeline();

                btnPlay_Click(null, null);
            }
        }

        private void showCommentsTimeline()
        {
            this.removeAllEvents('b');

            foreach (Comment item in this.comments)
            {
                //Console.WriteLine("ddd " + item.row + " " + dgTimeline.Items.Count);
                TimeScale temp = (TimeScale)dgTimeline.Items[item.row];

                for (int x = 0; x < item.duration; x++)
                {
                    PropertyInfo propertyInfo = temp.GetType().GetProperty("Time" + ((item.time + x) / 60));

                    if (propertyInfo.GetValue(temp, null) != null)
                    {
                        propertyInfo.SetValue(temp, Convert.ChangeType(propertyInfo.GetValue(temp, null) +
                            "," + ((item.time + x) % 60) + "b", propertyInfo.PropertyType), null);
                    }
                    else
                    {
                        propertyInfo.SetValue(temp, Convert.ChangeType((item.time + x) % 60 + "b", propertyInfo.PropertyType), null);
                    }
                }
            }

            dgTimeline.Items.Refresh();
        }

        private string getComments(int time, int row)
        {
            string temp = "";

            foreach (Comment item in this.comments)
            {
                for (int x = 0; x < item.duration; x++)
                {
                    if (time == item.time + x && (row == -1 ? true : row == item.row))
                    {
                        if (temp == "")
                        {
                            temp += item.comment;
                        }
                        else
                        {
                            temp += "\n===================\n" + item.comment;
                        }

                        break;
                    }
                }
            }

            return temp;
        }

        private void showCommentsTextbox()
        {
            string temp = this.getComments(this.lastUpdate, -1);

            if (temp != this.textBox1.Text)
            {
                this.textBox1.Text = temp;
            }
        }

        private void dgTimeline_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.DirectlyOver is Line)
            {
                Line rec = (Line)Mouse.DirectlyOver;

                if (rec.Name.IndexOf("b") != -1)
                {
                    int time = this.LineToTime(rec, (DependencyObject)e.OriginalSource);

                    string com = this.getComments(time, dgTimeline.Items.IndexOf(((TimeScale)dgTimeline.CurrentCell.Item)));

                    if (com.Trim() != "")
                    {
                        rec.ToolTip = com;
                    }
                }
            }
        }

        private void dgTimeline_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.DirectlyOver is Line)
            {
                Line rec = (Line)Mouse.DirectlyOver;

                if (rec.Name.IndexOf("b") != -1)
                {
                    int time = this.LineToTime(rec, (DependencyObject)e.OriginalSource);

                    for (int x = 0; x < this.comments.Count; x++)
                    {
                        for (int y = 0; y < this.comments[x].duration; y++)
                        {
                            if (time == this.comments[x].time + y && this.comments[x].row == dgTimeline.Items.IndexOf(((TimeScale)dgTimeline.CurrentCell.Item)))
                            {
                                this.comments.RemoveAt(x);

                                x = 0;

                                break;
                            }
                        }
                    }

                    this.showCommentsTimeline();
                }
            }
        }

        #endregion

        Point lastMousePosition;

        private void move_buttonDown(object sender, MouseButtonEventArgs e)
        {
            lastMousePosition = this.PointToScreen(Mouse.GetPosition(this));
        }

        private void move_mouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePoint = this.PointToScreen(Mouse.GetPosition(this));

                Grid tmp = (Grid)this.FindName(((Button)sender).Name.Replace("btn", "grid"));

                tmp.Margin = new Thickness(tmp.Margin.Left + (mousePoint.X - lastMousePosition.X),
                    tmp.Margin.Top + (mousePoint.Y - lastMousePosition.Y), tmp.Margin.Right, tmp.Margin.Bottom);

                lastMousePosition = mousePoint;

                Grid[] temp = new Grid[4];

                temp[0] = gridMediaPlayerMain0;
                temp[1] = gridMediaPlayerMain1;
                temp[2] = gridMediaPlayerMain2;
                temp[3] = gridMediaPlayerMain3;

                for (int x = 0; x < 4; x++)
                {
                    if (temp[x] == tmp)
                    {
                        Canvas.SetZIndex(tmp, 2);
                    }
                    else
                    {
                        Canvas.SetZIndex(temp[x], 1);
                    }
                }
            }
        }

        private void resize_buttonDown(object sender, MouseButtonEventArgs e)
        {
            lastMousePosition = this.PointToScreen(Mouse.GetPosition(this));
        }

        private void resize_mouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePoint = this.PointToScreen(Mouse.GetPosition(this));

                Grid tmp = (Grid)this.FindName(((Button)sender).Name.Replace("btnResize", "grid").Replace("Border", "Main"));

                if (tmp.Height + (mousePoint.Y - lastMousePosition.Y) > 0)
                {
                    tmp.Height = tmp.Height + (mousePoint.Y - lastMousePosition.Y);
                }

                if (tmp.Width + (mousePoint.X - lastMousePosition.X) > 0)
                {
                    tmp.Width = tmp.Width + (mousePoint.X - lastMousePosition.X);
                }

                lastMousePosition = mousePoint;
            }
        }
    }
}

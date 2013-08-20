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
using System.Xml;
using System.Collections;
using System.Threading;
using System.Collections.Concurrent;

// Namespaces for proximity toolkit
using ProximityToolkit;
using ProximityToolkit.Presence;
using ProximityToolkit.Networking;

//storage
using System.IO;

namespace prototype1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Objects windowObject;

        public List<relation> events = new List<relation>();
        private List<String> relationsList = new List<String>();
        private List<RelationPair> relations = new List<RelationPair>();

        //proximity
        private ProximityClientConnection client = null;
        private ProximitySpace space = null;

        //storage stuff
        private DateTime startTime;
        private String output;
        private List<String> entities = new List<String>();

        private static ConcurrentQueue<Object> queue = new ConcurrentQueue<Object>();
        readonly object listLock = new object();

        private bool stopRecording = false;

        private Recording recording = new Recording();

        private List<Object> array = new List<Object>();
        private List<double> times = new List<double>();

        private int count = 0;

        public MainWindow()
        {
            InitializeComponent();
            windowObject = new Objects(this);

            this.client = new ProximityClientConnection();
            this.client.Start("127.0.0.1", 888, true);

            // 3D space captured by the Vicon cameras
            this.space = client.GetSpace();

            //new Thread(new ThreadStart(ConsumerJob)).Start();

            recording.button1.Click += new RoutedEventHandler(stop);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            windowObject.ShowDialog();
            this.updateObjectList();
        }

        public void updateObjectList()
        {
            comboBox1.Items.Clear();
            ItemCollection list = windowObject.listBox1.Items;

            foreach(ListBoxItem item in list)
            {
                int tmp = 0;

                foreach (relation temp in events)
                {
                    if (temp.obj1 == item.Content.ToString())
                    {
                        tmp++;
                    }
                }

                if (tmp < list.Count - 1)
                {
                    comboBox1.Items.Add(item.Content);
                }
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                int tmp;
                comboBox2.Items.Clear();

                foreach (ListBoxItem item in windowObject.listBox1.Items)
                {
                    if (item.Content.ToString() != comboBox1.SelectedItem.ToString())
                    {
                        tmp = 0;

                        foreach (relation temp in events)
                        {
                            if (comboBox1.SelectedItem.ToString() == temp.obj1 && temp.obj2 == item.Content.ToString())
                            {
                                tmp = 1;
                                break;
                            }
                        }

                        if (tmp == 0)
                        {
                            comboBox2.Items.Add(item.Content.ToString());
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1)
            {
                if((bool)checkBox1.IsChecked || (bool)checkBox2.IsChecked || (bool)checkBox3.IsChecked || (bool)checkBox4.IsChecked ||
                    (bool)checkBox5.IsChecked || (bool)checkBox6.IsChecked || (bool)checkBox7.IsChecked)
                {
                    int tmp = 0;

                    foreach (var item in listBox1.Items)
                    {
                        if (item.ToString() == comboBox1.SelectedItem.ToString())
                        {
                            tmp = 1;
                        }
                    }

                    if (tmp == 0)
                    {
                        listBox1.Items.Add(comboBox1.SelectedItem);
                    }

                    events.Add(new relation(comboBox1.SelectedItem.ToString(), comboBox2.SelectedItem.ToString(), (bool)checkBox1.IsChecked,
                        (bool)checkBox2.IsChecked, (bool)checkBox3.IsChecked, (bool)checkBox4.IsChecked, (bool)checkBox5.IsChecked,
                        (bool)checkBox6.IsChecked, (bool)checkBox7.IsChecked));

                    button2_Click(null, null);

                    comboBox2.Items.Clear();

                    updateObjectList();
                }
            }
            else
            {
                if (listBox1.SelectedItem != null && listBox2.SelectedItem != null)
                {
                    foreach (relation item in events)
                    {
                        if (listBox1.SelectedIndex != -1 && listBox2.SelectedIndex != -1)
                        {
                            if (item.obj1 == listBox1.SelectedItem.ToString() && item.obj2 == listBox2.SelectedItem.ToString())
                            {
                                item.collision = (bool)checkBox1.IsChecked;
                                item.pointing = (bool)checkBox2.IsChecked;
                                item.direction = (bool)checkBox3.IsChecked;
                                item.location = (bool)checkBox4.IsChecked;
                                item.motion = (bool)checkBox5.IsChecked;
                                item.orientation = (bool)checkBox6.IsChecked;
                                item.rotation = (bool)checkBox7.IsChecked;

                                button2_Click(null, null);

                                comboBox2.Items.Clear();
                            }
                        }
                    }
                }
            }
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listBox2.Items.Clear();
            checkBox1.IsChecked = false;
            checkBox2.IsChecked = false;
            checkBox3.IsChecked = false;
            checkBox4.IsChecked = false;
            checkBox5.IsChecked = false;
            checkBox6.IsChecked = false;
            checkBox7.IsChecked = false;

            if (listBox1.SelectedIndex != -1)
            {
                foreach (relation item in events)
                {
                    if (item.obj1 == listBox1.SelectedItem.ToString())
                    {
                        listBox2.Items.Add(item.obj2);
                    }
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            checkBox1.IsChecked = false;
            checkBox2.IsChecked = false;
            checkBox3.IsChecked = false;
            checkBox4.IsChecked = false;
            checkBox5.IsChecked = false;
            checkBox6.IsChecked = false;
            checkBox7.IsChecked = false;

            listBox1.SelectedIndex = -1;
        }

        private void listBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && listBox2.SelectedIndex != -1)
            {
                foreach (relation item in events)
                {
                    if (item.obj1 == listBox1.SelectedItem.ToString() && item.obj2 == listBox2.SelectedItem.ToString())
                    {
                        checkBox1.IsChecked = item.collision;
                        checkBox2.IsChecked = item.pointing;
                        checkBox3.IsChecked = item.direction;
                        checkBox4.IsChecked = item.location;
                        checkBox5.IsChecked = item.motion;
                        checkBox6.IsChecked = item.orientation;
                        checkBox7.IsChecked = item.rotation;
                    }
                }
            }
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (listBox1.SelectedIndex != -1 && listBox2.SelectedIndex != -1)
                {
                    foreach (relation item in events)
                    {
                        if (item.obj1 == listBox1.SelectedItem.ToString() && item.obj2 == listBox2.SelectedItem.ToString())
                        {
                            events.Remove(item);
                            break;
                        }
                    }

                    listBox1_SelectionChanged(null, null);

                    if (listBox2.Items.Count == 0)
                    {
                        listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                    }

                    updateObjectList();
                }
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (listBox1.SelectedIndex != -1)
                {
                    foreach (relation item in events)
                    {
                        if (item.obj1 == listBox1.SelectedItem.ToString())
                        {
                            events.Remove(item);
                            break;
                        }
                    }

                    listBox1_SelectionChanged(null, null);

                    if (listBox2.Items.Count == 0)
                    {
                        listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                        updateObjectList();
                    }
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".prt";
            dlg.Filter = "PRT Files (*.prt)|*.prt";

            // Get the selected file name and display in a TextBox 
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    windowObject.listBox1.Items.Clear();
                    events.Clear();
                    updateObjectList();
                    listBox1.Items.Clear();
                    listBox2.Items.Clear();

                    XmlTextReader objXmlTextReader = new XmlTextReader(dlg.FileName);
                    string sName = "";

                    while (objXmlTextReader.Read())
                    {
                        switch (objXmlTextReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                sName = objXmlTextReader.Name;
                                break;

                            case XmlNodeType.Text:
                                switch (sName)
                                {
                                    case "objects":
                                        String[] objects = objXmlTextReader.Value.Split('|');

                                        if (objects.Length > 0)
                                        {
                                            foreach (String temp in objects)
                                            {
                                                String[] content = temp.ToString().Split(',');

                                                ListBoxItem myItem = new ListBoxItem();
                                                myItem.Content = content[0];

                                                tag tag = new tag(content[0], content[1], content[2], Convert.ToInt32(content[3]));

                                                myItem.Tag = tag;

                                                windowObject.listBox1.Items.Add(myItem);
                                            }
                                        }

                                        break;
                                    case "events":
                                        String[] array = objXmlTextReader.Value.Split('|');

                                        if (array.Length > 0)
                                        {
                                            foreach (String temp in array)
                                            {
                                                String[] content = temp.ToString().Split(',');

                                                events.Add(new relation(content[0], content[1], content[2] == "True" ? true : false,
                                                    content[3] == "True" ? true : false, content[4] == "True" ? true : false,
                                                    content[5] == "True" ? true : false, content[6] == "True" ? true : false,
                                                    content[7] == "True" ? true : false, content[8] == "True" ? true : false));


                                                bool add = true;

                                                if (this.listBox1.Items.Count > 0)
                                                {
                                                    foreach (var item in listBox1.Items)
                                                    {
                                                        if (item.ToString() == content[0])
                                                        {
                                                            add = false;
                                                        }
                                                    }
                                                }

                                                if (add)
                                                {
                                                    listBox1.Items.Add(content[0]);
                                                }
                                            }
                                        }

                                        break;
                                }

                                break;
                        }
                    }

                    updateObjectList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".prt";
            dlg.Filter = "PRT Files (*.prt)|*.prt";

            if (dlg.ShowDialog() == true)
            {
                string s = "<xml><objects>";

                foreach (ListBoxItem item in windowObject.listBox1.Items)
                {
                    s += item.Content.ToString() + "," + ((tag)item.Tag).presence + "," + ((tag)item.Tag).type + "," +
                        ((tag)item.Tag).index + "|";
                }

                s = s.Remove(s.Length - 1);
                
                s += "</objects><events>";

                foreach (relation item in events)
                {
                    s += item.obj1 + "," + item.obj2 + "," + item.collision + "," + item.pointing + "," + item.direction + "," + 
                        item.location + "," + item.motion + "," + item.orientation + "," + item.rotation + "|";
                }

                s = s.Remove(s.Length - 1);
                
                s += "</events></xml>";

                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(s);
                xdoc.Save(dlg.FileName);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            events.Clear();
            windowObject.listBox1.Items.Clear();
            updateObjectList();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".ptr";
            dlg.Filter = "Proximity Toolkit Recorded Session Files (*.ptr)|*.ptr";

            if (dlg.ShowDialog() == true)
            {
                output = dlg.FileName;
            }

            foreach(relation item in events)
            {
                if(!entities.Contains(item.obj1))
                {
                    entities.Add(item.obj1);
                }

                if (!entities.Contains(item.obj2))
                {
                    entities.Add(item.obj2);
                }
            }

            PresenceBase[] presences = new PresenceBase[entities.Count];

            // Initialize objects
            for(int x = 0; x < entities.Count; x++)
            {
                foreach (ListBoxItem item in this.windowObject.listBox1.Items)
                {
                    if (((tag)item.Tag).name == entities[x])
                    {
                        if (((tag)item.Tag).type == "Display")
                        {
                            presences[x] = this.space.GetDisplay(entities[x]);
                        }
                        else
                        {
                            presences[x] = this.space.GetPresence(entities[x]);
                        }

                        break;
                    }
                }
            }

            // Check if the objects are available in the Vicon software (with a timeout of 3 seconds)
            for (int x = 0; x < entities.Count; x++)
            {
                presences[x].WaitForEmbodiment(3);
            }

            //initialize relations
            this.startTime = DateTime.Now;

            //store objects info
            this.storeFirstInfo();

            foreach (relation item in events)
            {
                if (!relationsList.Contains(item.obj1+item.obj2))
                {
                    relationsList.Add(item.obj1 + item.obj2);

                    RelationPair temp = new RelationPair(presences[entities.IndexOf(item.obj1)], presences[entities.IndexOf(item.obj2)],
                        RelationMonitor.Location | RelationMonitor.Pointing | RelationMonitor.Motion | RelationMonitor.Direction);

                    /*if(item.collision)
                    {
                        temp.OnCollisionUpdatedAsynch += new CollisionRelationHandler(onCollisionUpdatedAsynch);
                    }*/

                    if (item.pointing)
                    {
                        temp.OnPointingUpdatedAsynch += new PointingRelationHandler(onPointingUpdatedAsynch);
                    }

                    if (item.direction)
                    {
                        temp.OnDirectionUpdatedAsynch += new DirectionRelationHandler(onDirectionUpdatedAsynch);
                    }

                    if (item.location)
                    {
                        temp.OnLocationUpdatedAsynch += new LocationRelationHandler(onLocationUpdatedAsynch);
                    }

                    if (item.motion)
                    {
                        temp.OnMotionUpdatedAsynch += new MotionRelationHandler(onMotionUpdatedAsynch);
                    }

                    /*if (item.orientation)
                    {
                        temp.OnOrientationUpdatedAsynch += new OrientationRelationHandler(onOrientationUpdatedAsynch);
                    }

                    if (item.rotation)
                    {
                        temp.OnRotationUpdatedAsynch += new RotationRelationHandler(onRotationUpdatedAsynch);
                    }*/

                    relations.Add(temp);
                }
            }

            recording.ShowDialog();
        }

        private void stop(object sender, RoutedEventArgs e)
        {
            //lock (listLock)
            //{
                this.recording.label1.Content = "Stopping...";
                this.stopRecording = true;
            //}

            this.stopRecording = true;

            this.relationsList.Clear();

            foreach (RelationPair temp in relations)
            {
                temp.OnPointingUpdatedAsynch -= onPointingUpdatedAsynch;
                temp.OnLocationUpdatedAsynch -= onLocationUpdatedAsynch;
                temp.OnMotionUpdatedAsynch -= onMotionUpdatedAsynch;
                temp.OnDirectionUpdatedAsynch -= onDirectionUpdatedAsynch;
            }

            relations.Clear();

            this.recording.label1.Content = "Storing...";

            this.ConsumerJob();

            //System.Windows.Application.Current.Dispatcher.Invoke(new System.Action(() =>
            //{
                this.recording.Hide();
                this.recording.label1.Content = "Recording...";
           // }));

            this.stopRecording = false;
        }

        string truncate(string name)
        {
            name = name.Substring(11);
            name = name.Split('.')[1];

            if (name.IndexOf(')') != -1)
            {
                name = name.Remove(name.IndexOf(')'));
            }

            return name;
        }

        private void ConsumerJob()
        {
            /*while (true)
            {
                lock (listLock)
                {
                    Object o = null;

                    if (!queue.TryDequeue(out o))
                    {
                        Console.WriteLine("No object to dequeue, waiting...");

                        if (this.stopRecording)
                        {
                            this.relationsList.Clear();

                            foreach (RelationPair temp in relations)
                            {
                                temp.OnPointingUpdatedAsynch -= onPointingUpdatedAsynch;
                                temp.OnLocationUpdatedAsynch -= onLocationUpdatedAsynch;
                                temp.OnMotionUpdatedAsynch -= onMotionUpdatedAsynch;
                                temp.OnDirectionUpdatedAsynch-= onDirectionUpdatedAsynch;
                            }

                            relations.Clear();

                            System.Windows.Application.Current.Dispatcher.Invoke(new System.Action(() => { 
                                this.recording.Hide();
                                this.recording.label1.Content = "Recording...";
                            }));

                            this.stopRecording = false;
                        }

                        // Threads will wait here and only one will be released when .Pulse()d
                        Monitor.Wait(listLock);
                        o = queue.Take(1);
                    }*/

                for(int x = 0; x < this.array.Count; x++){
                    var o = array[x];

                    if (o != null)
                    {
                        using (FileStream stream = new FileStream(this.output, FileMode.Append))
                        {
                            using (BinaryWriter writer = new BinaryWriter(stream))
                            {
                                if (o is PointingRelationEventArgs)
                                {
                                    PointingRelationEventArgs args = (PointingRelationEventArgs)o;

                                    //header
                                    writer.Write(1);
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                                    writer.Write(this.times[x]);
                                    //writer.Write(DateTime.Now.ToString("hh:mm:ss"));

                                    //content
                                    writer.Write(args.DefaultPointer.Distance);
                                    writer.Write(args.DefaultPointer.DisplayPoint.X);
                                    writer.Write(args.DefaultPointer.DisplayPoint.Y);
                                    writer.Write(args.DefaultPointer.Intersection.X);
                                    writer.Write(args.DefaultPointer.Intersection.Y);
                                    writer.Write(args.DefaultPointer.Intersection.Z);
                                    writer.Write(args.DefaultPointer.IsTouching);
                                    writer.Write(args.DefaultPointer.PointsAt);
                                    writer.Write(args.DefaultPointer.PointsToward);
                                }

                                if (o is DirectionRelationEventArgs)    
                                {
                                    DirectionRelationEventArgs args = (DirectionRelationEventArgs)o;

                                    //header
                                    writer.Write(2);
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                                    writer.Write(this.times[x]);
                                    //writer.Write(DateTime.Now.ToString() + '|');

                                    //content
                                    writer.Write(args.PresenceA.Location.X);
                                    writer.Write(args.PresenceA.Location.Y);
                                    writer.Write(args.PresenceA.Location.Z);
                                    writer.Write(args.PresenceB.Location.X);
                                    writer.Write(args.PresenceB.Location.Y);
                                    writer.Write(args.PresenceB.Location.Z);
                                    writer.Write(args.AFromB);
                                    writer.Write(args.BFromA);
                                    writer.Write(args.ATowardsB);
                                    writer.Write(args.BTowardsA);
                                    writer.Write(args.Parallel);
                                    writer.Write(args.Purpendicular);
                                }

                                if (o is LocationRelationEventArgs)
                                {
                                    LocationRelationEventArgs args = (LocationRelationEventArgs)o;

                                    //header
                                    writer.Write(3);
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                                    writer.Write(this.times[x]);
                                    //writer.Write(DateTime.Now.ToString("hh:mm:ss"));

                                    //content
                                    writer.Write(args.PresenceA.Location.X);
                                    writer.Write(args.PresenceA.Location.Y);
                                    writer.Write(args.PresenceA.Location.Z);
                                    writer.Write(args.PresenceB.Location.X);
                                    writer.Write(args.PresenceB.Location.Y);
                                    writer.Write(args.PresenceB.Location.Z);
                                    writer.Write(args.Distance);
                                    writer.Write(args.AFromB);
                                    writer.Write(args.BFromA);
                                    writer.Write(args.ATowardsB);
                                    writer.Write(args.BTowardsA);
                                    writer.Write(args.IsAMoving);
                                    writer.Write(args.IsBMoving);
                                    writer.Write(args.Parallel);
                                    writer.Write(args.Purpendicular);
                                }

                                if (o is MotionRelationEventArgs)
                                {
                                    MotionRelationEventArgs args = (MotionRelationEventArgs)o;

                                    //header
                                    writer.Write(4);
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                                    writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                                    writer.Write(this.times[x]);
                                    //writer.Write(DateTime.Now.ToString("hh:mm:ss"));

                                    //content
                                    writer.Write(args.IsAMoving);
                                    writer.Write(args.IsBMoving);
                                    writer.Write(args.VelocityDifference);
                                    writer.Write(args.XAccelerationAgrees);
                                    writer.Write(args.XVelocityAgrees);
                                    writer.Write(args.YAccelerationAgrees);
                                    writer.Write(args.YVelocAgrees);
                                    writer.Write(args.ZAccelerationAgrees);
                                    writer.Write(args.ZVelocAgrees);
                                }
                            }
                        }
                    }
                }

            /*
            }*/
        }

        void storeFirstInfo()
        {
            File.Delete(this.output);

            using (FileStream stream = new FileStream(this.output, FileMode.Append))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(this.entities.Count());

                    foreach (String item in this.entities)
                    {
                        Console.WriteLine(item);
                        writer.Write(item + "|");
                    }

                    writer.Write(startTime.ToString("G") + "^");
                }
            }
        }

        void onCollisionUpdatedAsynch(CollisionRelationEventArgs args)
        {
            /*if (args.Collides)
            {
                using (FileStream stream = new FileStream(this.output, FileMode.Append))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        //header
                        writer.Write(0);
                        //writer.Write(Array.IndexOf(this.objects, args.PresenceA.Name));
                        //writer.Write(Array.IndexOf(this.objects, args.PresenceB.Name));
                        writer.Write((DateTime.Now - startTime).TotalSeconds);

                        //content
                        writer.Write(args.Distance);
                        writer.Write(args.Intersection.X);
                        writer.Write(args.Intersection.Y);
                        writer.Write(args.Intersection.Z);
                        writer.Write(args.Collides);
                    }
                }
            }*/
        }

        void onPointingUpdatedAsynch(PointingRelationEventArgs args)
        {
            if (!this.stopRecording)
            {
                if (args != null && args.DefaultPointer != null && args.DefaultPointer.DisplayPoint != null && args.DefaultPointer.PointsAt)
                {
                    //lock (listLock)
                    //{
                    //    queue.Enqueue(args);
                    //    Monitor.Pulse(listLock);
                    //}

                   /*using (FileStream stream = new FileStream(this.output, FileMode.Append))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            //header
                            writer.Write(1);
                            writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                            writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                            writer.Write((DateTime.Now - startTime).TotalSeconds);
                            //writer.Write(DateTime.Now.ToString() + '|');

                            //content
                            writer.Write(args.DefaultPointer.Distance);
                            writer.Write(args.DefaultPointer.DisplayPoint.X);
                            writer.Write(args.DefaultPointer.DisplayPoint.Y);
                            writer.Write(args.DefaultPointer.Intersection.X);
                            writer.Write(args.DefaultPointer.Intersection.Y);
                            writer.Write(args.DefaultPointer.Intersection.Z);
                            writer.Write(args.DefaultPointer.IsTouching);
                            writer.Write(args.DefaultPointer.PointsAt);
                            writer.Write(args.DefaultPointer.PointsToward);
                        }
                    }*/

                    this.array.Add(args);
                    this.times.Add((DateTime.Now - startTime).TotalSeconds);
                }
            }
        }

        void onDirectionUpdatedAsynch(DirectionRelationEventArgs args)
        {
            if (!this.stopRecording)
            {
                //lock (listLock)
                //{
                //    queue.Enqueue(args);
                //    Monitor.Pulse(listLock);
                //} 

                /*using (FileStream stream = new FileStream(this.output, FileMode.Append))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        //header
                        writer.Write(2);
                        writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                        writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                        writer.Write((DateTime.Now - startTime).TotalSeconds);
                        //writer.Write(DateTime.Now.ToString() + '|');

                        //content
                        writer.Write(args.PresenceA.Location.X);
                        writer.Write(args.PresenceA.Location.Y);
                        writer.Write(args.PresenceA.Location.Z);
                        writer.Write(args.PresenceB.Location.X);
                        writer.Write(args.PresenceB.Location.Y);
                        writer.Write(args.PresenceB.Location.Z);
                        writer.Write(args.AFromB);
                        writer.Write(args.BFromA);
                        writer.Write(args.ATowardsB);
                        writer.Write(args.BTowardsA);
                        writer.Write(args.Parallel);
                        writer.Write(args.Purpendicular);
                    }
                }*/

                if (this.truncate(((PresenceBase)args.PresenceA).FullName) == "TabletC" && this.truncate(((PresenceBase)args.PresenceB).FullName) == "WhiteHat" && (args.ATowardsB || args.BTowardsA))
                {
                    Console.WriteLine(this.count + " " + args.ATowardsB + " " + args.BTowardsA);

                    this.count++;
                }

                this.array.Add(args);
                this.times.Add((DateTime.Now - startTime).TotalSeconds);

                /*if (args.ATowardsB)
                {
                    Console.WriteLine(this.truncate(((PresenceBase)args.PresenceA).FullName) + " " + this.truncate(((PresenceBase)args.PresenceB).FullName) + " " + args.ATowardsB + " " + args.BTowardsA);
                }*/

                /*if (args.ATowardsB && (this.truncate(((PresenceBase)args.PresenceA).FullName) == "BlueHat" && this.truncate(((PresenceBase)args.PresenceB).FullName) == "TabletC"))
                {
                    Console.WriteLine(this.truncate(((PresenceBase)args.PresenceA).FullName) + " " + this.truncate(((PresenceBase)args.PresenceB).FullName) + " " + args.ATowardsB + " " + args.BTowardsA);
                }

                if (args.BTowardsA && (this.truncate(((PresenceBase)args.PresenceB).FullName) == "BlueHat" && this.truncate(((PresenceBase)args.PresenceA).FullName) == "TabletC"))
                {
                    Console.WriteLine(this.truncate(((PresenceBase)args.PresenceA).FullName) + " " + this.truncate(((PresenceBase)args.PresenceB).FullName) + " " + args.ATowardsB + " " + args.BTowardsA);
                }*/

                //Console.WriteLine("direction " + this.truncate(((PresenceBase)args.PresenceA).FullName) + " " + this.truncate(((PresenceBase)args.PresenceB).FullName) + " " + args.ATowardsB + " " + args.BTowardsA);
            }
        }

        void onLocationUpdatedAsynch(LocationRelationEventArgs args)
        {
            if (!this.stopRecording)
            {
                /*using (FileStream stream = new FileStream(this.output, FileMode.Append))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        //header
                        writer.Write(3);
                        writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                        writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                        writer.Write((DateTime.Now - startTime).TotalSeconds);
                        //writer.Write(DateTime.Now.ToString() + '|');

                        //content
                        writer.Write(args.PresenceA.Location.X);
                        writer.Write(args.PresenceA.Location.Y);
                        writer.Write(args.PresenceA.Location.Z);
                        writer.Write(args.PresenceB.Location.X);
                        writer.Write(args.PresenceB.Location.Y);
                        writer.Write(args.PresenceB.Location.Z);
                        writer.Write(args.Distance);
                        writer.Write(args.AFromB);
                        writer.Write(args.BFromA);
                        writer.Write(args.ATowardsB);
                        writer.Write(args.BTowardsA);
                        writer.Write(args.IsAMoving);
                        writer.Write(args.IsBMoving);
                        writer.Write(args.Parallel);
                        writer.Write(args.Purpendicular);
                    }
                }*/

                this.array.Add(args);
                this.times.Add((DateTime.Now - startTime).TotalSeconds);

                //lock (listLock)
                //{
                //    queue.Enqueue(args);
                //    Monitor.Pulse(listLock);
                //}
            }
        }

        void onMotionUpdatedAsynch(MotionRelationEventArgs args)
        {
            if (!this.stopRecording)
            {
                /*using (FileStream stream = new FileStream(this.output, FileMode.Append))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        //header
                        writer.Write(4);
                        writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceA).FullName)));
                        writer.Write(this.entities.IndexOf(this.truncate(((PresenceBase)args.PresenceB).FullName)));
                        writer.Write((DateTime.Now - startTime).TotalSeconds);
                        //writer.Write(DateTime.Now.ToString());

                        //content
                        writer.Write(args.IsAMoving);
                        writer.Write(args.IsBMoving);
                        writer.Write(args.VelocityDifference);
                        writer.Write(args.XAccelerationAgrees);
                        writer.Write(args.XVelocityAgrees);
                        writer.Write(args.YAccelerationAgrees);
                        writer.Write(args.YVelocAgrees);
                        writer.Write(args.ZAccelerationAgrees);
                        writer.Write(args.ZVelocAgrees);
                    }
                }*/

                this.array.Add(args);
                this.times.Add((DateTime.Now - startTime).TotalSeconds);

                //ock (listLock)
                //{
                //    queue.Enqueue(args);
                //    Monitor.Pulse(listLock);
                //}
            }
        }

        void onOrientationUpdatedAsynch(OrientationRelationEventArgs args)
        {
            //useless
        }

        void onRotationUpdatedAsynch(RotationRelationEventArgs args)
        {
            //usless
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }

    public class relation
    {
        public String obj1;
        public String obj2;
        public bool collision;
        public bool pointing;
        public bool direction;
        public bool location;
        public bool motion;
        public bool orientation;
        public bool rotation;

        public relation(String obj1, String obj2, bool collision, bool pointing, bool direction, bool location, bool motion, 
            bool orientation, bool rotation)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
            this.collision = collision;
            this.pointing = pointing;
            this.direction = direction;
            this.location = location;
            this.motion = motion;
            this.orientation = orientation;
            this.rotation = rotation;
        }
    }
}

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


namespace TimelineSample
{
    public partial class TimelineControl : UserControl
    {
        public static DependencyProperty MinutesProperty =
            DependencyProperty.Register("Minutes", typeof(string), typeof(TimelineControl));

        public static DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(int), typeof(TimelineControl));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public string Minutes
        {
            get { return (string)GetValue(MinutesProperty); }
            set { SetValue(IdProperty, value); }
        }

        private string[] colors = new string[12] { "#B8951D", "#778635", "#568D3C", "#1B8442", "#127C65", "#13697A", "#37588D", 
            "#324388", "#3A3E7E", "#552F86", "#652B78", "#82267B" };

        public TimelineControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(TimelineControl_Loaded);
        }

        void setVisibility(String time)
        {
            BrushConverter bc = new BrushConverter();  

            switch (time.Trim())
            {
                case "0": Minute00.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "1": Minute01.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "2": Minute02.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "3": Minute03.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "4": Minute04.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "5": Minute05.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "6": Minute06.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "7": Minute07.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "8": Minute08.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "9": Minute09.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "10": Minute10.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "11": Minute11.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "12": Minute12.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "13": Minute13.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "14": Minute14.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "15": Minute15.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "16": Minute16.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "17": Minute17.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "18": Minute18.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "19": Minute19.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "20": Minute20.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "21": Minute21.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "22": Minute22.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "23": Minute23.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "24": Minute24.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "25": Minute25.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "26": Minute26.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "27": Minute27.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "28": Minute28.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "29": Minute29.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "30": Minute30.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "31": Minute31.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "32": Minute32.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "33": Minute33.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "34": Minute34.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "35": Minute35.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "36": Minute36.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "37": Minute37.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "38": Minute38.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "39": Minute39.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "40": Minute40.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "41": Minute41.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "42": Minute42.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "43": Minute43.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "44": Minute44.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "45": Minute45.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "46": Minute46.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "47": Minute47.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "48": Minute48.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "49": Minute49.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "50": Minute50.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "51": Minute51.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "52": Minute52.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "53": Minute53.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "54": Minute54.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "55": Minute55.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "56": Minute56.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "57": Minute57.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "58": Minute58.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;
                case "59": Minute59.Stroke = (Brush)bc.ConvertFrom(this.colors[this.Id % this.colors.Count()]);
                    break;

                case "0b": Minute00b.Visibility = Visibility.Visible;
                    break;
                case "1b": Minute01b.Visibility = Visibility.Visible;
                    break;
                case "2b": Minute02b.Visibility = Visibility.Visible;
                    break;
                case "3b": Minute03b.Visibility = Visibility.Visible;
                    break;
                case "4b": Minute04b.Visibility = Visibility.Visible;
                    break;
                case "5b": Minute05b.Visibility = Visibility.Visible;
                    break;
                case "6b": Minute06b.Visibility = Visibility.Visible;
                    break;
                case "7b": Minute07b.Visibility = Visibility.Visible;
                    break;
                case "8b": Minute08b.Visibility = Visibility.Visible;
                    break;
                case "9b": Minute09b.Visibility = Visibility.Visible;
                    break;
                case "10b": Minute10b.Visibility = Visibility.Visible;
                    break;
                case "11b": Minute11b.Visibility = Visibility.Visible;
                    break;
                case "12b": Minute12b.Visibility = Visibility.Visible;
                    break;
                case "13b": Minute13b.Visibility = Visibility.Visible;
                    break;
                case "14b": Minute14b.Visibility = Visibility.Visible;
                    break;
                case "15b": Minute15b.Visibility = Visibility.Visible;
                    break;
                case "16b": Minute16b.Visibility = Visibility.Visible;
                    break;
                case "17b": Minute17b.Visibility = Visibility.Visible;
                    break;
                case "18b": Minute18b.Visibility = Visibility.Visible;
                    break;
                case "19b": Minute19b.Visibility = Visibility.Visible;
                    break;
                case "20b": Minute20b.Visibility = Visibility.Visible;
                    break;
                case "21b": Minute21b.Visibility = Visibility.Visible;
                    break;
                case "22b": Minute22b.Visibility = Visibility.Visible;
                    break;
                case "23b": Minute23b.Visibility = Visibility.Visible;
                    break;
                case "24b": Minute24b.Visibility = Visibility.Visible;
                    break;
                case "25b": Minute25b.Visibility = Visibility.Visible;
                    break;
                case "26b": Minute26b.Visibility = Visibility.Visible;
                    break;
                case "27b": Minute27b.Visibility = Visibility.Visible;
                    break;
                case "28b": Minute28b.Visibility = Visibility.Visible;
                    break;
                case "29b": Minute29b.Visibility = Visibility.Visible;
                    break;
                case "30b": Minute30b.Visibility = Visibility.Visible;
                    break;
                case "31b": Minute31b.Visibility = Visibility.Visible;
                    break;
                case "32b": Minute32b.Visibility = Visibility.Visible;
                    break;
                case "33b": Minute33b.Visibility = Visibility.Visible;
                    break;
                case "34b": Minute34b.Visibility = Visibility.Visible;
                    break;
                case "35b": Minute35b.Visibility = Visibility.Visible;
                    break;
                case "36b": Minute36b.Visibility = Visibility.Visible;
                    break;
                case "37b": Minute37b.Visibility = Visibility.Visible;
                    break;
                case "38b": Minute38b.Visibility = Visibility.Visible;
                    break;
                case "39b": Minute39b.Visibility = Visibility.Visible;
                    break;
                case "40b": Minute40b.Visibility = Visibility.Visible;
                    break;
                case "41b": Minute41b.Visibility = Visibility.Visible;
                    break;
                case "42b": Minute42b.Visibility = Visibility.Visible;
                    break;
                case "43b": Minute43b.Visibility = Visibility.Visible;
                    break;
                case "44b": Minute44b.Visibility = Visibility.Visible;
                    break;
                case "45b": Minute45b.Visibility = Visibility.Visible;
                    break;
                case "46b": Minute46b.Visibility = Visibility.Visible;
                    break;
                case "47b": Minute47b.Visibility = Visibility.Visible;
                    break;
                case "48b": Minute48b.Visibility = Visibility.Visible;
                    break;
                case "49b": Minute49b.Visibility = Visibility.Visible;
                    break;
                case "50b": Minute50b.Visibility = Visibility.Visible;
                    break;
                case "51b": Minute51b.Visibility = Visibility.Visible;
                    break;
                case "52b": Minute52b.Visibility = Visibility.Visible;
                    break;
                case "53b": Minute53b.Visibility = Visibility.Visible;
                    break;
                case "54b": Minute54b.Visibility = Visibility.Visible;
                    break;
                case "55b": Minute55b.Visibility = Visibility.Visible;
                    break;
                case "56b": Minute56b.Visibility = Visibility.Visible;
                    break;
                case "57b": Minute57b.Visibility = Visibility.Visible;
                    break;
                case "58b": Minute58b.Visibility = Visibility.Visible;
                    break;
                case "59b": Minute59b.Visibility = Visibility.Visible;
                    break;

                default:
                    break;
            }
        }

        public void TimelineControl_Loaded(object sender, RoutedEventArgs e)
        {
            string minuteList = Minutes;

            if (!string.IsNullOrEmpty(minuteList))
            {
                string[] timeWords = minuteList.Split(',');

                foreach (string item in timeWords)
                {
                    setVisibility(item);
                }
            }
        }
    }
}

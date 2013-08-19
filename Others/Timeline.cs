using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace TimelineSample.Others
{
    class Timeline
    {
        public string name { get; set; }

        private int position;
        private Canvas canvas;
        private List<Line> lines = new List<Line>();

        public Timeline(int position, int totalSeconds, string name, Canvas canvas)
        {
            this.position = position;
            this.name = name;
            this.canvas = canvas;

            for (int x = 0; x < totalSeconds; x++)
            {
                Line temp = new Line();

                //event, time
                temp.X1 = x * 2;
                temp.X2 = x * 2;
                temp.Y1 = position * 24;
                temp.Y1 = (position + 1) * 24;
                temp.StrokeThickness = 2.0;
                temp.Visibility = Visibility.Hidden;
                temp.Stroke = Brushes.White;
                temp.Name = x.ToString("D3a");

                this.canvas.Children.Add(temp);
            }
        }
    }
}

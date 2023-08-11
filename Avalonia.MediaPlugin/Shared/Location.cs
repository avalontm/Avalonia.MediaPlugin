using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalonia.MediaPlugin.Abstractions
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double HorizontalAccuracy { get; set; }
        public double Speed { get; set; }
        public double Direction { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

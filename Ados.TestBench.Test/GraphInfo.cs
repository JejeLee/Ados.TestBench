using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace Ados.TestBench.Test
{
    public class GraphInfo : INotifyPropertyChanged
    {
        static public string Path { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string VerticalTitle
        {
            get;
            set;
        }
        public int Max { get { return _max; }
            set
            {
                if (_max != value)
                {
                    _max = value;
                    OnPropertyChanged("Max");
                }
            }
        }
        public int Min
        {
            get { return _min; }
            set
            {
                if (_min != value)
                {
                    _min = value;
                    OnPropertyChanged("Min");
                }
            }
        }
        public int Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged("Height");
                }
            }
        }
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged("Visible");
                    OnPropertyChanged("Vity");
                }
            }
        }

        public System.Windows.Visibility Vity
        {
            get
            {
                return this.Visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public string Description1 { get; private set; }

        public string Description2 { get; private set; }

        public static double TimeUnit(DateTime aTime)
        {
            return (aTime.Ticks / 10000) / 1000.0; // seconds to one unit.
        }

        private double Mapping(StateShot aShot)
        {
            var x = TimeUnit(aShot.Time) - StateShot.TimeBase;
            return x;
        }

        public void SetDataSource(System.Collections.ObjectModel.ObservableCollection<StateShot> aSource)
        {
            _ds = new EnumerableDataSource<StateShot>(aSource);
            _ds.SetXMapping(Mapping);
            _ds2 = new EnumerableDataSource<StateShot>(aSource);
            _ds2.SetXMapping(Mapping);

            switch (this.Name)
            {
                case "a1":
                    _ds.SetYMapping(y => y.SpeedM);
                    _ds2.SetYMapping(yy => yy.SpeedR);
                    break;
                case "a2":
                    _ds.SetYMapping(y => y.DoorAngle);
                    break;
                case "a3":
                    _ds.SetYMapping(y => y.MotorV);
                    _ds2.SetYMapping(yy => yy.MotorA);
                    break;
                case "a4":
                    _ds.SetYMapping(y => y.DistanceF);
                    _ds2.SetYMapping(yy => yy.DistanceR);
                    break;
                case "d1":
                    _ds.SetYMapping(y => y.DoorRun > 0 ? this.Min : this.Max);
                    break;
                case "d2":
                    _ds.SetYMapping(y => y.DirectionOpen > 0 ? this.Min : this.Max);
                    break;
                case "d3":
                    _ds.SetYMapping(y => y.DirectionClose > 0 ? this.Min : this.Max);
                    break;
                case "d4":
                    _ds.SetYMapping(y => y.LatchOn > 0 ? this.Min : this.Max);
                    break;
                case "d5":
                    _ds.SetYMapping(y => y.ReleaseOn > 0 ? this.Min : this.Max);
                    break;
                case "d6":
                    _ds.SetYMapping(y => y.Clutch > 0 ? this.Min : this.Max);
                    break;
                case "d7":
                    _ds.SetYMapping(y => y.Test > 0 ? this.Min : this.Max);
                    break;
            }

        }

        public static double TimeRange { get; set; }

        public static List<GraphInfo> Load(string aPath)
        {
            var str = File.ReadAllText(aPath);
            var root = Newtonsoft.Json.Linq.JObject.Parse(str);

            var gs = new List<GraphInfo>();
            GraphInfo.Path = aPath;

            GraphInfo.TimeRange = root["Graphs"]["Common"].Value<double>("TimeRange");

            foreach (var ja in root["Graphs"]["Details"])
            {
                var p = new GraphInfo()
                {
                    Name = ja["ID"].ToString(),
                    Title = ja["Title"].ToString(),
                    VerticalTitle = ja.Value<string>("VerticalTitle"),
                    Min = ja.Value<int>("Min"),
                    Max = ja.Value<int>("Max"),
                    Height = ja.Value<int>("Height"),
                    Visible = ja.Value<bool>("Visible"),
                    Description1 = ja["Description1"].ToString(),
                    Description2 = ja["Description2"].ToString(),
                };
                gs.Add(p);
            }

            Log.i("Graph 설정 정보를 로드했습니다.");

            return gs;
        }

        public static void Save(List<GraphInfo> aInfos, bool aUpdateTime = false)
        {
            var contents = File.ReadAllText(GraphInfo.Path);
            var root = Newtonsoft.Json.Linq.JObject.Parse(contents);

            if (aUpdateTime)
                root["Header"]["UpdateDate"] = DateTime.Now.ToString("s");

            var gs = root["Graphs"]["Details"];

            int i = 0;
            foreach(var info in aInfos)
            {
                var g = gs[i++];
                g["ID"] = info.Name;
                g["Title"] = info.Title;
                g["VeticalTitle"] = info.VerticalTitle;
                g["Min"] = info.Min;
                g["Max"] = info.Max;
                g["Height"] = info.Height;
                g["Visible"] = info.Visible;
            }

            File.WriteAllText(GraphInfo.Path, root.ToString());

            Log.i("Graph 설정 정보를 저장했습니다.");

        }

        public EnumerableDataSource<StateShot> DataSource { get { return _ds; } }
        public EnumerableDataSource<StateShot> DataSource2 { get { return _ds2; } }

        private int _max;
        private int _min;
        private int _height;
        private bool _visible;
        private EnumerableDataSource<StateShot> _ds;
        private EnumerableDataSource<StateShot> _ds2;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

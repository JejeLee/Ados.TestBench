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
        public string Name { get; set; }
        public string Title { get; set; }
        public string VeticalTitle { get; set; }
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
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged("Visibility");
                }
            }
        }

        public string Description1 { get; private set; }

        public string Description2 { get; private set; }

        public void SetDataSource(System.Collections.ObjectModel.ObservableCollection<StateShot> aSource)
        {
            _ds = new EnumerableDataSource<StateShot>(aSource);
            _ds.SetXMapping(x => x.Time.Ticks / 100000);

            switch(this.Name)
            {
                case "a1":
                    _ds.SetYMapping(y => y.SpeedM);
                    _ds2 = new EnumerableDataSource<StateShot>(aSource);
                    _ds2.SetXMapping(x => x.Time.Ticks / 100000);
                    _ds.SetYMapping(y => y.SpeedR);
                    break;
                case "a2":
                    _ds.SetYMapping(y => y.DoorAngle);
                    break;
                case "a3":
                    _ds.SetYMapping(y => y.MotorV);
                    _ds2 = new EnumerableDataSource<StateShot>(aSource);
                    _ds2.SetXMapping(x => x.Time.Ticks / 100000);
                    _ds.SetYMapping(y => y.MotorA);
                    break;
                case "a4":
                    _ds.SetYMapping(y => y.DistanceF);
                    _ds2 = new EnumerableDataSource<StateShot>(aSource);
                    _ds2.SetXMapping(x => x.Time.Ticks / 100000);
                    _ds.SetYMapping(y => y.DistanceR);
                    break;
                case "d1":
                    _ds.SetYMapping(y => y.DoorRun ? 1 : 0);
                    break;
                case "d2":
                    _ds.SetYMapping(y => y.DirectionOpen ? 1 : 0);
                    break;
                case "d3":
                    _ds.SetYMapping(y => y.DirectionClose ? 1 : 0);
                    break;
                case "d4":
                    _ds.SetYMapping(y => y.LatchOn ? 1 : 0);
                    break;
                case "d5":
                    _ds.SetYMapping(y => y.ReleaseOn ? 1 : 0);
                    break;
                case "d6":
                    _ds.SetYMapping(y => y.Clutch ? 1 : 0);
                    break;
                case "d7":
                    _ds.SetYMapping(y => y.Test ? 1 : 0);
                    break;
            }

           
        }

        public System.Windows.Visibility Visibility { get {
                return Visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            } }

        public static double TimeRange { get; set; }

        public static Dictionary<string, GraphInfo> Load(string aPath)
        {
            var str = File.ReadAllText(aPath);
            var root = Newtonsoft.Json.Linq.JObject.Parse(str);

            var gs = new Dictionary<string, GraphInfo>();

            GraphInfo.TimeRange = root["Graphs"]["Common"].Value<double>("TimeRange");

            foreach (var ja in root["Graphs"]["Details"])
            {
                var p = new GraphInfo()
                {
                    Name = ja["Name"].ToString(),
                    Title = ja["Title"].ToString(),
                    VeticalTitle = ja.Value<string>("VerticalTitle"),
                    Min = ja.Value<int>("Min"),
                    Max = ja.Value<int>("Max"),
                    Visible = ja.Value<bool>("Visible"),
                    Description1 = ja["Description1"].ToString(),
                    Description2 = ja["Description2"].ToString(),
                };
                gs[p.Name] = p;
            }

            Log.i("Graph 설정 정보를 로드했습니다.");

            return gs;
        }

        public static void Save(string aPath, Dictionary<string, GraphInfo> aInfos)
        {
            var contents = File.ReadAllText(aPath);
            var root = Newtonsoft.Json.Linq.JObject.Parse(contents);

            root["Header"]["UpdateDate"] = DateTime.Now.ToString("s");

            var gs = root["Graphs"];

            int i = 0;
            foreach(var info in aInfos.Values)
            {
                var g = gs[i++];
                g["Name"] = info.Name;
                g["Title"] = info.Title;
                g["VeticalTitle"] = info.VeticalTitle;
                g["Min"] = info.Min;
                g["Max"] = info.Max;
                g["Visible"] = info.Visible;
            }

            File.WriteAllText(aPath, root.ToString());

            Log.i("Graph 설정 정보를 저장했습니다.");

        }

        public EnumerableDataSource<StateShot> DataSource { get { return _ds; } }
        public EnumerableDataSource<StateShot> DataSource2 { get { return _ds2; } }

        private int _max;
        private int _min;
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

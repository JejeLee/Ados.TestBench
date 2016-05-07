using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
                    OnPropertyChanged("Visible");
                }
            }
        }

        public static Dictionary<string, GraphInfo> Load(string aPath)
        {
            var str = File.ReadAllText(aPath);
            var root = Newtonsoft.Json.Linq.JObject.Parse(str);

            var gs = new Dictionary<string, GraphInfo>();

            foreach (var ja in root["Graphs"])
            {
                var p = new GraphInfo()
                {
                    Name = ja["Name"].ToString(),
                    Title = ja["Title"].ToString(),
                    VeticalTitle = ja.Value<string>("VeticalTitle"),
                    Min = ja.Value<int>("Min"),
                    Max = ja.Value<int>("Max"),
                    Visible = ja.Value<bool>("Visible"),
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


        private int _max;
        private int _min;
        private bool _visible;

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

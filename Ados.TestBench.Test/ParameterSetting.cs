using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace Ados.TestBench.Test
{
    public class ParameterSetting : ICloneable, INotifyPropertyChanged
    {
        public ParameterSetting()
        {
        }
        public ParameterSetting(string aName)
        {
            this.Info = ParameterInfo.Parameters.FirstOrDefault(x => x.Name == aName);
        }

        public ParameterInfo Info { get; private set; }
        public bool Use { get; set; }
        public int ReadValue
        {
            get { return _readValue; }
            set {
                if (_readValue != value)
                {
                    _readValue = value;
                    OnPropertyChanged("ReadValue");
                }
            }
        }
        
        public int WriteValue { get; set; }

        int _readValue = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public object Clone()
        {
            var p = new ParameterSetting()
            {
                Info = this.Info,
                Use = this.Use,
                ReadValue = this.ReadValue,
                WriteValue = this.WriteValue,
            };
            return p;
        }
    }

    public class ParameterSettings : ICloneable
    {
        private ParameterSettings()
        { }

        public string Path { get; set; }
        public DateTime CreateDate{ get; private set; }
        public DateTime UpdateDate { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }

        public IEnumerable<ParameterSetting> Settings { get { return _settings; } }

        private List<ParameterSetting> _settings = new List<ParameterSetting>();

        public static ParameterSettings Load(string aPath)
        {
            var str = File.ReadAllText(aPath);
            var root = Newtonsoft.Json.Linq.JObject.Parse(str);

            var header = root["Header"];

            var ps = new ParameterSettings();
            ps.Path = aPath;
            ps.CreateDate = DateTime.Parse(header["CreateDate"].ToString());
            ps.UpdateDate = DateTime.Parse(header["UpdateDate"].ToString());
            ps.Version = header["Version"].ToString();
            ps.Description = header["Description"].ToString();
            ps.Name = header["Name"].ToString();

            foreach (var ja in root["ParametersSetting"])
            {
                var p = new ParameterSetting(ja["Name"].ToString())
                {
                    Use = ja.Value<bool>("Use"),
                    ReadValue = ja.Value<int>("ReadValue"),
                    WriteValue = ja.Value<int>("WriteValue"),
                };
                ps._settings.Add(p);
            }

            Log.i("Parameter 설정을 {0}에서 로드했습니다.", aPath);

            return ps;
        }

        public void Save()
        {
            var root = new Newtonsoft.Json.Linq.JObject();
            var header = new Newtonsoft.Json.Linq.JObject(); 
            header["CreateDate"] = this.CreateDate;
            header["UpdateDate"] = this.UpdateDate;
            header["Version"] = this.Version;
            header["Description"] = this.Description;
            header["DataType"] = "ParametersSetting";
            header["Name"] = this.Name;
            root["Header"] = header;

            var ar = new Newtonsoft.Json.Linq.JArray();
            foreach(var p in _settings)
            {
                var s = new Newtonsoft.Json.Linq.JObject();
                s["Name"] = p.Info.Name;
                s["Use"] = p.Use;
                s["ReadValue"] = p.ReadValue;
                s["WriteValue"] = p.WriteValue;

                ar.Add(s);
            }

            root["ParametersSetting"] = ar;

            File.WriteAllText(Path, root.ToString());

            Log.i("Parameter 설정을 {0}에 저장했습니다.", Path);
        }

        public object Clone()
        {
            var ps = new ParameterSettings()
            {
                CreateDate = this.CreateDate,
                UpdateDate = this.UpdateDate,
                Version = this.Version,
                Description = this.Description,
            };

            foreach(var s in this.Settings)
            {
                var ns = (ParameterSetting)s.Clone();
                ps._settings.Add(ns);
            }

            return ps;
        }
    }
}

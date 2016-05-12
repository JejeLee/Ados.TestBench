using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ados.TestBench.Test
{
    public class TestSheet
    {
        public TestSheet()
        {
            Success = false;
        }

        public string Path { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Success { get; set; }
        public string ResultPath {
            get {
                var fi = new FileInfo(this.Path);
                return string.Format("{0}\\{1}\\{2}", fi.DirectoryName, "Results", Start.ToString("yyyyMMdd_HHmmss"));
            }
        }

        public DateTime CreateDate { get; private set; }
        public DateTime UpdateDate { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public string Name { get; private set; }
        public bool Use { get; set; }

        public List<TestSequence> Sequences { get { return _seqs; } }
        public List<ParameterSettings> Parameters { get { return _params; } }

        private List<TestSequence> _seqs = new List<TestSequence>();
        private List<ParameterSettings> _params = new List<ParameterSettings>();

        public ParameterSettings GetParameter(string aName)
        {
            var ps = _params.FirstOrDefault(x => x.Name == aName);
            return ps;
        }
        /*
           Dir: appDir/TestSheets/{SheetName}/TestSheet.json
                                              Parameters/
                                              Results/
                                                       {yyyyMMdd_HHmmss}/Result.json
                                                                         result.cvs
                                                                         result.log
         */

        public static TestSheet Load(string aPath)
        {
            var str = File.ReadAllText(aPath);
            var root = Newtonsoft.Json.Linq.JObject.Parse(str);

            var header = root["Header"];

            var ps = new TestSheet();
            ps.Path = aPath;
            ps.CreateDate = DateTime.Parse(header["CreateDate"].ToString());
            ps.UpdateDate = DateTime.Parse(header["UpdateDate"].ToString());
            ps.Version = header["Version"].ToString();
            ps.Description = header["Description"].ToString();
            ps.Name = header["Name"].ToString();

            ps.LoadParameters();

            foreach (var seq in root["TestSheet"])
            {
                var s = new TestSequence()
                {
                    Index = seq.Value<int>("Index"),
                    Duration = seq.Value<int>("Duration"),
                    Repeat = seq.Value<int>("Repeat"),
                };
                s.Parameter = ps.GetParameter(seq.Value<string>("Parameter"));
                ps._seqs.Add(s);
            }

            Log.i("테스트 시트:{0} 설정을 {1}에서 로드했습니다.", ps.Name, aPath);

            return ps;
        }

        public void LoadParameters()
        {
            var fi = new FileInfo(this.Path);
            var dir = System.IO.Path.Combine(fi.DirectoryName, "Parameters");

            var di = new DirectoryInfo(dir);
            foreach(var pf in di.GetFiles("*.json"))
            {
                try
                {
                    var ps = ParameterSettings.Load(pf.FullName);
                    _params.Add(ps);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
        }

        public void Save()
        {
            var root = new Newtonsoft.Json.Linq.JObject();
            var header = new Newtonsoft.Json.Linq.JObject();
            header["CreateDate"] = this.CreateDate;
            header["UpdateDate"] = this.UpdateDate;
            header["Version"] = this.Version;
            header["Description"] = this.Description;
            header["DataType"] = "TestSheet";
            header["Name"] = this.Name;
            root["Header"] = header;

            var ar = new Newtonsoft.Json.Linq.JArray();
            foreach (var p in _seqs)
            {
                var s = new Newtonsoft.Json.Linq.JObject();
                s["Index"] = p.Index;
                s["Parameter"] = p.ParameterName;
                s["Duration"] = p.Duration;
                s["Repeat"] = p.Repeat;
                ar.Add(s);
            }

            root["TestSheet"] = ar;

            File.WriteAllText(this.Path, root.ToString());

            Log.i("테스트 시트:{0} 설정을 {1}에 저장했습니다.", this.Name, Path);
        }

        public static List<TestSheet> LoadAll()
        {
            var list = new List<TestSheet>();
            var di = new DirectoryInfo(Helper.SheetsDir);
            var dirs = di.GetDirectories("*", SearchOption.TopDirectoryOnly);

            foreach(var d in dirs)
            {
                string path = d.FullName + @"\TestSheet.json";
                try
                {
                    var sheet = TestSheet.Load(path);
                    list.Add(sheet);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
            return list;
        }
    }

    public class TestSequence
    {
        public TestSequence()
        {
            this.Current = 0;
        }

        public int Index { get; set; }
        public int Duration { get; set; }
        public int Repeat { get; set; }
        public ParameterSettings Parameter { get; set; }
        public string ParameterName { get { return this.Parameter.Name; } }

        public int Current { get; set; }
    }
   
}

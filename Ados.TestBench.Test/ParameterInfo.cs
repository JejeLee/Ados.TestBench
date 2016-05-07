using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ados.TestBench.Test
{
    public class ParameterInfo
    {
        public ParameterInfo()
        {
        }
        public string Name { get; private set; }
        public byte Address { get; private set; }
        public string Description { get; private set; }
        public int Min { get; private set; }
        public int  Max { get; private set; }

        public static IEnumerable<ParameterInfo> Load(string aPath)
        {
            var str = File.ReadAllText(aPath);
            var root = Newtonsoft.Json.Linq.JObject.Parse(str);

            _params.Clear();

            foreach (var ja in root["Parameters"])
            {
                var p = new ParameterInfo()
                {
                    Name = ja.Value<string>("Name"),
                    Address = (byte)ja["Address"].ToString().Int(),
                    Description = ja.Value<string>("Description"),
                    Min = ja.Value<int>("Min"),
                    Max = ja.Value<int>("Max"),
                };
                _params.Add(p);
            }

            Log.i("Parameter 정보를 로드했습니다.");

            return _params;
        }

        public static IEnumerable<ParameterInfo> Parameters { get { return _params; } }

        private static List<ParameterInfo> _params = new List<ParameterInfo>();
    }

}

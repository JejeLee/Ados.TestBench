using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ados.TestBench.Test
{
    public class StateShot
    {
        public StateShot()
        {
            Time = DateTime.Now;
            for (int i = 0; i < _states.Length; i++)
                _states.SetValue(0, i);
        }

        public void SetBits(byte aValue)
        {
            DoorError = (aValue & 0x01) > 0 ? 1 :0;
            Test = (aValue & 0x02) > 0 ? 1 : 0;
            DoorRun = (aValue & 0x04) > 0 ? 1 : 0;
            DirectionOpen = (aValue & 0x08) > 0 ? 1 : 0;
            DirectionClose = DirectionOpen == 0 ? 1 :0;
            LatchOn = (aValue & 0x10) > 0 ? 1 : 0;
            ReleaseOn = (aValue & 0x20) > 0 ? 1 : 0;
            Clutch = (aValue & 0x40) > 0 ? 1 : 0;
        }

        public void SetState1(byte[] aValue)
        {
            SetBits(aValue[1]);

            this.SpeedM = (aValue[2] << 8) & aValue[3];
            this.SpeedR = (aValue[4] << 8) & aValue[5];
            this.DoorAngle = (aValue[6] << 8) & aValue[7];

            aValue.CopyTo(_states, 0);
        }

        public void SetState2(byte[] aValue)
        {
            this.MotorV = (aValue[0] << 8) & aValue[1];
            this.MotorA = (aValue[2] << 8) & aValue[3];
            this.DistanceF = aValue[4];
            this.DistanceR = (aValue[5] << 8) & aValue[6];

            aValue.CopyTo(_states, 8);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Rx Bytes <<");
            foreach (var s in _states)
                sb.AppendFormat(" {0:X2}", s);

            return sb.ToString();
        }

        public DateTime Time { get; private set; }
        public int DoorError { get; set; }
        public int DoorRun { get; set; }
        public int DirectionOpen { get; set; }
        public int DirectionClose { get; set; }
        public int LatchOn { get; set; }
        public int ReleaseOn { get; set; }
        public int Clutch { get; set; }
        public int Test { get; set; }

        public int SpeedM { get; set; }
        public int SpeedR { get; set; }
        public int DoorAngle { get; set; }
        public int MotorV { get; set; }
        public int MotorA { get; set; }
        public int DistanceF { get; set; }
        public int DistanceR { get; set; }

        private byte[] _states = new byte[16];

        public static void SaveCsv(string aPath, IEnumerable<StateShot> aShots)
        {
            try
            {
                using (System.IO.StreamWriter ws = new System.IO.StreamWriter(aPath))
                {
                    ws.NewLine = "\n";
                    ws.WriteLine("Time, SpeedM, SpeedR, DoorAngle ,MotorV, MotorA, DistanceF, DistanceR, DoorError,	DoorRun, DirectionOpen, DirectionClose, LatchOn, ReleaseOn, Clutch, Test");

                    foreach (var s in aShots)
                    {
                        var str = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                            s.Time, s.SpeedM, s.SpeedR, s.DoorAngle, s.MotorV, s.MotorA, s.DistanceF, s.DistanceR, s.DoorError, s.DoorRun, s.DirectionOpen, s.DirectionClose, s.LatchOn, s.ReleaseOn, s.Clutch, s.Test);
                        ws.WriteLine(str);
                    }
                }
            }
            catch(Exception e)
            {
                Log.e("데이터 파일(CSV) 저장 중 에러: {0}, Path=[{1}]" , e, aPath);
            }
        }
    }
}

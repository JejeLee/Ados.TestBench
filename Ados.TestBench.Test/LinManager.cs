using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ados.TestBench.Test
{
    // ADOS PID 값들
    public enum PID : byte {
        COMMAND = 0xC1,
        STATE1 = 0xB1,
        STATE2 = 0x32,
        WR_ADDR = 0x73,
        RD_ADDR = 0xB4,
        ADDR_REQUEST = 0xF5
    };

    public delegate void StateReceivedHandler(StateShot aShot);
    public delegate void ParameterReceivedHandler(int aAddr, int aValue);

    public delegate void JobStateChangedHandler(bool aUnderLoopJob);

    public sealed class LinManager
    {
        public LinManager()
        {
            UnderLoopJob = false;
        }

        public bool WriteCommand(params byte[] aData)
        {
            Log.i("LIN/Command>> Pid:{1}, {0}", (aData != null && aData.Length > 0) ? aData[0] : 0, (int)PID.COMMAND);

            return WriteMessage(PID.COMMAND, aData);
        }

        public const byte DOOR_OPEN = 1;
        public const byte DOOR_STOP = 2;

        public bool ReadState()
        {
            Peak.Lin.TLINRcvMsg rmsg1;

            Log.i("LIN/ReadState-1>> Pid:{1}, {0}", 0, (int)PID.STATE1);
            WriteMessage(PID.STATE1, 0);
            
            if (!ReadMessages(out rmsg1, 50))
            {
                return false;
            }
#if !TYPE_A
            Peak.Lin.TLINRcvMsg rmsg2;
            Log.i("LIN/ReadState-2>> PId:{1}, {0}", 0,  (int)PID.STATE1);
            WriteMessage(PID.STATE2, 0);
            if (!ReadMessages(out rmsg2, 50))
            {
                return false;
            }
#endif
            var shot = new StateShot();
            shot.SetState1(rmsg1.Data);
#if !TYPE_A
            shot.SetState2(rmsg2.Data);
#endif
            InvokeStateReceived(shot);

            return true;
        }

        public void ReadStateLoop(int aPeriodMS, bool aUpdateJob = true)
        {
            try
            {
                Log.i("상태 데이터 읽는 중...");
                if (aUpdateJob)
                    UnderLoopJob = true;

                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();

                while (watch.ElapsedMilliseconds < aPeriodMS && !_stopLoopJob)
                {
                    if (!ReadState())
                    {
                        //return false;
                    }
                    //System.Threading.Thread.Sleep(30);
                } 
                watch.Stop();
            }
            catch (Exception e)
            {
                Log.e("상태 정보 읽는 중 예외 발생:" + e.ToString());
            }
            finally
            {
                if (aUpdateJob)
                    UnderLoopJob = false;
            }
        }

        public void ReadStateLoopAsync(int aPeriodMS)
        {
            try
            {
                Task.Run(() => { ReadStateLoop(aPeriodMS); });
                //ReadStateLoop(aPeriodMS);
            }
            catch (Exception e)
            {
                Log.e("상태 정보 읽는 중 예외 발생:" + e.ToString());
            }
        }

        public bool WriteParameter(ParameterSetting aSetting)
        {
            byte high = (byte)((aSetting.WriteValue >> 8) & 0xFF);
            byte low = (byte)(aSetting.WriteValue & 0xFF);

            Log.i("LIN/Parameter >> Pid:{2} Addr:{0} Value:{1}", aSetting.Info.Address, aSetting.WriteValue, (int)PID.WR_ADDR);

            if (!WriteMessage(PID.WR_ADDR, high, low))
            {
                return false;
            }

            return true;
        }

        public void WriteParameters(IEnumerable<ParameterSetting> aSettings)
        {
            try
            {
                Log.i("파라미터들 쓰는 중...");
                UnderLoopJob = true;

                foreach (var item in aSettings)
                {
                    if (_stopLoopJob)
                    {
                        break;
                    }

                    if (!WriteParameter(item))
                    {
                        Log.e("WriteParameter Error");
                        return;
                    }
                    //System.Threading.Thread.Sleep(30);
                }
            }
            catch (Exception e)
            {
                Log.e("파라미터 쓰는 중 예외 발생:" + e.ToString());
            }
            finally
            {
                UnderLoopJob = false;
            }
        }

        public void WriteParametersAsync(IEnumerable<ParameterSetting> aSettings)
        {
            try
            {
                Task.Run(() => { WriteParameters(aSettings); });
            }
            catch (Exception e)
            {
                Log.e("파라미터 쓰는 중 예외 발생:" + e.ToString());
            }
        }

        public bool ReadParameter(int aAddr)
        {
            Log.i("LIN/Parameter READ >> Pid:{1} Addr:{0}", aAddr, (int)PID.WR_ADDR);
            if (!WriteMessage(PID.RD_ADDR, 0))
            {
                return false;
            }

            Peak.Lin.TLINRcvMsg rmsg;
            if (!ReadMessages(out rmsg, 200))
            {
                return false;
            }

            int value = (rmsg.Data[1] << 8) | rmsg.Data[2];
            InvokeParameterReceived(rmsg.Data[0], value);

            return true;
        }

        public void ReadParameters(IEnumerable<ParameterSetting> aSettings)
        {
            try
            {
                Log.i("파라미터들 읽는 중...<<<");
                UnderLoopJob = true;

                foreach (var item in aSettings)
                {
                    if (_stopLoopJob)
                    {
                        break;
                    }

                    if (!ReadParameter(item.Info.Address))
                    {
                        Log.e("ReadParameter Error");
                        return;
                    }
                    //System.Threading.Thread.Sleep(30);
                }
            }
            catch (Exception e)
            {
                Log.e("파라미터 값 읽는 중 예외 발생:" + e.ToString());
            }
            finally
            {
                UnderLoopJob = false;
            }
        }

        public void ReadParametersAsync(IEnumerable<ParameterSetting> aSettings)
        {
            try
            {
                Task.Run(() => { ReadParameters(aSettings); });
            }
            catch (Exception e)
            {
                Log.e("파라미터 읽는 중 예외 발생:" + e.ToString());
            }
        }

        //
        // TODO: read parameter, enumerable read/write
        //       read parameter event.

        public int RefreshHardware()
        {
            _devices = new List<LinDevice>();
            this._dev = null;

            // Get the buffer length needed...
            ushort lwCount = 0;
            _err = Peak.Lin.PLinApi.GetAvailableHardware(new ushort[0], 0, out lwCount);
            if (_err != Peak.Lin.TLINError.errOK || lwCount == 0)
            {
                LinError("검색된 LIN 통신 장치가 없습니다.", _err);
                return 0;
            }

            var lwHwHandles = new ushort[lwCount];
            var lwBuffSize = Convert.ToUInt16(lwCount * sizeof(ushort));

            // Get all available LIN hardware.
            _err = Peak.Lin.PLinApi.GetAvailableHardware(lwHwHandles, lwBuffSize, out lwCount);
            if (_err != Peak.Lin.TLINError.errOK || lwCount == 0)
            {
                LinError("LIN 장치를 읽는 중 에러,", _err);
                return 0;
            }

            for (int i = 0; i < lwCount; i++)
            {
                int lnHwType, lnDevNo, lnChannel;
                // Get the handle of the hardware.
                var dev = new LinDevice(lwHwHandles[i]);
                // Read the type of the hardware with the handle lwHw.
                Peak.Lin.PLinApi.GetHardwareParam(dev.HwHandle, Peak.Lin.TLINHardwareParam.hwpType, out lnHwType, 0);
                dev.SetHwType(lnHwType);
                // Read the device number of the hardware with the handle lwHw.
                Peak.Lin.PLinApi.GetHardwareParam(dev.HwHandle, Peak.Lin.TLINHardwareParam.hwpDeviceNumber, out lnDevNo, 0);
                dev.DevNo = lnDevNo;
                // Read the channel number of the hardware with the handle lwHw.
                Peak.Lin.PLinApi.GetHardwareParam(dev.HwHandle, Peak.Lin.TLINHardwareParam.hwpChannelNumber, out lnChannel, 0);
                dev.Channel = lnChannel;

                _devices.Add(dev);
            }

            return _devices.Count();
        }

        public List<LinDevice> _devices = new List<LinDevice>();

        public bool Connect(int aBaudrate = 19200, Peak.Lin.TLINHardwareMode aMode = Peak.Lin.TLINHardwareMode.modMaster)
        {
            if (this.Device == null)
            {
                if (RefreshHardware() <= 0)
                    return false;
            }

            Disconnect();

            m_wBaudrate = aBaudrate;
            m_HwMode = aMode;

           if (m_hClient == 0)
                // Register this application with LIN as client.
                _err = Peak.Lin.PLinApi.RegisterClient(LinManager.ClientName, IntPtr.Zero, out m_hClient);

            int lnMode;
            int lnCurrBaud;
            // The local hardware handle is valid.
            // Get the current mode of the hardware
            Peak.Lin.PLinApi.GetHardwareParam(Device.HwHandle, Peak.Lin.TLINHardwareParam.hwpMode, out lnMode, 0);
            // Get the current baudrate of the hardware
            Peak.Lin.PLinApi.GetHardwareParam(Device.HwHandle, Peak.Lin.TLINHardwareParam.hwpBaudrate, out lnCurrBaud, 0);
            // Try to connect the application client to the
            // hardware with the local handle.
            Device.Connected = false;
            _err = Peak.Lin.PLinApi.ConnectClient(m_hClient, Device.HwHandle);
            if (_err == Peak.Lin.TLINError.errOK)
            {                
                // Get the selected hardware channel
                if (((Peak.Lin.TLINHardwareMode)lnMode == Peak.Lin.TLINHardwareMode.modNone)
                            || m_wBaudrate != lnCurrBaud)
                {
                    // Only if the current hardware is not initialize
                    // try to Intialize the hardware with mode and baudrate
                    _err = Peak.Lin.PLinApi.InitializeHardware(m_hClient, Device.HwHandle, m_HwMode, (ushort)m_wBaudrate);
                }
                if (_err == Peak.Lin.TLINError.errOK)
                {
                    Device.Connected = true;
                    Log.i("Connected - {0}", this.Device);
                    return true;
                }

            }

            LinError( string.Format("Connection Error(Dev:{0}), ", this.Device), _err);
            return false;
        }

        private bool Disconnect()
        {
            if (this.IsConnected == false)
                return true;

            // If the application was registered with LIN as client.
            if (Device.HwHandle != 0)
            {
                // The client was connected to a LIN hardware.
                // Before disconnect from the hardware check
                // the connected clients and determine if the
                // hardware configuration have to reset or not.

                // Initialize the locale variables.
                bool lfOtherClient = false;
                bool lfOwnClient = false;
                byte[] lhClients = new byte[255];

                // Get the connected clients from the LIN hardware.
                _err = Peak.Lin.PLinApi.GetHardwareParam(Device.HwHandle, Peak.Lin.TLINHardwareParam.hwpConnectedClients, lhClients, 255);
                if (_err == Peak.Lin.TLINError.errOK)
                {
                    // No errors !
                    // Check all client handles.
                    for (int i = 0; i < lhClients.Length; i++)
                    {
                        // If client handle is invalid
                        if (lhClients[i] == 0)
                            continue;
                        // Set the boolean to true if the handle isn't the
                        // handle of this application.
                        // Even the boolean is set to true it can never
                        // set to false.
                        lfOtherClient = lfOtherClient || (lhClients[i] != m_hClient);
                        // Set the boolean to true if the handle is the
                        // handle of this application.
                        // Even the boolean is set to true it can never
                        // set to false.
                        lfOwnClient = lfOwnClient || (lhClients[i] == m_hClient);
                    }
                }
                // If another application is also connected to
                // the LIN hardware do not reset the configuration.
                if (lfOtherClient == false)
                {
                    // No other application connected !
                    // Reset the configuration of the LIN hardware.
                    Peak.Lin.PLinApi.ResetHardwareConfig(m_hClient, Device.HwHandle);
                }
                // If this application is connected to the hardware
                // then disconnect the client. Otherwise not.
                if (lfOwnClient == true)
                {
                    // Disconnect if the application was connected to a LIN hardware.
                    _err = Peak.Lin.PLinApi.DisconnectClient(m_hClient, Device.HwHandle);
                    if (_err != Peak.Lin.TLINError.errOK)
                    {
                        return false;
                    }
                    Device.Connected = false;
                    LinError(string.Format("Disconnected Corrupt- Dev:{0}", this.Device), _err);
                }               
            }

            if (Device.Connected)
                LinError(string.Format("Disconnected Corrupt- Dev:{0}", this.Device), _err);

            return !Device.Connected;
        }

        public void BlinkDevice(LinDevice aDevice)
        {
            if (aDevice != null)
            {
                Log.i("blink device lamp: dev:{0}, ch:{1}", aDevice.DevNo, aDevice.Channel);
                // makes the corresponding PCAN-USB-Pro's LED blink
                Peak.Lin.PLinApi.IdentifyHardware(aDevice.HwHandle);
            }
        }

        public string GetDeviceStatus(LinDevice aDevice)
        {
            Peak.Lin.TLINHardwareStatus lStatus;
            if (aDevice == null)
                return "0(Invalid LIN device)";
            // Retrieves the status of the LIN Bus and outputs its state in the information listView
            _err = Peak.Lin.PLinApi.GetStatus(aDevice.HwHandle, out lStatus);
            if (_err == Peak.Lin.TLINError.errOK)
                switch (lStatus.Status)
                {
                    case Peak.Lin.TLINHardwareState.hwsActive:
                        return "1Bus: Active";
                    case Peak.Lin.TLINHardwareState.hwsAutobaudrate:
                        return "2Hardware: Baudrate Detection";
                    case Peak.Lin.TLINHardwareState.hwsNotInitialized:
                        return "3Hardware: Not Initialized";
                    case Peak.Lin.TLINHardwareState.hwsShortGround:
                        return "4Bus - Line: Shorted Ground";
                    case Peak.Lin.TLINHardwareState.hwsSleep:
                        return "5Bus: Sleep";
                }
            return string.Format("0(Check Error - {0})", _err);
        }

        public bool IsConnected { get { return this.Device != null && this.Device.Connected; } }

        public bool IsLastError { get { return _err != Peak.Lin.TLINError.errOK; } }

        public LinDevice Device {
            get {
                return _dev;
            }
            set {
                if (value != _dev)
                {
                    if (_dev != null)
                        Disconnect();

                    _dev = value;
                    if (_dev != null)
                    {
                        BlinkDevice(_dev);
                        Connect();
                    }
                }
            } }
        private LinDevice _dev = null;

        public IEnumerable<LinDevice> Devices { get { return _devices; } }

        static public void StopLoopJob()
        {
            if (UnderLoopJob)
            {
                _stopLoopJob = true;
            }
        }


        private static bool _stopLoopJob = false;
        private static bool _underLoopJob = false;

        static public bool UnderLoopJob {
            get { return _underLoopJob;
            }
            set {
                if (_underLoopJob != value)
                {
                    _underLoopJob = value;
                    if (!_underLoopJob)
                        _stopLoopJob = false;

                    if (JobStateChangedEvent != null)
                        JobStateChangedEvent(_underLoopJob);
                }
            }
        }

        public static event JobStateChangedHandler JobStateChangedEvent;

        static public event StateReceivedHandler StateReceivedEvent;

        static private void InvokeStateReceived(StateShot aShot)
        {
            Log.i("LIN/State <<" + aShot.ToString());
            if (StateReceivedEvent != null)
            {
                StateReceivedEvent(aShot);
            }
        }

        static public event ParameterReceivedHandler ParameterReceivedEvent;

        static private void InvokeParameterReceived(int aAddr,int aValue)
        {
            Log.i("LIN/Parameter << {0}={1}", aAddr, aValue);
            if (ParameterReceivedEvent != null)
            {
                ParameterReceivedEvent(aAddr, aValue);
            }
        }

        // transmission rx, tx indicator
        public const int TransSurveyTime = 120 * 10000; // 1tic/100 nano sec = 000 ms
        static public long RxTics { get; private set; }
        static public long TxTics { get; private set; }
        static public bool IsRxError { get; private set; }
        static public bool IsTxError { get; private set; }


        static Peak.Lin.TLINError Read(byte hClient, out Peak.Lin.TLINRcvMsg aMsg)
        {
            RxTics = DateTime.Now.Ticks + TransSurveyTime;

            Peak.Lin.TLINError err = Peak.Lin.PLinApi.Read(hClient, out aMsg);

            IsRxError = err != Peak.Lin.TLINError.errOK;
            if (IsRxError)
            {
                LinError("Read", err);
            }
            
            return err;
        }

        static Peak.Lin.TLINError Write(byte hClient, ushort hHw, ref Peak.Lin.TLINMsg aMsg)
        {
            TxTics = DateTime.Now.Ticks + TransSurveyTime;

            Peak.Lin.TLINError err = Peak.Lin.PLinApi.Write(hClient, hHw, ref aMsg);

            IsTxError = err != Peak.Lin.TLINError.errOK;
            if (IsTxError)
            {
                LinError("Write", err);
            }

            return err;
        }

        private bool ReadMessages(out Peak.Lin.TLINRcvMsg aMsg, int aTimeout = 50 /* msec */)
        {
#if SIMULATION
            System.Threading.Thread.Sleep(8);
            aMsg = new Peak.Lin.TLINRcvMsg();
            aMsg.Length = 8;
            aMsg.Data = new byte[aMsg.Length];
            Random r = new Random(Environment.TickCount/10);
            r.Next();
            r.NextBytes(aMsg.Data);            
            return true;
#endif
            int timeout = aTimeout;
            // We read at least one time the queue looking for messages.
            // If a message is found, we look again trying to find more.
            // If the queue is empty or an error occurs, we get out from
            // the dowhile statement.
            //	
            const int sleep = 5;
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            do
            {
                _err = LinManager.Read(m_hClient, out aMsg);
                // If at least one Frame is received by the LinApi.
                // Check if the received frame is a standard type.
                // If it is not a standard type than ignore it.
                if (_err == Peak.Lin.TLINError.errOK)
                {
                    if (aMsg.Type == Peak.Lin.TLINMsgType.mstStandard)
                    {
                        watch.Stop();
                        return true;
                    }
                    else
                    {
                        Log.i("표준 이외의 메세지 타입 수신 - {0}", aMsg.Type.ToString());
                    }
                }
                else
                {
                    LinError("Read Error: ", _err);
                }

                System.Threading.Thread.Sleep(sleep);

            } while ((_err == Peak.Lin.TLINError.errOK
                    || _err == Peak.Lin.TLINError.errRcvQueueEmpty)
                    && timeout > watch.ElapsedMilliseconds);

            watch.Stop();
            return false;
        }

        private bool WriteMessage(PID aPID, params byte[] aData)
        {
#if SIMULATION
            return true;
#endif
            byte frameid = (byte)((byte)aPID & Peak.Lin.PLinApi.LIN_MAX_FRAME_ID);
            Peak.Lin.PLinApi.GetPID(ref frameid);

            Peak.Lin.TLINMsg pMsg = new Peak.Lin.TLINMsg();
            pMsg.Data = new byte[8];
            pMsg.FrameId = frameid;
            pMsg.FrameId = (byte)aPID;
            pMsg.Direction = Peak.Lin.TLINDirection.dirPublisher;
            pMsg.ChecksumType = Peak.Lin.TLINChecksumType.cstClassic;
            pMsg.Length = (byte)aData.Length;
            // Fill data array
            if (pMsg.Length == 0)
            {
                pMsg.Length = 1;
                pMsg.Data[0] = (byte)0xFF;
            }
            else
            {
                for (int i = 0; i < pMsg.Length; i++)
                {
                    pMsg.Data[i] = aData[i];
                }
            }
            // Check if the hardware is initialize as master
            if (m_HwMode == Peak.Lin.TLINHardwareMode.modMaster)
            {
                // Calculate the checksum contained with the
                // checksum type that set some line before.
                Peak.Lin.PLinApi.CalculateChecksum(ref pMsg);
                // Try to send the LIN frame message with LIN_Write.
                _err = LinManager.Write(m_hClient, Device.HwHandle, ref pMsg);
            }

            return !IsLastError;
        }

        static private void LinError(string aMsg, Peak.Lin.TLINError aError)
        {
            Log.e("LIN/Error: {0}, err:{1}={2}", aMsg, aError, GetErrorText(aError));
        }

        static private string GetErrorText(Peak.Lin.TLINError aError)
        {
            StringBuilder sErrText = new StringBuilder(255);
            // If any error are occured
            // display the error text in a message box.
            // 0x00 = Neutral
            // 0x07 = Language German
            // 0x09 = Language English
            if (Peak.Lin.PLinApi.GetErrorText(aError, 0x09, sErrText, sErrText.Capacity) != Peak.Lin.TLINError.errOK)
                sErrText.AppendFormat("An error occurred. Error-code's text ({0}) couldn't be retrieved", aError);

            return sErrText.ToString();
        }

        /// <summary>
        /// Client handle
        /// </summary>
        private byte m_hClient = 0;
        /// <summary>
        /// LIN Hardware Modus (Master/Slave)
        /// </summary>
        private Peak.Lin.TLINHardwareMode m_HwMode = Peak.Lin.TLINHardwareMode.modNone;
        /// <summary>
        /// Baudrate Index of Hardware
        /// </summary>
        private int m_wBaudrate = 0;

        Peak.Lin.TLINError _err = Peak.Lin.TLINError.errOK;

        static public readonly string ClientName = "ADOS_LIN";
    }
}

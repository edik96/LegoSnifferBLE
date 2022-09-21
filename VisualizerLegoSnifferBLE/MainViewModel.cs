using LiveCharts;
using LiveCharts.Defaults;
using MQTTnet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using System.Threading;
using MQTTnet.Packets;

namespace VisualizerLegoSnifferBLE
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private int m_ID;
        private DateTime m_date;
        private double m_cm = 0;
        private Color m_sensorcolor = Colors.White;
        public ChartValues<DateTimePoint> m_values;
        private int curr_cm_val = 0;
        private int prev_value = 0;
        private Func<double, string> m_formatter;
        private double m_startdate, m_enddate;
        private bool m_danger = false;
        private int m_pitch = 0;
        private int m_yaw = 0;
        private int m_roll = 0;
        private Vector3D m_rotation;
        private MqttFactory mqttFactory;
        private IMqttClient mqttClient;

        public Vector3D Rotation
        {
            get
            {
                return m_rotation;
            }
            set
            {
                m_rotation = value;
                OnPropertyChanged("Rotation");
            }
        }
        public int Pitch
        {
            get
            {
                return m_pitch;
            }
            set
            {
                m_pitch = value;
                OnPropertyChanged("Pitch");
            }
        }
        public int Yaw
        {
            get
            {
                return m_yaw;
            }
            set
            {
                m_yaw = value;
                OnPropertyChanged("Yaw");
            }
        }
        public int Roll
        {
            get
            {
                return m_roll;
            }
            set
            {
                m_roll = value;
                OnPropertyChanged("Roll");
            }
        }
        public bool Danger
        {
            get
            {
                return m_danger;
            }
            set
            {
                m_danger = value;
                OnPropertyChanged("Danger");
            }
        }

        public double Startdate
        {
            get
            {
                return m_startdate;
            }
            set
            {
                m_startdate = value;
                OnPropertyChanged("Startdate");
            }
        }
        public double Enddate
        {
            get
            {
                return m_enddate;
            }
            set
            {
                m_enddate = value;
                OnPropertyChanged("Enddate");
            }
        }
        public Func<double, string> Formatter
        {
            get
            {
                return m_formatter;
            }
            set
            {
                m_formatter = value;
                OnPropertyChanged("Formatter");
            }
        }
        public void InitNew()
        {
            mqttFactory = new MqttFactory();
            //var client = new NamedPipeClientStream("a8:e2:c1:9c:71:4a");
            MqttClientOptions options = new MqttClientOptionsBuilder()
                                    .WithClientId("webkumbl")
                                    .WithTcpServer("localhost", 1883).Build();

            mqttClient = mqttFactory.CreateMqttClient();
            
            //managerModel = new ManagedMqttClient(mqttClient, null);
            //ClientConnect(mqttClient);

            //StreamReader reader = new StreamReader(client);
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                if(e.ApplicationMessage.Payload != null)
                {
                    DataSet stuff = JsonConvert.DeserializeObject<DataSet>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                    if (stuff != null)
                    {
                        try
                        {

                            curr_cm_val = stuff.m_distance;
                            // if (curr_cm_val != prev_value)
                            //{
                            //  prev_value = curr_cm_val;
                            if (Values.Count == 0 || Values.Last().DateTime.AddSeconds(0.5).Ticks < DateTime.Now.Ticks)
                                Values.Add(new DateTimePoint(DateTime.Now, Convert.ToDouble(curr_cm_val)));
                            if (curr_cm_val > 4)
                                Cm = curr_cm_val;
                            if (Values.Count >= 100)
                                Values.RemoveAt(0);

                            // }
                            Dispatcher.CurrentDispatcher.Invoke(() =>
                                SensorColor = Color.FromRgb((byte)stuff.m_rgb[0], (byte)stuff.m_rgb[1], (byte)stuff.m_rgb[2]));

                            //Pitch = stuff.m_gyro[0];
                            //Yaw = stuff.m_gyro[1];
                            //Roll = stuff.m_gyro[2];

                            ChangeRotaion(stuff.m_gyro[0], stuff.m_gyro[1], stuff.m_gyro[2]);

                            Startdate = DateTime.Now.AddSeconds(-5).Ticks;

                            // Rotation = new Vector3D(stuff.m_gyro[0], stuff.m_gyro[1], stuff.m_gyro[2]);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Bad message => Proceeding to next message");
                        }

                    }
                }
                    
                return Task.CompletedTask;
            };
            ClientConnectAsync(mqttClient, options);

        }


        private async void ClientConnectAsync(IMqttClient mqttClient, MqttClientOptions options)
        {
            Console.WriteLine("Connecting...");
            await mqttClient.ConnectAsync(options, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => { f.WithTopic("LegoWave"); })
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        }

        public void Init()
        {
            var client = new NamedPipeClientStream(LegoSnifferBLE.Program.mac_addres);
            client.Connect();
            StreamReader reader = new StreamReader(client);

            Formatter = value => DateTime.Now.ToString("t");
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    DataSet stuff = JsonConvert.DeserializeObject<DataSet>(line);
                    if (stuff != null)
                    {
                        try
                        {

                            curr_cm_val = stuff.m_distance;
                            // if (curr_cm_val != prev_value)
                            //{
                            //  prev_value = curr_cm_val;
                                if (Values.Count == 0 || Values.Last().DateTime.AddSeconds(0.5).Ticks < DateTime.Now.Ticks)
                                  Values.Add(new DateTimePoint(DateTime.Now, Convert.ToDouble(curr_cm_val)));
                                if(curr_cm_val > 4)
                                    Cm = curr_cm_val;
                                if (Values.Count >= 100)
                                    Values.RemoveAt(0);

                            // }
                            Dispatcher.CurrentDispatcher.Invoke(() =>
                                SensorColor = Color.FromRgb((byte)stuff.m_rgb[0], (byte)stuff.m_rgb[1], (byte)stuff.m_rgb[2]));

                            //Pitch = stuff.m_gyro[0];
                            //Yaw = stuff.m_gyro[1];
                            //Roll = stuff.m_gyro[2];

                            ChangeRotaion(stuff.m_gyro[0], stuff.m_gyro[1], stuff.m_gyro[2]);

                            Startdate = DateTime.Now.AddSeconds(-5).Ticks;

                           // Rotation = new Vector3D(stuff.m_gyro[0], stuff.m_gyro[1], stuff.m_gyro[2]);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Bad message => Proceeding to next message");
                        }

                    }
                }
            });
        }
        public void ChangeRotaion(int _p, int _y, int _r)
        {
            int pm = 1;
            int ym = 1;
            int rm = 1;

            if (_p < Pitch)
                pm = -1;
            if (_y < Yaw)
                ym = -1;
            if (_r < Roll)
                rm = -1;

            while(Pitch != _p || Yaw != _y || Roll != _r)
            {
                if(Pitch != _p)
                    Pitch += pm;

                if (Yaw != _y)
                    Yaw += ym;

                if (Roll != _r)
                    Roll += rm;
            }
            Pitch = _p;
            Yaw = _y;
            Roll = _r;
        }

        public ChartValues<DateTimePoint> Values
        {
            get
            {
                return m_values;
            }
            set
            {
                m_values = value;
                OnPropertyChanged("Values");
            }
        }

        #region INotifyPropertyChanged Members  
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public SeriesCollection LastHourSeries { get; set; }

        public int ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
                OnPropertyChanged("ID");
            }
        }
        public DateTime Date
        {
            get
            {
                return m_date;
            }
            set
            {
                m_date = value;
                OnPropertyChanged("Date");
            }
        }
        public double Cm
        {
            get
            {
                return m_cm;
            }
            set
            {
                m_cm = value;
                if (value < 7)
                    Danger = true;
                else
                    Danger = false;
                OnPropertyChanged("Cm");
            }
        }
        public Color SensorColor
        {
            get
            {
                return m_sensorcolor;
            }
            set
            {
                m_sensorcolor = value;
                OnPropertyChanged("SensorColor");
            }
        }
    }
}

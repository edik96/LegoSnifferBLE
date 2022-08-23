using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualizerLegoSnifferBLE
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private int m_ID;
        private DateTime m_date;
        private double m_cm = 0;
        private string m_sensorcolor = "white";
        public ChartValues<DateTimePoint> m_values;
        private int curr_cm_val = 0;
        private int prev_value = 0;
        private Func<double, string> m_formatter;
        private DateTime m_startdate, m_enddate;
        private bool m_danger = false;

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

        public DateTime Startdate
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
        public DateTime Enddate
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
                    try
                    {
                        if (line.Length > 1 && !line.Contains("None"))
                        {
                            curr_cm_val = Convert.ToInt32(line.Split(';')[0]);
                            if (curr_cm_val != prev_value)
                            {
                                prev_value = curr_cm_val;
                                Values.Add(new DateTimePoint(DateTime.Now, Convert.ToDouble(curr_cm_val)));
                                Cm = curr_cm_val;
                            }
                            SensorColor = reader.ReadLine().Split(';')[1];

                        }
                    }
                    catch(Exception ex)
                    {
                        Console.Write("Bad message => Proceeding to next message");
                    }
                }
            });
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
        public string SensorColor
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

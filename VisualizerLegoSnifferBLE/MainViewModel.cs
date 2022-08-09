﻿using LiveCharts;
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
        private int m_cm = 1;
        public ChartValues<DateTimePoint> m_values;
        private int current_value = 0;
        private int prev_value = 0;
        private Func<double, string> m_formatter;
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
                    current_value = Convert.ToInt32(reader.ReadLine());
                    if (current_value != prev_value)
                    {
                        prev_value = current_value;
                        Values.Add(new DateTimePoint(DateTime.Now, Convert.ToDouble(current_value)));
                 
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
        public int Cm
        {
            get
            {
                return m_cm;
            }
            set
            {
                m_cm = value;
                OnPropertyChanged("Cm");
            }
        }
    }
}

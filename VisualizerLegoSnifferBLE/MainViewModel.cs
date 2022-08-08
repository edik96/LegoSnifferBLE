using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VisualizerBrainDamage
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private int m_ID;
        private DateTime m_date;
        private int m_cm;
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
                m_date = DateTime.Now;
                OnPropertyChanged("Name");
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
                m_cm = Convert.ToInt32(LegoBraindamage.Program.p_value);
                OnPropertyChanged("Price");
            }
        }
    }
}

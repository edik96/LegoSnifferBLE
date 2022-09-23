using LegoSnifferBLE;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualizerLegoSnifferBLE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MainViewModel VM = new MainViewModel();
            VM.Values = new ChartValues<DateTimePoint>();
            
            var mapper = Mappers.Xy<DateTimePoint>()
               .X(x => x.DateTime.Ticks)
               .Y(x => x.Value);
         //   VM.Values.Add(new KeyValuePair<DateTime, int>(DateTime.Now, payloadval));
          //  VM.Cm = payloadval;
            //save the mapper globally         
            Charting.For<DateTimePoint>(mapper);
            this.DataContext = VM;
            VM.InitNew();
            InitializeComponent();
        }
    }
    public class CmToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double maxval = 200;
            double j = (double)value;
            return (100 / maxval) * j;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int maxval = 200;
            int j = (int)value;
            return 50;
        }
    }
}

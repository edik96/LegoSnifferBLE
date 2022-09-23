using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebUIVisualizerLegoSnifferBLE
{
    public class DataSet
    {
        public IList<int> m_gyro { get; set; }
        public IList<int> m_rgb { get; set; }
        public int m_distance { get; set; } = 0;
        
        public DataSet(int[] _gyro, int[] _rgb, int _distance)
        {
            m_gyro = _gyro;
            m_rgb = _rgb;
            m_distance = _distance;
        }
    }
}

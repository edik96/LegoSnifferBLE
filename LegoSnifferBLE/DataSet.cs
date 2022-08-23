using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoSnifferBLE
{
    public class DataSet
    {
        public int[] m_gyro = new int[3];
        public int[] m_rgb = new int[3];
        public int m_distance = 0;
        
        public DataSet(int[] _gyro, int[] _rgb, int _distance)
        {
            m_gyro = _gyro;
            m_rgb = _rgb;
            m_distance = _distance;
        }
    }
}

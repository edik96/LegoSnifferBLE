using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using System.Net.Sockets;
using Npgsql;
using LiveCharts.Defaults;
using LiveCharts;
using LiveCharts.Configurations;
using System.IO.Pipes;
using System.IO;

namespace LegoSnifferBLE
{
    public class Program
    {
        public static DeviceInformationCollection collection;
        private static BluetoothDeviceInfo BTDevice;
        private static BluetoothEndPoint EP;
        private static BluetoothClient BC;
        private static NetworkStream stream = null;
        private static string con_str = "Host=10.10.242.82;Username=postgres;Password=secret;Database=postgres";
        private static string payload = string.Empty;
        public static string p_value = string.Empty;
        public static string mac_addres = "a8:e2:c1:9c:71:4a"; //Default
        private static StreamWriter writer;
        private static int usePipes = 0;
        static void Main(string[] args)
        {
            Console.Title = "Lego Sniffer BLE";
            if (args.Length > 0)
            {
                mac_addres = args[0];
                if(args.Length > 1)
                   usePipes = Convert.ToInt32(args[1]);
            }
            var server = new NamedPipeServerStream(mac_addres);
            
            if(usePipes == 1)
            {
                server.WaitForConnection();
                writer = new StreamWriter(server);
            }
          

            NpgsqlConnection connection = InitDB(con_str);
            string sql_q = "INSERT INTO postgres.discover.process_lego (data) VALUES ("+payload+")";

            EnumerateSnapshot();
            int payloadval = 0;
            while (true)
            {
                if (payload.Length > 0 && int.TryParse(payload, out payloadval))
                {
                    Console.WriteLine("{" + DateTime.Now + ", " + payloadval + "}");
                    if (usePipes == 1)
                    {
                        writer.WriteLine(payloadval);
                        writer.Flush();
                    }
                    sql_q = "INSERT INTO postgres.discover.process_lego (data) VALUES (" + payload + ")";
                    var cmd = new NpgsqlCommand(sql_q, connection);
                    cmd.ExecuteScalar();
                }
              
                System.Threading.Thread.Sleep(500);
            }
        }
        static NpgsqlConnection InitDB(string _con_str)
        {
            NpgsqlConnection con = new NpgsqlConnection(_con_str);
            con.Open();
            return con;
        }
        static async void EnumerateSnapshot()
        {
            collection = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true));
            EP = new BluetoothEndPoint(BluetoothAddress.Parse(mac_addres), BluetoothService.BluetoothBase);
            BC = new BluetoothClient();
            //Get trough collection
            //BTDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(collection[0].Id.Substring(collection[0].Id.Length - 17, 17)));

            //Get with MAC ADDRESS
            BTDevice = new BluetoothDeviceInfo(BluetoothAddress.Parse(mac_addres));
            BC.BeginConnect(BTDevice.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect), EP);
        }
        private static void Connect(IAsyncResult result)
        {
            stream = BC.GetStream();
            if (result.IsCompleted)
            {               
                while (stream.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    byte[] myWriteBuffer = new byte[1024];
                    string mystr = "";
                    do
                    {
                        stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        mystr += Encoding.Default.GetString(Decode(myReadBuffer));
                        if (mystr.StartsWith("val:"))
                        {
                            payload = mystr.Replace("val:", "");
                        }
                    } while (stream.DataAvailable);
                }
            }
        }
        
        public static byte[] Decode(byte[] packet)
        {
            var i = packet.Length - 1;
            while (packet[i] == 0)
            {
                --i;
            }
            var temp = new byte[i + 1];
            Array.Copy(packet, temp, i + 1);
            return temp;
        }
    }
}

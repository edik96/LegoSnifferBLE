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
using System.Media;
using Newtonsoft.Json;
using MQTTnet.Client;
using MQTTnet.Server;
using MQTTnet;
using System.Threading;

namespace LegoSnifferBLE
{
    public static class Beep
    {
        public static bool TryParseJson<T>(this string @this, out T result)
        {
            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(@this, settings);
            return success;
        }

    }

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
        private static int useDB = 1;
        private static NpgsqlConnection connection = null;

        private static MqttServer mqttServer = null;
        private static IMqttClient mqttClient = null;
        private static MqttFactory mqttFactory = new MqttFactory();

        public static int[] p_gyro = new int[3];
        public static int p_dinstance = 0;
        public static int[] p_color = new int[3];
        static void Main(string[] args)
        {
            Console.Title = "Lego Sniffer BLE";
            if (args.Length > 0)
            {
                mac_addres = args[0];
                if (args.Length > 1)
                    usePipes = Convert.ToInt32(args[1]);
                if (args.Length > 2)
                    useDB = Convert.ToInt32(args[2]);
            }
            var server = new NamedPipeServerStream(mac_addres);

            if (usePipes == 1)
            {
                //server.WaitForConnection();
                //writer = new StreamWriter(server);

                //MQTT Server
                InitMQTTServer();
             

            }

            if (useDB == 1)
                connection = InitDB(con_str);
            //string sql_q = "INSERT INTO postgres.discover.process_lego (data) VALUES ("+payload+")";
            string sql_q = string.Empty;
            EnumerateSnapshot();
            while (true)
            {
                if (usePipes == 1)
                {
                    //writer.WriteLine(payload);
                    //writer.Flush();

                    //MQTT PUBLISH
                    if(mqttClient.IsConnected)
                        BustPayload(payload);

                }
                if (useDB == 1)
                {
                    //Needs Rework with payload json ;)
                    /*   sql_q = "INSERT INTO postgres.discover.process_lego (data) VALUES (" + distance + ")";

                       var cmd = new NpgsqlCommand(sql_q, connection);
                       cmd.ExecuteScalar();*/
                }
                //Console.Title = Convert.ToString(distance);
                //Console.Beep((37 + distance) * 140, 100);
                System.Threading.Thread.Sleep(50);
            }
        }

        private static async void BustPayload(string _payload)
        {
            var message = new MqttApplicationMessageBuilder()
                              .WithTopic("LegoWave")
                              .WithPayload(_payload)
                              .Build();

            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        private static async void InitMQTTServer()
        {
            MqttServerOptions mqttServerOptions = new MqttServerOptions();
            mqttServerOptions.TlsEndpointOptions.Port = 1883;
            mqttServerOptions.TlsEndpointOptions.ClientCertificateRequired = false;
            mqttServerOptions.EnablePersistentSessions = true;
            mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
            await mqttServer.StartAsync();
            mqttClient = mqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(mac_addres)
                .WithTcpServer("localhost", 1883)
                .Build();
            await mqttClient.ConnectAsync(options, CancellationToken.None) ;
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
                while (stream != null && stream.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    byte[] myWriteBuffer = new byte[1024];
                    string mystr = "";
                    do
                    {
                        stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        mystr = Encoding.Default.GetString(Decode(myReadBuffer));
                        var reader = new StringReader(mystr);
                        if (mystr.StartsWith("{\"m\":0") && !mystr.Contains("MicroPython"))
                        {
                            Console.Clear();
                            dynamic stuff = JsonConvert.DeserializeObject(reader.ReadLine());
                            //Gyro
                            try
                            {
                                p_gyro[0] = stuff["p"][8][0];
                                p_gyro[1] = stuff["p"][8][1];
                                p_gyro[2] = stuff["p"][8][2];
                                //Acceleration Module = stuff["p"][7]
                                //Color Sensor 0 - 1000 RGB stuff["p"][4]
                                //Range Finder stuff["p"][3] cm
                                int.TryParse(stuff["p"][3][1][0].ToString(), out p_dinstance);
                                //Console.WriteLine(stuff["p"][4]);
                                p_color[0] = Convert.ToInt32((255m / 1000m) * Convert.ToInt32(stuff["p"][4][1][2]));
                                p_color[1] = Convert.ToInt32((255m / 1000m) * Convert.ToInt32(stuff["p"][4][1][3]));
                                p_color[2] = Convert.ToInt32((255m / 1000m) * Convert.ToInt32(stuff["p"][4][1][4]));


                                DataSet j = new DataSet(p_gyro, p_color, p_dinstance);

                                payload = JsonConvert.SerializeObject(j);
                            }
                            catch (Exception ex)
                            {

                            }



                            //payload = mystr.Replace("payload:", "").Split(';');
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


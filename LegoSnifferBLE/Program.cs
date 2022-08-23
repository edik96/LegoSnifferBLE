﻿using System;
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

namespace LegoSnifferBLE
{
    public class Beep
    {
        public static void BeepBeep(int Amplitude, int Frequency, int Duration)
        {
            double A = ((Amplitude * (System.Math.Pow(2, 15))) / 1000) - 1;
            double DeltaFT = 2 * Math.PI * Frequency / 44100.0;

            int Samples = 441 * Duration / 10;
            int Bytes = Samples * 4;
            int[] Hdr = { 0X46464952, 36 + Bytes, 0X45564157, 0X20746D66, 16, 0X20001, 44100, 176400, 0X100004, 0X61746164, Bytes };
            using (MemoryStream MS = new MemoryStream(44 + Bytes))
            {
                using (BinaryWriter BW = new BinaryWriter(MS))
                {
                    for (int I = 0; I < Hdr.Length; I++)
                    {
                        BW.Write(Hdr[I]);
                    }
                    for (int T = 0; T < Samples; T++)
                    {
                        short Sample = System.Convert.ToInt16(A * Math.Sin(DeltaFT * T));
                        BW.Write(Sample);
                        BW.Write(Sample);
                    }
                    BW.Flush();
                    MS.Seek(0, SeekOrigin.Begin);
                    using (SoundPlayer SP = new SoundPlayer(MS))
                    {
                        SP.PlaySync();
                    }
                }
            }
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
        private static string[] payload = new string[2];
        public static string p_value = string.Empty;
        public static string mac_addres = "a8:e2:c1:9c:71:4a"; //Default
        private static StreamWriter writer;
        private static int usePipes = 0;
        private static int useDB = 1;
        private static NpgsqlConnection connection = null;
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
                server.WaitForConnection();
                writer = new StreamWriter(server);
            }

            if (useDB == 1)
                connection = InitDB(con_str);
            //string sql_q = "INSERT INTO postgres.discover.process_lego (data) VALUES ("+payload+")";
            string sql_q = string.Empty;
            EnumerateSnapshot();
            int distance = 0;
            while (true)
            {
                if (payload.Length > 0)
                {
                    int.TryParse(payload[0], out distance);
                    if (payload[1] == "None" || payload[1] == string.Empty)
                        payload[1] = "white";
                    Console.WriteLine("{" + DateTime.Now + ", " + distance + ", " + payload[1] + "}");
                    if (usePipes == 1)
                    {
                        writer.WriteLine(distance + ";" + payload[1]);
                        writer.Flush();
                    }
                    if (useDB == 1)
                    {
                        sql_q = "INSERT INTO postgres.discover.process_lego (data) VALUES (" + distance + ")";

                        var cmd = new NpgsqlCommand(sql_q, connection);
                        cmd.ExecuteScalar();
                    }
                }
                Console.Title = Convert.ToString(distance);
                //Console.Beep((37 + distance) * 140, 100);
                Beep.BeepBeep(1000, distance * 100, 5);
                // System.Threading.Thread.Sleep(50);
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
                while (stream != null && stream.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    byte[] myWriteBuffer = new byte[1024];
                    string mystr = "";
                    do
                    {
                        stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        mystr += Encoding.Default.GetString(Decode(myReadBuffer));
                        if (mystr.StartsWith("payload:"))
                        {
                            payload = mystr.Replace("payload:", "").Split(';');
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


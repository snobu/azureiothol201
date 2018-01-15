using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using Microsoft.Azure.Devices.Client;
using System.Timers;
using System.Configuration;

namespace CpuUsage2Azure
{
    class Program
    {
        private static Timer _timer = null;
        private static PerformanceCounter _cpuCounter;        
        private static PerformanceCounter _ramCounter;        

        private static string _deviceConnectionString = null;

        
        private static DeviceClient _deviceClient = null;
        private static Message _eventMessage = null;

        static void Main(string[] args)
        {

            // We need to use decimal number using . as separator
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            _deviceConnectionString = ConfigurationManager.ConnectionStrings["deviceConnectionString"].ToString();

            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Http1);

            // If you get problem run "lodctr /r" from command prompt and restart computer
            // https://technet.microsoft.com/en-us/library/cc768048.aspx
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            // read CPU usage every second
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            // application runs until closed
            while (true)
            {                
            }
        }

        private static async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Get % of CPU usage, decimal number
            float cpuUsage = _cpuCounter.NextValue();
            // Get MB of free RAM, decimal number
            float ramUsage = _ramCounter.NextValue();
            
            // create payload for IoT Hub
            string payload = String.Format("{{'c':{0},'fr':{1}}}", cpuUsage, ramUsage);
            Console.WriteLine(payload);
            
            // prepare message
            _eventMessage = new Message(Encoding.UTF8.GetBytes(payload));
            // send message
            await _deviceClient.SendEventAsync(_eventMessage);
        }
    }
}

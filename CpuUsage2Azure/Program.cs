using System;
using System.Text;
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
            Console.WriteLine("Running...");
            // We need to use decimal number using . as separator
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            _deviceConnectionString = ConfigurationManager.ConnectionStrings["deviceConnectionString"].ToString();

            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Http1);

            // If you get problem run "lodctr /r" from command prompt and restart computer
            // https://technet.microsoft.com/en-us/library/cc768048.aspx
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            // read CPU usage every x seconds
            _timer = new Timer(9 * 1000); // every 9 seconds
            _timer.AutoReset = true;
            _timer.Elapsed += SendTelemetry;
            _timer.Start();

            // application runs until closed
            while (true)
            {                
            }
        }

        private static async void SendTelemetry(object sender, ElapsedEventArgs e)
        {
            // Get % of CPU usage, decimal number
            float cpuUsage = _cpuCounter.NextValue();
            // Get MB of free RAM, decimal number
            float ramUsage = _ramCounter.NextValue();
            
            // create payload for IoT Hub
            string payload = String.Format("{{'cpu_usage':{0},'memory_usage':{1}}}", cpuUsage, ramUsage);
            Console.WriteLine(payload);
            
            // prepare message
            _eventMessage = new Message(Encoding.UTF8.GetBytes(payload));
            // send message
            await _deviceClient.SendEventAsync(_eventMessage);
        }
    }
}

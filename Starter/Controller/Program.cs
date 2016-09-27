using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

using System.IO;
using HttpLibrary;
using HttpFileServer;

using TempHumid;

namespace Home
{
    public class Program
    {
        static HttpServer Server;                   // server object
        static Credential ServerCredential;         // server security
        static Configuration ServerConfiguration;   // configuration settings
        static DhtSensor Sensor;                    //humidity and temperature sensor
        
        static Double temperature = 0;              // graus Celsius
        static Double humidity = 0;                 // %

        static string cmd = "on";                      // On board led status
        static string thingStatus = "DHT Sensor: Ligado";   // Thing status

        public static void Main()
        {
            TimeCounter timeCounter = new TimeCounter();
            TimeSpan elapsed = TimeSpan.Zero;
            int i = 0;

            // Try to get clock at system start
            try
            {
                var time = NtpClient.GetNetworkTime();
                Utility.SetLocalTime(time);
            }
            catch (Exception ex)
            {
                // Don't depend on time
                Debug.Print("Error setting clock: " + ex.Message);
            }

            // On board led
            OutputPort onBoardLed = new OutputPort(Pins.ONBOARD_LED, false);

            // Humidity and Temperature
            Sensor = new Dht22Sensor(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1, PullUpResistor.Internal);
            
            Thread.Sleep(1000);

            // Web Server
            ServerConfiguration = new Configuration(80);
            ServerCredential = new Credential("Administrator", "admin", "admin");
            Server = new HttpServer(ServerConfiguration, ServerCredential, @"\SD\");
            Server.OnServerError += new OnServerErrorDelegate(Server_OnServerError);
            Server.OnRequestReceived += new OnRequestReceivedDelegate(Server_OnRequestReceived);
            Server.Start();

            // File Server
            FileServer server = new FileServer(@"\SD\", 1554);

            while (true)
            {
                timeCounter.Start();
                {
                    elapsed += timeCounter.Elapsed;
                    if (elapsed.Seconds >= 1)
                    {
                        if (Sensor.Read())
                        {
                            temperature = Sensor.Temperature;
                            humidity = Sensor.Humidity;
                            thingStatus = "DHT Sensor: RH = " + humidity.ToString("F1") + "%  Temp = " + temperature.ToString("F1") + "°C " + "Cmd = " + cmd.ToString();
                        }
                        elapsed = TimeSpan.Zero;
                        onBoardLed.Write((i++ & 0x01) == 0); // blink on board led

                        #region nulltask

                        #endregion
                    }
                }
                timeCounter.Stop();
                
            }
        }

        static void Server_OnRequestReceived(HttpRequest Request, HttpResponse Response)
        {
            if (Request.RequestedCommand != null)
            {
                switch (Request.RequestedCommand.ToLower())
                {
                    case "on":
                        cmd = "ON";     // command ON
                        break;
                    case "off":
                        cmd = "OFF";     // command OFF
                        break;
                }

                Response.WriteFilesList(thingStatus + "<br><br>" + "Comando " + Request.RequestedCommand.ToLower() + ": Status = " + cmd);
            }
            else if (Request.RequestedFile != null)
            {
                string FullFileName = Request.FilesPath + Request.RequestedFile;
                if (File.Exists(FullFileName))
                {
                    Response.WriteFile(FullFileName);
                }
                else
                {
                    Response.WriteNotFound();
                }
            }
            else
            {
                Response.WriteFilesList(thingStatus); 
                //Response.WriteFile(Request.FilesPath + "home.html"); // TODO: product page
            }
        }

        static void Server_OnServerError(ErrorEventArgs e)
        {
            Debug.Print(e.EventMessage);
        }

    }
}
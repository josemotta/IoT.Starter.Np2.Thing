using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace TempHumid
{
    /// <summary>
    /// Represents an instance of the DHT22 sensor.
    /// </summary>
    /// <remarks>
    /// Humidity measurement range 0 - 100%, accuracy ±2-5%.
    /// Temperature measurement range -40 - 80°C, accuracy ±0.5°C.
    /// </remarks>
    public class Dht22Sensor : DhtSensor
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="Dht22Sensor"/> class.
        /// </summary>
        /// <param name="pin1">The identifier for the sensor's data bus port.</param>
        /// <param name="pin2">The identifier for the sensor's data bus port.</param>
        /// <param name="pullUp">The pull-up resistor type.</param>
        /// <remarks>
        /// The ports identified by <paramref name="pin1"/> and <paramref name="pin2"/>
        /// must be wired together.
        /// </remarks>
        public Dht22Sensor(Cpu.Pin pin1, Cpu.Pin pin2, PullUpResistor pullUp)
            : base(pin1, pin2, pullUp)
        {
            // This constructor is intentionally left blank.
        }

        protected override int StartDelay
        {
            get
            {
                return 2; // At least 1 ms
            }
        }

        protected override void Convert(byte[] data)
        {
            Debug.Assert(data != null);
            Debug.Assert(data.Length == 4);

            // The first byte is integral, the second decimal part
            //Humidity = ((data[0] << 8) | data[1]) * 0.1F;
            Humidity = data[0];


            //var temp = (((data[2] & 0x7F) << 8) | data[3]) * 0.1F;
            //Temperature = (data[2] & 0x80) == 0 ? temp : -temp; // MSB = 1 means negative

            Temperature = data[2];
            
        }
    }
}
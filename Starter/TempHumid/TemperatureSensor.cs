using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace TempHumid
{
    public class TemperatureSensor
    {
        TimeCounter timeCounter = new TimeCounter();
        Thread gather_input;

        TimeSpan elapsed = TimeSpan.Zero;
        TimeSpan elapsed1 = TimeSpan.Zero;

        AnalogInput analogInput;
        Double temperature = 0;

        Double[] temp_history = new Double[100];
        Int32 history_index = 0;


        public TemperatureSensor(Cpu.AnalogChannel AnalogInputPin)
        {
            analogInput = new AnalogInput(AnalogInputPin);

            gather_input = new Thread(GatherInput);
            gather_input.Priority = ThreadPriority.Lowest;
            gather_input.Start();
        }


        void GatherInput()
        {
            while (true)
            {
                timeCounter.Start();
                {
                    elapsed += timeCounter.Elapsed;
                    elapsed1 += timeCounter.Elapsed;

                    if (elapsed.Milliseconds >= 20)
                    {
                        temp_history[history_index] = // read here
                            ((analogInput.Read() * 3.33f) - 0.5f) / 0.01f;

                        history_index++;

                        if (history_index >= temp_history.Length)
                            history_index = 0;

                        elapsed = TimeSpan.Zero;
                    }

                    if (elapsed1.Seconds >= 1)
                    {
                        Double tmp = 0.0f;

                        for (Byte i = 0; i < temp_history.Length; i++)
                            tmp += temp_history[i];

                        temperature = (tmp / temp_history.Length);

                        elapsed1 = TimeSpan.Zero;
                    }
                }
                timeCounter.Stop();
            }
        }

        public Double Temperature
        {
            get
            {
                return temperature;
            }
        }
    }
}
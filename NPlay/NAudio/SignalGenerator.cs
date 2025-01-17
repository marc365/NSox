/*
  from the NAudio library by Mark Heath © 2001-2013  
  modified for use in NPlay
 */

using System;

namespace NSox.NAudio
{
    /// <summary>
    /// Signal Generator
    /// Sin, Square, Triangle, SawTooth, White Noise, Pink Noise, Sweep.
    /// </summary>
    /// <remarks>
    /// Posibility to change ISampleProvider
    /// Example :
    /// ---------
    /// WaveOut _waveOutGene = new WaveOut();
    /// WaveGenerator wg = new SignalGenerator();
    /// wg.Type = ...
    /// wg.Frequency = ...
    /// wg ...
    /// _waveOutGene.Init(wg);
    /// _waveOutGene.Play();
    /// </remarks>
    internal class SignalGenerator : ISampleProvider
    {
        // Wave format
        private readonly WaveFormat waveFormat;

        // Random Number for the White Noise & Pink Noise Generator
        private readonly Random random = new Random();

        // Const Math
        private const double TwoPi = 2 * Math.PI;

        // Generator variable
        private int nSample;

        /// <summary>
        /// Initializes a new instance for the Generator (UserDef SampleRate &amp; Channels)
        /// </summary>
        /// <param name="sampleRate">Desired sample rate</param>
        /// <param name="channel">Number of channels</param>
        /// internal SignalGenerator(SignalGeneratorType type, int sampleRate, int channel, float freq, float vol)
        /// //SignalGenerator signal = new SignalGenerator(signalType, Rate, Channels, Frequency, Volume);
        internal SignalGenerator(SignalGeneratorType type, int sampleRate, int channel, float freq, float vol)
        {
            waveFormat = new WaveFormat(sampleRate, 32, channel);

            Type = type;
            Frequency = freq;
            Gain = vol;
            PhaseReverse = new bool[channel];
        }

        /// <summary>
        /// The waveformat of this WaveProvider (same as the source)
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        /// <summary>
        /// Frequency for the Generator. (20.0 - 20000.0 Hz)
        /// Sin, Square, Triangle, SawTooth, Sweep (Start Frequency).
        /// </summary>
        internal double Frequency { get; set; }

        /// <summary>
        /// Gain for the Generator. (0.0 to 1.0)
        /// </summary>
        internal double Gain { get; set; }

        /// <summary>
        /// Channel PhaseReverse
        /// </summary>
        internal bool[] PhaseReverse { get; private set; }

        /// <summary>
        /// Type of Generator.
        /// </summary>
        internal SignalGeneratorType Type { get; set; }

        /// <summary>
        /// Reads from this provider.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            int outIndex = offset;

            // Generator current value
            double multiple;
            double sampleValue;

            // Complete Buffer
            for (int sampleCount = 0; sampleCount < count / waveFormat.Channels; sampleCount++)
            {
                switch (Type)
                {
                    case SignalGeneratorType.Sin:

                        // Sinus Generator
                        multiple = TwoPi * Frequency / waveFormat.SampleRate;
                        sampleValue = Gain * Math.Sin(nSample * multiple);

                        nSample++;
                        break;                    

                    case SignalGeneratorType.White:

                        // White Noise Generator
                        sampleValue = (Gain * NextRandomTwo());
                        break;                    

                    default:
                        sampleValue = 0.0;
                        break;
                }

                // Phase Reverse Per Channel
                for (int i = 0; i < waveFormat.Channels; i++)
                {
                    if (PhaseReverse[i])
                        buffer[outIndex++] = (float)-sampleValue;
                    else
                        buffer[outIndex++] = (float)sampleValue;
                }
            }
            return count;
        }

        /// <summary>
        /// Private :: Random for WhiteNoise &amp; Pink Noise (Value form -1 to 1)
        /// </summary>
        /// <returns>Random value from -1 to +1</returns>
        private double NextRandomTwo()
        {
            return 2 * random.NextDouble() - 1;
        }

    }

    /// <summary>
    /// Signal Generator type
    /// </summary>
    internal enum SignalGeneratorType
    {
        /// <summary>
        /// White noise
        /// </summary>
        White,
        /// <summary>
        /// Sine wave
        /// </summary>
        Sin,
    }

}

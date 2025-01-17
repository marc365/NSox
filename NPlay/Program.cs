/* Created by SharpDevelop.
   User: Marc365
   Date: 05/05/2024
   Time: 18:43
   Update: 19/05/2024
 */

using NSox.NAudio;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace NSox
{
	internal class Program
	{
        private static int c;
        private static int n;
        
        private static Mode Context;
        private static int bufferSize = Def.bufferSize;
        private static Stream con;
        private static WaveFormat waveFormat;
        private static WaveOut waveOut;
        private static SignalGeneratorType signalType;

        #region Callbacks

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (waveOut != null) waveOut.Stop();
        }

        #endregion

        #region Process

        private static void ConvertToByte(byte[] destBuffer, float[] sourceBuffer, int bytesRead)
        {
            for (int n = 0; n < bytesRead; n++) BitConverter.GetBytes(sourceBuffer[n]).CopyTo(destBuffer, n * 4);
        }

        private static void Convert32To16(byte[] destBuffer, byte[] sourceBuffer, int bytesRead)
        {
            for (int n = 0; n < bytesRead; n++) BitConverter.GetBytes((short)(BitConverter.ToSingle(sourceBuffer, n * 4) * Int16.MaxValue)).CopyTo(destBuffer, n * 2);
        }

        private static void Convert32To8(byte[] destBuffer, byte[] sourceBuffer, int bytesRead)
        {
            for (int n = 0; n < bytesRead; n++) destBuffer[n] = Convert.ToByte(((short)(BitConverter.ToSingle(sourceBuffer, n * 4) * 127) + 127));
        }

        #endregion

        private static void Main(string[] args)
        {
            #region Defaults

            Console.CancelKeyPress += OnCancelKeyPress;
            Console.OutputEncoding = Encoding.UTF8;

            int Rate = Def.Rate;
            int Channels = Def.Channels;
            int Bits = Def.Bits;
            float Frequency = Def.Frequency;
            float Volume = Def.Volume;

            #endregion

            #region Arguments
            
            if (args.Length > n) try
            {
                switch (args[n])
                {
                    case Def._H:
                    case Def._Help:
                        Context = Mode.Help;
                        break;
                    case Def._Tone:
                        Context = Mode.Signal;
                        signalType = SignalGeneratorType.Sin;
                        break;
                    case Def._Version:
                        Context = Mode.Information;
                        break;
                    case Def._White:
                        Context = Mode.Signal;
                        signalType = SignalGeneratorType.White;
                        break;
                    default:
                        n--;
                        break;
                }
                for (n++; n < args.Length; n++)
                {
                    string arg = args[n];
                    n++;
                    if (n == args.Length) break;
                    int.TryParse(args[n], out c);
                    switch (arg)
                    {
                        case Def._Buffer:
                            bufferSize = c;
                            break;
                        case Def._Channels:
                            Channels = c;
                            break;
                        case Def._Bits:
                            Bits = c;
                            break;
                        case Def._Rate:
                            Rate = c;
                            break;
                        case Def._Frequency:
                            Frequency = float.Parse(args[n]);
                            break;
                        case Def._Volume:
                            Volume = float.Parse(args[n]);
                            break;
                    }
                }
            }
            catch(Exception exc)
            {
                Help.Fatal(exc);
            }

            #endregion

            #region Set Format

            c = -1;
            waveFormat = new WaveFormat(Rate, Bits, Channels);

            #endregion
                        
            switch (Context)
            {
                #region Help

                case Mode.Help:
                    Help.Show();
                    break;
                case Mode.Information:
                    Help.Version();
                    break;

                #endregion

                #region Play

                case Mode.Play:                    
                    con = Console.OpenStandardInput();
                    
                    byte[] buffer = new byte[bufferSize];
                    BufferedWaveProvider waveProvider = new BufferedWaveProvider(waveFormat);
                    waveOut = new WaveOut()
                    {
                        Volume = Volume
                    };
                    try
                    {
                        waveOut.Init(waveProvider);
                        waveOut.Play();
                    }
                    catch (Exception exc)
                    {
                        Help.Fatal(exc);
                    }
                    while (c != 0)
                    {                        
                        c = con.Read(buffer, 0, buffer.Length);
                        waveProvider.AddSamples(buffer, 0, c);
                        Sleep(waveProvider.BufferedBytes);
                    }
                    break;

                #endregion

                #region Tones

                case Mode.Signal:
                    con = Console.OpenStandardOutput();

                    float[] floatBuffer = new float[bufferSize];
                    byte[] byteBuffer = new byte[bufferSize * 4];
                    SignalGenerator signal = new SignalGenerator(signalType, Rate, Channels, Frequency, Volume);
                    while (c != 0)
                    {
                        c = signal.Read(floatBuffer, 0, bufferSize);
                        ConvertToByte(byteBuffer, floatBuffer, c);
                        switch (Bits)
                        {
                            case 16:
                                Convert32To16(byteBuffer, byteBuffer, c);
                                con.Write(byteBuffer, 0, c * 2);
                                break;
                            case 8:
                                Convert32To8(byteBuffer, byteBuffer, c);
                                con.Write(byteBuffer, 0, c);
                                break;
                            default:
                                con.Write(byteBuffer, 0, c * 4);                                
                                break;
                        }
                        Sleep(c * 4);
                        
                    }
                    break;

                #endregion
            }
		}

        private static void Sleep(int bytes)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds((int)(((double)(bytes) / waveFormat.AverageBytesPerSecond)) * Def.SYNC));
        }
	}
}

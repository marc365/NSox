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
        private static int milli = Def.Latency;
        private static Stream con;
        private static WaveFormat waveFormat;
        private static WaveOut waveOut;
        private static WaveInEvent waveIn;
        private static WasapiLoopbackCapture loopbackIn;
        private static SignalGeneratorType signalType;

        #region Callbacks

        private static void DataAvailable(object sender, WaveInEventArgs e)
        {
            con.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private static void RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.Dispose();
            }
            if (loopbackIn != null)
            {
                loopbackIn.Dispose();
            }
            Help.Fatal(e.Exception);
        }

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

            int Tag = Def.Encoding;
            int Rate = Def.Rate;
            int Channels = Def.Channels;
            int Bits = Def.Bits;
            int Device = Def.Device;
            float Frequency = Def.Frequency;
            float Volume = Def.Volume;

            string fileName = null;

            #endregion

            #region Arguments
                        
            if (args.Length > n) try
            {
                if (File.Exists(args[n]))
                {
                    fileName = args[n];
                    Context = Mode.File;
                }
                else switch (args[n])
                {
                    case Def._Devices:
                        Context = Mode.Devices;
                        break;
                    case Def._Format:
                        Context = Mode.Format;
                        break;
                    case Def._H:
                    case Def._Help:
                        Context = Mode.Help;
                        break;
                    case Def._Mixer:
                        Context = Mode.Mixer;
                        break;
                    case Def._Play:
                        Context = Mode.Play;
                        break;
                    case Def._Record:
                        Context = Mode.Record;
                        break;
                    case Def._SawTooth:
                        Context = Mode.Signal;
                        signalType = SignalGeneratorType.SawTooth;
                        break;
                    case Def._Sine:
                    case Def._Tone:
                        Context = Mode.Signal;
                        signalType = SignalGeneratorType.Sin;
                        break;
                    case Def._Square:
                        Context = Mode.Signal;
                        signalType = SignalGeneratorType.Square;
                        break;
                    case Def._Triangle:
                        Context = Mode.Signal;
                        signalType = SignalGeneratorType.Triangle;
                        break;
                    case Def._Pink:
                        Context = Mode.Signal;
                        signalType = SignalGeneratorType.Pink;
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
                        case Def._Milli:
                            milli = c;
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
                        case Def._Device:
                            Device = c;
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
            //Help.WriteLine("what");           
            switch (Context)
            {
                #region Help

                case Mode.Help:
                    Help.Show();
                    break;
                case Mode.Information:
                    Help.Version();
                    break;
                case Mode.Devices:
                    Help.Devices();
                    break;
                case Mode.Format:
                    loopbackIn = new WasapiLoopbackCapture();
                    Help.WriteLine(loopbackIn.WaveFormat.ToString());
                    break;

                #endregion

                #region Play

                case Mode.Play:
                    con = Console.OpenStandardInput();
                    
                    byte[] buffer = new byte[bufferSize];
                    BufferedWaveProvider waveProvider = new BufferedWaveProvider(waveFormat);
                    Help.WriteLine(Device.ToString());
                    waveOut = new WaveOut()
                    {
                        DeviceNumber = Device,
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

                #region Play File

                case Mode.File:
                    con = Console.OpenStandardOutput();

                    try
                    {
                        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, useAsync: false))
                        {
                            int i;
                            int f;
                            byte[] buf = new byte[4];
                            byte[] readBuffer = new byte[bufferSize];
                            fs.Position = 0;
                            fs.Read(buf, 0, 4);
                            switch (Encoding.ASCII.GetString(buf))
                            {
                                case Def.RIFF:
                                    f = 0;
                                    for (i = 0; i < Def.headSize; i++)
                                    {
                                        char x = (char)fs.ReadByte();
                                        if (x == Def.fmt[f]) f++;
                                        if (f == 4) break;
                                    }
                                    if (i == Def.headSize) goto fail;
                                    fs.Position = fs.Position + 4;
                                    Tag = fs.ReadByte(); fs.Position++;
                                    Channels = fs.ReadByte(); fs.Position++; fs.Read(buf, 0, 4);
                                    Rate = BitConverter.ToInt32(buf, 0); fs.Position = fs.Position + 6; //BitConverter.IsLittleEndian
                                    Bits = fs.ReadByte();
                                    f = 0;
                                    for (i = 0; i < Def.headSize; i++)
                                    {
                                        char x = (char)fs.ReadByte();
                                        if (x == Def.data[f]) f++;
                                        if (f == 4) break;
                                    }
                                    if (i == Def.headSize) goto fail;
                                    fs.Position = fs.Position + 4;
                                    break;
                                default:
                                    Help.WriteLine(Def.FormatNotSupported);
                                    goto fail;
                            }
                            while (c != 0)
                            {
                                c = fs.Read(readBuffer, 0, bufferSize);
                                con.Write(readBuffer, 0, c);
                                Sleep(c * 4);
                            }
                        fail:
                            ;
                        }
                    }
                    catch (Exception exc)
                    {
                        Help.Fatal(exc);
                    }
                    break;

                #endregion

                #region Record

                case Mode.Record:
                    con = Console.OpenStandardOutput();

                    waveIn = new WaveInEvent(waveFormat, milli);
                    waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(DataAvailable);
                    waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(RecordingStopped);

                    try
                    {
                        waveIn.StartRecording();
                    }
                    catch (Exception exc)
                    {

                        Help.Fatal(exc);
                    }
                    while (true)
                    {
                        Thread.Sleep(waveFormat.AverageBytesPerSecond * 10);
                    }

                #endregion

                #region Mixer

                case Mode.Mixer:
                    con = Console.OpenStandardOutput();

                    loopbackIn = new WasapiLoopbackCapture();
                    loopbackIn.DataAvailable += new EventHandler<WaveInEventArgs>(DataAvailable);
                    loopbackIn.RecordingStopped += new EventHandler<StoppedEventArgs>(RecordingStopped);

                    try
                    {
                        loopbackIn.StartRecording();
                    }
                    catch (Exception exc)
                    {
                        Help.Fatal(exc);
                    }
                    while (true)
                    {
                        Sleep(loopbackIn.WaveFormat.AverageBytesPerSecond * 10);
                    }

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

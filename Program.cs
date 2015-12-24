/*
 * Created by SharpDevelop.
 * User: Marc365
 * Date: 10/05/2015
 * Time: 18:04
 * 
 * Update:
 * Date: 18/06/2015
 * Time: 18:04
 */
using BigMansStuff.NAudio.FLAC; //todo not working 'FLAC: could not open single!'?
using NAudio.Codecs;
using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Threading;

namespace NAudio //NSox
{
	class Program
    {

        #region Initialize

        private static int sourceSample;
        private static byte[] sourceBuffer;
        private static WaveBuffer sourceWaveBuffer;
        private static int sourceSamples;
        private static bool Loop = false;
        private static int _milli = 20;
        private static int _buffer = 256;
        private static int _rate = 8000;
        private static int _bits = 8;
        private static int _channels = 1;
        private static int _rate_out = 8000;
        private static int _bits_out = 8;
        private static int _channels_out = 1;
        private static int _bitrate = 8;
        private static int _frequency = 240;
        private static bool IsTimed = false;
        private static bool HasTimer = false;
        private static ManualResetEvent Tick;
        private static NAudio.Wave.WaveFormat frm; //todo private static NAudio.Wave.AdpcmWaveFormat adpcmfrm; //AdpcmWaveFormat(8000, 1) //NAudio.MnException: WaveBadFormat calling waveOutOpen
        private static BufferedWaveProvider bwProvider;
        private static WaveInEvent tx;
        private static WasapiLoopbackCapture mx;
        private static IWavePlayer rx;
        private static G722CodecState _state;
        private static G722Codec _codec = new G722Codec();
        private static IMp3FrameDecompressor decompressor;
        private static LameMP3FileWriter writer;
        private static bool G722Audio = false;
        private static bool Mp3Audio = false;
        private static int count;
        private static int count2;
        private static Stream stdin;
        private static Stream stdout;
        private static TimeSpan x = TimeSpan.FromSeconds((double)1);

        #endregion

		private static void OpenSoundChannel()
		{
            if (tx == null)
            {
                tx = new WaveInEvent()
                {
                    //DeviceNumber = 1,
                    BufferMilliseconds = _milli,
                    WaveFormat = frm
                };
                tx.DataAvailable += new EventHandler<WaveInEventArgs>(SoundChannel_DataAvailable);
                tx.RecordingStopped += new EventHandler<StoppedEventArgs>(SoundChannel_RecordingStopped);
            }

            try
            {
                tx.StartRecording();
            }
            catch (Exception exc)
            {

                Console.Error.WriteLine(exc.ToString());
            }
        }

        private static void OpenMixerChannel()
        {
            //todo frm = new WaveFormat(44100, 32, 2); //probably

            if (mx == null)
            {
                mx = new WasapiLoopbackCapture();
                mx.DataAvailable += new EventHandler<WaveInEventArgs>(SoundChannel_DataAvailable);
                mx.RecordingStopped += new EventHandler<StoppedEventArgs>(SoundChannel_RecordingStopped);
            }

            try
            {
                mx.StartRecording();
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.ToString());
            }
        }
   
        public static void SoundChannel_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (IsTimed)
            {
                Tick.Set();
            }
            else if (mx != null)
            {
                byte[] to16 = new byte[e.BytesRecorded / 2];
                int destOffset = 0;
                try
                {
                    //todo Convert32To16 crashes here reporting 'corrupt memory' my own ConvertIeeeTo16 gives no output?
                    WaveBuffer sourceWaveBuffer = new WaveBuffer(e.Buffer);
                    WaveBuffer destWaveBuffer = new WaveBuffer(to16);
                    int sourceSamples = e.BytesRecorded / 4;
                    
                    for (int sample = 0; sample < sourceSamples; sample++)
                    {
                        // adjust volume
                        float sample32 = sourceWaveBuffer.FloatBuffer[sample];
                        // clip
                        if (sample32 > 1.0f)
                            sample32 = 1.0f;
                        if (sample32 < -1.0f)
                            sample32 = -1.0f;
                        destWaveBuffer.ShortBuffer[destOffset++] = (short)(sample32 * 32767);
                    }
                }
                catch
                {
                }
                if (G722Audio)
                {
                    //todo resample to 4000Hz
                    if (_state == null)
                    {
                        _state = new G722CodecState(_bitrate, G722Flags.Packed);
                    }
                    var wb = new byte[e.BytesRecorded];
                    //todo is this the way to use the codec?
                    count = _codec.Encode(_state, wb, ConvertToShort(to16, to16.Length), to16.Length / 2);
                    stdout.Write(wb, 0, count);
                }
                else if (Mp3Audio)
                {
                    if (writer == null)
                    {
                        writer = new LameMP3FileWriter(stdout, frm, _bitrate);
                    }

                    if (writer != null)
                    {
                        writer.Write(to16, 0, destOffset * 2);
                    }

                }
                else
                {
                    //todo convert format
                    stdout.Write(to16, 0, destOffset * 2);
                }
            }
            else if (tx != null)
            {
                if (G722Audio)
                {
                    if (_state == null)
                    {
                        _state = new G722CodecState(_bitrate, G722Flags.Packed);
                    }
                    var wb = new byte[e.BytesRecorded];
                    //todo is this the way to use the codec?
                    count = _codec.Encode(_state, wb, ConvertToShort(e.Buffer, e.BytesRecorded), e.BytesRecorded / 2);
                    stdout.Write(wb, 0, count);
                }
                else if (Mp3Audio)
                {
                    if (writer == null)
                    {
                        writer = new LameMP3FileWriter(stdout, frm, _bitrate);
                    }

                    if (writer != null)
                    {
                        writer.Write(e.Buffer, 0, e.BytesRecorded);
                    }

                }
                else
                {
                    stdout.Write(e.Buffer, 0, e.BytesRecorded);
                }
            }
            
        }

        private static void SoundChannel_RecordingStopped(object sender, StoppedEventArgs e)
        {	
            if (tx != null)
            {
                tx.Dispose();
            }
            if (mx != null)
            {
                mx.Dispose();
            }
            Console.Error.WriteLine(e.Exception);
            System.Environment.Exit(0);
        }

        public static float[] ConvertToFloat(byte[] input, int count)
        {
            float[] output = new float[count / 4];
            for (int n = 0; n < output.Length; n++)
            {
                output[n] = BitConverter.ToInt32(input, n * 4);
            }
            return output;
        }

        public static short[] ConvertToShort(byte[] input, int count)
        {
            short[] output = new short[count / 2];
            for (int n = 0; n < output.Length; n++)
            {
                output[n] = BitConverter.ToInt16(input, n * 2);
            }
            return output;
        }

        public static short[] ConvertToShort(float[] input, int count)
        {
            short[] output = new short[count];
            for (int n = 0; n < output.Length; n++)
            {
                output[n] = (short)input[n];
            }
            return output;
        }

        public static byte[] ConvertToByte(short[] input, int count)
        {
            byte[] output = new byte[count * 2];
            for (int n = 0; n < count; n++)
            {
                BitConverter.GetBytes(input[n]).CopyTo(output, n * 2);
            }
            return output;
        }

        public static byte[] ConvertToByte(float[] input, int count)
        {
            byte[] output = new byte[count * 4];
            for (int n = 0; n < count; n++)
            {
                BitConverter.GetBytes(input[n]).CopyTo(output, n * 4);
            }
            return output;
        }

        public static unsafe void Convert32To16(byte[] destBuffer, byte[] sourceBuffer, int bytesRead)
        {
            fixed (byte* pDestBuffer = &destBuffer[0],
                pSourceBuffer = &sourceBuffer[0])
            {
                short* psDestBuffer = (short*)pDestBuffer;
                float* pfSourceBuffer = (float*)pSourceBuffer;
                for (int n = 0; n < bytesRead; n++)
                {
                    psDestBuffer[n] = (short)(pfSourceBuffer[n] * 32767);
                }
            }
        }

        //todo what's wrong here, there is no output?
        //public static void ConvertIeeeTo16(byte[] destBuffer, byte[] sourceBuffer, int bytesRead)
        //{
        //    Console.Error.WriteLine(destBuffer.Length + " " + sourceBuffer.Length + "  " + bytesRead);
        //    for (int n = 0; n < bytesRead / 2; n++)
        //    { 
        //        BitConverter.GetBytes((float)sourceBuffer[n] * 32767).CopyTo(destBuffer, n * 2);
        //    }
        //}

        public static void Convert16To8(byte[] destBuffer, byte[] input)
        {
            //todo use similar method to Convert32To16
            for (int n = 0; n < destBuffer.Length; n++)
            {
                short sample = BitConverter.ToInt16(input, n * 2);
                int s = (int)(sample);
                s = s + 32767;
                destBuffer[n] = Convert.ToByte(s / 256);
            }
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        public static WaveStream SetReader(WaveStream reader, string[] args)
        {
            if (args[0].EndsWith(".mp3"))
            {
                reader = new Mp3FileReader(args[0]);
            }
            else if (args[0].EndsWith(".wav"))
            {
                reader = new WaveFileReader(args[0]);
            }
            else if (args[0].EndsWith(".aif") || args[0].EndsWith(".aiff"))
            {
                reader = new AiffFileReader(args[0]);
            }
            else if (args[0].EndsWith(".flac")) //todo not working
            {
                reader = new FLACFileReader(args[0]);
            }
            else
            {
                Console.Error.WriteLine("Unknown format");
                System.Environment.Exit(-1);
            }

            return reader;
        }

		public static void Main(string[] args = null)
        {
            #region Help

            if (args.Length == 0 || args[0] == "-h" || args[0] == "-help")
            {
                Help.Run();
                System.Environment.Exit(0);
            }
            else if (args[0] == "-drivers")
            {
                Help.Drivers();
                System.Environment.Exit(0);
            }

            #endregion

            #region Audio Modes

            //G722
            foreach (string arg in args)
            {
                if (arg == "-g722")
                {
                    _bits = 16;
                    //_milli = 10;
                    _rate = 4000;
                    _buffer = 2048;
                    _bitrate = 56000;
                    G722Audio = true;
                }
            }

            //MP3
            foreach (string arg in args)
            {
                if (arg == "-mp3" || arg == "-mpg")
                {
                    _bits = 16;
                    _milli = 3000; //todo for different rates
                    Mp3Audio = true;    
                }
            }

            #endregion

            #region Read Arguments

            //Timer
            foreach (string arg in args)
            {
                if (arg == "-t")
                {
                    HasTimer = true;
                }
            }
            //Loop
            foreach (string arg in args)
            {
                if (arg == "-l")
                {
                    Loop = true;
                }
            }
            //Milliseconds
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-ms")
                {
                    _milli = int.Parse(args[i + 1]);
                }
            }
            //Bitrate
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-br")
                {
                    _bitrate = int.Parse(args[i + 1]);
                }
                if (G722Audio && _bitrate != 48000 && _bitrate != 56000 && _bitrate != 64000)
                {
                    _bitrate = 56000;
                }
            }
            //Channels
            bool Out = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-c")
                {
                    if (!Out)
                    {
                        Out = true;
                        _channels = int.Parse(args[i + 1]);
                    }
                    else
                    {
                        _channels_out = int.Parse(args[i + 1]); //todo convert
                    }
                }
            }
            //Bits
            Out = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-b" && !G722Audio && !Mp3Audio)
                {
                    if (!Out)
                    {
                        Out = true;
                        _bits = int.Parse(args[i + 1]);
                    }
                    else
                    {
                        _bits_out = int.Parse(args[i + 1]); //todo convert
                    }
                }
            }
            //Rate
            Out = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-r")
                {
                    if (!Out)
                    {
                        Out = true;
                        _rate = int.Parse(args[i + 1]);
                    }
                    else
                    {
                        _rate_out = int.Parse(args[i + 1]); //todo convert
                    }
                }
            }            
            #region Buffer defaults
            //Signal Generator defaults
            foreach (string arg in args)
            {
                if (arg == "-tone" || arg == "-pink")
                {
                    _buffer = -1;
                }
            }
            if (_buffer == -1)
            {
                _buffer = 128;
            }
            else if (_rate > 8000 && G722Audio && !Mp3Audio) //G722
            {
                _buffer = 2048;
            }
            else if (_rate > 8000 && !G722Audio && !Mp3Audio) //Raw
            {
                _buffer = 2048;
            }
            else if (Mp3Audio && _bitrate == 8) //Mp3
            {
                _buffer = 2048 + 1024;
            }
            else if (Mp3Audio)
            {
                _buffer = 16384 * 6;
            }
            #endregion
            //Buffers
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--buffer")
                {
                    _buffer = int.Parse(args[i + 1]);
                }
            }
            //Frequency
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-f")
                {
                    _frequency = int.Parse(args[i + 1]);
                    if (_frequency < 1 || _frequency > 20000)
                    {
                        Console.Error.WriteLine("Frequency out of bounds");
                        _frequency = 440;
                    }
                }
            }
            #endregion

            #region Set Format

            frm = new WaveFormat(_rate, _bits, _channels);

            #endregion

            #region Play

            if (args[0].ToLower() == "-play")
            {

                stdin = System.Console.OpenStandardInput();
                byte[] buffer;
                count = -1;
                x = TimeSpan.FromSeconds((double)2);
                #region Stream Encoder

                if (Mp3Audio)
                {
                    try
                    {
                        buffer = new byte[_buffer];
                        using (var stdinStream = stdin)
                        {
                            var readFullyStream = new ReadFullyStream(stdinStream);

                            Mp3Frame frame;

                            while (true)
                            { 
                                try
                                {
                                    frame = Mp3Frame.LoadFromStream(readFullyStream);
                                }
                                catch (EndOfStreamException)
                                {
                                    break;
                                }
                                catch (Exception)
                                {
                                    break;
                                }
                                if (decompressor == null)
                                {
                                    decompressor = CreateFrameDecompressor(frame);
                                    #region Start Audio Device

                                    if (bwProvider == null)
                                    {
                                        bwProvider = new BufferedWaveProvider(decompressor.OutputFormat)
                                        {
                                            BufferDuration = TimeSpan.FromSeconds(4),
                                            DiscardOnBufferOverflow = true
                                        };

                                        rx = new WaveOut();

                                        new Thread(() =>
                                        {
                                            rx.Init(bwProvider);
                                            Thread.Sleep(5000);//todo for different rates
                                            rx.Play();
                                        }).Start();
                                    }

                                    #endregion
                                }
                                count = decompressor.DecompressFrame(frame, buffer, 0);
                                if (count != 0)
                                {
                                    bwProvider.AddSamples(buffer, 0, count);
                                }
                                if (HasTimer && bwProvider.BufferedDuration > x)
                                {
                                    Thread.Sleep(1750);
                                }
                            }
                        }
                    }
                    catch
                    {
                        rx.Stop();
                        rx.Dispose();
                    }
                    finally
                    {
                    }
                }

                #endregion
                #region Block Encoder

                else
                {
                    buffer = new byte[_buffer];
                    if (G722Audio)
                    {
                        if (_state == null)
                        {
                            _state = new G722CodecState(_bitrate, G722Flags.Packed);
                        }
                    }
                    while (count != 0)
                    {
                        count = stdin.Read(buffer, 0, buffer.Length);
                        #region Start Start Audio Device

                        if (bwProvider == null)
                        {
                            bwProvider = new BufferedWaveProvider(frm);
                            //if (G722Audio)
                            //{
                            //    bwProvider.BufferLength = buffer.Length * 8; // _buffer = _milli * frm.AverageBytesPerSecond / 1000;
                            //    //bwProvider.BufferDuration = TimeSpan.FromSeconds(4);
                            //}
                            //else
                            //{
                            //    bwProvider.BufferDuration = TimeSpan.FromSeconds(4);
                            //}
                            bwProvider.BufferDuration = TimeSpan.FromSeconds(4);
                            bwProvider.DiscardOnBufferOverflow = true;

                            rx = new WaveOut();
                            new Thread(() =>
                            {
                                rx.Init(bwProvider);
                                //Thread.Sleep(500);
                                rx.Play();
                            }).Start();

                        }

                        #endregion
                        if (rx != null)
                        {
                            if (G722Audio)
                            {
                                var wb = new short[count * 4];
                                int decoded = _codec.Decode(_state, wb, buffer, count);
                                bwProvider.AddSamples(ConvertToByte(wb, decoded), 0, decoded * 2);
                            }
                            else //Raw Audio
                            {
                                bwProvider.AddSamples(buffer, 0, count);
                            }
                            if (HasTimer && bwProvider.BufferedDuration > x)
                            {
                                Thread.Sleep(1750);
                            }
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region Play Files

            else if (System.IO.File.Exists(args[0]))
            {
                WaveStream reader = null;
                reader = SetReader(reader, args);
                if (HasTimer)
                {
                    Tick = new ManualResetEvent(false);
                }
                IsTimed = true;
                stdout = System.Console.OpenStandardOutput();
                count = -1;
                count2 = 0;
                bool FirstPacket = false;
                byte[] buffer = new byte[_milli * frm.AverageBytesPerSecond / 1000];
                if (HasTimer)
                {
                    OpenSoundChannel();
                }
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    if (G722Audio)
                    {
                        if (_state == null)
                        {
                            _state = new G722CodecState(_bitrate, G722Flags.Packed);
                        }
                        using (WaveStream rawStream = new WaveFormatConversionStream(frm, pcmStream))
                        {
                            while (count != 0 || Loop)
                            {
                                count = rawStream.Read(buffer, 0, buffer.Length);
                                if (Loop && count != buffer.Length)
                                {
                                    reader = SetReader(reader, args);
                                    rawStream.Position = 0;
                                    count2 = rawStream.Read(buffer, count, buffer.Length - count);
                                    count += count2;
                                }
                                var wb = new byte[count];
                                //todo is this the way to use the codec?
                                count = _codec.Encode(_state, wb, ConvertToShort(buffer, count), (count / 4) * 2); //dont have odd numbers
                                stdout.Write(wb, 0, count);
                                if (HasTimer && !FirstPacket)
                                {
                                    Tick.WaitOne();
                                    Tick.Reset();
                                }
                                if (FirstPacket)
                                {
                                    Tick.Reset();
                                    FirstPacket = false;
                                }
                            }
                        }
                    }
                    else if (Mp3Audio)
                    {
                        using (WaveStream rawStream = new WaveFormatConversionStream(frm, pcmStream))
                        {
                            if (writer == null)
                            {
                                writer = new LameMP3FileWriter(stdout, frm, _bitrate);
                            }
                            while (count != 0 || Loop)
                            {
                                count = rawStream.Read(buffer, 0, buffer.Length);
                                if (Loop && count != buffer.Length)
                                {
                                    reader = SetReader(reader, args);
                                    rawStream.Position = 0;
                                    count2 = rawStream.Read(buffer, count, buffer.Length - count);
                                    count += count2;
                                }
                                writer.Write(buffer, 0, count);
                                if (HasTimer && !FirstPacket)
                                {
                                    Tick.WaitOne();
                                    Tick.Reset();
                                }
                                if (FirstPacket)
                                {
                                    Tick.Reset();
                                    FirstPacket = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        using (WaveStream rawStream = new WaveFormatConversionStream(frm, pcmStream))
                        {
                            while (count != 0 || Loop)
                            {
                                count = rawStream.Read(buffer, 0, buffer.Length);
                                if (Loop && count != buffer.Length)
                                {
                                    reader = SetReader(reader, args);
                                    rawStream.Position = 0;
                                    count2 = rawStream.Read(buffer, count, buffer.Length - count);
                                    count += count2;
                                }
                                stdout.Write(buffer, 0, count);
                                if (HasTimer && !FirstPacket)
                                {
                                    Tick.WaitOne();
                                    Tick.Reset();
                                }
                                if (FirstPacket)
                                {
                                    Tick.Reset();
                                    FirstPacket = false;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Record

            else if (args[0].ToLower() == "-rec")
            {
                stdout = System.Console.OpenStandardOutput();
                OpenSoundChannel();
                while (true)
                {
                    Thread.Sleep(8000);
                }
            }

            #endregion

            #region Record File

            //todo filename for output, use  nsox ... > outfilname.mp3 as output is always stout
            //todo headers

            #endregion

            #region Mixer

            else if (args[0].ToLower() == "-mixer")
            {
                stdout = System.Console.OpenStandardOutput();
                OpenMixerChannel();
                while (true)
                {
                    Thread.Sleep(8000);
                }
            }

            #endregion

            #region Convert

            else if (args[0].ToLower() == "-convert")
            {
                IsTimed = true;
                if (Mp3Audio) { _milli = _milli * 3; }
                if (G722Audio) { _milli = _milli * 10; }
                _buffer = _milli * frm.AverageBytesPerSecond / 1000;
                Console.Error.WriteLine(_buffer);
                stdout = System.Console.OpenStandardOutput();
                if (HasTimer)
                {
                    Tick = new ManualResetEvent(false);
                    OpenSoundChannel();
                }
                stdin = System.Console.OpenStandardInput();

                byte[] buffer = new byte[_buffer];
                count = -1;
                if (G722Audio)
                {
                    if (_state == null)
                    {
                        _state = new G722CodecState(_bitrate, G722Flags.Packed);
                    }
                    while (count != 0)
                    {
                        //todo put this through a WaveBuffer
                        count = stdin.Read(buffer, 0, buffer.Length);
                        if (count > 0)
                        {
                            var wb = new byte[buffer.Length * 2];
                            //todo is this the way to use the codec?
                            count = _codec.Encode(_state, wb, ConvertToShort(buffer, count), count / 2);
                            stdout.Write(wb, 0, count);
                            if (HasTimer) //32To16
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                        else
                        {
                            Thread.Sleep(20);
                        }
                        //mono
                        Thread.Sleep(20);
                    }
                }
                else if (Mp3Audio)
                {
                    if (writer == null)
                    {
                        writer = new LameMP3FileWriter(stdout, frm, _bitrate);
                    }
                    if (writer != null)
                    {
                        while (count != 0)
                        {
                            //todo put this through a WaveBuffer
                            count = stdin.Read(buffer, 0, buffer.Length);
                            writer.Write(buffer, 0, count);
                            if (HasTimer)
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                    }
                }
                else
                {
                    while (count != 0)
                    {
                        count = stdin.Read(buffer, 0, buffer.Length);
                        if (_bits == 16)
                        {
                            //todo put this through a WaveBuffer
                            stdout.Write(buffer, 0, count);
                            if (HasTimer)
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                        else if (_bits == 8)
                        {
                            stdout.Write(buffer, 0, count);
                            if (HasTimer)
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                        else
                        {
                            stdout.Write(buffer, 0, count);
                            if (HasTimer)
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                    }
                }
            }
            #endregion

            #region Tones

            else if (args[0].ToLower() == "-tone" || args[0].ToLower() == "-pink")
            {
                IsTimed = true;
                if (Mp3Audio) { _milli = _milli * 3; }
                if (G722Audio) { _milli = _milli * 10; }
                _buffer = _milli * frm.AverageBytesPerSecond / 1000;
                Console.Error.WriteLine(_buffer);
                stdout = System.Console.OpenStandardOutput();
                if (HasTimer)
                {
                    Tick = new ManualResetEvent(false);
                    OpenSoundChannel();
                }
                var signal = new SignalGenerator(_rate, _channels);
                if (args[0].ToLower() == "-tone")
                {
                    signal.Type = SignalGeneratorType.Sin;
                }
                else if (args[0].ToLower() == "-pink")
                {
                    signal.Type = SignalGeneratorType.Pink;
                }
                signal.Frequency = _frequency;
                signal.Gain = 0.12;
                float[] buffer = new float[_buffer];
                byte[] buffer_16 = new byte[_buffer * 2];
                byte[] buffer_8 = new byte[_buffer];
                byte[] buffer_empty = new byte[_buffer * 2];
                count = -1;
                if (G722Audio)
                {
                    if (_state == null)
                    {
                        _state = new G722CodecState(_bitrate, G722Flags.Packed);
                    }
                    while (count != 0)
                    {
                        //todo put this through a WaveBuffer
                        count = signal.Read(buffer, 0, buffer.Length);
                        Convert32To16(buffer_16, ConvertToByte(buffer, buffer.Length), buffer.Length);
                        var wb = new byte[buffer.Length * 2];
                        //todo is this the way to use the codec?
                        count = _codec.Encode(_state, wb, ConvertToShort(buffer_16, buffer_16.Length), buffer.Length);
                        stdout.Write(wb, 0, count);
                        if (HasTimer) //32To16
                        {
                            Tick.WaitOne();
                            Tick.Reset();
                            Tick.WaitOne();
                            Tick.Reset();
                        }
                    }
                }
                else if (Mp3Audio)
                {
                    if (writer == null)
                    {
                        writer = new LameMP3FileWriter(stdout, frm, _bitrate);
                    }
                    if (writer != null)
                    {
                        while (count != 0)
                        {
                            //todo put this through a WaveBuffer
                            count = signal.Read(buffer, 0, buffer.Length);
                            Convert32To16(buffer_16, ConvertToByte(buffer, buffer.Length), buffer.Length);
                            writer.Write(buffer_16, 0, count * 2);
                            if (HasTimer) //32To16
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                    }
                }
                else
                {
                    while (count != 0)
                    {
                        count = signal.Read(buffer, 0, buffer.Length);
                        if (_bits == 16)
                        {
                            //todo put this through a WaveBuffer
                            Convert32To16(buffer_16, ConvertToByte(buffer, buffer.Length), buffer.Length);
                            stdout.Write(buffer_16, 0, count * 2);
                            if (HasTimer) //32To16
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                        else if (_bits == 8)
                        {
                            //todo put this through a WaveBuffer
                            Convert32To16(buffer_16, ConvertToByte(buffer, buffer.Length), buffer.Length);
                            Convert16To8(buffer_8, buffer_16);
                            stdout.Write(buffer_8, 0, count);
                            if (HasTimer)
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                        else
                        {
                            stdout.Write(ConvertToByte(buffer, count), 0, count * 4);
                            if (HasTimer)
                            {
                                Tick.WaitOne();
                                Tick.Reset();
                            }
                        }
                    }
                }
            }
            #endregion

            #region Help

            else
            {
                Help.Run();
            }

            #endregion

        }
	}
}
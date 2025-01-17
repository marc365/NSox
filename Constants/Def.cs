/* Created by SharpDevelop.
   User: Marc365
   Date: 05/05/2024
   Time: 18:43
   Update: 19/05/2024
 */

public static class Def
{
    public const int BUFFERS = 3;
    public const int SECONDS = 2;
    public const int SYNC = 900;

    public const int Encoding = 3;
    public const int Rate = 44100;
    public const int Channels = 2;
    public const int Bits = 32;
    public const int Device = 0;
    public const int Frequency = 440;
    public const float Volume = 1.0F;
    public const int bufferSize = 256;

    public const string NSox_Copyright = "NSox 2.0 Copyright © 2016-2024 Marc Williams";
    public const string NSox_Help = @"
SoX 'like' command usage. Additional NAudio classes by Mark Heath © 2001-2013 and CoreAudioApi classes by Ray Molenkamp © 2007.
Pipe input/output to/from external programs (where supported).

Usage:
    nsox --help -h              show this text
    nsox --version              display version number and exit
    nsox --devices              show available devices and capabilities
    nsox --format               show the loopback mixer format
    nsox -play                  play from stdin
    nsox -file                  play a wav file to stdout
    nsox -rec                   audio in record to stdout
    nsox -mixer                 loopback record to stdout    
    nsox -tone                  play a tone to stdout; -saw, -sin -squ -tri
    nsox -pink                  play pink noise to stdout
    nsox -white                 play white noise to stdout
Options:    
    --buffer                    set the buffer size
    --milli                     set the record milliseconds buffer size
    -d                          input/output device selection
    -c                          number of channels of audio data
    -b                          encoded sample size in bits
    -r                          sample rate Hz of audio
    -f                          tone generator frequency (20.0 - 20000.0 Hz)    
    -v                          set the volume (0.0 - 1.0)  
Tests:
    nsox -tone | nsox -play
    nsox -squ -f 432 -v 0.05 -c 2 -b 32 -r 96000 | nsox -play -c 2 -b 32 -r 96000
    nsox -white -v 0.0005 -c 2 -b 16 -r 8000 | nsox -play -c 2 -b 16 -r 8000
    nsox -white -c 1 -b 8 -r 64 > random.raw
    nsox -mixer | nsox -play -v 0.75 -c 2 -b 32 -r 48000
    nsox -pink | nsox -play
    nsox -rec | nsox -play    
Linux:
    mono NSox.exe -tone | play -b 32 -r 44100 -c 2 -e floating-point -t f32 -";

    public const string NPlay_Copyright = "NPlay from " + NSox_Copyright;
    public const string NPlay_Help = @"
SoX 'like' command usage. Additional NAudio classes by Mark Heath © 2001-2013.
Pipe input/output to/from external programs (where supported).

Usage:
    nplay --help -h             show this text
    nplay --version             display version number and exit
    nplay                       play from stdin
    nplay -tone                 play a tone to stdout
    nplay -white                play white noise to stdout
Options:
    --buffer                    set the buffer size
    -c                          number of channels of audio data
    -b                          encoded sample size in bits
    -r                          sample rate Hz of audio
    -f                          tone generator frequency Hz (20.0 - 20000.0 Hz)
    -v                          set the volume (0.0 - 1.0)
Tests:
    nplay -tone | nplay
    nplay -tone -f 432 -v 0.05 -c 2 -b 32 -r 96000 | nplay -c 2 -b 32 -r 96000
    nplay -white -v 0.0005 -c 2 -b 16 -r 8000 | nplay -c 2 -b 16 -r 8000
    nplay -white -c 1 -b 8 -r 64 > random.raw";

    public const string Space = " ";

    public const string _Bits = "-b";
    public const string _Buffer = "--buffer";
    public const string _Channels = "-c";
    public const string _Device = "-d";
    public const string _Devices = "--devices";
    public const string _Format = "--format";
    public const string _Frequency = "-f";
    public const string _H = "-h";
    public const string _Help = "--help";
    public const string _Milli = "--milli";
    public const string _Mixer = "-mixer";    
    public const string _Pink = "-pink";
    public const string _Play = "-play";
    public const string _Rate = "-r";
    public const string _Record = "-rec";
    public const string _SawTooth = "-saw";
    public const string _Sine = "-sin";
    public const string _Square = "-squ";
    public const string _Tone = "-tone";
    public const string _Triangle = "-tri";
    public const string _Volume = "-v";
    public const string _Version = "--version";
    public const string _White = "-white";

    public const string RIFF = "RIFF";
    public const int headSize = 64;
    public const string fmt = "fmt ";
    public const string data = "data";
    public const int Latency = 300;

    public const string WinMM = "winmm.dll";
    public const string Id = "Id: "; 
    public const string ChannelsSupported = " channel";
    public const string FormatNotSupported = "Format not supported";
    public const string SupportsPlaybackRateControl = "Playback rate control supported";
    public const string BufferFull = "Buffer full";
    public const string ErrorCalling = "{0} calling {1}";
    public const string CannotBeZero = "Handle cannot be zero";
    public const string NumberOfChannels = "Channels must be 1 or greater";
    public const string WaveOutOpen = "waveOutOpen";
    public const string WaveOutReset = "waveOutReset";
    public const string WaveOutSetVolume = "waveOutSetVolume";
    public const string WaveOutNotClosed = "WaveOut device was not closed";
    public const string WaveOutPrepare = "waveOutPrepareHeader";
    public const string WaveOutWrite = "waveOutWrite";
    public const string BuffersAlreadyQueued = "Buffers already queued on play";
    public const string VolumeRange = "Volume must be between 0.0 and 1.0";
    public const string WaveBufferNotDisposed = "WaveBuffer was not disposed";
}
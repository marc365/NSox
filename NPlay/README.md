Project Description:
--------------------

Version 2.0 has all features removed other than; play and test signal generation. It is a rewrite influenced by the original but with a tiny 32k foot-print and single-thread operation.
The cut down version of the NAudio library is from work by Mark Heath (https://github.com/naudio/NAudio).
The console app connects the stdin and stdout to the input output streams, generating test data and outputing audio through the default audio device on Windows.
This is not a complete implementation of SoX, it is command line compatible for use in multi-platform applications in tandem with SoX on Linux.

"nsox -tone -v 0.5 | nsox -play" - this command will play a test signal at half volume, stream through stdout, pipe it to a second nsox stdin and play.

C# Example:
-----------

ProcessStartInfo play = new ProcessStartInfo
{
    UseShellExecute = false,
    RedirectStandardInput = true,
    RedirectStandardOutput = false,
    RedirectStandardError = false,
    CreateNoWindow = true
};
MemoryStream Buffer = [data];
private async Task Play(int start, int end, int loop)
{
    if (MultiPlatform.Id == MultiPlatform.Platform.Windows)
    {
        play.FileName = "nsox";
        play.Arguments = "-play ";
    }
    else play.FileName = "play"; //sox
    play.Arguments += "--buffer 4096 -q -c " + channels + " -b " + bits + " -r " + rate;
    if (media.bits == 32) play.Arguments += " -e floating-point -t f32 -";
    else if (media.bits == 8) play.Arguments += " -e unsigned -t raw -";
    else play.Arguments += " -e signed -t raw -";
    try
    {
        using (Process sox = await Task.Run(() => Process.Start(play)))
        {
            int b = BUFFER;
            byte[] buffer = new byte[b];
            long l;
            int i;
            loop:
            Buffer.Position = Align(start, bytes);
            l = Align(end - start, bytes);
            lc = l;
            i = 0;
            while (l > 0)
            {
                if (b > l) b = (int) l;
                i = await media.Buffer.ReadAsync(buffer, 0, b);
                if (i > 0)
                {
                    await sox.StandardInput.BaseStream.WriteAsync(buffer, 0, i);
                    l -= i;
                }
                else break;
            }
            await sox.StandardInput.FlushAsync();
            if (loop) { b = BUFFER; goto loop; }
        }
    }
    catch (Exception exc)
    {
        //Audio Error
    }
}
private long Align(long @ref, int bytes)
{
    @ref = @ref / bytes;
    return @ref * bytes;
}

NSox 2.0 Copyright (c) Marc 2016-2024

SoX 'like' command usage. Additional NAudio classes by Mark Heath (c) 2001-2013.
Pipe input/output to/from external programs (where supported).

Usage:
    nsox -help (-h)             show this text    
    nsox -play                  play from stdin
    nsox -tone                  play a tone to stdout; -saw, -sin -squ -swe -tri
    nsox -pink                  play pink noise to stdout
    nsox -white                 play white noise to stdout
    nsox --version              display version number of NSox and exit 
Options:    
    --buffer                    set the buffer size
    -c                          number of channels of audio data; e.g. 2 = stereo
    -b                          encoded sample size in bits
    -r                          sample rate Hz of audio
    -f                          tone generator frequency Hz ( > 0)    
    -v                          tone generator volume (real number)  
Tests:
    nsox -tone | nsox -play
    nsox -tone -f 432 -v 0.5 -c 2 -b 32 -r 96000 | nsox -play -c 2 -b 32 -r 96000
    nsox -white -v 0.25 -c 1 -b 8 -r 44100 | nsox -play -c 1 -b 8 -r 44100
    nsox -white -c 1 -b 8 -r 64 > recording.raw
    nsox -pink -v 0.01 -c 1 -b 8 -r 4000 | nsox -play -c 1 -b 8 -r 4000
	nsox -pink | nsox -play

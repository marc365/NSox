Project Description:

I have embedded a cut down version of the NAudio library by Mark Heath (https://github.com/naudio/NAudio) into the console app and connected the input output streams to the stdin and stdout through all sorts of processes to convert and encode, the main program code is in Program.cs - it is designed to take the output of other programs (running as a background process) and is good for streaming audio over networks, using it this way myself. The '-mixer' option is my favourite - it gives you all the audio being played by your machine, so a live broadcast directly from your desktop is possible, mixing everything including any microphone inputs.
 
"NSox.exe -rec | NSox.exe -play" - this will record and stream through stdout, then pipe it into stdin and play.

C# Example:
 
byte[] buffer = new byte[812];
int totalbytes;
string mode;
System.Net.Sockets.Socket clientSocket;
//...
//network stuff
System.Diagnostics.Process ChannelProcess = new System.Diagnostics.Process ();
System.Diagnostics.ProcessStartInfo ChannelInfo = new System.Diagnostics.ProcessStartInfo ();
ChannelInfo.FileName = "nsox";
ChannelInfo.Arguments = string.Format ("{0} -c 2 -b 32 -r 192000", mode);
//or ChannelInfo.Arguments = string.Format("{0} -c 1 -r 22050 -br 64 -mp3", mode);
ChannelInfo.UseShellExecute = false;
ChannelInfo.RedirectStandardInput = true;
ChannelInfo.RedirectStandardOutput = true;
ChannelInfo.RedirectStandardError = true;
ChannelProcess.StartInfo = ChannelInfo;  
ChannelProcess.Start ();
while (totalbytes > 0 && !ChannelProcess.HasExited && clientSocket.Connected) 
{
	totalbytes = ChannelProcess.StandardOutput.BaseStream.Read (buffer, 0, buffer.Length);
	//...
	//build packet from 'buffer' and send tcp/udp

Usage:

     NSox.exe -rec | NSox.exe -play (this will record and stream through stdout, then pipe it into stdin and play)
     NSox help (windows)
     mono NSox.exe help (linux)
     NSox -mixer | NSox -play -c 2 -b 32 -r 44100
     NSox -rec -mp3 -c 2 -b 16 -r 44100 -br 128 > recording.mp3
     NSox -rec -g722 | NSox -play -g722
     NSox -pink
 
Notes:
 
 MP3, G722 encoding, conversion etc. sox 'like' command-line usage, full functionality under windows, partial functionality under mono in linux e.g. arecord -q -D hw -c 2 -f S16_LE -r 44100 | sox -q --buffer 17 -c 2 -b 16 -r 44100 -e signed -t raw - -c 1 -r 4000 -t raw - | mono NSox.exe -convert -g722 -br 48000

# nsox

Project Description:

This uses the mighty NAudio library by Mark Heath - I have embedded this into the console app so there are no dll dependencies, my code is in Program.cs
Usage:

run in a command shell "NSox.exe -rec | NSox.exe -play" this will record and stream through stdout, then pipe it into stdin and play.
Tricks:

    NSox help (windows)
    mono NSox.exe help (linux)
    NSox -mixer | NSox -play -c 2 -b 32 -r 44100
    NSox -rec -mp3 -c 2 -b 16 -r 44100 -br 128 > recording.mp3
    NSox -rec -g722 | NSox -play -g722

Notes:

MP3, G722 encoding, conversion etc. near full sox (linux) command-line compatibility, full functionality under windows, partial functionality under mono in linux e.g. arecord -q -D hw -c 2 -f S16_LE -r 44100 | sox -q --buffer 17 -c 2 -b 16 -r 44100 -e signed -t raw - -c 1 -r 4000 -t raw - | mono NSox.exe -convert -g722 -br 48000

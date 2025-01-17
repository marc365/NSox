NSox 2.0 Copyright © 2016-2024 Marc Williams

SoX 'like' command usage. Additional NAudio classes by Mark Heath © 2001-2013 and CoreAudioApi classes by Ray Molenkamp © 2007.
Pipe input/output to/from external programs (where supported).

"nsox -tone -v 0.5 | nsox -play" - This command will play a test signal at half volume, stream through stdout, pipe it to a second nsox stdin and play. 
Alternatively "nsox -tone -v 0.5 | nplay" does the same.

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
    nsox -squ -v 0.05 -c 2 -b 32 -r 96000 | nsox -play -c 2 -b 32 -r 96000
    nsox -white -v 0.0005 -c 2 -b 16 -r 8000 | nsox -play -c 2 -b 16 -r 8000
    nsox -white -c 1 -b 8 -r 64 > random.raw
    nsox -mixer | nsox -play -v 0.75 -c 2 -b 32 -r 48000
    nsox -pink | nsox -play
    nsox -rec | nsox -play
Linux:
    mono NSox.exe -tone | play -b 32 -r 44100 -c 2 -e floating-point -t f32 -


NPlay
-----

NPlay from NSox 2.0 Copyright © 2016-2024 Marc Williams

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
    nplay -white -c 1 -b 8 -r 64 > random.raw

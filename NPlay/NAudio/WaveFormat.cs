/*
  from the NAudio library by Mark Heath © 2001-2013 Mark Heath
 * 
 * modified for use in NSox
 */

/*
  from the NAudio library by Mark Heath © 2001-2013  
  modified for use in NPlay
 */

using System;
using System.Runtime.InteropServices;

namespace NSox.NAudio
{
    /// <summary>
    /// Represents a Wave file format
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormat
    {
        /// <summary>format type</summary>
        protected WaveFormatEncoding waveFormatTag;
        /// <summary>number of channels</summary>
        protected short channels;
        /// <summary>sample rate</summary>
        protected int sampleRate;
        /// <summary>for buffer estimation</summary>
        protected int averageBytesPerSecond;
        /// <summary>block size of data</summary>
        protected short blockAlign;
        /// <summary>number of bits per sample of mono data</summary>
        protected short bitsPerSample;
        /// <summary>number of following bytes</summary>
        protected short extraSize;

        /// <summary>
        /// Creates a new PCM 44.1Khz stereo 16 bit format
        /// </summary>
        internal WaveFormat()
            : this(44100, 16, 2)
        {

        }

        /// <summary>
        /// Gets the size of a wave buffer equivalent to the latency in milliseconds.
        /// </summary>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <returns></returns>
        internal int ConvertLatencyToByteSize(int milliseconds)
        {
            int bytes = (int)((AverageBytesPerSecond / 1000.0) * milliseconds);
            if ((bytes % BlockAlign) != 0)
            {
                // Return the upper BlockAligned
                bytes = bytes + BlockAlign - (bytes % BlockAlign);
            }
            return bytes;
        }

        /// <summary>
        /// Creates a new PCM format with the specified sample rate, bit depth and channels
        /// </summary>
        internal WaveFormat(int rate, int bits, int channels)
        {
            if (channels < 1)
            {
                throw new ArgumentOutOfRangeException(Def.NumberOfChannels);
            }
            if (bits == 32) this.waveFormatTag = WaveFormatEncoding.IeeeFloat;
            // minimum 16 bytes, sometimes 18 for PCM
            else this.waveFormatTag = WaveFormatEncoding.Pcm;
            this.channels = (short)channels;
            this.sampleRate = rate;
            this.bitsPerSample = (short)bits;
            this.extraSize = 0;

            this.blockAlign = (short)(channels * (bits / 8));
            this.averageBytesPerSecond = this.sampleRate * this.blockAlign;
        }

        /// <summary>
        /// Returns the number of channels (1=mono,2=stereo etc)
        /// </summary>
        internal int Channels
        {
            get
            {
                return channels;
            }
        }

        /// <summary>
        /// Returns the sample rate (samples per second)
        /// </summary>
        internal int SampleRate
        {
            get
            {
                return sampleRate;
            }
        }

        /// <summary>
        /// Returns the average number of bytes used per second
        /// </summary>
        internal int AverageBytesPerSecond
        {
            get
            {
                return averageBytesPerSecond;
            }
        }

        /// <summary>
        /// Returns the block alignment
        /// </summary>
        internal virtual int BlockAlign
        {
            get
            {
                return blockAlign;
            }
        }
    }
}

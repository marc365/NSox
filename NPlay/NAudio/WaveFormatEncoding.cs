/*
  from the NAudio library by Mark Heath © 2001-2013  
  modified for use in NPlay
 */

namespace NSox.NAudio
{
    /// <summary>
    /// Summary description for WaveFormatEncoding.
    /// </summary>
    public enum WaveFormatEncoding : ushort
    {
        /// <summary>WAVE_FORMAT_PCM		Microsoft Corporation</summary>
        Pcm = 0x0001,

        /// <summary>WAVE_FORMAT_IEEE_FLOAT Microsoft Corporation</summary>
        IeeeFloat = 0x0003
    }
}

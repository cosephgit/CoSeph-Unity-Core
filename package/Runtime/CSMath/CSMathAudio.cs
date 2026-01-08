using UnityEngine;

namespace CoSeph.Core
{
    // Converts linear volume (0–1) to a decibel-like scale based on
    // perceived loudness rather than physical sound intensity (and back).
    //
    // Uses a base-2 logarithmic mapping so that each halving of volume
    // feels roughly like a halving in loudness, which is more intuitive
    // for user-facing controls than standard 20*log10() scaling.
    public static class CSMathAudio
    {
        // DECIBELS_MIN:
        //   Hard lower bound used to avoid log(0) and extreme values.
        //   Represents "effectively zero" volume for math purposes.
        //
        // DECIBELS_SILENT:
        //   Perceptual silence threshold. Below this value audio systems
        //   should treat the sound as inaudible.
        private const float DECIBELS_MIN = -80f;
        private const float DECIBELS_SILENT = -65f;
        // Small positive threshold to avoid log(0) and insignificantly low dB values.
        // Values below this are treated as silence.
        private const float VOLUME_EPSILON = 0.01f;

        /// <summary>
        /// Convert volume value to decibels.
        /// </summary>
        /// <param name="vol">volume value 0...1</param>
        /// <returns>decibels -80...0</returns>
        public static float VolToDecibels(float vol)
        {
            float decibels;
            if (vol < VOLUME_EPSILON)
            {
                // can't do log 0
                decibels = DECIBELS_MIN;
            }
            else
            {
                decibels = Mathf.Log(vol, 2f); // so each halving of volume is -1
                decibels *= 10f; // -10 decibels is approximately half volume
            }
            return decibels;
        }

        /// <summary>
        /// Convert volume value to the same 0...1 range, remapped through the
        /// perceptual decibel scale for linear application in audio systems.
        /// </summary>
        /// <param name="vol">volume value 0...1</param>
        /// <returns>decibels value 0...1</returns>
        public static float VolToDecibelsScaled(float vol)
        {
            float volScaled = VolToDecibels(vol);

            volScaled = Mathf.Clamp((volScaled - DECIBELS_MIN) / -DECIBELS_MIN, 0f, 1f);

            return volScaled;
        }
        /// <summary>
        /// Convert decibels to volume value.
        /// </summary>
        /// <param name="dec">decibels -80...0</param>
        /// <returns>volume value 0...1</returns>
        public static float DecibelsToVol(float dec)
        {
            float volume;
            if (dec < DECIBELS_SILENT)
            {
                // at this level of decibels, there's no point doing any math, just zero it
                volume = 0f;
            }
            else
            {
                volume = dec * 0.1f; // a value from -80 to 0 -> -8 to 0
                volume = Mathf.Pow(2, volume); // a value from 0 to 1
            }
            return volume;
        }
    }
}

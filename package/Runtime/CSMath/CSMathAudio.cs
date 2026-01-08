using UnityEngine;

namespace CoSeph.Core
{
    public static class CSMathAudio
    {
        private const float DECIBELS_MIN = -80f;
        private const float DECIBELS_SILENT = -65f;
        private const float SILENCETHRESHOLD = 0.01f;
        
        public static float VolToDecibels(float vol)
        {
            float decibels;
            if (vol < SILENCETHRESHOLD)
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

        // this is specifically to interact with decibel-based sound systems like FMOD in a user-friendly way
        // so when a value from 0 to 1 is sent to FMOD it's already adjusted so it can be linearly applied to decibels
        public static float VolToDecibelsScaled(float vol)
        {
            float volScaled = VolToDecibels(vol);

            volScaled = Mathf.Clamp((volScaled - DECIBELS_MIN) / -DECIBELS_MIN, 0f, 1f);

            return volScaled;
        }
        public static float DecibelsToVol(float dec)
        {
            float volume;
            if (dec < DECIBELS_SILENT)
            {
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

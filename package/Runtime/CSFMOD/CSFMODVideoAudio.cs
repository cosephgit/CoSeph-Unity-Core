//--------------------------------------------------------------------
//
// This is a Unity behavior script that demonstrates how to capture
// audio data from a VideoPlayer using Unity's AudioSampleProvider and
// play it back through an FMOD.Sound. This example uses the
// VideoPlayer's APIOnly output mode and can be used to get audio from
// a video when UnityAudio is disabled.
//
// Steps to use:
// 1. Add a Unity VideoPlayer component to the same GameObject as this
//    script.
// 2. Untick the VideoPlayer component's Play On Awake option.
// 3. Set the VideoPlayer component's Source to a VideoClip.
// 4. Set the VideoPlayer component's Renderer to a Mesh.
//
// More information on how to configure a Unity VideoPlayer component
// can be found here:
// https://docs.unity3d.com/Manual/class-VideoPlayer.html
//
// For documentation on writing audio data to an FMOD.Sound. See
// https://fmod.com/docs/2.02/api/core-api-sound.html#sound_lock
//
// This document assumes familiarity with Unity scripting. See
// https://unity3d.com/learn/tutorials/topics/scripting for resources
// on learning Unity scripting.
//
//--------------------------------------------------------------------

using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Video;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CSFMODVideoAudio : MonoBehaviour
{
    private const int LATENCY_MS = 50; /* Some devices will require higher latency to avoid glitches */
    private const int DRIFT_MS = 1;
    private const float DRIFT_CORRECTION_PERCENTAGE = 0.5f;

    private VideoPlayer mVideoPlayer;
    private AudioSampleProvider mProvider;

    private FMOD.CREATESOUNDEXINFO mExinfo;
    private FMOD.Channel mChannel;
    private FMOD.Sound mSound;

    private List<float> mBuffer = new List<float>();

    private int mSampleRate;
    private uint mDriftThresholdSamples;
    private uint mTargetLatencySamples;
    private uint mAdjustedLatencySamples;
    private int mActualLatencySamples;

    private uint mTotalSamplesWritten;
    private uint mMinimumSamplesWritten = uint.MaxValue;
    private uint mTotalSamplesRead;

    private uint mLastReadPositionBytes;

    private bool playing;

    private void Start()
    {
        mVideoPlayer = GetComponent<VideoPlayer>();
        if (mVideoPlayer == null)
        {
            Debug.LogWarning("A VideoPlayer is required to use this script. " +
                "See Unity Documentation on how to use VideoPlayer: " +
                "https://docs.unity3d.com/Manual/class-VideoPlayer.html");
            return;
        }

        mVideoPlayer.audioOutputMode = VideoAudioOutputMode.APIOnly;
        mVideoPlayer.prepareCompleted += Prepared;
        mVideoPlayer.loopPointReached += VideoEnded;
        playing = false;

#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += EditorStateChange;
#endif
    }

    public void Play()
    {
        if (playing == false)
        {
            mVideoPlayer.Prepare();
            playing = true;
        }
    }

    public void Stop()
    {
        playing = false;
        mVideoPlayer.Stop();
        mChannel.stop();
        mSound.release();
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged -= EditorStateChange;
#endif
    }

#if UNITY_EDITOR
    private void EditorStateChange(PauseState state)
    {
        if (mChannel.hasHandle())
        {
            mChannel.setPaused(state == PauseState.Paused);
        }
    }
#endif

    private void OnDestroy()
    {
        mChannel.stop();
        mSound.release();

#if UNITY_EDITOR
        EditorApplication.pauseStateChanged -= EditorStateChange;
#endif
    }

    private void VideoEnded(VideoPlayer vp)
    {
        if (!vp.isLooping)
        {
            mChannel.setPaused(true);
        }
    }

    private void Prepared(VideoPlayer vp)
    {
        mProvider = vp.GetAudioSampleProvider(0);
        mSampleRate = (int)(mProvider.sampleRate * mVideoPlayer.playbackSpeed);

        mDriftThresholdSamples = (uint)(mSampleRate * DRIFT_MS) / 1000;
        mTargetLatencySamples = (uint)(mSampleRate * LATENCY_MS) / 1000;
        mAdjustedLatencySamples = mTargetLatencySamples;
        mActualLatencySamples = (int)mTargetLatencySamples;

        mExinfo.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
        mExinfo.numchannels = mProvider.channelCount;
        mExinfo.defaultfrequency = mSampleRate;
        mExinfo.length = mTargetLatencySamples * (uint)mExinfo.numchannels * sizeof(float);
        mExinfo.format = FMOD.SOUND_FORMAT.PCMFLOAT;

        FMODUnity.RuntimeManager.CoreSystem.createSound("", FMOD.MODE.LOOP_NORMAL | FMOD.MODE.OPENUSER, ref mExinfo, out mSound);

        mProvider.sampleFramesAvailable += SampleFramesAvailable;
        mProvider.enableSampleFramesAvailableEvents = true;
        mProvider.freeSampleFrameCountLowThreshold = mProvider.maxSampleFrameCount - mTargetLatencySamples;

        vp.Play();
    }

    private void SampleFramesAvailable(AudioSampleProvider provider, uint sampleFrameCount)
    {
        using (NativeArray<float> buffer = new NativeArray<float>((int)sampleFrameCount * provider.channelCount, Allocator.Temp))
        {
            uint samplesWritten = provider.ConsumeSampleFrames(buffer);
            mBuffer.AddRange(buffer);

            /*
             * Drift compensation
             * If we are behind our latency target, play a little faster
             * If we are ahead of our latency target, play a little slower
             */
            mTotalSamplesWritten += samplesWritten;

            if (samplesWritten != 0 && (samplesWritten < mMinimumSamplesWritten))
            {
                mMinimumSamplesWritten = samplesWritten;
                mAdjustedLatencySamples = Math.Max(samplesWritten, mTargetLatencySamples);
            }

            int latency = (int)mTotalSamplesWritten - (int)mTotalSamplesRead;
            mActualLatencySamples = (int)((0.93f * mActualLatencySamples) + (0.03f * latency));

            int playbackRate = mSampleRate;
            if (mActualLatencySamples < (int)(mAdjustedLatencySamples - mDriftThresholdSamples))
            {
                playbackRate = mSampleRate - (int)(mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            else if (mActualLatencySamples > (int)(mAdjustedLatencySamples + mDriftThresholdSamples))
            {
                playbackRate = mSampleRate + (int)(mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            mChannel.setFrequency(playbackRate);
        }
    }

    public bool IsPlayerReady()
    {
        return mVideoPlayer.isPrepared;
    }

    private void Update()
    {
        if (!IsPlayerReady())
        {
            return;
        }
        if (!playing)
            return;

        /*
         * Need to wait before playing to provide adequate space between read and write positions
         */
        if (!mChannel.hasHandle() && mTotalSamplesWritten > mAdjustedLatencySamples)
        {
            FMOD.ChannelGroup mMasterChannelGroup;
            FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out mMasterChannelGroup);
            FMODUnity.RuntimeManager.CoreSystem.playSound(mSound, mMasterChannelGroup, false, out mChannel);
        }

        if (mBuffer.Count > 0 && mChannel.hasHandle())
        {
            uint readPositionBytes;
            mChannel.getPosition(out readPositionBytes, FMOD.TIMEUNIT.PCMBYTES);

            /*
             * Account for wrapping
             */
            uint bytesRead = readPositionBytes - mLastReadPositionBytes;
            if (readPositionBytes < mLastReadPositionBytes)
            {
                bytesRead += mExinfo.length;
            }

            if (bytesRead > 0 && mBuffer.Count >= bytesRead)
            {
                /*
                 * Fill previously read data with fresh samples
                 */
                IntPtr ptr1, ptr2;
                uint lenBytes1, lenBytes2;
                var res = mSound.@lock(mLastReadPositionBytes, bytesRead, out ptr1, out ptr2, out lenBytes1, out lenBytes2);
                if (res != FMOD.RESULT.OK) Debug.LogError(res);

                /*
                 * Though exinfo.format is float, data retrieved from Sound::lock is in bytes,
                 * therefore we only copy (len1+len2)/sizeof(float) full float values across
                 */
                int lenFloats1 = (int)(lenBytes1 / sizeof(float));
                int lenFloats2 = (int)(lenBytes2 / sizeof(float));
                int totalFloatsRead = lenFloats1 + lenFloats2;
                float[] tmpBufferFloats = new float[totalFloatsRead];

                mBuffer.CopyTo(0, tmpBufferFloats, 0, tmpBufferFloats.Length);
                mBuffer.RemoveRange(0, tmpBufferFloats.Length);

                if (lenBytes1 > 0)
                {
                    Marshal.Copy(tmpBufferFloats, 0, ptr1, lenFloats1);
                }
                if (lenBytes2 > 0)
                {
                    Marshal.Copy(tmpBufferFloats, lenFloats1, ptr2, lenFloats2);
                }

                res = mSound.unlock(ptr1, ptr2, lenBytes1, lenBytes2);
                if (res != FMOD.RESULT.OK) Debug.LogError(res);
                mLastReadPositionBytes = readPositionBytes;
                mTotalSamplesRead += (uint)(totalFloatsRead / mExinfo.numchannels);
            }
        }
    }
}
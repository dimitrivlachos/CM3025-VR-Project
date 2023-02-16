  using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicForge
{
    public class ClipCombiner : MonoBehaviour
    {
        #region Cocatenate audios
        /// <summary>
        /// Recieves an array of audio clips and returns a cocatenation of all the array
        /// </summary>
        /// <param name="clips"></param>
        /// <returns></returns>
        public static AudioClip Cocatenate(params AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;

            int length = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                    continue;

                length += clips[i].samples * clips[i].channels;
            }

            float[] data = new float[length];
            length = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                    continue;

                float[] buffer = new float[clips[i].samples * clips[i].channels];
                clips[i].GetData(buffer, 0);
                //System.Buffer.BlockCopy(buffer, 0, data, length, buffer.Length);
                buffer.CopyTo(data, length);
                length += buffer.Length;
            }

            if (length == 0)
                return null;

            AudioClip result = AudioClip.Create("Combine", length / 2, 2, 44100, false);
            result.SetData(data, 0);

            return result;
        }

        /// <summary>
        /// Recieves an array of audio clips and returns a cocatenation of all the array, with a crossfade
        /// at the end of every audio.
        /// </summary>
        /// <param name="clips"></param>
        /// <returns></returns>
        public static AudioClip Cocatenate(AudioClip[] clips, float crossFadeDuration)
        {
            if (clips == null || clips.Length == 0)
                return null;

            int samplesToFade = milisecondsToSamples(crossFadeDuration, clips[0].frequency, clips[0].channels);

            //Gets the length of the new audio clip
            int length = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                    continue;
                if (i == 0)
                    length += (clips[i].samples * clips[i].channels);
                else
                    length += (clips[i].samples * clips[i].channels) - samplesToFade;

            }

            float[] data = new float[length];
            length = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                    continue;

                float[] buffer = new float[clips[i].samples * clips[i].channels];
                clips[i].GetData(buffer, 0);
                if (i == 0)
                {
                    buffer.CopyTo(data, 0);
                    length += buffer.Length;
                }
                else
                {
                    buffer = CrossFadeClips(clips[i - 1], clips[i], samplesToFade);
                    buffer.CopyTo(data, length - samplesToFade);
                    length += buffer.Length - samplesToFade;
                }
            }

            if (length == 0)
                return null;

            AudioClip result = AudioClip.Create("Combine", length / 2, 2, 44100, false);
            result.SetData(data, 0);

            return result;
        }

        private static float[] CrossFadeClips(AudioClip clipA, AudioClip clipB, int fadeDurationSamples)
        {
            float[] floatSamplesA = new float[clipA.samples * clipA.channels];
            clipA.GetData(floatSamplesA, 0);

            float[] floatSamplesB = new float[clipB.samples * clipB.channels];
            clipB.GetData(floatSamplesB, 0);

            float[] mixedFloatArray = CrossFadeAndClampFloatBuffers(floatSamplesA, floatSamplesB, fadeDurationSamples);
            return mixedFloatArray;
        }

        //Recieves miliseconds and returns how many samples it is equivalent
        private static int milisecondsToSamples(float miliseconds, float frecuency, int chanels)
        {
            int res = (int)Mathf.Floor((frecuency / 1000) * miliseconds * chanels);
            return res;
        }

        #endregion

        #region Mix clips


        /// <summary>
        /// Receives two audioclips and returns both mixed. The audioclips have to have the same lenghth and sample rate
        /// </summary>
        /// <param name="clipA"></param>
        /// <param name="AudioClipB"></param>
        /// <returns></returns>
        public static AudioClip Mix(AudioClip clipA, AudioClip clipB, string song_name)
        {
            float[] floatSamplesA = new float[clipA.samples * clipA.channels];
            clipA.GetData(floatSamplesA, 0);

            float[] floatSamplesB = new float[clipB.samples * clipB.channels];
            clipB.GetData(floatSamplesB, 0);

            float[] mixedFloatArray = MixAndClampFloatBuffers(floatSamplesA, floatSamplesB);
            AudioClip result = AudioClip.Create(song_name, mixedFloatArray.Length, clipA.channels, clipA.frequency, false);
            result.SetData(mixedFloatArray, 0);
            return result;
        }

        /// <summary>
        /// Receives two audioclips and returns both mixed. The audioclips have to have the same lenghth and sample rate. Also need the volume that A will have. It has to be from 0 - 1.
        /// </summary>
        /// <param name="clipA"></param>
        /// <param name="AudioClipB"></param>
        /// <returns></returns>
        public static AudioClip Mix(AudioClip clipA, float APercent, AudioClip clipB, string song_name)
        {
            float BPercent = 1 - APercent;

            float[] floatSamplesA = new float[clipA.samples * clipA.channels];
            clipA.GetData(floatSamplesA, 0);

            float[] floatSamplesB = new float[clipB.samples * clipB.channels];
            clipB.GetData(floatSamplesB, 0);

            float[] mixedFloatArray = MixAndClampFloatBuffers(floatSamplesA, floatSamplesB, APercent, BPercent);
            AudioClip result = AudioClip.Create(song_name, mixedFloatArray.Length, clipA.channels, clipA.frequency, false);
            result.SetData(mixedFloatArray, 0);
            return result;
        }


        /// <summary>
        /// Mixed to audio float buffers
        /// </summary>
        /// <param name="bufferA"></param>
        /// <param name="bufferB"></param>
        /// <returns></returns>
        private static float[] MixAndClampFloatBuffers(float[] bufferA, float[] bufferB)
        {
            int maxLength = Mathf.Min(bufferA.Length, bufferB.Length);
            float[] mixedFloatArray = new float[maxLength];

            for (int i = 0; i < maxLength; i++)
            {
                mixedFloatArray[i] = bufferB[i];
                mixedFloatArray[i] = ClampToValidRange((bufferA[i] + bufferB[i]) / 2);
            }
            return mixedFloatArray;
        }

        /// <summary>
        /// Mixed to audio float buffer
        /// </summary>
        /// <param name="bufferA"></param>
        /// <param name="bufferB"></param>
        /// <param name="APercent"></param>
        /// <param name="BPercent"></param>
        /// <returns></returns>
        private static float[] MixAndClampFloatBuffers(float[] bufferA, float[] bufferB, float APercent, float BPercent)
        {
            int maxLength = Mathf.Min(bufferA.Length, bufferB.Length);
            float[] mixedFloatArray = new float[maxLength];

            for (int i = 0; i < maxLength; i++)
            {
                mixedFloatArray[i] = bufferB[i];
                mixedFloatArray[i] = ClampToValidRange((bufferA[i] * APercent + bufferB[i] * BPercent) / 1.5f);
            }
            return mixedFloatArray;
        }

        private static float[] CrossFadeAndClampFloatBuffers(float[] bufferA, float[] bufferB, int crossFadeSamples)
        {
            float bufferAElement, bufferBElement;
            float currSampleNormalized;
            float currSample = 0;
            float[] mixedFloatArray = new float[bufferB.Length];
            if (bufferA.Length > crossFadeSamples)
            {
                for (int i = 0; i < bufferB.Length; i++)
                {
                    if (i < crossFadeSamples)
                    {
                        currSampleNormalized = currSample / crossFadeSamples;
                        currSample++;

                        bufferAElement = bufferA[bufferA.Length + i - crossFadeSamples] * (1 - currSampleNormalized);
                        bufferBElement = bufferB[i];
                        mixedFloatArray[i] = ClampToValidRange(bufferBElement + bufferAElement);
                    }
                    else
                    {
                        mixedFloatArray[i] = bufferB[i];
                    }
                }
            }
            else
            {
                mixedFloatArray = bufferB;
            }

            return mixedFloatArray;
        }

        private static float ClampToValidRange(float value)
        {
            float min = -1.0f;
            float max = 1.0f;
            return (value < min) ? min : (value > max) ? max : value;
        }

        #endregion
    }
}

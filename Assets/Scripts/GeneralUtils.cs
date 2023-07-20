using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public static class GeneralUtils 
    {
        /// <summary>
        /// Shuffle array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrayToShuffle"></param>
        public static void ShuffleArray<T>(T[] arrayToShuffle)
        {
            if (arrayToShuffle != null)
            {
                if (arrayToShuffle.Length > 0)
                {
                    T tempItem;

                    for (int index = 0; index < arrayToShuffle.Length; index++)
                    {
                        int rnd = Random.Range(0, arrayToShuffle.Length);
                        tempItem = arrayToShuffle[rnd];
                        arrayToShuffle[rnd] = arrayToShuffle[index];
                        arrayToShuffle[index] = tempItem;
                    }
                }
            }
        }
        
     

        public static List<T> ShuffleList<T>(this List<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = Random.Range(0, n + 1);  
                (list[k], list[n]) = (list[n], list[k]);
            }

            return list;
        }

        public static Color MakeColorTransparent(Color color)
        {
            return new Color(color.r, color.g, color.b, 0);
        }

        public static IEnumerator WaitAndPerform(float seconds, System.Action action)
        {
            yield return null;
            yield return new WaitForSeconds(seconds);
            action();
        }

        /// <summary>
        /// Go over recursivly on a gameobject and increase the SpriteRenderer order in layer
        /// </summary>
        /// <param name="go"></param>
        /// <param name="increaseSize"></param>
        public static void IncreaseObjectSpritesLayer(Transform go, int increaseSize)
        {
            for (int childIndex = 0; childIndex < go.childCount; childIndex++)
            {
                Transform childT = go.transform.GetChild(childIndex);

                if (childT != null)
                {
                    IncreaseObjectSpritesLayer(childT, increaseSize);
                }
            }

            // Check if need to increase 
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.sortingOrder += increaseSize;
            }

            SpriteMask sm = go.GetComponent<SpriteMask>();

            if (sm != null)
            {
                sm.backSortingOrder += increaseSize;
                sm.frontSortingOrder += increaseSize;
            }
        }
    
        public static T[] GetEnums<T>(int to = int.MaxValue, int from = 0) where T : Enum
        {
            T[] allEnums = (T[]) Enum.GetValues(typeof(T));
            int copyUntil = Math.Min(to, allEnums.Length), copyFrom = Math.Max(from, 0);
            int arrayLength = copyUntil - copyFrom;
            T[] enums = new T[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                enums[i] = allEnums[i + copyFrom];
            }
            return enums;
        }


        public static T RandomEnum<T>(int to = int.MaxValue, int from = 0) where T : Enum
        {
            T[] enumArray = GetEnums<T>();
            T enumValue = enumArray[Random.Range(from, Math.Min(enumArray.Length, to))];
            return enumValue;
        }

        public static string getTimeText(int timeInSec)
        {
            int minutes = timeInSec / 60;
            int seconds = timeInSec % 60;

            string minutesStr;
            string secondsStr;

            if (minutes < 10)
            {
                minutesStr = "0" + minutes.ToString();
            }
            else
            {
                minutesStr = minutes.ToString();
            }

            if (seconds < 10)
            {
                secondsStr = "0" + seconds.ToString();
            }
            else
            {
                secondsStr = seconds.ToString();
            }

            return (minutesStr + ":" + secondsStr);
        }

        public static string Get8Digits()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }

        public static string RemoveSpecialCharacters(string input)
        {
            Regex r1 = new Regex("r'^[a-zA-Z\u0590-\u05FF\u200f\u200e ]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            //Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return r1.Replace(input, String.Empty);
        }

    }
}

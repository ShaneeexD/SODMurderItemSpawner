using System;
using System.Collections;
using UnityEngine;
using UniverseLib;

namespace MurderItemSpawner
{
    /// <summary>
    /// Helper class for running coroutines using UniverseLib
    /// </summary>
    public static class CoroutineHelper
    {
        /// <summary>
        /// Starts a coroutine using UniverseLib.RuntimeHelper
        /// </summary>
        /// <param name="routine">The coroutine to start</param>
        public static void StartCoroutine(IEnumerator routine)
        {
            try
            {
                UniverseLib.RuntimeHelper.StartCoroutine(routine);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error starting coroutine: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Creates a simple delay coroutine
        /// </summary>
        /// <param name="seconds">The number of seconds to delay</param>
        /// <returns>The delay coroutine</returns>
        public static IEnumerator WaitForSeconds(float seconds)
        {
            float startTime = Time.time;
            while (Time.time - startTime < seconds)
            {
                yield return null;
            }
        }
        
        /// <summary>
        /// Creates a coroutine that executes an action after a delay
        /// </summary>
        /// <param name="seconds">The number of seconds to delay</param>
        /// <param name="action">The action to execute after the delay</param>
        /// <returns>The delay coroutine</returns>
        public static IEnumerator DelayedAction(float seconds, Action action)
        {
            yield return WaitForSeconds(seconds);
            action?.Invoke();
        }
    }
}

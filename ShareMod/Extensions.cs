using System;
using System.Collections;
using UnityEngine;

namespace ShareMod
{
    internal static class Extensions
    {
        public static void WaitCoroutine(this IEnumerator func)
        {
            while (func.MoveNext())
            {
                if (func.Current != null)
                {
                    IEnumerator num;
                    try
                    {
                        num = (IEnumerator)func.Current;
                    }
                    catch (InvalidCastException)
                    {
                        return;  // Skip WaitForSeconds, WaitForEndOfFrame and WaitForFixedUpdate
                    }
                    WaitCoroutine(num);
                }
            }
        }

        public static Rect CenterInScreen(this Rect rect)
        {
            return new Rect(Screen.width / 2 - rect.width / 2, Screen.height / 2 - rect.height / 2, rect.width, rect.height);
        }
    }
}

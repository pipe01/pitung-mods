using System;
using System.Collections;

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
    }
}

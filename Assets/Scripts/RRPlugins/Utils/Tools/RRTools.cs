using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRLib
{
    public static class RRTools
    {

        // Parse an enum from a string
        // Returns a default value if it failed
        public static T ParseEnumSafe<T>(string sEnum, T eDefault)
        {
            try
            {
                object oEnum = System.Enum.Parse(typeof(T), sEnum);
                return (T)oEnum;
            }
            catch
            {
                // ignored
            }
#if UNITY_EDITOR
            Debug.LogError("Can't parse " + sEnum + " to enum " + typeof(T));
#endif
            return eDefault;
        }
    }

}

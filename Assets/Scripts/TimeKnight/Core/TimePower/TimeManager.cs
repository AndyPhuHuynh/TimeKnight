using UnityEngine;

namespace TimeKnight.Core.TimePower
{
    public class TimeManager : MonoBehaviour
    {
        public static float CurrentTimeModifier { get; private set; }  = 1f;
        public static float CustomDelta => Time.deltaTime * CurrentTimeModifier;

        // Code for getting player input to activate slow down
    }
}
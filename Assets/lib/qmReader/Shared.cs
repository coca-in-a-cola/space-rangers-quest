using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Тонкая связка между фреймворком и моделью
namespace SRQ {
    public class Shared {
        public static Action<object> Warn = Debug.LogWarning;
        public static Action<object> Log = Debug.Log;
    }
}
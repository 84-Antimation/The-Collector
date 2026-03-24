using UnityEngine;
using System;

namespace FoxX.TheCollector
{
    public class Vacuum : Powerup
    {
        [Header(" Actions ")]
        public static Action started;

        private void TriggerPowerupStart()
        {
            started?.Invoke();
        }
    }
}
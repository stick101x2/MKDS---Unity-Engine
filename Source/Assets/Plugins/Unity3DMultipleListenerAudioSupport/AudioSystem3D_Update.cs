using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reign.Audio
{
    public class AudioSystem3D_Update : MonoBehaviour
    {
        public delegate void UpdateCallbackMethod();
        public static event UpdateCallbackMethod UpdateCallback;

        private void LateUpdate()
        {
            if (UpdateCallback != null) UpdateCallback();
        }
    }
}
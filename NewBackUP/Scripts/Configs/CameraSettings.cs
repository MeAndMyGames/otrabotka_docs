using UnityEngine;

namespace Otrabotka.Configs
{
    [System.Serializable]
    public class CameraSettings
    {
        [Tooltip("Применить настройки камеры при взаимодействии")]
        public bool overrideSettings = false;

        [Header("Movement Response")]
        public float pushBackDistance = 2f;
        public float pushBackDuration = 0.1f;
        public float overshootDistance = 1f;
        public float overshootDuration = 2f;
        public float returnDuration = 1f;

        [Header("Idle Zoom")]
        public float standStillDelay = 3f;
        public float zoomInDistance = 1f;
        public float zoomInDuration = 2f;

        [Header("Smoothing")]
        public bool smoothEnabled = false;
        public float smoothingMultiplier = 1f;
        public float zoomOutDuration = 0.1f;

        [Header("Movement Z Response")]
        public float movementZDistance = 2f;
        public float movementZDuration = 1f;

        [Header("Height Response")]
        public float zoomHeight = 1.25f;

        [Header("Inertia")]
        public bool inertiaEnabled = false;
        public float smoothTime = 0.5f;
    }
} 
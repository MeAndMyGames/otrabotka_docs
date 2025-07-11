using UnityEngine;
using System.Collections;
using Otrabotka.Systems;

namespace Otrabotka.Systems
{
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform персонажа, за которым следует камера")]  
        [SerializeField] private Transform player;

        [Header("Movement Response")]
        [Tooltip("Отступ камеры назад при начале движения (м)")]
        [SerializeField] private float pushBackDistance = 2f;
        [Tooltip("Время отступа назад (сек)")]
        [SerializeField] private float pushBackDuration = 1f;
        [Tooltip("Перемещение вперед после отступа (м)")]
        [SerializeField] private float overshootDistance = 1f;
        [Tooltip("Время перемещения вперед (сек)")]
        [SerializeField] private float overshootDuration = 2f;
        [Tooltip("Время возврата к базовой позиции (сек)")]
        [SerializeField] private float returnDuration = 1f;

        [Header("Idle Zoom")]
        [Tooltip("Задержка до зума при простое (сек)")]
        [SerializeField] private float standStillDelay = 3f;
        [Tooltip("Смещение камеры ближе к игроку при зуме (м)")]
        [SerializeField] private float zoomInDistance = 1f;
        [Tooltip("Время зума (сек)")]
        [SerializeField] private float zoomInDuration = 2f;

        [Header("Smoothing")]
        [Tooltip("Enables global smoothing multiplier for all camera transitions")]
        [SerializeField] private bool smoothEnabled = false;
        [Tooltip("Smoothing multiplier: durations *= smoothingEnabled? smoothingMultiplier : 1")]
        [SerializeField] private float smoothingMultiplier = 1f;
        [Tooltip("Duration for zoom-out when movement starts (sec)")]
        [SerializeField] private float zoomOutDuration = 2f;

        [Header("Movement Z Response")]
        [Tooltip("Distance camera moves away on Z during movement (m)")]
        [SerializeField] private float movementZDistance = 2f;
        [Tooltip("Duration of camera Z away movement (sec)")]
        [SerializeField] private float movementZDuration = 1f;

        [Header("Height Response")]
        [Tooltip("Смещение камеры вверх при zoom in (м)")]
        [SerializeField] private float zoomHeight = 1.25f;

        [Header("Inertia")]
        [Tooltip("Enable inertial smoothing for all camera movements")]
        [SerializeField] private bool inertiaEnabled = false;
        [Tooltip("Time (sec) for camera to 'catch up' to target position")]
        [SerializeField] private float smoothTime = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool enableCameraMovement = true;
        // Saved original camera settings for vehicle mode
        private float _savedPushBackDistance;
        private float _savedPushBackDuration;
        private float _savedOvershootDistance;
        private float _savedOvershootDuration;
        private float _savedReturnDuration;
        private float _savedStandStillDelay;
        private float _savedZoomInDistance;
        private float _savedZoomInDuration;
        private bool _savedSmoothEnabled;
        private float _savedSmoothingMultiplier;
        private float _savedZoomOutDuration;
        private float _savedMovementZDistance;
        private float _savedMovementZDuration;
        private float _savedZoomHeight;
        private bool _savedInertiaEnabled;
        private float _savedSmoothTime;

        private Vector3 _initialOffset;
        private Vector3 _currentOffset;
        private float _dynamicX;
        private float _dynamicZ;
        private float _dynamicY;
        private Vector3 _smoothVelocity;

        private Coroutine _moveRoutine;
        private Coroutine _zoomRoutine;
        private float _standTimer;
        private bool _isMoving;
        private Coroutine _moveZRoutine;
        private Coroutine _zResetRoutine;
        private float _prevDir;

        void Awake()
        {
            if (player == null)
                Debug.LogError("CameraController: player не назначен");
            _initialOffset = transform.position - player.position;
            _currentOffset = _initialOffset;
            _dynamicX = 0f;
            _dynamicZ = 0f;
            _dynamicY = 0f;
            _prevDir = 0f;
        }

        void Update()
        {
            if (!enableCameraMovement || (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)) return;
            // определяем движение по A/D
            bool keyA = Input.GetKey(KeyCode.A);
            bool keyD = Input.GetKey(KeyCode.D);
            bool movingRaw = keyA || keyD;
            // направление: A=-1, D=+1
            float dir = keyD ? 1f : (keyA ? -1f : 0f);
            if (movingRaw && !_isMoving)
            {
                StartMovement(dir);
            }
            else if (movingRaw && _isMoving && _prevDir != 0f && dir != _prevDir)
            {
                RestartMovement(dir);
            }
            else if (!movingRaw && _isMoving)
            {
                StopMovement();
            }

            // обновляем таймер простоя
            if (movingRaw)
                _standTimer = 0f;
            else
                _standTimer += Time.deltaTime;

            // плавное обновление Z-смещения: при движении отдаляем, при долгом простое приближаем
            float targetZ;
            float durationZ;
            if (_isMoving)
            {
                targetZ = -movementZDistance;
                durationZ = movementZDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            }
            else if (_standTimer >= standStillDelay)
            {
                targetZ = zoomInDistance;
                durationZ = zoomInDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            }
            else
            {
                // до срабатывания зума сохраняем текущее значение
                targetZ = _dynamicZ;
                durationZ = 1f;
            }
            if (durationZ > 0f)
                _dynamicZ = Mathf.Lerp(_dynamicZ, targetZ, Time.deltaTime / durationZ);

            // плавное обновление Y-смещения: при движении возвращаем к 0, при idle-зуми поднимаем
            float targetY;
            float durationY;
            if (_isMoving)
            {
                targetY = 0f;
                durationY = movementZDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            }
            else if (_standTimer >= standStillDelay)
            {
                targetY = zoomHeight;
                durationY = zoomInDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            }
            else
            {
                targetY = _dynamicY;
                durationY = 1f;
            }
            if (durationY > 0f)
                _dynamicY = Mathf.Lerp(_dynamicY, targetY, Time.deltaTime / durationY);

            _prevDir = dir;
        }

        void LateUpdate()
        {
            if (!enableCameraMovement || (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive))
                return;
            // целевое смещение от игрока без сглаживания
            Vector3 targetOffset = _initialOffset + new Vector3(_dynamicX, _dynamicY, _dynamicZ);
            if (inertiaEnabled)
            {
                // сглаживаем оффсет
                _currentOffset = Vector3.SmoothDamp(_currentOffset, targetOffset, ref _smoothVelocity, smoothTime);
            }
            else
            {
                _currentOffset = targetOffset;
            }
            // позиционируем камеру: игрок + сглаженный оффсет
            transform.position = player.position + _currentOffset;
        }

        private void StartMovement(float dir)
        {
            _isMoving = true;
            _standTimer = 0f;
            if (_zoomRoutine != null)
            {
                StopCoroutine(_zoomRoutine);
                _zoomRoutine = null;
            }
            if (_moveZRoutine != null)
                StopCoroutine(_moveZRoutine);
            _moveZRoutine = StartCoroutine(MoveZRoutine(dir));
            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);
            _moveRoutine = StartCoroutine(MoveRoutine(dir));
        }

        private void StopMovement()
        {
            _isMoving = false;
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }
            _moveRoutine = StartCoroutine(ReturnRoutine());
        }

        private IEnumerator MoveRoutine(float dir)
        {
            // фаза 1: отступ назад (-X)
            float startX = _dynamicX;
            float targetX = -pushBackDistance * dir;
            float duration1 = pushBackDistance > 0 ? pushBackDuration * (smoothEnabled ? smoothingMultiplier : 1f) : 0f;
            float t = 0f;
            while (t < duration1)
            {
                t += Time.deltaTime;
                _dynamicX = Mathf.Lerp(startX, targetX, t / duration1);
                yield return null;
            }
            // фаза 2: овершут вперед (+X)
            float duration2 = overshootDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            startX = _dynamicX;
            targetX = overshootDistance * dir;
            t = 0f;
            while (t < duration2)
            {
                t += Time.deltaTime;
                _dynamicX = Mathf.Lerp(startX, targetX, t / duration2);
                yield return null;
            }
            _moveRoutine = null;
        }

        private IEnumerator ReturnRoutine()
        {
            // возврат X-оффсета к 0
            float startX = _dynamicX;
            float durReturn = returnDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            float t = 0f;
            while (t < durReturn)
            {
                t += Time.deltaTime;
                _dynamicX = Mathf.Lerp(startX, 0f, t / durReturn);
                yield return null;
            }
            _moveRoutine = null;
        }

        private void StartZoomIn()
        {
            if (_moveRoutine != null)
                return;
            _zoomRoutine = StartCoroutine(ZoomInRoutine());
        }

        private IEnumerator ZoomInRoutine()
        {
            // Z-оффсет для зума ближе (+Z)
            float startZ = _dynamicZ;
            float durZoomIn = zoomInDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            float t = 0f;
            while (t < durZoomIn)
            {
                t += Time.deltaTime;
                _dynamicZ = Mathf.Lerp(startZ, zoomInDistance, t / durZoomIn);
                yield return null;
            }
            _zoomRoutine = null;
        }

        private IEnumerator MoveZRoutine(float dir)
        {
            float startZ = _dynamicZ;
            float targetZ = -movementZDistance;
            float t = 0f;
            float dur = movementZDuration * (smoothEnabled ? smoothingMultiplier : 1f);
            while (t < dur)
            {
                t += Time.deltaTime;
                _dynamicZ = Mathf.Lerp(startZ, targetZ, t / dur);
                yield return null;
            }
            _moveZRoutine = null;
        }

        private void RestartMovement(float dir)
        {
            if (_zoomRoutine != null)
            {
                StopCoroutine(_zoomRoutine);
                _zoomRoutine = null;
            }
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }
            _moveRoutine = StartCoroutine(MoveRoutine(dir));
        }

        /// <summary>
        /// Применить параметры камеры из настроек взаимодействия.
        /// </summary>
        public void ApplySettings(Otrabotka.Configs.CameraSettings settings)
        {
            if (settings == null || !settings.overrideSettings) return;
            pushBackDistance = settings.pushBackDistance;
            pushBackDuration = settings.pushBackDuration;
            overshootDistance = settings.overshootDistance;
            overshootDuration = settings.overshootDuration;
            returnDuration = settings.returnDuration;
            standStillDelay = settings.standStillDelay;
            zoomInDistance = settings.zoomInDistance;
            zoomInDuration = settings.zoomInDuration;
            smoothEnabled = settings.smoothEnabled;
            smoothingMultiplier = settings.smoothingMultiplier;
            zoomOutDuration = settings.zoomOutDuration;
            movementZDistance = settings.movementZDistance;
            movementZDuration = settings.movementZDuration;
            zoomHeight = settings.zoomHeight;
            inertiaEnabled = settings.inertiaEnabled;
            smoothTime = settings.smoothTime;
        }
        /// <summary>
        /// Save current camera settings before override.
        /// </summary>
        public void SaveSettings()
        {
            _savedPushBackDistance = pushBackDistance;
            _savedPushBackDuration = pushBackDuration;
            _savedOvershootDistance = overshootDistance;
            _savedOvershootDuration = overshootDuration;
            _savedReturnDuration = returnDuration;
            _savedStandStillDelay = standStillDelay;
            _savedZoomInDistance = zoomInDistance;
            _savedZoomInDuration = zoomInDuration;
            _savedSmoothEnabled = smoothEnabled;
            _savedSmoothingMultiplier = smoothingMultiplier;
            _savedZoomOutDuration = zoomOutDuration;
            _savedMovementZDistance = movementZDistance;
            _savedMovementZDuration = movementZDuration;
            _savedZoomHeight = zoomHeight;
            _savedInertiaEnabled = inertiaEnabled;
            _savedSmoothTime = smoothTime;
        }
        /// <summary>
        /// Restore camera settings after exiting vehicle.
        /// </summary>
        public void RestoreSettings()
        {
            pushBackDistance = _savedPushBackDistance;
            pushBackDuration = _savedPushBackDuration;
            overshootDistance = _savedOvershootDistance;
            overshootDuration = _savedOvershootDuration;
            returnDuration = _savedReturnDuration;
            standStillDelay = _savedStandStillDelay;
            zoomInDistance = _savedZoomInDistance;
            zoomInDuration = _savedZoomInDuration;
            smoothEnabled = _savedSmoothEnabled;
            smoothingMultiplier = _savedSmoothingMultiplier;
            zoomOutDuration = _savedZoomOutDuration;
            movementZDistance = _savedMovementZDistance;
            movementZDuration = _savedMovementZDuration;
            zoomHeight = _savedZoomHeight;
            inertiaEnabled = _savedInertiaEnabled;
            smoothTime = _savedSmoothTime;
        }
    }
} 
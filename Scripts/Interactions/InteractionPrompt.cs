using UnityEngine;

namespace Otrabotka.Interactions
{
    /// <summary>
    /// Управляет отображением подсказки 'E' над указанным объектом.
    /// Префаб должен содержать Canvas (World Space) с текстом либо спрайтом 'E'.
    /// </summary>
    public class InteractionPrompt : MonoBehaviour
    {
        [Tooltip("Префаб подсказки 'E' (World Space Canvas)")]
        [SerializeField] private GameObject promptPrefab;

        [Tooltip("Высота над объектом, на которой будет отображаться подсказка")]
        [SerializeField] private float heightOffset = 2f;

        private GameObject _promptInstance;
        private Transform _followTarget;

        private void Awake()
        {
            if (promptPrefab == null)
            {
                Debug.LogError("InteractionPrompt: promptPrefab не назначен");
                return;
            }
            _promptInstance = Instantiate(promptPrefab, Vector3.zero, Quaternion.identity);
            _promptInstance.SetActive(false);
        }

        private void Update()
        {
            if (_promptInstance != null && _promptInstance.activeSelf && _followTarget != null)
            {
                Vector3 worldPos = _followTarget.position + Vector3.up * heightOffset;
                _promptInstance.transform.position = worldPos;
                // Можно добавить вращение к камере, чтобы всегда смотреть на игрока
            }
        }

        /// <summary>
        /// Показать подсказку над указанным объектом.
        /// </summary>
        public void ShowPrompt(Transform target)
        {
            if (_promptInstance == null || target == null) return;
            _followTarget = target;
            _promptInstance.SetActive(true);
            Update();
        }

        /// <summary>
        /// Скрыть подсказку.
        /// </summary>
        public void HidePrompt()
        {
            if (_promptInstance == null) return;
            _promptInstance.SetActive(false);
            _followTarget = null;
        }
    }
} 
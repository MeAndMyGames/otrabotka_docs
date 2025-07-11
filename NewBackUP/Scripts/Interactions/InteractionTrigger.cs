using UnityEngine;
using Otrabotka.Interfaces;

namespace Otrabotka.Interactions
{
    [RequireComponent(typeof(Collider))]
    public class InteractionTrigger : MonoBehaviour
    {
        [Tooltip("Компонент для отображения подсказки 'E'.")]
        [SerializeField] private InteractionPrompt prompt;

        [Tooltip("Слой игрока")] [SerializeField] public LayerMask playerLayerMask;

        private IInteractable _interactable;
        private bool _playerInRange;

        private void Awake()
        {
            _interactable = GetComponent<IInteractable>();
            if (_interactable == null)
                Debug.LogError("InteractionTrigger: на объекте нет IInteractable компонента.");

            var col = GetComponent<Collider>();
            if (!col.isTrigger)
                col.isTrigger = true;
            var rb = GetComponent<Rigidbody>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            if (prompt == null)
                prompt = FindObjectOfType<InteractionPrompt>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((playerLayerMask.value & (1 << other.gameObject.layer)) == 0)
                return;
            _playerInRange = true;
            prompt?.ShowPrompt(other.transform);
        }

        private void OnTriggerExit(Collider other)
        {
            if ((playerLayerMask.value & (1 << other.gameObject.layer)) == 0)
                return;
            _playerInRange = false;
            prompt?.HidePrompt();
        }

        private void Update()
        {
            if (_playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                prompt?.HidePrompt();
                _interactable?.ExecuteInteraction();
            }
        }
    }
} 
using UnityEngine;
using Otrabotka.Scenario.Configs;
using Otrabotka.Systems;

namespace Otrabotka.Systems
{
    public class DialogueTrigger : MonoBehaviour
    {
        [Tooltip("Ассет диалога, который будет запущен")]        
        [SerializeField] private DialogueAsset asset;
        [Tooltip("При открытии диалога останавливать время?")]
        [SerializeField] private bool pauseOnOpen = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (pauseOnOpen)
                    Time.timeScale = 0f;

                DialogueManager.Instance.OnDialogueComplete += HandleDialogueComplete;
                DialogueManager.Instance.StartDialogue(asset);
            }
        }

        private void HandleDialogueComplete()
        {
            if (pauseOnOpen)
                Time.timeScale = 1f;

            DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
        }
    }
} 
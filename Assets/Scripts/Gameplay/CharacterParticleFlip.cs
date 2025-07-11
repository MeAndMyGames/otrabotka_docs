using UnityEngine;
using Otrabotka.Systems;

namespace Otrabotka.Gameplay
{
    [RequireComponent(typeof(ParticleSystemRenderer))]
    public class CharacterParticleFlip : MonoBehaviour
    {
        private ParticleSystemRenderer _psRenderer;

        void Awake()
        {
            _psRenderer = GetComponent<ParticleSystemRenderer>();
        }

        void Update()
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
                return;
            if (Input.GetKey(KeyCode.D))
            {
                SetFlipX(1f);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                SetFlipX(0f);
            }
        }

        private void SetFlipX(float value)
        {
            Vector3 flip = _psRenderer.flip;
            if (!Mathf.Approximately(flip.x, value))
            {
                flip.x = value;
                _psRenderer.flip = flip;
            }
        }
    }
} 
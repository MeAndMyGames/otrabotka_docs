using UnityEngine;

namespace Otrabotka.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Configs/EventDialogSettings", fileName = "EventDialogSettings")]
    public class EventDialogSettings : ScriptableObject
    {
        [Header("UI Animation")]
        [SerializeField] private float dialogFadeInTime = 0.3f;
        [SerializeField] private float dialogFadeOutTime = 0.2f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Text Settings")]
        [SerializeField] private float textTypeSpeed = 0.05f;
        [SerializeField] private Color successButtonColor = Color.green;
        [SerializeField] private Color failButtonColor = Color.red;
        [SerializeField] private Color neutralButtonColor = Color.gray;

        [Header("Audio")]
        [SerializeField] private AudioClip dialogOpenSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip eventFailSound;

        public float DialogFadeInTime => dialogFadeInTime;
        public float DialogFadeOutTime => dialogFadeOutTime;
        public AnimationCurve FadeCurve => fadeCurve;
        public float TextTypeSpeed => textTypeSpeed;
        public Color SuccessButtonColor => successButtonColor;
        public Color FailButtonColor => failButtonColor;
        public Color NeutralButtonColor => neutralButtonColor;
        public AudioClip DialogOpenSound => dialogOpenSound;
        public AudioClip ButtonClickSound => buttonClickSound;
        public AudioClip EventFailSound => eventFailSound;
    }
} 
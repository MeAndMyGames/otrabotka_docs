namespace Otrabotka.Systems
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;
    using Otrabotka.Core;
    using Otrabotka.Interfaces;

    [DisallowMultipleComponent]
    public class CinematicController : MonoBehaviour, ICinematicPlayer
    {
        public static CinematicController Instance { get; private set; }
        public UnityEvent onCinematicComplete;
        [SerializeField] private string failureSceneName = "FailureCinematic";

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ServiceLocator.Register<ICinematicPlayer>(this);
            }
        }

        public void PlayFailureCinematic(Action onComplete)
        {
            onCinematicComplete.AddListener(() => onComplete?.Invoke());
            SceneManager.LoadScene(failureSceneName);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Otrabotka.Core;

namespace Otrabotka.Systems
{
    /// <summary>
    /// Точка входа: автоматически находит все менеджеры в сцене и управляет их жизненным циклом.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private List<IInitializable> _initializables;
        private List<IUpdatable> _updatables;
        private List<IShutdownable> _shutdownables;

        private void Awake()
        {
            // Не уничтожать при смене сцены
            DontDestroyOnLoad(gameObject);

            // Автоматически собираем все компоненты-менеджеры
            var allBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);

            _initializables = allBehaviours.OfType<IInitializable>().ToList();
            _updatables = allBehaviours.OfType<IUpdatable>().ToList();
            _shutdownables = allBehaviours.OfType<IShutdownable>().ToList();

            // Инициализируем всех менеджеров
            foreach (var init in _initializables)
            {
                try { init.Initialize(); }
                catch (Exception ex) { Debug.LogError($"Ошибка инициализации {init.GetType().Name}: {ex}"); }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var upd in _updatables)
            {
                try { upd.Tick(dt); }
                catch (Exception ex) { Debug.LogError($"Ошибка Tick у {upd.GetType().Name}: {ex}"); }
            }
        }

        private void OnDestroy()
        {
            foreach (var shut in _shutdownables)
            {
                try { shut.Shutdown(); }
                catch (Exception ex) { Debug.LogError($"Ошибка Shutdown у {shut.GetType().Name}: {ex}"); }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // После загрузки новой сцены добавляем новых менеджеров
            var newBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);

            // Ищем компоненты, которых ещё нет
            var newInits = newBehaviours.OfType<IInitializable>()
                .Where(i => !_initializables.Contains(i));
            var newUps = newBehaviours.OfType<IUpdatable>()
                .Where(u => !_updatables.Contains(u));
            var newShuts = newBehaviours.OfType<IShutdownable>()
                .Where(s => !_shutdownables.Contains(s));

            // Добавление и инициализация новых
            foreach (var init in newInits)
            {
                _initializables.Add(init);
                try { init.Initialize(); }
                catch (Exception ex) { Debug.LogError($"Ошибка инициализации {init.GetType().Name}: {ex}"); }
            }
            _updatables.AddRange(newUps);
            _shutdownables.AddRange(newShuts);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Otrabotka.Core;

namespace Otrabotka.Systems
{
    /// <summary>
    /// ����� �����: ������������� ������� ��� ��������� � ����� � ��������� �� ��������� ������.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private List<IInitializable> _initializables;
        private List<IUpdatable> _updatables;
        private List<IShutdownable> _shutdownables;

        private void Awake()
        {
            // сохраняем свой объект между загрузками сцен
            DontDestroyOnLoad(gameObject);
            // Загружаем прогресс взаимодействий и событий
            ScenarioSaveManager.LoadProgress();
            // Инициализация TransportController для централизованной обработки транспорта
            if (TransportController.Instance == null)
            {
                var transportGO = new GameObject("TransportController");
                transportGO.AddComponent<TransportController>();
                DontDestroyOnLoad(transportGO);
            }

            // собираем все IInitializable, IUpdatable, IShutdownable
            var allBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);

            _initializables = allBehaviours.OfType<IInitializable>().ToList();
            _updatables = allBehaviours.OfType<IUpdatable>().ToList();
            _shutdownables = allBehaviours.OfType<IShutdownable>().ToList();

            // �������������� ���� ����������
            foreach (var init in _initializables)
            {
                try { init.Initialize(); }
                catch (Exception ex) { Debug.LogError($"������ ������������� {init.GetType().Name}: {ex}"); }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var upd in _updatables)
            {
                try { upd.Tick(dt); }
                catch (Exception ex) { Debug.LogError($"������ Tick � {upd.GetType().Name}: {ex}"); }
            }
        }

        private void OnDestroy()
        {
            foreach (var shut in _shutdownables)
            {
                try { shut.Shutdown(); }
                catch (Exception ex) { Debug.LogError($"������ Shutdown � {shut.GetType().Name}: {ex}"); }
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
            // ����� �������� ����� ����� ��������� ����� ����������
            var newBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsSortMode.None);

            // ���� ����������, ������� ��� ���
            var newInits = newBehaviours.OfType<IInitializable>()
                .Where(i => !_initializables.Contains(i));
            var newUps = newBehaviours.OfType<IUpdatable>()
                .Where(u => !_updatables.Contains(u));
            var newShuts = newBehaviours.OfType<IShutdownable>()
                .Where(s => !_shutdownables.Contains(s));

            // ���������� � ������������� �����
            foreach (var init in newInits)
            {
                _initializables.Add(init);
                try { init.Initialize(); }
                catch (Exception ex) { Debug.LogError($"������ ������������� {init.GetType().Name}: {ex}"); }
            }
            _updatables.AddRange(newUps);
            _shutdownables.AddRange(newShuts);
        }
    }
}

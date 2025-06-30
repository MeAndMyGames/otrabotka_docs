using UnityEngine;

namespace Otrabotka.Core
{
    /// <summary>
    /// Интерфейс для инициализации менеджеров.
    /// </summary>
    public interface IInitializable
    {
        void Initialize();
    }

    /// <summary>
    /// Интерфейс для обновления менеджеров каждый кадр.
    /// </summary>
    public interface IUpdatable
    {
        /// <param name="deltaTime">Время в секундах с последнего кадра.</param>
        void Tick(float deltaTime);
    }

    /// <summary>
    /// Интерфейс для корректного завершения работы менеджеров.
    /// </summary>
    public interface IShutdownable
    {
        void Shutdown();
    }

    /// <summary>
    /// Базовый абстрактный класс для всех менеджеров игры.
    /// Определяет единый жизненный цикл: Initialize, Tick, Shutdown.
    /// </summary>
    public abstract class ManagerBase : MonoBehaviour, IInitializable, IUpdatable, IShutdownable
    {
        /// <summary>
        /// Вызывается при старте сцены или игры. Здесь можно подгрузить зависимости и подготовить начальное состояние.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Ежекарновое обновление логики менеджера.
        /// </summary>
        /// <param name="deltaTime">Время в секундах с последнего кадра.</param>
        public abstract void Tick(float deltaTime);

        /// <summary>
        /// Вызывается при выгрузке сцены или выходе из игры. Освобождает ресурсы и сохраняет прогресс.
        /// </summary>
        public abstract void Shutdown();
    }
}

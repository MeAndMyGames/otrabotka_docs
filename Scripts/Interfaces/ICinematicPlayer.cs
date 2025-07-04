namespace Otrabotka.Interfaces
{
    using System;

    /// <summary>
    /// Контракт для проигрывания кинематиков.
    /// </summary>
    public interface ICinematicPlayer
    {
        void PlayFailureCinematic(Action onComplete);
    }
}
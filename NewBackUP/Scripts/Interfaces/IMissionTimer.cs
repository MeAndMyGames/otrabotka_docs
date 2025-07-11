namespace Otrabotka.Interfaces
{
    using System;

    /// <summary>
    /// Контракт для системы учёта реального времени миссии с учётом игровых задержек.
    /// </summary>
    public interface IMissionTimer
    {
        void RegisterShift(float hours);
        event Action OnTimeout;
    }
}
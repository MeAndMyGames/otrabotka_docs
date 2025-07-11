namespace Otrabotka.Interfaces
{
    using System;

    /// <summary>
    ///  онтракт дл€ проигрывани€ кинематиков.
    /// </summary>
    public interface ICinematicPlayer
    {
        void PlayFailureCinematic(Action onComplete);
    }
}
namespace Otrabotka.Interfaces
{
    using System;

    /// <summary>
    /// �������� ��� ������������ �����������.
    /// </summary>
    public interface ICinematicPlayer
    {
        void PlayFailureCinematic(Action onComplete);
    }
}
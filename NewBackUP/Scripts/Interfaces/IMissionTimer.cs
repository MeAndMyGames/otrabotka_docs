namespace Otrabotka.Interfaces
{
    using System;

    /// <summary>
    /// �������� ��� ������� ����� ��������� ������� ������ � ������ ������� ��������.
    /// </summary>
    public interface IMissionTimer
    {
        void RegisterShift(float hours);
        event Action OnTimeout;
    }
}
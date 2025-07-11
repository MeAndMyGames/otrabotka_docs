using UnityEngine;

namespace Otrabotka.Core
{
    /// <summary>
    /// ��������� ��� ������������� ����������.
    /// </summary>
    public interface IInitializable
    {
        void Initialize();
    }

    /// <summary>
    /// ��������� ��� ���������� ���������� ������ ����.
    /// </summary>
    public interface IUpdatable
    {
        /// <param name="deltaTime">����� � �������� � ���������� �����.</param>
        void Tick(float deltaTime);
    }

    /// <summary>
    /// ��������� ��� ����������� ���������� ������ ����������.
    /// </summary>
    public interface IShutdownable
    {
        void Shutdown();
    }

    /// <summary>
    /// ������� ����������� ����� ��� ���� ���������� ����.
    /// ���������� ������ ��������� ����: Initialize, Tick, Shutdown.
    /// </summary>
    public abstract class ManagerBase : MonoBehaviour, IInitializable, IUpdatable, IShutdownable
    {
        /// <summary>
        /// ���������� ��� ������ ����� ��� ����. ����� ����� ���������� ����������� � ����������� ��������� ���������.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// ����������� ���������� ������ ���������.
        /// </summary>
        /// <param name="deltaTime">����� � �������� � ���������� �����.</param>
        public abstract void Tick(float deltaTime);

        /// <summary>
        /// ���������� ��� �������� ����� ��� ������ �� ����. ����������� ������� � ��������� ��������.
        /// </summary>
        public abstract void Shutdown();
    }
}

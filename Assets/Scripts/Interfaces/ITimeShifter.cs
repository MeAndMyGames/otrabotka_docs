namespace Otrabotka.Interfaces
{
    /// <summary>
    /// Контракт для систем, умеющих сдвигать игровое время.
    /// </summary>
    public interface ITimeShifter
    {
        void ShiftTime(float hours);
    }
}
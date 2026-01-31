namespace KalponicStudio.Health.Extensions.Persistence
{
    public interface IHealthSerializable
    {
        HealthSnapshot CaptureSnapshot();
        void RestoreSnapshot(HealthSnapshot snapshot);
    }
}

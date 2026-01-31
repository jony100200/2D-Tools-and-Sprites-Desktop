namespace KalponicStudio
{
    /// <summary>
    /// Maps high-level AnimationType values to internal AnimationId/state names.
    /// </summary>
    public interface IAnimationMapper
    {
        bool TryGetId(AnimationType type, out AnimationId id);
        string GetStateName(AnimationType type);
    }

    /// <summary>
    /// Default mapper using AnimationTypeExtensions.
    /// </summary>
    public sealed class DefaultAnimationMapper : IAnimationMapper
    {
        public bool TryGetId(AnimationType type, out AnimationId id) => type.TryToAnimationId(out id);

        public string GetStateName(AnimationType type) => type.ToStateName();
    }
}

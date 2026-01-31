using System;

namespace KalponicStudio
{
    public static class AnimationTypeExtensions
    {
        public static bool TryToAnimationId(this AnimationType type, out AnimationId id)
        {
            switch (type)
            {
                case AnimationType.Idle: id = AnimationId.Idle; return true;
                case AnimationType.Walk: id = AnimationId.Walk; return true;
                case AnimationType.Run: id = AnimationId.Run; return true;
                case AnimationType.Jump: id = AnimationId.Jump; return true;
                case AnimationType.Fall: id = AnimationId.Fall; return true;
                case AnimationType.Land: id = AnimationId.Land; return true;
                case AnimationType.Attack: id = AnimationId.Attack1; return true;
                case AnimationType.AttackHeavy: id = AnimationId.Attack2; return true;
                case AnimationType.Hurt: id = AnimationId.Hit; return true;
                case AnimationType.Death: id = AnimationId.Dead; return true;
                case AnimationType.Crouch: id = AnimationId.Crouch; return true;
                case AnimationType.Dash: id = AnimationId.Dash; return true;
                case AnimationType.Climb: id = AnimationId.Climb; return true;
                case AnimationType.WallSlide: id = AnimationId.WallSlide; return true;
                case AnimationType.Freeze: id = AnimationId.Freeze; return true;
                default:
                    id = AnimationId.Idle;
                    return false;
            }
        }

        public static string ToStateName(this AnimationType type)
        {
            return Enum.GetName(typeof(AnimationType), type);
        }
    }
}

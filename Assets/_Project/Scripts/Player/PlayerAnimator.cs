using UnityEngine;

namespace PuzzleMaster.Player
{
    /// <summary>
    /// Drives the character's Animator from gameplay (PB-26, ADR-0012). Kept deliberately simple,
    /// as in PA1: the CharacterController moves the player and the animation only follows.
    /// Speed → Idle / Walk / Run blend; every swing → the MeleeSwing clip (Attack trigger).
    /// Accepted glitches: some foot sliding, and a swing pressed mid-clip restarts nothing
    /// (the attack cooldown is about the clip length).
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        // Parameter names hashed once; setting by int id avoids a string lookup every frame.
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int AttackId = Animator.StringToHash("Attack");

        [Tooltip("Animator on the character model (e.g. Model/Witch).")]
        [SerializeField] private Animator animator;
        [Tooltip("Speed source.")]
        [SerializeField] private PlayerMotor motor;
        [Tooltip("Swing source.")]
        [SerializeField] private PlayerAttack attack;
        [Tooltip("Smoothing of the Speed parameter in seconds.")]
        [SerializeField, Min(0f)] private float speedDampTime = 0.1f;

        private void OnEnable()
        {
            if (attack != null) attack.Swung += OnSwung;
        }

        private void OnDisable()
        {
            if (attack != null) attack.Swung -= OnSwung;
        }

        private void Update()
        {
            if (animator == null || motor == null) return;
            // The damped overload eases the value toward the real speed, so the blend doesn't snap.
            animator.SetFloat(SpeedId, motor.CurrentSpeed, speedDampTime, Time.deltaTime);
        }

        // Triggers fire once and reset automatically when the transition consumes them.
        private void OnSwung()
        {
            if (animator != null) animator.SetTrigger(AttackId);
        }
    }
}

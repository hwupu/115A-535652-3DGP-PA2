using UnityEngine;

namespace PuzzleMaster.Combat
{
    /// <summary>
    /// A monster's spell (PB-10): flies straight at a fixed speed and damages the first Health of
    /// the other team it touches; any solid collider (wall, prop, ground) destroys it.
    /// Needs a trigger collider and a kinematic Rigidbody on the root (trigger events with the
    /// player's CharacterController need a Rigidbody on one side). Spawned by SpellCaster.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SpellProjectile : MonoBehaviour
    {
        [Tooltip("Seconds before the spell disappears if it hits nothing.")]
        [SerializeField] private float lifetime = 5f;

        private Vector3 velocity;
        private float damage;
        private Team ownerTeam;
        private Transform owner;
        private Rigidbody body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;   // moved by code, not by physics
        }

        /// <summary>Called by the caster right after spawning.</summary>
        public void Launch(Vector3 direction, float speed, float hitDamage, Team team, Transform caster)
        {
            velocity = direction.normalized * speed;
            damage = hitDamage;
            ownerTeam = team;
            owner = caster;
            transform.rotation = Quaternion.LookRotation(velocity);
            Destroy(gameObject, lifetime);
        }

        // Kinematic bodies are moved with MovePosition in FixedUpdate, so physics sees the movement.
        private void FixedUpdate() => body.MovePosition(body.position + velocity * Time.fixedDeltaTime);

        // RUBRIC 1b · Monster attack / recovery (10): the spell damages the first Health of the other team it touches.
        private void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner)) return;   // don't hit the caster

            var target = other.GetComponentInParent<Health>();
            if (target != null && target.Team != ownerTeam)
            {
                target.Damage(damage);
                Destroy(gameObject);
                return;
            }
            // Other triggers (pickups, puzzle zones) don't stop spells; solid geometry does.
            if (!other.isTrigger && target == null) Destroy(gameObject);
        }
    }
}

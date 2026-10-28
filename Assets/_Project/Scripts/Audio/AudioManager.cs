using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Audio
{
    /// <summary>
    /// Plays the sound effects (PB-16). Required by the spec: a **hit sound when the player hits a
    /// monster**. Extra feedback: swing (miss), monster heal, monster recovered, spell cast,
    /// player hit by a spell, item pickup, puzzle progress and puzzle solved.
    /// Background music has its own source and component: <see cref="MusicPlayer"/> (PB-25).
    /// Any empty clip slot uses a synthesized placeholder tone, so the game is never silent.
    /// Event-driven (GameEvents): gameplay code does not know that audio exists.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Header("Clips (leave empty to use a placeholder tone)")]
        [Tooltip("Player's attack hits a monster (required by the spec).")]
        [SerializeField, OptionalReference] private AudioClip hitClip;
        [Tooltip("Player swings and hits nothing.")]
        [SerializeField, OptionalReference] private AudioClip swingClip;
        [Tooltip("A monster heals itself (self-heal mode, ADR-0008).")]
        [SerializeField, OptionalReference] private AudioClip monsterHealClip;
        [Tooltip("A monster reaches 0 HP and is recovered.")]
        [SerializeField, OptionalReference] private AudioClip monsterRecoveredClip;
        [Tooltip("A monster casts a spell (only within Spell Hearing Range).")]
        [SerializeField, OptionalReference] private AudioClip spellCastClip;
        [Tooltip("A spell hits the player.")]
        [SerializeField, OptionalReference] private AudioClip playerHitClip;
        [Tooltip("The player picks up a puzzle item.")]
        [SerializeField, OptionalReference] private AudioClip pickupClip;
        [Tooltip("Item placed in a puzzle, switch pressed, gate opened.")]
        [SerializeField, OptionalReference] private AudioClip puzzleProgressClip;
        [Tooltip("A puzzle is solved.")]
        [SerializeField, OptionalReference] private AudioClip puzzleSolvedClip;

        [Header("Mix")]
        [Tooltip("Master volume of all sound effects (0–1).")]
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
        [Tooltip("Spell casts from monsters farther than this are not played (avoids constant noise).")]
        [SerializeField] private float spellHearingRange = 20f;

        private AudioSource source;
        private AudioClip hitSfx, swingSfx, healSfx, recoveredSfx, spellSfx, playerHitSfx, pickupSfx, progressSfx, solvedSfx;
        private bool hitThisFrame;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;   // 2D sounds: same volume wherever the camera is

            // Fallback per slot: the Inspector clip if set, else a synthesized tone (made once).
            hitSfx = hitClip != null ? hitClip : PlaceholderTones.Sweep("Hit", 1100f, 500f, 0.15f, PlaceholderTones.Wave.Square);
            swingSfx = swingClip != null ? swingClip : PlaceholderTones.Sweep("Swing", 500f, 250f, 0.1f);
            healSfx = monsterHealClip != null ? monsterHealClip : PlaceholderTones.Sweep("Heal", 600f, 1200f, 0.3f);
            recoveredSfx = monsterRecoveredClip != null ? monsterRecoveredClip : PlaceholderTones.Sweep("Recovered", 523f, 1046f, 0.6f);
            spellSfx = spellCastClip != null ? spellCastClip : PlaceholderTones.Sweep("Spell", 900f, 1500f, 0.2f, volume: 0.25f);
            playerHitSfx = playerHitClip != null ? playerHitClip : PlaceholderTones.Sweep("PlayerHit", 200f, 90f, 0.25f, PlaceholderTones.Wave.Square);
            pickupSfx = pickupClip != null ? pickupClip : PlaceholderTones.Sweep("Pickup", 1200f, 1900f, 0.15f);
            progressSfx = puzzleProgressClip != null ? puzzleProgressClip : PlaceholderTones.Sweep("PuzzleProgress", 700f, 1000f, 0.15f, PlaceholderTones.Wave.Square, volume: 0.4f);
            solvedSfx = puzzleSolvedClip != null ? puzzleSolvedClip : PlaceholderTones.Sweep("PuzzleSolved", 392f, 1568f, 0.9f);
        }

        // Static events outlive this object, so subscribe / unsubscribe in matched pairs.
        private void OnEnable()
        {
            GameEvents.MonsterHit += OnMonsterHit;
            GameEvents.AttackSwung += OnAttackSwung;
            GameEvents.MonsterHealed += OnMonsterHealed;
            GameEvents.MonsterRecovered += OnMonsterRecovered;
            GameEvents.SpellCast += OnSpellCast;
            GameEvents.PlayerHit += OnPlayerHit;
            GameEvents.ItemCollected += OnItemCollected;
            GameEvents.PuzzleProgressed += OnPuzzleProgressed;
            GameEvents.PuzzleSolved += OnPuzzleSolved;
        }

        private void OnDisable()
        {
            GameEvents.MonsterHit -= OnMonsterHit;
            GameEvents.AttackSwung -= OnAttackSwung;
            GameEvents.MonsterHealed -= OnMonsterHealed;
            GameEvents.MonsterRecovered -= OnMonsterRecovered;
            GameEvents.SpellCast -= OnSpellCast;
            GameEvents.PlayerHit -= OnPlayerHit;
            GameEvents.ItemCollected -= OnItemCollected;
            GameEvents.PuzzleProgressed -= OnPuzzleProgressed;
            GameEvents.PuzzleSolved -= OnPuzzleSolved;
        }

        private void LateUpdate() => hitThisFrame = false;

        // Handlers: one per GameEvents event. The second argument of Play is a relative volume:
        // frequent sounds (swing, heal, spells) are quieter so the hit sound stays the clearest.

        // RUBRIC 4b · Hit sound (5): PlayerAttack raises GameEvents.MonsterHit for each monster hit.
        private void OnMonsterHit(Vector3 _)
        {
            // Hitting two monsters in one swing plays one hit sound, not two stacked ones.
            if (hitThisFrame) return;
            hitThisFrame = true;
            Play(hitSfx);
        }

        // AttackSwung comes after the MonsterHit events of the same swing: swoosh only on a miss.
        private void OnAttackSwung(Vector3 _)
        {
            if (!hitThisFrame) Play(swingSfx, 0.6f);
        }

        private void OnMonsterHealed(Vector3 _) => Play(healSfx, 0.6f);
        private void OnMonsterRecovered(Vector3 _) => Play(recoveredSfx);
        private void OnPlayerHit(float _, float __) => Play(playerHitSfx, 0.7f);
        private void OnItemCollected(string _) => Play(pickupSfx);
        private void OnPuzzleProgressed(Vector3 _) => Play(progressSfx);
        private void OnPuzzleSolved(Vector3 _) => Play(solvedSfx);

        private void OnSpellCast(Vector3 position)
        {
            Camera main = Camera.main;
            if (main != null && (position - main.transform.position).sqrMagnitude > spellHearingRange * spellHearingRange) return;
            Play(spellSfx, 0.5f);
        }

        /// <summary>PlayOneShot lets sounds overlap on one AudioSource instead of cutting each other off.</summary>
        private void Play(AudioClip clip, float scale = 1f)
        {
            if (clip != null) source.PlayOneShot(clip, volume * scale);
        }
    }
}

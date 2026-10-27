using System;
using UnityEngine;

namespace PuzzleMaster.Core
{
    /// <summary>
    /// Game-wide events (ADR-0008). Monsters, spells and pickups are prefabs and can't hold
    /// references to scene systems, so they announce what happened here, and the score,
    /// audio and UI listen. The raisers don't know who listens (loose coupling).
    /// Arguments are plain values (positions, names), so Core needs no gameplay types.
    /// Listeners subscribe in OnEnable and unsubscribe in OnDisable: static events outlive objects.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>The player swung (hit or miss). Position = player.</summary>
        public static event Action<Vector3> AttackSwung;
        /// <summary>A player attack damaged a monster. Position = monster.</summary>
        public static event Action<Vector3> MonsterHit;
        /// <summary>A monster healed itself. Position = monster.</summary>
        public static event Action<Vector3> MonsterHealed;
        /// <summary>A monster reached 0 HP and was recovered (purified). Position = monster.</summary>
        public static event Action<Vector3> MonsterRecovered;
        /// <summary>A monster cast a spell. Position = caster.</summary>
        public static event Action<Vector3> SpellCast;
        /// <summary>A spell hit the player: (current HP, max HP). Background HP only (PO decision).</summary>
        public static event Action<float, float> PlayerHit;
        /// <summary>The player picked up an item: its display name.</summary>
        public static event Action<string> ItemCollected;
        /// <summary>A puzzle made progress (item placed, switch pressed, gate opened). Position = puzzle.</summary>
        public static event Action<Vector3> PuzzleProgressed;
        /// <summary>A puzzle was solved. Position = puzzle.</summary>
        public static event Action<Vector3> PuzzleSolved;
        /// <summary>A short message for the player (puzzle hints, "solved!"), shown by the HUD.</summary>
        public static event Action<string> PuzzleMessage;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Needed when "Enter Play Mode Options" skip the domain reload: drop stale listeners.
            AttackSwung = null;
            MonsterHit = null;
            MonsterHealed = null;
            MonsterRecovered = null;
            SpellCast = null;
            PlayerHit = null;
            ItemCollected = null;
            PuzzleProgressed = null;
            PuzzleSolved = null;
            PuzzleMessage = null;
        }

        // Only this class can invoke its events, so other classes call these methods.
        public static void RaiseAttackSwung(Vector3 position) => AttackSwung?.Invoke(position);
        public static void RaiseMonsterHit(Vector3 position) => MonsterHit?.Invoke(position);
        public static void RaiseMonsterHealed(Vector3 position) => MonsterHealed?.Invoke(position);
        public static void RaiseMonsterRecovered(Vector3 position) => MonsterRecovered?.Invoke(position);
        public static void RaiseSpellCast(Vector3 position) => SpellCast?.Invoke(position);
        public static void RaisePlayerHit(float current, float max) => PlayerHit?.Invoke(current, max);
        public static void RaiseItemCollected(string itemName) => ItemCollected?.Invoke(itemName);
        public static void RaisePuzzleProgressed(Vector3 position) => PuzzleProgressed?.Invoke(position);
        public static void RaisePuzzleSolved(Vector3 position) => PuzzleSolved?.Invoke(position);
        public static void RaisePuzzleMessage(string message) => PuzzleMessage?.Invoke(message);
    }
}

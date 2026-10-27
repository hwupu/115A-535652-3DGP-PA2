using System;
using UnityEngine;

namespace PuzzleMaster.Core
{
    /// <summary>
    /// Holds the score (PB-15). Sources call <see cref="Add"/> (monster recovered +100,
    /// puzzle solved +200, from config.json; connected in Sprints 2 and 3). The HUD and the
    /// win / lose screens listen to <see cref="Changed"/> or read <see cref="Score"/>.
    /// The score resets with the scene on restart.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        /// <summary>Raised with the new total whenever points are added.</summary>
        public event Action<int> Changed;

        public int Score { get; private set; }

        /// <summary>Adds points (ignored if not positive) and announces the new total.</summary>
        public void Add(int points, string reason)
        {
            if (points <= 0) return;
            Score += points;
            Debug.Log($"Score +{points} ({reason}) = {Score}");
            Changed?.Invoke(Score);
        }
    }
}

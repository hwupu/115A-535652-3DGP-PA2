using System.Collections;
using PuzzleMaster.Core;
using UnityEngine;

namespace PuzzleMaster.Audio
{
    /// <summary>
    /// Background music (PB-25, required by the spec). Follows the game state (ADR-0006):
    /// silent on the intro screen, starts when gameplay begins (SPACE), loops, and fades out on
    /// Win / Lose. R reloads the scene, so the music starts again from the beginning.
    /// It has its own AudioSource, separate from the AudioManager's SFX source, so the music
    /// volume is independent and the music is never cut off by sound effects.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [Tooltip("Game state source: music plays only while Playing.")]
        [SerializeField] private GameStateManager gameState;
        [Tooltip("The looping music track.")]
        [SerializeField] private AudioClip music;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float volume = 0.3f;
        [Tooltip("Seconds the music takes to fade out on Win / Lose.")]
        [SerializeField, Min(0f)] private float fadeOutSeconds = 1.5f;

        private AudioSource source;
        private Coroutine fade;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;   // 2D: same loudness wherever the camera is
            source.priority = 0;        // highest priority: never culled when many SFX play at once
            source.clip = music;
            if (music == null) Debug.LogWarning($"{nameof(MusicPlayer)}: Music is not assigned; the game has no background music.", this);
        }

        private void OnEnable()
        {
            if (gameState == null)
            {
                Debug.LogError($"{nameof(MusicPlayer)}: Game State is not assigned; the music won't start.", this);
                return;
            }
            gameState.StateChanged += OnStateChanged;
            OnStateChanged(gameState.State);   // in case the first state was announced before we subscribed
        }

        private void OnDisable()
        {
            if (gameState != null) gameState.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Playing) Play();
            else if (source.isPlaying) FadeOut();
        }

        private void Play()
        {
            if (music == null || source.isPlaying) return;
            if (fade != null) StopCoroutine(fade);
            source.volume = volume;
            source.Play();
        }

        private void FadeOut()
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            float start = source.volume;
            // Unscaled time: Win / Lose set timeScale to 0, which would freeze a scaled fade.
            for (float t = 0f; t < fadeOutSeconds; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(start, 0f, t / fadeOutSeconds);
                yield return null;
            }
            source.Stop();
            source.volume = volume;
            fade = null;
        }
    }
}

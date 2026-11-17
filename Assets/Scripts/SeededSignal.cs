using UnityEngine;

namespace RelayYard
{
    // The six-switch logic stays deterministic; only its diagnostic signal
    // varies per run. Soundtrack tempo sets the visual beat, not the puzzle rules.
    public sealed class SeededSignal : MonoBehaviour
    {
        private const float GameplayBpm = 132f;
        private const float SwitchPulseSeconds = 0.65f;
        private const float SolveTransitionSeconds = 0.6f;

        [SerializeField] private int seed = 386;
        [SerializeField, Range(24, 160)] private int sampleCount = 96;
        [SerializeField, Range(0.2f, 10f)] private float noiseScale = 3.8f;
        [SerializeField, Range(0f, 1f)] private float noiseStrength = 0.24f;
        [SerializeField, Range(0f, 2f)] private float waveAmplitude = 0.24f;
        [SerializeField, Range(0f, 5f)] private float speed = 0.65f;
        [SerializeField, Range(-2f, 2f)] private float beatOffsetSeconds = 0f;

        private LineRenderer line;
        private bool solved;
        private float startTime;
        private float switchPulseRemaining;
        private float solvedBlend;
        private float beatClock;
        private int runtimeSeed;
        private int waveformFamily;
        private float noiseOffsetX;
        private float noiseOffsetY;
        private float runtimeNoiseStrength;
        private float carrierFrequency;
        private float carrierPhase;

        private void Awake()
        {
            // Frame count and elapsed real time change between scene loads,
            // including Start, R-hold reset, and the mouse Restart button.
            // The seed is picked ONCE, not continuously while drawing.
            unchecked
            {
                int elapsedMs = (int)(Time.realtimeSinceStartupAsDouble * 1000.0);
                runtimeSeed = seed * 73856093 ^
                              Time.frameCount * 19349663 ^
                              elapsedMs * 83492791;
            }

            var random = new System.Random(runtimeSeed);
            waveformFamily = random.Next(0, 3); // sine / triangle / softened square
            noiseOffsetX = (float)random.NextDouble() * 2048f;
            noiseOffsetY = (float)random.NextDouble() * 2048f;
            runtimeNoiseStrength = noiseStrength * Mathf.Lerp(
                0.75f, 1.3f, (float)random.NextDouble());
            carrierFrequency = Mathf.Lerp(
                2f, 3f, (float)random.NextDouble());
            carrierPhase = (float)random.NextDouble() * Mathf.PI * 2f;

            Debug.Log("[Relay Yard] Signal seed " + runtimeSeed +
                ", family " + waveformFamily + ", BPM " + GameplayBpm);

            line = gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = sampleCount;
            line.widthMultiplier = 0.035f;
            line.numCornerVertices = 2;
            line.sortingOrder = -5;
            // URP 2D draws the procedural trace with a compatible unlit shader.
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader != null)
                line.material = new Material(shader);
            startTime = Time.time;
        }

        // Repeated presses refresh a single burst rather than accumulating effects.
        public void PulseSwitch()
        {
            if (!solved) switchPulseRemaining = SwitchPulseSeconds;
        }

        public void SetSolved(bool value)
        {
            solved = value;
            if (value) switchPulseRemaining = 0f;
        }

        private void Update()
        {
            if (line == null) return;

            // Time-scaled effects freeze on pause, like the original waveform.
            switchPulseRemaining = Mathf.Max(0f, switchPulseRemaining - Time.deltaTime);
            solvedBlend = Mathf.MoveTowards(solvedBlend, solved ? 1f : 0f,
                Time.deltaTime / SolveTransitionSeconds);

            // The persistent AudioDirector does not restart its BGM on a Game reset.
            // Follow that clip's playback position instead of the scene's start time.
            // If no music is available, use a free-running scene clock.
            if (Time.timeScale > 0f)
            {
                AudioDirector audio = AudioDirector.Instance;
                float playback = audio != null ? audio.MusicPlaybackSeconds : -1f;
                beatClock = playback >= 0f ? playback : Time.time - startTime;
            }
            float beatNumber = Mathf.Max(0f, beatClock + beatOffsetSeconds) *
                (GameplayBpm / 60f);
            float beatPhase = beatNumber - Mathf.Floor(beatNumber);
            int beatIndex = Mathf.FloorToInt(beatNumber);
            float beatAccent = beatIndex % 4 == 0 ? 1f : 0.65f;
            float beatPulse = Mathf.Pow(1f - beatPhase, 5f) * beatAccent;

            float pulse = switchPulseRemaining / SwitchPulseSeconds;
            pulse = Mathf.Pow(pulse, 1.5f);

            Color color = Color.Lerp(
                new Color(0.35f, 0.70f, 0.96f, 0.52f),
                new Color(0.45f, 1f, 0.78f, 0.80f),
                solvedBlend);
            line.startColor = color;
            line.endColor = color;

            float t = (Time.time - startTime) * speed;
            int count = Mathf.Max(2, sampleCount);
            if (line.positionCount != count) line.positionCount = count;
            for (int i = 0; i < count; i++)
            {
                float u = i / (float)(count - 1);
                float x = Mathf.Lerp(-7.7f, 7.7f, u);
                float phase = x * carrierFrequency - t * 3.1f + carrierPhase;
                float carrier = Mathf.Sin(phase);
                if (waveformFamily == 1)
                    carrier = Mathf.Asin(carrier) * (2f / Mathf.PI);
                else if (waveformFamily == 2)
                    carrier = Mathf.Clamp(carrier * 2f, -1f, 1f);

                float n = Mathf.PerlinNoise(
                    (x + noiseOffsetX) / noiseScale,
                    (t + noiseOffsetY) / noiseScale);
                float unSolvedY = carrier * waveAmplitude +
                    (n - 0.5f) * 2f * runtimeNoiseStrength;
                float solvedY = Mathf.Sin(x * 2.4f - t * 3.1f) * waveAmplitude;

                // A small 132 BPM emphasis is present during ordinary play.
                // Beat accent alters amplitude, not frequency or puzzle state.
                float y = Mathf.Lerp(unSolvedY, solvedY, solvedBlend)
                    * (1f + 0.18f * beatPulse);

                if (pulse > 0f)
                {
                    float interference =
                        (Mathf.PingPong(x * 2.7f - t * 4.8f + noiseOffsetX, 2f) - 1f) * 0.30f
                        + Mathf.Sin(x * 4.1f + t * 14f) * 0.11f;
                    y += interference * pulse * (1f - solvedBlend);
                }

                line.SetPosition(i, new Vector3(x, y, 0));
            }
        }

        private void OnDestroy()
        {
            if (line != null && line.material != null)
                Destroy(line.material);
        }

#if UNITY_EDITOR
        public void EditorConfigure(int assignedSeed) => seed = assignedSeed;
#endif
    }
}

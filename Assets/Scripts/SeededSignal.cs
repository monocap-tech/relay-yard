using UnityEngine;

namespace RelayYard
{
    // Deterministic Perlin noise and a traveling sine wave form the baseline.
    // Gameplay events only modulate the display; puzzle logic is unaffected.
    public sealed class SeededSignal : MonoBehaviour
    {
        private const float SwitchPulseSeconds = 0.5f;
        private const float SolveTransitionSeconds = 0.6f;

        [SerializeField] private int seed = 386;
        [SerializeField, Range(24, 160)] private int sampleCount = 96;
        [SerializeField, Range(0.2f, 10f)] private float noiseScale = 3.8f;
        [SerializeField, Range(0f, 1f)] private float noiseStrength = 0.24f;
        [SerializeField, Range(0f, 2f)] private float waveAmplitude = 0.24f;
        [SerializeField, Range(0f, 5f)] private float speed = 0.65f;

        private LineRenderer line;
        private bool solved;
        private float startTime;
        private float switchPulseRemaining;
        private float solvedBlend;

        private void Awake()
        {
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

        // Repeated presses refresh one short disturbance rather than stacking effects.
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

            // Both envelopes advance in game time, so pause freezes the signal.
            switchPulseRemaining = Mathf.Max(0f, switchPulseRemaining - Time.deltaTime);
            solvedBlend = Mathf.MoveTowards(solvedBlend, solved ? 1f : 0f,
                Time.deltaTime / SolveTransitionSeconds);

            float pulse = switchPulseRemaining / SwitchPulseSeconds;
            pulse *= pulse; // Quick burst, then a soft decay.

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
                float n = Mathf.PerlinNoise((x + seed) / noiseScale, (t + seed) / noiseScale);
                float wave = Mathf.Sin(x * 2.4f - t * 3.1f) * waveAmplitude;
                // Completion removes noise while keeping the original moving carrier.
                float y = wave + (n - 0.5f) * 2f * noiseStrength * (1f - solvedBlend);

                if (pulse > 0f)
                {
                    // Triangle corners add brief jaggedness without random allocations.
                    float interference =
                        (Mathf.PingPong(x * 2.7f - t * 4.8f + seed, 2f) - 1f) * 0.18f
                        + Mathf.Sin(x * 4.1f + t * 14f) * 0.07f;
                    y += interference * pulse;
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

using UnityEngine;
using USCG.Core.Telemetry;

public class PencilBreakTracker : MonoBehaviour
{
    public static PencilBreakTracker Instance { get; private set; }

    private MetricId _pencilBreakCountMetric;
    private MetricId _pencilBreakLocationMetric;

    private bool _metricsCreated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        TryCreateMetrics();
    }

    private bool TryCreateMetrics()
    {
        if (TelemetryManager.instance == null)
        {
            Debug.LogError(
                "TelemetryManager not found! Pencil breaks cannot be recorded.",
                this
            );

            return false;
        }

        if (!_metricsCreated)
        {
            // Total number of pencil breaks.
            _pencilBreakCountMetric =
                TelemetryManager.instance.CreateAccumulatedMetric(
                    "TotalPencilBreaks"
                );

            // Player position at each pencil break.
            _pencilBreakLocationMetric =
                TelemetryManager.instance.CreateSampledMetric<Vector3>(
                    "PencilBreakLocations"
                );

            _metricsCreated = true;
        }

        return true;
    }

    public void RecordPencilBreak(
        string pencilColor,
        Vector3 playerPosition)
    {
        if (!TryCreateMetrics())
            return;

        // Add one break to the total.
        TelemetryManager.instance.AccumulateMetric(
            _pencilBreakCountMetric,
            1
        );

        // Record where the PLAYER was when it happened.
        TelemetryManager.instance.AddMetricSample(
            _pencilBreakLocationMetric,
            playerPosition
        );

        Debug.Log(
            $"{pencilColor} pencil broke. " +
            $"Player position: {playerPosition}",
            this
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
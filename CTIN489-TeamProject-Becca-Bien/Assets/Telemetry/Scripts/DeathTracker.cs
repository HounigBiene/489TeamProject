using UnityEngine;
using USCG.Core.Telemetry;

public class DeathTracker : MonoBehaviour
{
    public static DeathTracker Instance;

    // Metric IDs
    private MetricId _deathCountMetric;
    private MetricId _deathLocationMetric;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Make sure TelemetryManager exists
        if (TelemetryManager.instance == null)
        {
            Debug.LogError("TelemetryManager not found in scene!");
            return;
        }

        // 1. Accumulated Metric for counting deaths
        _deathCountMetric = TelemetryManager.instance.CreateAccumulatedMetric("TotalDeaths");

        // 2. Sampled Metric for recording death locations (as Vector3)
        _deathLocationMetric = TelemetryManager.instance.CreateSampledMetric<Vector3>("DeathLocations");
    }

    public void RecordDeath(Vector3 deathPosition)
    {
        // Increment death count by 1
        if (TelemetryManager.instance != null)
        {
            TelemetryManager.instance.AccumulateMetric(_deathCountMetric, 1);
            TelemetryManager.instance.AddMetricSample(_deathLocationMetric, deathPosition);
        }
        
        Debug.Log($"Death recorded at {deathPosition}");
    }
}
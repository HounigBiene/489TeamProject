using UnityEngine;
using USCG.Core.Telemetry;

public class CollectibleTimeTracker : MonoBehaviour
{
    public static CollectibleTimeTracker Instance { get; private set; }

    [Header("Tracking")]
    [Tooltip("Record only the first pickup instead of every pickup.")]
    [SerializeField] private bool firstCollectibleOnly = false;

    private MetricId _collectionTimeMetric;

    private double _startTime;
    private bool _timerStarted;
    private bool _metricCreated;
    private bool _hasRecorded;

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
        // Begin timing when this tracker starts in the gameplay scene.
        _startTime = Time.timeAsDouble;
        _timerStarted = true;

        TryCreateMetric();
    }

    private bool TryCreateMetric()
    {
        if (TelemetryManager.instance == null)
        {
            Debug.LogError(
                "TelemetryManager not found! Collection time cannot be recorded.",
                this
            );

            return false;
        }

        if (!_metricCreated)
        {
            _collectionTimeMetric =
                TelemetryManager.instance.CreateSampledMetric<float>(
                    "TimeToCollectibleSeconds"
                );

            _metricCreated = true;
        }

        return true;
    }

    public void RecordCollection(string collectibleName)
    {
        // Ignore later pickups when tracking only the first collectible.
        if (firstCollectibleOnly && _hasRecorded)
            return;

        if (!_timerStarted)
        {
            Debug.LogWarning(
                "Collection happened before the timer started.",
                this
            );

            return;
        }

        if (!TryCreateMetric())
            return;

        float elapsedSeconds = Mathf.Max(
            0f,
            (float)(Time.timeAsDouble - _startTime)
        );

        TelemetryManager.instance.AddMetricSample(
            _collectionTimeMetric,
            elapsedSeconds
        );

        _hasRecorded = true;

        Debug.Log(
            $"Collected {collectibleName} after {elapsedSeconds:F1} seconds.",
            this
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
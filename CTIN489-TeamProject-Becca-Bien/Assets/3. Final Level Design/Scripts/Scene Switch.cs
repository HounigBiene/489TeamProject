
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using USCG.Core.Telemetry;


public class SceneSwitch : MonoBehaviour
{
    [Header("Scene to Load")]
    [SerializeField] private string sceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //record drawing telemetry & delay
            SaveDrawingsTelemetry.SaveDrawingData(sceneName);
            
            //save csv
            if (TelemetryManager.instance != null)
            {
                TelemetryManager.instance.PrintMetricsImmediate();
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}

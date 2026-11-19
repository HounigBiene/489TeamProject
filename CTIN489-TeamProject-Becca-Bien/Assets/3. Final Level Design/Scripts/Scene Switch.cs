
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using USCG.Core.Telemetry;


public class SceneSwitch : MonoBehaviour
{
    [Header("Scene to Load")]
    [SerializeField] private string sceneName;

    [Header("Current Level")]
    [SerializeField] private string currentLevel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //record drawing telemetry & delay
            SaveDrawingsTelemetry.SaveDrawingData(currentLevel);
            
            //save csv
            if (TelemetryManager.instance != null)
            {
                TelemetryManager.instance.PrintMetricsImmediate();
            }

            StartCoroutine(LoadSceneDelay());
        }
    }

    IEnumerator LoadSceneDelay()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(sceneName);
    }
}

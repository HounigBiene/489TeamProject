using UnityEngine;
using System.IO;
using System.Text;

public class SaveDrawingsTelemetry : MonoBehaviour
{
    // A static function so we can call it from anywhere easily!
    public static void SaveDrawingData(string levelName)
    {
        StringBuilder sb = new StringBuilder();
        
        // Find ALL lines drawn in the current scene
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();

        sb.AppendLine($"--- PLAYER DRAWINGS FOR {levelName} ---");
        sb.AppendLine($"Total Strokes: {allLines.Length}");
        sb.AppendLine();
        
        foreach (LineRenderer line in allLines)
        {
            // We only want to save the strokes the player drew
            if (!line.gameObject.name.Contains("Stroke")) continue;

            sb.AppendLine($"Stroke (Color: {line.startColor}):");
            for (int i = 0; i < line.positionCount; i++)
            {
                // Convert local coordinates to world coordinates so you can see where they are on the map
                Vector3 worldPos = line.transform.TransformPoint(line.GetPosition(i));
                sb.AppendLine($"  {worldPos.x}, {worldPos.y}");
            }
            sb.AppendLine(); // Empty line to separate strokes
        }

        // Save the file to the same folder as the TelemetryManager CSVs
        string dateTime = System.DateTime.Now.ToString("HH-mm-ss"); 
        string dateFormatted = System.DateTime.Now.ToString("MM-dd-yyyy"); // To keep it readable
        string filePath = $"{Application.persistentDataPath}/{levelName}_Drawings_{dateFormatted}_{dateTime}.csv";
                
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"Drawing data saved to: {filePath}");
    }
}
using UnityEngine;
using System.IO;
using System.Text;

public class SaveDrawingsTelemetry : MonoBehaviour
{
    public static void SaveDrawingData(string levelName)
    {
        StringBuilder sb = new StringBuilder();
        
        // Find ALL LineRenderers in the scene
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        int validStrokes = 0;

        sb.AppendLine($"--- PLAYER DRAWINGS FOR {levelName} ---");
        
        foreach (LineRenderer line in allLines)
        {
            // Check if the line has points (prevents saving empty lines)
            if (line.positionCount < 2) continue;

            validStrokes++;
            sb.AppendLine($"Stroke (Color: {line.startColor}):");
            
            for (int i = 0; i < line.positionCount; i++)
            {
                // Convert local coordinates to world coordinates
                Vector3 worldPos = line.transform.TransformPoint(line.GetPosition(i));
                sb.AppendLine($"  {worldPos.x}, {worldPos.y}");
            }
            sb.AppendLine(); 
        }

        // Add the total stroke count at the top
        sb.Insert(sb.ToString().IndexOf('\n') + 1, $"Total Strokes: {validStrokes}\n");

        string dateTime = System.DateTime.Now.ToString("HH-mm-ss"); 
        string dateFormatted = System.DateTime.Now.ToString("MM-dd-yyyy"); 
        
        string filePath = $"{Application.persistentDataPath}/{levelName}_Drawings_{dateFormatted}_{dateTime}.csv";
                
        File.WriteAllText(filePath, sb.ToString());
        
        // Force save to disk before scene unloads
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        Debug.Log($"Drawing data saved to: {filePath} with {validStrokes} strokes.");
    }
}
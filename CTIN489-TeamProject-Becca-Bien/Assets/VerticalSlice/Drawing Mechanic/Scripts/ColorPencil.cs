using UnityEngine;

public class ColorPencil : MonoBehaviour
{
    public string colorName = "Green";
    public Color colorValue = Color.green;
    public bool isDrawingPencil = false;
    
    [Header("Drop Settings")]
    public bool canBePickedUp = true; //After swapping pencil, the pencil that we put down can no longer be picked back up
    
    [Header("Pencil Prefabs (for dropping)")]
    public GameObject redPencilPrefab;
    public GameObject yellowPencilPrefab;
    public GameObject bluePencilPrefab;
    public GameObject greenPencilPrefab;
    public GameObject orangePencilPrefab;
    public GameObject purplePencilPrefab;
    public GameObject brownPencilPrefab;
    public GameObject blackPencilPrefab;

    void Start()
    {
        switch (colorName)
        {
            case "Red": colorValue = Color.red; break;
            case "Yellow": colorValue = Color.yellow; break;
            case "Blue": colorValue = Color.blue; break;
            case "Green": colorValue = new Color(0.267f, 0.667f, 0.290f); break;
            case "Orange": colorValue = new Color(0.851f, 0.471f, 0.106f); break;
            case "Purple": colorValue = new Color(0.459f, 0.290f, 0.702f); break;
            case "Brown": colorValue = new Color(0.318f, 0.247f, 0.192f); break;
            case "Black": colorValue = Color.black; break;
        }
    }

    void OnMouseDown()
    {
        // Can't pick up if disabled
        if (!canBePickedUp)
        {
            Debug.Log("This pencil can't be picked up again!");
            return;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        PlayerPencil playerPencil = player.GetComponent<PlayerPencil>();
        if (playerPencil == null) return;
        
        if (playerPencil.isHoldingPencil)
        {
            DropOldPencil(playerPencil);
        }
        
        playerPencil.PickUpPencil(colorName, colorValue, isDrawingPencil);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPencilPickupSound();
        
        Destroy(gameObject);
        Debug.Log("Picked up " + colorName + " pencil!");
    }

    void DropOldPencil(PlayerPencil playerPencil)
    {
        GameObject prefabToSpawn = GetPrefabForColor(playerPencil.heldColorName);
        
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("No prefab found for " + playerPencil.heldColorName);
            return;
        }
        
        Vector3 dropPos = new Vector3(
            transform.position.x,
            transform.position.y + 1f,
            transform.position.z
        );
        
        GameObject droppedPencil = Instantiate(prefabToSpawn, dropPos, transform.rotation);
        
        // DISABLE the dropped pencil so it can't be picked up again
        ColorPencil droppedScript = droppedPencil.GetComponent<ColorPencil>();
        if (droppedScript != null)
        {
            droppedScript.canBePickedUp = false;
        }
        
        // Fade it out visually to show it's disabled
        SpriteRenderer sr = droppedPencil.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.5f; // Half transparent
            sr.color = c;
        }
        
        Debug.Log("Dropped " + playerPencil.heldColorName + " pencil (can't pick up again)");
    }

    GameObject GetPrefabForColor(string colorName)
    {
        switch (colorName)
        {
            case "Red": return redPencilPrefab;
            case "Yellow": return yellowPencilPrefab;
            case "Blue": return bluePencilPrefab;
            case "Green": return greenPencilPrefab;
            case "Orange": return orangePencilPrefab;
            case "Purple": return purplePencilPrefab;
            case "Brown": return brownPencilPrefab;
            case "Black": return blackPencilPrefab;
            default: return null;
        }
    }
}
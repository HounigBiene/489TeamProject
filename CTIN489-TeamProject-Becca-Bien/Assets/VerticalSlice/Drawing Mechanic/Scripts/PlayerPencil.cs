using UnityEngine;
/** 
*
* Script for detecting what pencil has been picked up/set down
*
**/
public class PlayerPencil : MonoBehaviour
{
    public static PlayerPencil Instance;
    
    [Header("Currently Held Pencil")]
    public string heldColorName = "";
    public Color heldColor = Color.white;
    public bool isHoldingPencil = false;
    public bool isDrawingPencil = false;
    
    [Header("Pencil Indicator")]
    public SpriteRenderer pencilIndicator;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (pencilIndicator != null)
            pencilIndicator.gameObject.SetActive(false);
    }

    void Update()
    {
        if (pencilIndicator != null)
        {
            if (isHoldingPencil)
            {
                pencilIndicator.gameObject.SetActive(true);
                pencilIndicator.color = heldColor;
            }
            else
            {
                pencilIndicator.gameObject.SetActive(false);
            }
        }
    }

    public void PickUpPencil(string colorName, Color color, bool isDrawing = false)
    {
        heldColorName = colorName;
        heldColor = color;
        isHoldingPencil = true;
        isDrawingPencil = isDrawing;
        Debug.Log("Picked up: " + colorName + " pencil" + (isDrawing ? " (drawing)" : ""));
    }

    public void DropPencil()
    {
        heldColorName = "";
        heldColor = Color.white;
        isHoldingPencil = false;
        isDrawingPencil = false;
        Debug.Log("Dropped pencil");
    }
}
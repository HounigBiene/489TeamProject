using UnityEngine;
/** 
*
* Script for detecting what pencil has been picked up/set down
*
**/
public class PlayerPencil : MonoBehaviour
{
    public static PlayerPencil Instance;

    [Header("Current Pencil")]
    public ColorPencil currentPencil;
    
    [Header("Current Pencil Details")]
    public string heldColorName = "";
    public Color heldColor = Color.white;
    public bool isHoldingPencil = false;

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
            }
            else
            {
                pencilIndicator.gameObject.SetActive(false);
            }
        }
    }

    public void PickUpPencil(ColorPencil pencil)
    {
        if (pencil == null)
            return;

        currentPencil = pencil;

        heldColorName = pencil.colorName;
        heldColor = pencil.colorValue;
        isHoldingPencil = true;

        PencilDurability durability =
            GetComponent<PencilDurability>();

        if (durability != null)
            durability.ResetDurability();

        Debug.Log("Picked up: " + heldColorName + " pencil");
    }

    public void DropPencil(Vector3 dropPosition)
    {
        if (currentPencil == null)
            return;

        currentPencil.transform.position = dropPosition;

        currentPencil.SetHeldState(false);

        heldColorName = "";
        heldColor = Color.white;
        isHoldingPencil = false;

        currentPencil = null;

        if (pencilIndicator != null)
            pencilIndicator.gameObject.SetActive(false);

        Debug.Log("Dropped pencil");
    }
}
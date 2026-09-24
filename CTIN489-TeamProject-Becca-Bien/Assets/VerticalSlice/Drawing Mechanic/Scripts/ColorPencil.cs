using UnityEngine;

public class ColorPencil : MonoBehaviour
{

    [Header("Pencil Color")]
    public string colorName = "Green";
    public Color colorValue = Color.green;

    [Header("Pencil Durability")]
    public Sprite sharpSprite;
    public Sprite dullSprite;
    public Sprite brokenSprite;

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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        PlayerPencil playerPencil = player.GetComponent<PlayerPencil>();
        if (playerPencil == null) return;

        // Get new pencil position
        Vector3 pickupPosition = transform.position;

        // Put down pencil in new pencil's position
        if (playerPencil.isHoldingPencil)
        {
            playerPencil.DropPencil(pickupPosition);
        }

        // Pick up new pencil
        playerPencil.PickUpPencil(this);

        SetHeldState(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPencilPickupSound();

        Debug.Log("Picked up " + colorName + " pencil!");
    }

    public void SetHeldState(bool isHeld)
    {
        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();

        Collider2D col =
            GetComponent<Collider2D>();

        if (sr != null)
            sr.enabled = !isHeld;

        if (col != null)
            col.enabled = !isHeld;
    }

}
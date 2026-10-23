using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class PencilSharpener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera interactionCamera;

    private BoxCollider2D clickArea;

    private void Awake()
    {
        clickArea = GetComponent<BoxCollider2D>();

        if (interactionCamera == null)
            interactionCamera = Camera.main;

        if (interactionCamera == null)
        {
            Debug.LogError(
                "Assign the gameplay camera to PencilSharpener.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null ||
            !mouse.leftButton.wasPressedThisFrame ||
            !Application.isFocused ||
            Time.timeScale == 0f ||
            clickArea == null ||
            !clickArea.enabled)
        {
            return;
        }

        Vector2 mousePosition = mouse.position.ReadValue();

        if (!interactionCamera.pixelRect.Contains(mousePosition))
            return;

        // Find the clicked position on the sharpener's 2D plane.
        Ray ray = interactionCamera.ScreenPointToRay(mousePosition);
        Plane plane = new Plane(Vector3.forward, transform.position);

        if (!plane.Raycast(ray, out float distance))
            return;

        Vector3 worldPosition = ray.GetPoint(distance);

        // Only sharpen when the click is inside this object.
        if (clickArea.OverlapPoint(worldPosition))
        {
            SharpenPencil();
        }
    }

    public void SharpenPencil()
    {
        PlayerPencil player = PlayerPencil.Instance;

        if (player == null ||
            !player.isHoldingPencil ||
            player.currentPencil == null)
        {
            Debug.Log("Pick up a pencil before using the sharpener.", this);
            return;
        }

        ColorPencil pencil = player.currentPencil;

        // Restore THIS pencil's durability.
        pencil.currentUseTime = 0f;
        pencil.isDull = false;
        pencil.isBroken = false;

        // Update the pencil's world sprite for when it is dropped.
        // This does not make the hidden, held pencil visible.
        pencil.UpdateWorldSprite();

        // Update the pencil indicator beside the player.
        PencilDurability durability =
            player.GetComponent<PencilDurability>();

        if (durability != null)
        {
            durability.UpdatePencilSprite();
        }

        Debug.Log(
            $"Sharpened {pencil.colorName} pencil! Drawing time reset to 0.",
            this
        );
    }
}
using UnityEngine;
using UnityEngine.UI;

public class CollectableUI : MonoBehaviour
{
    private Image itemImage;

    void Awake()
    {
        itemImage = GetComponent<Image>();
        itemImage.color = Color.black;
    }

    public void SetCollected()
    {
        itemImage.color = Color.white;
    }
}
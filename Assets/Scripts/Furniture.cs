using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Furniture : MonoBehaviour
{
    public TextMeshProUGUI titleTxt;
    public Image furnitureImg;
    public Button furnitureBtn;

    public void Init(FurnitureSO furnitureSO)
    {
        titleTxt.text = furnitureSO.title;
        furnitureImg.sprite = furnitureSO.furnitureImg;
    }

    public void SetButton(UnityAction callback)
    {
        furnitureBtn.onClick.AddListener(callback);
    }

}
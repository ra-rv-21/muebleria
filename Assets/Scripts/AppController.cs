using TMPro;
using UnityEngine;

public class AppController : MonoBehaviour
{
    [Header("Furniture")]
    public int startIndex = 0;
    public FurnitureObject furnitureObject;

    [Header("UI")]
    public TextMeshProUGUI titleTxt;
    public Furniture furniturePrefab;
    public Transform furnitureContainter;

    [Header("Data")]
    public FurnitureSO[] data;

    private Furniture _furniture;

    private void Start()
    {
        CreatePrefabs();

        ChangeFurniture(data[startIndex]);
    }

    private void CreatePrefabs()
    {
        for (int i = 0; i < data.Length; i++)
        {
            _furniture = Instantiate(furniturePrefab, furnitureContainter);
            _furniture.Init(data[i]);

            int index = i;
            _furniture.SetButton(() => ChangeFurniture(data[index]));
        }
    }

    private void ChangeFurniture(FurnitureSO furnitureSO)
    {
        titleTxt.text = furnitureSO.title;
        furnitureObject.SetObject(furnitureSO.prefab);
    }

}
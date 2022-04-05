using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ItemsScrollView : MonoBehaviour
{
    [SerializeField] private GameObject itemViewPrefab;


    public ItemView CreateItem()
    {
        return Instantiate(itemViewPrefab, gameObject.transform).GetComponent<ItemView>();
    }
}

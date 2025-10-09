using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Metadata;

public class StickerManager : MonoBehaviour
{
    List<UnityEngine.UI.Image> _images;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _images = new List<UnityEngine.UI.Image>();

        // Consigo los hijos para tener los stickers
        foreach (Transform child in transform)
        {
            _images.Add(child.GetComponent<UnityEngine.UI.Image>());
        }

        // Quito filtro en negro a los conseguidos
        int collec = 2;
        List<bool[]> info = SceneLoader.Instance.GetCollectablesInfo();
        for(int i = 0; i < info.Count; i++)
        {
            if (info[i][collec])
            {
                UnlockSticker(i);
            }
        }
    }

    // Restablece el color
    void UnlockSticker(int index)
    {
        _images[index].color = Color.white;
    }
}

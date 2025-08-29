using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    [SerializeField]
    Sprite _tickImage;

    [SerializeField]
    Sprite _crossImage;

    [SerializeField]
    Sprite _noneImage;

    [SerializeField]
    GameObject _imageGO;
    Image _image;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _image = _imageGO.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Pone Imagen de Tick o Cross dependiendo del parámetro
    public void SetTick(bool enabled)
    {
        if (enabled)
            _image.sprite = _tickImage;
        else
            _image.sprite = _crossImage;
    }

    // Pone imagen transparente
    public void ClearImage()
    {
        _image.sprite = _noneImage;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerPictureEntry
{
    public string pictureId;
    public Sprite sprite;
}

public class PlayerPictureDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image playerPictureImage;

    [Header("Picture Library")]
    [SerializeField] private List<PlayerPictureEntry> pictures = new List<PlayerPictureEntry>();

    private void Awake()
    {
        Hide();
    }

    public void Show(string pictureId)
    {
        if (string.IsNullOrEmpty(pictureId) || playerPictureImage == null)
        {
            Hide();
            return;
        }

        Sprite sprite = GetPicture(pictureId);

        if (sprite == null)
        {
            Hide();
            return;
        }

        playerPictureImage.sprite = sprite;
        playerPictureImage.enabled = true;
    }

    public void Hide()
    {
        if (playerPictureImage == null)
        {
            return;
        }

        playerPictureImage.enabled = false;
        playerPictureImage.sprite = null;
    }

    private Sprite GetPicture(string pictureId)
    {
        foreach (PlayerPictureEntry entry in pictures)
        {
            if (entry != null && entry.pictureId == pictureId)
            {
                return entry.sprite;
            }
        }

        return null;
    }
}

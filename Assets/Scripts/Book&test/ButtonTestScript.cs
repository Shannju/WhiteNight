using UnityEngine;

public class ButtonTestScript : MonoBehaviour
{
    public void OnTestButtonClicked()
    {
        Debug.Log("=== TEST BUTTON CLICKED ===");
    }

    public void OnFlipForwardClicked()
    {
        Debug.Log("=== FLIP FORWARD BUTTON CLICKED ===");
    }

    public void OnFlipBackwardClicked()
    {
        Debug.Log("=== FLIP BACKWARD BUTTON CLICKED ===");
    }
}

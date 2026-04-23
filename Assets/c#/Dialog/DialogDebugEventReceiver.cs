using UnityEngine;

public class DialogDebugEventReceiver : MonoBehaviour
{
    public void Log(string message)
    {
        Debug.Log(message, this);
    }
}

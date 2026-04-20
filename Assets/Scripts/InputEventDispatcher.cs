using UnityEngine;
using UnityEngine.Events;

public class InputEventDispatcher : MonoBehaviour
{
    [Header("Dialogue / Camera Input Events")]
    public UnityEvent onBoard;
    public UnityEvent onFriend;
    public UnityEvent onWindow;
    public UnityEvent onDesk;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            onBoard?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            onFriend?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            onWindow?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            onDesk?.Invoke();
        }
    }

    public void TriggerBoard()
    {
        onBoard?.Invoke();
    }

    public void TriggerFriend()
    {
        onFriend?.Invoke();
    }

    public void TriggerWindow()
    {
        onWindow?.Invoke();
    }

    public void TriggerDesk()
    {
        onDesk?.Invoke();
    }
}

using UnityEngine;
using UnityEngine.Events;

public class InputEventDispatcher : MonoBehaviour
{
    [Header("Dialogue / Camera Input Events")]
    public UnityEvent onBoard;
    public UnityEvent onFriend;
    public UnityEvent onWindow;
    public UnityEvent onDesk;

    [Header("Directional Input Locks")]
    [SerializeField] private bool boardInputLocked = true;
    [SerializeField] private bool friendInputLocked = true;
    [SerializeField] private bool windowInputLocked = true;
    [SerializeField] private bool deskInputLocked = true;

    void Awake()
    {
        LockAllDirectionalInputs();
    }

    void Update()
    {
        if (!boardInputLocked && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            onBoard?.Invoke();
        }

        if (!friendInputLocked && Input.GetKeyDown(KeyCode.RightArrow))
        {
            onFriend?.Invoke();
        }

        if (!windowInputLocked && Input.GetKeyDown(KeyCode.UpArrow))
        {
            onWindow?.Invoke();
        }

        if (!deskInputLocked && Input.GetKeyDown(KeyCode.DownArrow))
        {
            onDesk?.Invoke();
        }
    }

    public void LockAllDirectionalInputs()
    {
        boardInputLocked = true;
        friendInputLocked = true;
        windowInputLocked = true;
        deskInputLocked = true;
    }

    public void UnlockBoardInput()
    {
        boardInputLocked = false;
    }

    public void UnlockFriendInput()
    {
        friendInputLocked = false;
    }

    public void UnlockWindowInput()
    {
        windowInputLocked = false;
    }

    public void UnlockDeskInput()
    {
        deskInputLocked = false;
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

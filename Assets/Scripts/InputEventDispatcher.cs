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
    [SerializeField] private bool unlockAllInputsOnStart = true;
    [SerializeField] private bool rightMouseTriggersFriend = true;

    [Header("Debug")]
    [SerializeField] private CanvasManager canvasManager;
    [SerializeField] private KeyCode debugUiToggleKey = KeyCode.Z;
    [SerializeField] private bool debugDirectionalInput = true;

    void Awake()
    {
        LockAllDirectionalInputs();
    }

    void Start()
    {
        if (unlockAllInputsOnStart)
        {
            UnlockAllDirectionalInputs();
        }

        if (canvasManager == null)
        {
            canvasManager = FindObjectOfType<CanvasManager>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(debugUiToggleKey))
        {
            ToggleDebugUi();
        }

        bool boardPressed = Input.GetKeyDown(KeyCode.UpArrow);
        bool friendPressed = IsFriendInputPressed();
        bool windowPressed = Input.GetKeyDown(KeyCode.LeftArrow);
        bool deskPressed = Input.GetKeyDown(KeyCode.DownArrow);

        LogPressedInput("Board", boardPressed, boardInputLocked);
        LogPressedInput("Friend", friendPressed, friendInputLocked);
        LogPressedInput("Window", windowPressed, windowInputLocked);
        LogPressedInput("Desk", deskPressed, deskInputLocked);

        if (!boardInputLocked && boardPressed)
        {
            onBoard?.Invoke();
        }

        if (!friendInputLocked && friendPressed)
        {
            onFriend?.Invoke();
        }

        if (!windowInputLocked && windowPressed)
        {
            onWindow?.Invoke();
        }

        if (!deskInputLocked && deskPressed)
        {
            onDesk?.Invoke();
        }
    }

    void OnEnable()
    {
        LogLockState("enabled");
    }

    void OnDisable()
    {
        LogLockState("disabled");
    }

    public void LockAllDirectionalInputs()
    {
        boardInputLocked = true;
        friendInputLocked = true;
        windowInputLocked = true;
        deskInputLocked = true;
        LogLockState("LockAllDirectionalInputs");
    }

    public void UnlockAllDirectionalInputs()
    {
        boardInputLocked = false;
        friendInputLocked = false;
        windowInputLocked = false;
        deskInputLocked = false;
        LogLockState("UnlockAllDirectionalInputs");
    }

    public void UnlockBoardInput()
    {
        boardInputLocked = false;
        LogLockState("UnlockBoardInput");
    }

    public void UnlockFriendInput()
    {
        friendInputLocked = false;
        LogLockState("UnlockFriendInput");
    }

    public void UnlockWindowInput()
    {
        windowInputLocked = false;
        LogLockState("UnlockWindowInput");
    }

    public void UnlockDeskInput()
    {
        deskInputLocked = false;
        LogLockState("UnlockDeskInput");
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

    public void ToggleDebugUi()
    {
        if (canvasManager == null)
        {
            canvasManager = FindObjectOfType<CanvasManager>();
        }

        if (canvasManager == null)
        {
            Debug.LogWarning("[InputEventDispatcher] CanvasManager was not found, so debug UI cannot be toggled.", this);
            return;
        }

        canvasManager.ToggleDebugCanvas();
    }

    private bool IsFriendInputPressed()
    {
        return Input.GetKeyDown(KeyCode.RightArrow) ||
               (rightMouseTriggersFriend && Input.GetMouseButtonDown(1));
    }

    private void LogPressedInput(string inputName, bool pressed, bool locked)
    {
        if (!debugDirectionalInput || !pressed)
        {
            return;
        }

        Debug.Log($"[InputEventDispatcher] {inputName} input pressed. locked={locked}, enabled={enabled}", this);
    }

    private void LogLockState(string source)
    {
        if (!debugDirectionalInput)
        {
            return;
        }

        Debug.Log(
            $"[InputEventDispatcher] {source}: boardLocked={boardInputLocked}, friendLocked={friendInputLocked}, windowLocked={windowInputLocked}, deskLocked={deskInputLocked}, enabled={enabled}",
            this);
    }
}

using UnityEngine;
using Cinemachine;

public class FourDirectionCameraSwitcher : MonoBehaviour
{
    [Header("Virtual Cameras")]
    public CinemachineVirtualCamera camUp;
    public CinemachineVirtualCamera camDown;
    public CinemachineVirtualCamera camLeft;
    public CinemachineVirtualCamera camRight;

    [Header("Priority Settings")]
    public int activePriority = 10;
    public int inactivePriority = 0;

    [Header("Initial Camera")]
    [SerializeField] private bool randomizeInitialCamera = true;

    public InputEventDispatcher inputDispatcher;
    [SerializeField] private DialogManager dialogManager;

    void Awake()
    {
        if (dialogManager == null)
        {
            dialogManager = FindObjectOfType<DialogManager>();
        }

        SetInitialCamera();
    }

    void OnEnable()
    {
        if (inputDispatcher == null)
            return;

        inputDispatcher.onBoard.AddListener(OnBoardSelected);
        inputDispatcher.onFriend.AddListener(OnFriendSelected);
        inputDispatcher.onWindow.AddListener(OnWindowSelected);
        inputDispatcher.onDesk.AddListener(OnDeskSelected);
    }

    void OnDisable()
    {
        if (inputDispatcher == null)
            return;

        inputDispatcher.onBoard.RemoveListener(OnBoardSelected);
        inputDispatcher.onFriend.RemoveListener(OnFriendSelected);
        inputDispatcher.onWindow.RemoveListener(OnWindowSelected);
        inputDispatcher.onDesk.RemoveListener(OnDeskSelected);
    }

    public void OnBoardSelected()
    {
        SwitchToUp();
    }

    public void OnFriendSelected()
    {
        SwitchToRight();
    }

    public void OnWindowSelected()
    {
        SwitchToLeft();
    }

    public void OnDeskSelected()
    {
        SwitchToDown();
    }

    public void SwitchToUp()
    {
        SwitchTo(camUp);
    }

    public void SwitchToDown()
    {
        SwitchTo(camDown);
    }

    public void SwitchToLeft()
    {
        SwitchTo(camLeft);
    }

    public void SwitchToRight()
    {
        SwitchTo(camRight);
    }

    public void SetInitialCamera()
    {
        if (!randomizeInitialCamera)
        {
            SwitchToDown();
            return;
        }

        CinemachineVirtualCamera[] availableCameras =
        {
            camUp,
            camDown,
            camLeft,
            camRight
        };

        int availableCount = 0;

        foreach (CinemachineVirtualCamera camera in availableCameras)
        {
            if (camera != null)
            {
                availableCount++;
            }
        }

        if (availableCount == 0)
        {
            return;
        }

        int targetIndex = Random.Range(0, availableCount);
        int currentIndex = 0;

        foreach (CinemachineVirtualCamera camera in availableCameras)
        {
            if (camera == null)
            {
                continue;
            }

            if (currentIndex == targetIndex)
            {
                SwitchTo(camera);
                return;
            }

            currentIndex++;
        }
    }

    public void SwitchTo(CinemachineVirtualCamera targetCam)
    {
        if (dialogManager != null)
        {
            dialogManager.DisableInteractForCameraSwitch();
        }

        SetPriority(camUp, inactivePriority);
        SetPriority(camDown, inactivePriority);
        SetPriority(camLeft, inactivePriority);
        SetPriority(camRight, inactivePriority);

        if (targetCam != null)
        {
            targetCam.Priority = activePriority;
        }
    }

    private void SetPriority(CinemachineVirtualCamera targetCam, int priority)
    {
        if (targetCam != null)
        {
            targetCam.Priority = priority;
        }
    }
}

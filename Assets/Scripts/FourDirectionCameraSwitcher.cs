  using UnityEngine;
using Cinemachine;

public class FourDirectionCameraSwitcher : MonoBehaviour
{
    [Header("四个虚拟相机")]
    public CinemachineVirtualCamera camUp;
    public CinemachineVirtualCamera camDown;
    public CinemachineVirtualCamera camLeft;
    public CinemachineVirtualCamera camRight;

    [Header("优先级设置")]
    public int activePriority = 10;
    public int inactivePriority = 0;

    public InputEventDispatcher inputDispatcher;

    void Start()
    {
        // 默认先激活“上”
        SwitchTo(camUp);
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

    void OnBoardSelected()
    {
        SwitchTo(camLeft);
    }

    void OnFriendSelected()
    {
        SwitchTo(camRight);
    }

    void OnWindowSelected()
    {
        SwitchTo(camUp);
    }

    void OnDeskSelected()
    {
        SwitchTo(camDown);
    }

    void SwitchTo(CinemachineVirtualCamera targetCam)
    {
        camUp.Priority = inactivePriority;
        camDown.Priority = inactivePriority;
        camLeft.Priority = inactivePriority;
        camRight.Priority = inactivePriority;

        if (targetCam != null)
        {
            targetCam.Priority = activePriority;
        }
    }
}
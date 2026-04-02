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

    void Start()
    {
        // 默认先激活“上”
        SwitchTo(camUp);
    }

    void Update()
    {
        // W 或 上箭头 -> 上视角
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            SwitchTo(camUp);
        }

        // S 或 下箭头 -> 下视角
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            SwitchTo(camDown);
        }

        // A 或 左箭头 -> 左视角
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchTo(camLeft);
        }

        // D 或 右箭头 -> 右视角
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchTo(camRight);
        }
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ThreeButtonController : MonoBehaviour
{
    [Header("三个按钮")]
    public GameObject buttonObj1;
    public GameObject buttonObj2;
    public GameObject buttonObj3;

    [Header("事件")]
    public UnityEvent onAllButtonsPressed;

    private Button button1;
    private Button button2;
    private Button button3;

    private bool _button1Pressed = false;
    private bool _button2Pressed = false;
    private bool _button3Pressed = false;

    void Start()
    {
        if (buttonObj1 == null || buttonObj2 == null || buttonObj3 == null)
        {
            Debug.LogError("ThreeButtonController: 请在Inspector中分配三个按钮对象!");
            return;
        }

        // 从GameObject中获取Button组件
        button1 = buttonObj1.GetComponent<Button>();
        button2 = buttonObj2.GetComponent<Button>();
        button3 = buttonObj3.GetComponent<Button>();

        if (button1 == null || button2 == null || button3 == null)
        {
            Debug.LogError("ThreeButtonController: 分配的对象中找不到Button组件! 请确保选择的是UI Button");
            return;
        }

        // 为三个按钮的onClick事件添加监听
        button1.onClick.AddListener(OnButton1Clicked);
        button2.onClick.AddListener(OnButton2Clicked);
        button3.onClick.AddListener(OnButton3Clicked);

        Debug.Log("ThreeButtonController: 已为三个按钮添加点击监听");
    }

    void OnDestroy()
    {
        if (button1 != null) button1.onClick.RemoveListener(OnButton1Clicked);
        if (button2 != null) button2.onClick.RemoveListener(OnButton2Clicked);
        if (button3 != null) button3.onClick.RemoveListener(OnButton3Clicked);
    }

    public void OnButton1Clicked()
    {
        _button1Pressed = true;
        Debug.Log("按钮1被按下");
        CheckAllButtonsPressed();
    }

    public void OnButton2Clicked()
    {
        _button2Pressed = true;
        Debug.Log("按钮2被按下");
        CheckAllButtonsPressed();
    }

    public void OnButton3Clicked()
    {
        _button3Pressed = true;
        Debug.Log("按钮3被按下");
        CheckAllButtonsPressed();
    }

    private void CheckAllButtonsPressed()
    {
        if (_button1Pressed && _button2Pressed && _button3Pressed)
        {
            OnAllButtonsPressed();
        }
    }

    private void OnAllButtonsPressed()
    {
        Debug.Log("三个按钮全部被按下!");
        onAllButtonsPressed?.Invoke();
        
        // 重置状态
        ResetButtonStates();
    }

    public void ResetButtonStates()
    {
        _button1Pressed = false;
        _button2Pressed = false;
        _button3Pressed = false;
        Debug.Log("按钮状态已重置");
    }
}

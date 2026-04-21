using System.Collections.Generic;
using UnityEngine;

public class UIImageFillControllerManager : MonoBehaviour
{
    private List<UIImageFillController> options;
    private UIImageFillController currentSelectedOption = null;
    
    void Start()
    {
        // 自动从子物体中获取所有UIImageFillController
        options = new List<UIImageFillController>(GetComponentsInChildren<UIImageFillController>());
        
        // 验证是否找到至少一个选项
        if (options.Count == 0)
        {
            Debug.LogError("UIImageFillControllerManager: 未找到任何UIImageFillController组件!");
            return;
        }
        
        // 订阅所有选项的状态改变事件
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null)
            {
                options[i].OnFillStateChanged += OnAnyOptionStateChanged;
            }
        }
        
        Debug.Log($"UIImageFillControllerManager: 已自动获取 {options.Count} 个选项");
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        if (options != null)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] != null)
                {
                    options[i].OnFillStateChanged -= OnAnyOptionStateChanged;
                }
            }
        }
    }
    
    /// <summary>
    /// 当任何选项状态改变时的回调
    /// </summary>
    private void OnAnyOptionStateChanged(UIImageFillController changedOption)
    {
        if (changedOption == null)
            return;
        
        // 更新当前选中的选项
        currentSelectedOption = changedOption;
        
        // 清空所有其他选项
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null && options[i] != changedOption)
            {
                options[i].QuickClear();
            }
        }
    }
    
    /// <summary>
    /// 选择第一个选项
    /// </summary>
    public void SelectOption1()
    {
        SelectOptionByIndex(0);
    }
    
    /// <summary>
    /// 选择第二个选项
    /// </summary>
    public void SelectOption2()
    {
        SelectOptionByIndex(1);
    }
    
    /// <summary>
    /// 选择第三个选项
    /// </summary>
    public void SelectOption3()
    {
        SelectOptionByIndex(2);
    }
    
    /// <summary>
    /// 通过索引选择选项
    /// </summary>
    public void SelectOptionByIndex(int index)
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogError("UIImageFillControllerManager: 选项列表为空!");
            return;
        }
        
        if (index < 0 || index >= options.Count)
        {
            Debug.LogError($"UIImageFillControllerManager: 索引 {index} 超出范围!");
            return;
        }
        
        SelectOption(options[index]);
    }
    
    /// <summary>
    /// 通过UIImageFillController对象选择选项
    /// </summary>
    public void SelectOption(UIImageFillController selectedOption)
    {
        if (selectedOption == null)
        {
            Debug.LogError("UIImageFillControllerManager: 选项为空!");
            return;
        }
        
        // 调用选项的动画方法，事件会自动处理清空其他选项
        selectedOption.AnimateFill();
    }
    
    /// <summary>
    /// 获取当前选中的选项
    /// </summary>
    public UIImageFillController GetSelectedOption()
    {
        return currentSelectedOption;
    }
    
    /// <summary>
    /// 获取当前选中的选项索引（0, 1, 2... 或 -1表示未选中）
    /// </summary>
    public int GetSelectedOptionIndex()
    {
        if (currentSelectedOption == null || options == null)
            return -1;
        
        return options.IndexOf(currentSelectedOption);
    }
    
    /// <summary>
    /// 清空所有选项
    /// </summary>
    public void ClearAll()
    {
        foreach (UIImageFillController option in options)
        {
            if (option != null)
            {
                option.QuickClear();
            }
        }
        
        currentSelectedOption = null;
        Debug.Log("已清空所有选项");
    }
    
    /// <summary>
    /// 获取指定索引选项的状态
    /// </summary>
    public FillState GetOptionState(int index)
    {
        if (options == null || index < 0 || index >= options.Count || options[index] == null)
        {
            return new FillState { fillAmount = 0f, isFilling = false };
        }
        
        return options[index].GetState();
    }
    
    /// <summary>
    /// 获取所有选项的状态数组
    /// </summary>
    public FillState[] GetAllOptionsState()
    {
        if (options == null || options.Count == 0)
        {
            return new FillState[0];
        }
        
        FillState[] states = new FillState[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null)
            {
                states[i] = options[i].GetState();
            }
            else
            {
                states[i] = new FillState { fillAmount = 0f, isFilling = false };
            }
        }
        
        return states;
    }
    
    /// <summary>
    /// 获取所有选项的详细信息（包含选中状态）
    /// </summary>
    public OptionInfo[] GetAllOptionsInfo()
    {
        if (options == null || options.Count == 0)
        {
            return new OptionInfo[0];
        }
        
        OptionInfo[] infos = new OptionInfo[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            infos[i] = new OptionInfo
            {
                index = i,
                state = options[i] != null ? options[i].GetState() : new FillState { fillAmount = 0f, isFilling = false },
                isSelected = (options[i] == currentSelectedOption)
            };
        }
        
        return infos;
    }
}

/// <summary>
/// 选项的完整信息
/// </summary>
public struct OptionInfo
{
    public int index;           // 选项索引
    public FillState state;     // 选项的fill状态
    public bool isSelected;     // 是否被选中
}

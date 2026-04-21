using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum FillOptionLabel
{
    A,
    B,
    C
}

[System.Serializable]
public class FillToggleOptionBinding
{
    public FillOptionLabel label = FillOptionLabel.A;
    public Toggle toggle;
}

public class UIImageFillControllerManager : MonoBehaviour
{
    [SerializeField] private List<FillToggleOptionBinding> configuredOptions = new List<FillToggleOptionBinding>();

    private List<UIImageFillController> options;
    private List<Toggle> optionToggles;
    private List<Button> optionButtons;
    private List<UnityAction<bool>> optionToggleActions;
    private List<UnityAction> optionClickActions;
    private UIImageFillController currentSelectedOption = null;
    private int currentSelectedIndex = -1;
    
    void Start()
    {
        // 优先使用Inspector里手动配置的Toggle列表，没有配置时再自动查找
        options = BuildOptionList();
        optionToggles = new List<Toggle>();
        optionButtons = new List<Button>();
        optionToggleActions = new List<UnityAction<bool>>();
        optionClickActions = new List<UnityAction>();
        
        // 验证是否找到至少一个选项
        if (options.Count == 0)
        {
            Debug.LogError("UIImageFillControllerManager: 未找到任何UIImageFillController组件!");
            return;
        }

        // 由manager统一监听每个选项自己的Toggle或Button点击
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null)
            {
                Toggle toggle = options[i].GetComponentInParent<Toggle>();
                Button button = options[i].GetComponentInParent<Button>();

                optionToggles.Add(toggle);
                optionButtons.Add(button);

                if (toggle != null)
                {
                    int capturedIndex = i;
                    UnityAction<bool> toggleAction = isOn => OnOptionToggleChanged(capturedIndex, isOn);
                    optionToggleActions.Add(toggleAction);
                    optionClickActions.Add(null);
                    toggle.onValueChanged.AddListener(toggleAction);
                    toggle.SetIsOnWithoutNotify(false);
                }
                else if (button != null)
                {
                    int capturedIndex = i;
                    UnityAction clickAction = () => OnOptionClicked(capturedIndex);
                    optionToggleActions.Add(null);
                    optionClickActions.Add(clickAction);
                    button.onClick.AddListener(clickAction);
                }
                else
                {
                    optionToggleActions.Add(null);
                    optionClickActions.Add(null);
                    Debug.LogWarning($"UIImageFillControllerManager: 选项 {i} 上没有Toggle或Button组件!", options[i]);
                }
            }
            else
            {
                optionToggles.Add(null);
                optionButtons.Add(null);
                optionToggleActions.Add(null);
                optionClickActions.Add(null);
            }
        }
        
        Debug.Log($"UIImageFillControllerManager: 已自动获取 {options.Count} 个选项");
    }

    private List<UIImageFillController> BuildOptionList()
    {
        List<UIImageFillController> resolvedOptions = new List<UIImageFillController>();

        if (configuredOptions != null && configuredOptions.Count > 0)
        {
            foreach (FillToggleOptionBinding binding in configuredOptions)
            {
                if (binding == null || binding.toggle == null)
                {
                    resolvedOptions.Add(null);
                    continue;
                }

                UIImageFillController fillController =
                    binding.toggle.GetComponentInChildren<UIImageFillController>(true);

                if (fillController == null)
                {
                    Debug.LogWarning(
                        $"UIImageFillControllerManager: 配置的 {binding.label} 没有找到 UIImageFillController!",
                        binding.toggle);
                }

                resolvedOptions.Add(fillController);
            }

            return resolvedOptions;
        }

        return new List<UIImageFillController>(GetComponentsInChildren<UIImageFillController>(true));
    }
    
    void OnDestroy()
    {
        // 取消订阅按钮事件
        if (options == null)
        {
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            if (optionToggles != null &&
                i < optionToggles.Count &&
                optionToggles[i] != null &&
                optionToggleActions != null &&
                i < optionToggleActions.Count &&
                optionToggleActions[i] != null)
            {
                optionToggles[i].onValueChanged.RemoveListener(optionToggleActions[i]);
            }

            if (optionButtons != null &&
                i < optionButtons.Count &&
                optionButtons[i] != null &&
                optionClickActions != null &&
                i < optionClickActions.Count &&
                optionClickActions[i] != null)
            {
                optionButtons[i].onClick.RemoveListener(optionClickActions[i]);
            }
        }
    }
    
    /// <summary>
    /// manager统一响应按钮点击
    /// </summary>
    private void OnOptionClicked(int index)
    {
        SelectOptionByIndex(index);
    }

    /// <summary>
    /// manager统一响应Toggle变化
    /// </summary>
    private void OnOptionToggleChanged(int index, bool isOn)
    {
        if (index < 0 || index >= options.Count)
        {
            return;
        }

        if (!isOn)
        {
            if (index == currentSelectedIndex && optionToggles[index] != null)
            {
                optionToggles[index].SetIsOnWithoutNotify(true);
            }

            return;
        }

        SelectOptionByIndex(index);
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

    public void SelectOptionA()
    {
        SelectOptionByLabel(FillOptionLabel.A);
    }

    public void SelectOptionB()
    {
        SelectOptionByLabel(FillOptionLabel.B);
    }

    public void SelectOptionC()
    {
        SelectOptionByLabel(FillOptionLabel.C);
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

        if (index == currentSelectedIndex)
        {
            if (optionToggles != null &&
                index < optionToggles.Count &&
                optionToggles[index] != null)
            {
                optionToggles[index].SetIsOnWithoutNotify(true);
            }
            return;
        }
        
        SelectOption(options[index]);
    }

    public void SelectOptionByLabel(FillOptionLabel label)
    {
        int index = GetOptionIndexByLabel(label);
        if (index < 0)
        {
            Debug.LogError($"UIImageFillControllerManager: 没有找到标签为 {label} 的选项!");
            return;
        }

        SelectOptionByIndex(index);
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

        if (selectedOption == currentSelectedOption)
        {
            return;
        }

        if (currentSelectedOption != null)
        {
            currentSelectedOption.QuickClear();
        }

        int selectedIndex = options.IndexOf(selectedOption);

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] == null)
            {
                continue;
            }

            if (i != selectedIndex)
            {
                options[i].QuickClear();
            }

            if (optionToggles != null &&
                i < optionToggles.Count &&
                optionToggles[i] != null)
            {
                optionToggles[i].SetIsOnWithoutNotify(i == selectedIndex);
            }
        }

        if (!selectedOption.PlayFromZero())
        {
            return;
        }

        currentSelectedOption = selectedOption;
        currentSelectedIndex = selectedIndex;
    }
    
    /// <summary>
    /// 获取当前选中的选项
    /// </summary>
    public UIImageFillController GetSelectedOption()
    {
        return currentSelectedOption;
    }

    public FillOptionLabel? GetSelectedOptionLabel()
    {
        if (currentSelectedIndex < 0 ||
            configuredOptions == null ||
            currentSelectedIndex >= configuredOptions.Count)
        {
            return null;
        }

        return configuredOptions[currentSelectedIndex].label;
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

        if (optionToggles != null)
        {
            for (int i = 0; i < optionToggles.Count; i++)
            {
                if (optionToggles[i] != null)
                {
                    optionToggles[i].SetIsOnWithoutNotify(false);
                }
            }
        }
        
        currentSelectedOption = null;
        currentSelectedIndex = -1;
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

    private int GetOptionIndexByLabel(FillOptionLabel label)
    {
        if (configuredOptions == null || configuredOptions.Count == 0)
        {
            return -1;
        }

        for (int i = 0; i < configuredOptions.Count; i++)
        {
            if (configuredOptions[i] != null && configuredOptions[i].label == label)
            {
                return i;
            }
        }

        return -1;
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

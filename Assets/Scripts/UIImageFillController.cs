using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIImageFillController : MonoBehaviour
{
    [Header("Fill速度")]
    public float fillDuration = 2f; // 填充从0-1或1-0的时间（秒）
    
    private Image targetImage;
    private bool _isFilling = false;
    
    // 当状态改变时的回调
    public delegate void OnFillStateChangedDelegate(UIImageFillController sender);
    public event OnFillStateChangedDelegate OnFillStateChanged;
    
    void Start()
    {
        // 自动从当前物体获取Image组件
        targetImage = GetComponent<Image>();
        
        if (targetImage == null)
        {
            Debug.LogError("UIImageFillController: 当前物体上找不到Image组件!");
            return;
        }
        
        // 确保Image有Fill Amount属性
        if (targetImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("UIImageFillController: Image的Type不是Filled，已自动设置");
            targetImage.type = Image.Type.Filled;
        }
        
        // 初始化fillAmount为0（off状态）
        targetImage.fillAmount = 0f;
    }
    
    /// <summary>
    /// 控制UI Image的Fill Amount动画
    /// 从0-1渐变显示
    /// </summary>
    public void AnimateFill()
    {
        if (_isFilling)
        {
            Debug.LogWarning("UIImageFillController: 正在进行填充动画，请等待完成后再调用");
            return;
        }
        
        // 触发状态改变事件，传递自己作为参数
        OnFillStateChanged?.Invoke(this);
        
        StartCoroutine(FillCoroutine());
    }
    
    /// <summary>
    /// 指定方向控制填充
    /// </summary>
    /// <param name="forward">true表示0→1，false表示1→0</param>
    public void AnimateFill(bool forward)
    {
        if (_isFilling)
        {
            Debug.LogWarning("UIImageFillController: 正在进行填充动画，请等待完成后再调用");
            return;
        }
        
        StartCoroutine(FillCoroutine(forward));
    }
    
    private IEnumerator FillCoroutine()
    {
        return FillCoroutine(true);
    }
    
    private IEnumerator FillCoroutine(bool forward)
    {
        _isFilling = true;
        float elapsed = 0f;
        
        float startFill = forward ? 0f : 1f;
        float endFill = forward ? 1f : 0f;
        
        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fillDuration;
            
            // 线性插值计算当前Fill Amount
            targetImage.fillAmount = Mathf.Lerp(startFill, endFill, progress);
            
            yield return null;
        }
        
        // 确保最终值准确
        targetImage.fillAmount = endFill;
        
        _isFilling = false;
    }
    
    /// <summary>
    /// 立即设置Fill Amount值
    /// </summary>
    public void SetFillAmount(float value)
    {
        if (targetImage != null)
        {
            targetImage.fillAmount = Mathf.Clamp01(value);
        }
    }
    
    /// <summary>
    /// 一键清空，瞬间将Fill Amount设置为0
    /// </summary>
    public void QuickClear()
    {
        if (_isFilling)
        {
            StopAllCoroutines();
            _isFilling = false;
        }
        
        if (targetImage != null)
        {
            targetImage.fillAmount = 0f;
        }
    }
    
    /// <summary>
    /// 获取当前Fill Amount值
    /// </summary>
    public float GetFillAmount()
    {
        return targetImage != null ? targetImage.fillAmount : 0f;
    }
    
    /// <summary>
    /// 获取当前状态信息
    /// </summary>
    public FillState GetState()
    {
        return new FillState
        {
            fillAmount = GetFillAmount(),
            isFilling = _isFilling
        };
    }
}

/// <summary>
/// UIImageFillController的状态信息
/// </summary>
public struct FillState
{
    public float fillAmount;  // 当前fillAmount值（0-1）
    public bool isFilling;    // 是否正在动画中
}

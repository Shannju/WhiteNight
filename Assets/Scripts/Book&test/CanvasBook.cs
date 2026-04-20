using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasBook : MonoBehaviour, IPointerClickHandler
{
    [Header("设置")]
    public float flipDuration = 0.5f;
    public List<RectTransform> pages; 
    
    private int _currentPageIndex = 0; 
    private bool _isFliping = false;

    void Start()
    {
        Debug.Log($"CanvasBook started, pages count: {(pages != null ? pages.Count : 0)}");
        if (pages == null || pages.Count == 0)
        {
            Debug.LogError("Pages list is empty or not assigned!");
        }
        // 初始状态排列：最后一页在最下面(0)，第一页在最上面
        UpdateSiblingOrder();
    }

    public void FlipForward()
    {
        Debug.Log($"FlipForward called, isFliping: {_isFliping}, currentIndex: {_currentPageIndex}, pageCount: {pages.Count}");
        if (_isFliping || _currentPageIndex >= pages.Count)
        {
            Debug.Log("FlipForward blocked: isFliping=" + _isFliping + ", currentIndex >= pageCount=" + (_currentPageIndex >= pages.Count));
            return;
        }
        StartCoroutine(RoutineFlip(pages[_currentPageIndex], 1f, -1f, true));
    }

    public void FlipBackward()
    {
        Debug.Log($"FlipBackward called, isFliping: {_isFliping}, currentIndex: {_currentPageIndex}");
        if (_isFliping || _currentPageIndex <= 0)
        {
            Debug.Log("FlipBackward blocked: isFliping=" + _isFliping + ", currentIndex <= 0=" + (_currentPageIndex <= 0));
            return;
        }
        _currentPageIndex--; // 往回翻时先减索引
        StartCoroutine(RoutineFlip(pages[_currentPageIndex], -1f, 1f, false));
    }

    private IEnumerator RoutineFlip(RectTransform page, float startScale, float endScale, bool isForward)
    {
        _isFliping = true;

        // 1. 动画开始前：将当前页提到绝对最前方，避免被任何页面遮挡
        page.SetAsLastSibling();

        float elapsed = 0;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            float currentScaleX = Mathf.Lerp(startScale, endScale, t);
            page.localScale = new Vector3(currentScaleX, 1, 1);
            yield return null;
        }

        page.localScale = new Vector3(endScale, 1, 1);
        
        // 2. 动画结束后：如果是向后翻，索引增加
        if (isForward) _currentPageIndex++;

        // 3. 核心：重新排列全局层级
        UpdateSiblingOrder();
        
        _isFliping = false;
    }

    private void UpdateSiblingOrder()
    {
        /* 层级目标 (从下往上，即 SiblingIndex 0 -> N):
           1. 左侧堆栈：最早翻开的页 (pages[0]) 在最底。
           2. 右侧堆栈：最后一张页 (pages[N-1]) 在中间。
           3. 右侧堆栈：当前待翻开的页 (pages[_currentPageIndex]) 在最顶。
        */

        // 第一步：处理左侧已翻开的页面 (0 到 _currentPageIndex - 1)
        // 它们按顺序排在最下面
        for (int i = 0; i < _currentPageIndex; i++)
        {
            pages[i].SetSiblingIndex(i);
        }

        // 第二步：处理右侧未翻开的页面 (_currentPageIndex 到 pages.Count - 1)
        // 为了让“下一页”在最上面，我们需要从后往前设置
        for (int i = pages.Count - 1; i >= _currentPageIndex; i--)
        {
            pages[i].SetAsLastSibling(); 
            // 这样循环结束后，pages[_currentPageIndex] 就会是最后的 LastSibling，即最顶层
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CanvasBook pointer clicked!");
    }}
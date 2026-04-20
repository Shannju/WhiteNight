using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasBook : MonoBehaviour
{
    [Header("设置")]
    public float flipDuration = 0.5f; // 翻页耗时
    public List<RectTransform> pages; // 手动拖入或在Start里获取
    
    private int _currentPageIndex = 0; // 当前待翻开的页面索引
    private bool _isFliping = false;

    /// <summary>
    /// 向后翻一页 (从右向左翻)
    /// </summary>
    public void FlipForward()
    {
        if (_isFliping || _currentPageIndex >= pages.Count) return;
        
        StartCoroutine(RoutineFlip(pages[_currentPageIndex], 1f, -1f, true));
        _currentPageIndex++;
    }

    /// <summary>
    /// 向前翻一页 (从左向右翻)
    /// </summary>
    public void FlipBackward()
    {
        if (_isFliping || _currentPageIndex <= 0) return;
        
        _currentPageIndex--;
        StartCoroutine(RoutineFlip(pages[_currentPageIndex], -1f, 1f, false));
    }

    private IEnumerator RoutineFlip(RectTransform page, float startScale, float endScale, bool isForward)
    {
        _isFliping = true;
        float elapsed = 0;
        float previousScale = startScale;

        // 翻转开始前的层级处理：正在翻动的页应该在最上方
        page.SetAsLastSibling();

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            
            // 使用 Lerp 改变缩放
            float currentScaleX = Mathf.Lerp(startScale, endScale, t);
            page.localScale = new Vector3(currentScaleX, 1, 1);

            // 当 scale 穿过 0 时，page 应该从一个队列移动到另一个队列
            if ((isForward && previousScale > 0 && currentScaleX <= 0) ||
                (!isForward && previousScale < 0 && currentScaleX >= 0))
            {
                if (isForward)
                {
                    // 进入左边队列的顶部位置，但仍然低于右边队列的页面
                    page.SetSiblingIndex(_currentPageIndex);
                }
                else
                {
                    // 进入右边队列的顶部位置
                    page.SetSiblingIndex(pages.Count - 1);
                }
            }

            previousScale = currentScaleX;
            yield return null;
        }

        page.localScale = new Vector3(endScale, 1, 1);
        _isFliping = false;

        // 翻转结束后更新所有页面的 sibling index
        UpdateSiblingOrder();
    }

    private void UpdateSiblingOrder()
    {
        // 左侧页面（已翻过去的）：正序，Sibling Index 从 0 开始递增
        for (int i = 0; i < _currentPageIndex; i++)
        {
            pages[i].SetSiblingIndex(i);
        }

        // 右侧页面（未翻的）：倒序，Sibling Index 从高到低
        int rightStart = _currentPageIndex;
        int rightEnd = pages.Count - 1;
        for (int i = rightStart; i <= rightEnd; i++)
        {
            int siblingIndex = rightStart + (rightEnd - i);
            pages[i].SetSiblingIndex(siblingIndex);
        }
    }

 
}
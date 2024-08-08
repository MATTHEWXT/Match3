using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAnimation
{
    public IEnumerator AnimateSwap(RectTransform icon1, RectTransform icon2, float duration)
    {
        Vector2 startPos1 = icon1.position;
        Vector2 startPos2 = icon2.position;
        if ((int)startPos1.x == (int)startPos2.x)
        {
            float diff = startPos2.y - startPos1.y;
            startPos1.y = startPos1.y + diff;
            startPos2.y = startPos2.y - diff;
        }
        if ((int)startPos1.y == (int)startPos2.y)
        {
            float diff = startPos2.x - startPos1.x;
            startPos1.x = startPos1.x + diff;
            startPos2.x = startPos2.x - diff;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            icon1.position = Vector2.Lerp(startPos1, startPos2, t);
            icon2.position = Vector2.Lerp(startPos2, startPos1, t);

            yield return null;
        }
    }

    public IEnumerator AnimateUpdategrid(RectTransform icon1, float index, float duration)
    {
        float time = index;
        yield return new WaitForSeconds(0.05f + time);

        Vector2 startPos = icon1.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, 0);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            icon1.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        icon1.anchoredPosition = endPos;
    }
}

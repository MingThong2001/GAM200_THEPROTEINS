using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image barLow;
    public Image barMid;
    public Image barFull;
    public RectMask2D mask;
    public float subtractAmt;

    private void Awake()
    {
        mask.padding = new Vector4(0, 0, 0, subtractAmt);
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        float healthPercent = currentHealth / maxHealth;
        subtractAmt = (1 - healthPercent) * 283f;
        // barFull fades between 1 (at 100%) > 0 (at 66%)
        float fullAlpha = Mathf.InverseLerp(0.66f, 1f, healthPercent);
        // barMid fades between 1 (at 66%) > 0 (at 33%)
        float midAlpha = Mathf.InverseLerp(0.33f, 0.66f, healthPercent);

        SetImageAlpha(barFull, fullAlpha);
        SetImageAlpha(barMid, midAlpha);
        mask.padding = new Vector4(0, 0, 0, subtractAmt);
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
    public float GetCurrentFill()
    {
        // Estimate visible fill based on the current mask padding
        // 180f is your full range (taken from SetHealth)
        float ratio = 1f - (mask.padding.w / 180f);
        return Mathf.Clamp01(ratio);
    }
}

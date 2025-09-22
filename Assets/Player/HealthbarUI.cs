using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    //UI Element
    public Slider healthSlider;
    public Image healthFill;

    public void SetHealth(float currentHealth, float maxHealth)
    {
        float ratio = 0f;

        if (maxHealth > 0f)
        {
            ratio = Mathf.Clamp01(currentHealth / maxHealth);

        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

        }

        if (healthFill != null)
        {
            healthFill.fillAmount = ratio;

        }
    }
}

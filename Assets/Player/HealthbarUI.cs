using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    //UI Element
    public Slider healthSlider;
    public Image healthFill;
   [SerializeField] public TMP_Text healthText;

    private Color startcolor = Color.white;
    private Color endcolor = Color.black;

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

        

        if (healthText != null)
        { 
            healthText.text = $"{Mathf.RoundToInt(currentHealth)}/ {Mathf.RoundToInt(maxHealth)}";  

            float healthratio = currentHealth / maxHealth;
            healthText.color = Color.Lerp(endcolor, startcolor, healthratio);   
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    //[Header("UI Seesaw")]
    //public RectTransform seesawRect;

    //[Header("Player in Scene")]
    //public Transform playerTransform;

    //[Header("UI Buttons")]
    //public Button leftButton;   // Trigger when tilts left
    //public Button rightButton;  // Trigger when tilts right

    //[Header("Settings")]
    //public float maxTiltAngle = 30f;       // Max tilt in degrees
    //public float activationAngle = 20f;    // Angle to trigger buttons
    //public float tiltSpeed = 2f;           // How fast seesaw tilts

    //private float currentAngle = 0f;
    //private bool buttonActivated = false;

    //void Update()
    //{
    //    // Determine player position relative to seesaw center
    //    float seesawX = seesawRect.position.x;
    //    float playerX = playerTransform.position.x;

    //    float leftWeight = playerX < seesawX ? 1f : 0f;
    //    float rightWeight = playerX > seesawX ? 1f : 0f;

    //    // Calculate target angle
    //    float weightDiff = rightWeight - leftWeight;
    //    float targetAngle = Mathf.Clamp(weightDiff * maxTiltAngle, -maxTiltAngle, maxTiltAngle);

    //    // Smoothly rotate seesaw
    //    currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * tiltSpeed);
    //    seesawRect.localRotation = Quaternion.Euler(0, 0, currentAngle);

    //    // Trigger buttons
    //    if (currentAngle <= -activationAngle && !buttonActivated)
    //    {
    //        buttonActivated = true;
    //        leftButton.onClick.Invoke();
    //    }
    //    else if (currentAngle >= activationAngle && !buttonActivated)
    //    {
    //        buttonActivated = true;
    //        rightButton.onClick.Invoke();
    //    }

    //    // Reset
    //    if (Mathf.Abs(currentAngle) < 5f)
    //        buttonActivated = false;
    //}
}

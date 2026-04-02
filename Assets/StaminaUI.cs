using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public Slider staminaSlider;

    void Update()
    {
        staminaSlider.value = SharedBlackboard.Instance.sharedStamina;
    }
}
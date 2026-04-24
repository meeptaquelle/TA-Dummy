using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public Slider staminaSlider;

    void Update()
    {
        staminaSlider.value = SharedBlackboard.Instance.sharedStamina;
    }
}

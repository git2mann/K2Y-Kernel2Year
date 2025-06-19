using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("IsHovered", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("IsHovered", false);
    }
}
// This script animates menu buttons when hovered over.
// It uses Unity's Animator component to change the button's appearance based on pointer events.
// Make sure to attach an Animator with a "IsHovered" boolean parameter to the button GameObject.
// The Animator should have transitions set up to animate the button's appearance when "IsHovered"
// is true or false. This allows for smooth visual feedback when the user interacts with the menu.
// The script implements IPointerEnterHandler and IPointerExitHandler interfaces to detect pointer events.
// Ensure that the GameObject this script is attached to has an Animator component with the appropriate animations
// and transitions configured in the Animator window. The "IsHovered" parameter should control the animations
// for the button's hover state, allowing for visual feedback when the user hovers over or exits the button.
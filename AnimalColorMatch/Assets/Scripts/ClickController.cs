using UnityEngine;

public class ClickController : MonoBehaviour
{
    public Animator animator;

    public void OnButtonClicked()
    {
        if (animator != null)
        {
            animator.SetTrigger("Click");
        }
    }
}
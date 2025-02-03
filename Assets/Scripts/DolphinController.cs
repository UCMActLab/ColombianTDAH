using UnityEngine;

public class DolphinController : MonoBehaviour
{
    protected Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("Jump");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

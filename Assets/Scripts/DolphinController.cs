using UnityEngine;

public class DolphinController : MonoBehaviour
{
    protected Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Jump()
    {
        animator.SetTrigger("Jump");
    }
    void SpecialJump()
    {
        animator.SetTrigger("SpecialJump");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) //haciendo pruebas
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1)) //haciendo pruebas
        {
            SpecialJump();
        }
    }
}

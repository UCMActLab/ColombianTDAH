using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimatorManager : MonoBehaviour
{
    /// <summary>
    /// AnimatorManager instance
    /// </summary>
    static private AnimatorManager _instance;

    /// <summary>
    /// Public reference to AnimatorManager instance
    /// </summary>
    static public AnimatorManager Instance { get { return _instance; } }


    Animator animatorLibro;
    private string currentAnimation = "";

    /// <summary>
    /// Animator Manager instance initialization
    /// </summary>
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeAnimation(string newAnim, float crossfade = 0.2f)
    {
        if (currentAnimation != newAnim)
        {
            animatorLibro.speed = 1f;
            animatorLibro.CrossFade(newAnim, crossfade);
            currentAnimation = newAnim;
        }
    }

    public void PlayAndPauseAt(string animName, float pauseAtNormalizedTime)
    {
        animatorLibro.speed = 1f;
        animatorLibro.CrossFade(animName, 0); // Empieza desde el principio
        StartCoroutine(PauseAnimationAt(animName, pauseAtNormalizedTime));
    }

    private IEnumerator PauseAnimationAt(string animName, float targetNormalizedTime)
    {
        yield return null;

        while (true)
        {
            AnimatorStateInfo state = animatorLibro.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(animName) && state.normalizedTime >= targetNormalizedTime)
            {
                animatorLibro.speed = 0f;
                break;
            }
            yield return null;
        }
    }




    public void SetApplyRootMotion(bool newValue)
    {
        animatorLibro.applyRootMotion = newValue;
    }

    public void SetAnimatorLibro()
    {
        animatorLibro = LevelKitchenManager.Instance.GetLibro().GetComponent<Animator>();
    }
}

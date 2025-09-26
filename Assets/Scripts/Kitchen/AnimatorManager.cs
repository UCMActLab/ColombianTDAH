using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ObjetosAnim
{
    Libro,
    Tabla_De_Picar,
    Mezcladora,
    Licuadora,
    Horno,
    Olla,
    Olla_A_Presion,
    Sarten,
    Mano,
    Esponja
}

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


    private Dictionary<ObjetosAnim, Animator> animators = new Dictionary<ObjetosAnim, Animator>();
    private Dictionary<ObjetosAnim, string> currentAnimations = new Dictionary<ObjetosAnim, string>();

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

    public void ChangeAnimation(ObjetosAnim ob, string newAnim, float crossfade = 0.2f)
    {
        if (!animators.TryGetValue(ob, out var anim) || anim == null) return;
        if (currentAnimations[ob] != newAnim)
        {
            animators[ob].speed = 1f;
            animators[ob].CrossFade(newAnim, crossfade);
            currentAnimations[ob] = newAnim;
        }
    }

    public void PlayAndPauseAt(ObjetosAnim ob, string animName, float pauseAtNormalizedTime)
    {
        currentAnimations[ob] = animName;
        animators[ob].speed = 1f;
        animators[ob].CrossFade(animName, 0); // Empieza desde el principio
        StartCoroutine(PauseAnimationAt(ob, animName, pauseAtNormalizedTime));
    }

    private IEnumerator PauseAnimationAt(ObjetosAnim ob, string animName, float targetNormalizedTime)
    {
        yield return null;

        while (true)
        {
            AnimatorStateInfo state = animators[ob].GetCurrentAnimatorStateInfo(0);
            if (state.IsName(animName) && state.normalizedTime >= targetNormalizedTime)
            {
                animators[ob].speed = 0f;
                break;
            }
            yield return null;
        }
    }

    public void SetApplyRootMotion(ObjetosAnim ob, bool newValue)
    {
        animators[ob].applyRootMotion = newValue;
    }

    public void SetAnimator(ObjetosAnim ob, Animator anim)
    {
        animators[ob] = anim;
        currentAnimations[ob] = "";
    }
}

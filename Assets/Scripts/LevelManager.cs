using UnityEngine;

public class DolphinLevelSelectorManager : MonoBehaviour
{
    [SerializeField]
    DolphinLevelSelector[] _levelSelectors;

    private bool[] _levelsUnlocked;
    private int _lastUnlocked;

    [SerializeField]
    int currentGameType = 1;
    private void Awake()
    {
        if(_levelSelectors.Length == 0)
        {
            _levelSelectors = GetComponentsInChildren<DolphinLevelSelector>();
        }

        Debug.Log("Level manager started with " + _levelSelectors.Length + " level selectors.");
        _levelsUnlocked = new bool[_levelSelectors.Length];

        _lastUnlocked = 0;
        for (int i = 0; i < _levelsUnlocked.Length; ++i)
        {
            _levelsUnlocked[i] = _levelSelectors[i].IsUnlocked();
            if ((i > 0 && _levelsUnlocked[i - 1] && _levelsUnlocked[i]) || _levelsUnlocked[i])
            {
                _levelSelectors[i].UnlockLevel();
                if (i > 0) _levelSelectors[i - 1].SetIsLastUnlockedLevel(false);
                _lastUnlocked = i;
            }
            else
            {
                _levelSelectors[i].LockLevel();
            }
        }

        //EventRegister.Instance.CurrentGamePlaying
        UnlockUntil((SceneLoader.Instance.getMaxLevelId(currentGameType)).ToString());

    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && _lastUnlocked < _levelsUnlocked.Length - 1)
        {
            _levelSelectors[_lastUnlocked].SetIsLastUnlockedLevel(false);
            _levelsUnlocked[++_lastUnlocked] = true;
            _levelSelectors[_lastUnlocked].UnlockLevel();
            _levelSelectors[_lastUnlocked].SetIsLastUnlockedLevel(true);
        }
    }

    public void UnlockUntil(string lvlTxt)
    {
        //para que no salte error si pones algo distinto a numeros validos
        if (string.IsNullOrWhiteSpace(lvlTxt))
        {
            return;
        }

        if (!int.TryParse(lvlTxt, out int lvl)) //se prueba a parsear y ya lo tenemos
        {
            return;
        }

        Debug.Log($"Unlocking levels until {lvl}");
        if (lvl < 0) return;
        else if (lvl > _levelSelectors.Length) lvl = _levelSelectors.Length; //lvl es hasta el 7 

        SceneLoader.Instance.setMaxLevelId(lvl, currentGameType);

        _lastUnlocked = -1;

        for (int i = 0; i < _levelSelectors.Length; ++i)
        {
            if (i < lvl) // desbloquear hasta level
            {
                _levelsUnlocked[i] = true;
                _levelSelectors[i].UnlockLevel();
                _lastUnlocked = i;
            }
            else // bloquear los que esten por encima
            {
                _levelsUnlocked[i] = false;
                _levelSelectors[i].LockLevel();
            }

            // quitamos todos por defecto y luego ponemos el que sea de verdad
            _levelSelectors[i].SetIsLastUnlockedLevel(false);
        }

        if (_lastUnlocked >= 0)
        {
            _levelSelectors[_lastUnlocked].SetIsLastUnlockedLevel(true);
        }


    }
}
using UnityEngine;

public class DolphinLevelSelectorManager : MonoBehaviour
{
    [SerializeField]
    DolphinLevelSelector[] _levelSelectors;

    private bool[] _levelsUnlocked;
    private int _lastUnlocked;

    private void Awake()
    {
        if (_levelSelectors.Length == 0)
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

        Debug.Log("DolphinLevelSelectorManager Awake called.");
        SceneLoader sceneLoader = GameObject.Find("SceneLoader").GetComponent<SceneLoader>();
        if (sceneLoader.getLastLevelWon())
        {
            UnlockUntil((sceneLoader.getLevelId() + 1).ToString());
        }
        else
        {
            UnlockUntil((sceneLoader.getLevelId()).ToString());
        }
        //Lo devolvemos al default
        sceneLoader.setLastLevelWon(false);
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

        int lvl = int.Parse(lvlTxt);

        Debug.Log($"Unlocking levels until {lvl}");
        if (lvl < 0) return;
        else if (lvl > _levelSelectors.Length) lvl = _levelSelectors.Length;
        for (int i = 0; i < lvl; ++i)
        {
            if (!_levelsUnlocked[i])
            {
                _levelsUnlocked[i] = true;
                _levelSelectors[i].UnlockLevel();
                if (i > 0) _levelSelectors[i - 1].SetIsLastUnlockedLevel(false);
                _lastUnlocked = i;
            }
        }
        if (lvl < _levelsUnlocked.Length - 1)
        {
            _levelSelectors[lvl + 1].SetIsLastUnlockedLevel(true);
        }
        for (int i = lvl; i < _levelSelectors.Length; ++i)
        {
            _levelsUnlocked[i] = false;
            _levelSelectors[i].LockLevel();
        }
    }
}
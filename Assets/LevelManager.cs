using UnityEngine;

public class DolphinLevelSelectorManager : MonoBehaviour
{
    [SerializeField]
    DolphinLevelSelector[] _levelSelectors;

    private bool[] _levelsUnlocked;
    private int _lastUnlocked;

    private void Start()
    {
        if (_levelSelectors.Length == 0)
        {
            _levelSelectors = GetComponentsInChildren<DolphinLevelSelector>();
        }

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
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TypeRacerScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public KMRuleSeedable RuleSeedable;

    public KMSelectable[] LetterSels;
    public KMSelectable GoButton;
    public KMSelectable DeleteButton;
    public TextMesh[] ButtonTexts;
    public GameObject CarObj;
    public TextMesh ScreenText;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private int _aIndex;
    private float _currentCarPos;
    private float _endCarPos;
    private bool _isTyping;
    private int[] _shuffle = new int[26];
    private string _display = "";
    private Coroutine _timer;
    private string _solution;

    private string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private string[][] WORDLIST = new string[5][]
    {
        new string[16]
        {"equator","grounds","nursery","confine","rainbow","fantasy","assault","payment","nervous","distort","concern","academy","loyalty","compact","impound","tourist"},
        new string[16]
        {"inflate","funeral","breathe","project","storage","primary","attract","horizon","sweater","passive","capture","graphic","forward","holiday","descent","section"},
        new string[16]
        {"ceiling","install","mislead","retired","provoke","thirsty","freedom","abandon","confuse","kitchen","recruit","related","exploit","scatter","battery","wrestle"},
        new string[16]
        {"musical","elegant","extract","tension","serious","perfect","laundry","illness","benefit","peasant","welfare","strange","history","thought","harvest","teacher"},
        new string[16]
        {"harmful","reptile","impress","wording","soldier","highway","contain","brother","alcohol","healthy","lecture","venture","startup","miracle","virtual","caramel"}
    };

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        ShuffleLetters();
        FindSolutionWord();
        for (int i = 0; i < LetterSels.Length; i++)
        {
            int j = i;
            LetterSels[i].OnInteract += delegate ()
            {
                if (!_moduleSolved && _isTyping)
                    LetterPress(j);
                return false;
            };
        }
        GoButton.OnInteract += delegate ()
        {
            if (!_moduleSolved)
                GoButtonPress();
            return false;
        };
        DeleteButton.OnInteract += delegate ()
        {
            if (!_moduleSolved)
                DeleteButtonPress();
            return false;
        };
    }

    private void LetterPress(int letter)
    {
        if (_display.Length < 7)
        {
            _display += LETTERS[_shuffle[letter]];
            ScreenText.text = _display;
            ShuffleLetters();
        }
        if (_display.ToLowerInvariant() == _solution)
        {
            StopCoroutine(_timer);
            StartCoroutine(StopCar(true));
        }
    }

    private void GoButtonPress()
    {
        if (!_isTyping)
        {
            _isTyping = true;
            _timer = StartCoroutine(Timer());
        }
    }

    private void DeleteButtonPress()
    {
        if (_isTyping)
        {
            if (_display.Length > 0)
                _display = _display.Substring(0, _display.Length - 1);
        }
        else
            _display = "";
        ScreenText.text = _display;
    }

    private IEnumerator Timer()
    {
        var duration = 15f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            _currentCarPos = Mathf.Lerp(-0.0625f, 0.025f, elapsed / duration);
            CarObj.transform.localPosition = new Vector3(_currentCarPos, 0f, 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        _isTyping = false;
        StartCoroutine(StopCar(false));
    }

    private IEnumerator StopCar(bool isSolve)
    {
        _endCarPos = isSolve ? 0.025f : -0.0625f;
        var duration = 1.5f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            CarObj.transform.localPosition = new Vector3(Mathf.Lerp(_currentCarPos, _endCarPos, elapsed / duration), 0f, 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        if (isSolve)
        {
            Module.HandlePass();
            _moduleSolved = true;
        }
        else
        {
            Module.HandleStrike();
            FindSolutionWord();
        }
    }

    private void FindSolutionWord()
    {
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            if (_shuffle[_aIndex * 5 + i] > _shuffle[_aIndex * 5 + (i + 1)])
                index += (int)Math.Pow(2, i);
        }
        _solution = WORDLIST[_aIndex][index];
        Debug.LogFormat("solution is {0}", _solution);
    }

    private void ShuffleLetters()
    {
        _shuffle = Enumerable.Range(0, 26).ToArray().Shuffle();
        foreach (var i in _shuffle)
        {
            ButtonTexts[i].text = LETTERS[_shuffle[i]].ToString();
            if (LETTERS[_shuffle[i]].ToString() == "A")
                _aIndex = i != 25 ? i / 5 : 4;
        }
    }
}

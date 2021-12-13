using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class TypeRacerScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;

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
        {"fantasy","benefit","nursery","startup","related","laundry","install","thought","section","harvest","healthy","teacher","forward","contain","impress","funeral"},
        new string[16]
        {"confuse","tourist","passive","welfare","perfect","storage","breathe","compact","inflate","tension","provoke","musical","battery","concern","horizon","retired"},
        new string[16]
        {"distort","academy","freedom","extract","sweater","abandon","loyalty","payment","recruit","virtual","highway","grounds","graphic","scatter","exploit","kitchen"},
        new string[16]
        {"venture","thirsty","rainbow","project","serious","ceiling","strange","assault","alcohol","miracle","peasant","illness","nervous","caramel","wrestle","capture"},
        new string[16]
        {"elegant","confine","impound","mislead","descent","equator","history","primary","holiday","reptile","wording","attract","brother","harmful","soldier","lecture"}
    };
    private string[] POSITIONS = { "first", "second", "third", "fourth", "fifth" };

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
        LetterSels[letter].AddInteractionPunch(0.25f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
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
            _moduleSolved = true;
        }
    }

    private void GoButtonPress()
    {
        if (!_isTyping)
        {
            Audio.PlaySoundAtTransform("carGoPress", transform);
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
        _moduleSolved = true;
        _isTyping = false;
        StartCoroutine(StopCar(false));
    }

    private IEnumerator StopCar(bool isSolve)
    {
        if (!isSolve)
            Audio.PlaySoundAtTransform("carStrike", transform);
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
            Debug.LogFormat("[Type Racer #{0}] You typed {1}. Module solved!", _moduleId, _solution.ToUpperInvariant());
            Module.HandlePass();
            Audio.PlaySoundAtTransform("carSolve", transform);
        }
        else
        {
            _moduleSolved = false;
            Module.HandleStrike();
            FindSolutionWord();
        }
    }

    private void FindSolutionWord()
    {
        Debug.LogFormat("[Type Racer #{0}] The 'A' is in row {1}.", _moduleId, _aIndex + 1);
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            if (_shuffle[_aIndex * 5 + i] > _shuffle[_aIndex * 5 + (i + 1)])
            {
                index += (int)Math.Pow(2, 3 - i);
                Debug.LogFormat("[Type Racer #{0}] The {1} letter, {3}, comes later in the alphabet than the {2} letter, {4}.", _moduleId, POSITIONS[i], POSITIONS[i + 1], LETTERS[_shuffle[_aIndex * 5 + i]], LETTERS[_shuffle[_aIndex * 5 + (i + 1)]]);
            }
            else
            {
                Debug.LogFormat("[Type Racer #{0}] The {1} letter, {3}, comes earlier in the alphabet than the {2} letter, {4}.", _moduleId, POSITIONS[i], POSITIONS[i + 1], LETTERS[_shuffle[_aIndex * 5 + i]], LETTERS[_shuffle[_aIndex * 5 + (i + 1)]]);
            }
        }
        _solution = WORDLIST[_aIndex][index];
        Debug.LogFormat("[Type Racer #{0}] The word that must be typed is {1}.", _moduleId, _solution.ToUpperInvariant());
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
#pragma warning disable 414
    private string TwitchHelpMessage = "Submit the word by typing !{0} submit <word>";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (_moduleSolved)
            yield break;
        var m = Regex.Match(command, @"^\s*(?:submit)\s+([a-z]{7})\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;
        yield return null;
        for (int i = 0; i < 7; i++)
        {
            DeleteButtonPress();
            yield return new WaitForSeconds(0.05f);
        }
        GoButtonPress();
        yield return new WaitForSeconds(0.1f);
        var ans = m.Groups[1].Value.ToUpperInvariant();
        for (int i = 0; i < ans.Length; i++)
        {
            for (int j = 0; j < LETTERS.Length; j++)
            {
                if (ans[i] - 'A' == _shuffle[j])
                {
                    LetterPress(j);
                    yield return new WaitForSeconds(0.2f);
                    break;
                }
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        GoButtonPress();
        yield return null;
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 7; i++)
        {
            _display += _solution.Substring(i, 1).ToUpperInvariant();
            ScreenText.text = _display;
            ShuffleLetters();
            if (i == 6)
                LetterPress(0);
            yield return null;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
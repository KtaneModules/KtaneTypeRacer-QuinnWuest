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
    private int _aIndex;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private bool _isTyping;
    private int[] _shuffle = new int[26];
    private string _display = "";

    private string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private string[][] WORDLIST = new string[10][]
    {
        new string[8]
        {"equator","grounds","nursery","confine","rainbow","fantasy","assault","payment"},
        new string[8]
        {"nervous","distort","concern","academy","loyalty","compact","impound","tourist"},
        new string[8]
        {"inflate","funeral","breathe","project","storage","primary","attract","horizon"},
        new string[8]
        {"sweater","passive","capture","graphic","forward","holiday","descent","section"},
        new string[8]
        {"ceiling","install","mislead","retired","provoke","thirsty","freedom","abandon"},
        new string[8]
        {"confuse","kitchen","recruit","related","exploit","scatter","battery","wrestle"},
        new string[8]
        {"musical","elegant","extract","tension","serious","perfect","laundry","illness"},
        new string[8]
        {"benefit","peasant","welfare","strange","history","thought","harvest","teacher"},
        new string[8]
        {"harmful","reptile","impress","wording","soldier","highway","contain","brother"},
        new string[8]
        {"alcohol","healthy","lecture","venture","startup","miracle","virtual","caramel"}
    };

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        _shuffle = Enumerable.Range(0, 26).ToArray().Shuffle();
        foreach (var i in _shuffle)
        {
            ButtonTexts[i].text = LETTERS[_shuffle[i]].ToString();
            if (LETTERS[_shuffle[i]].ToString() == "A")
                _aIndex = i != 25 ? i / 5 : 4;
        }
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
        _display += LETTERS[_shuffle[letter]];
        Debug.LogFormat("{0}", _display);
        ScreenText.text = _display;
    }

    private void GoButtonPress()
    {
        if (!_isTyping)
        {
            _isTyping = true;
            StartCoroutine(Timer());
        }
    }

    private void DeleteButtonPress()
    {
        
    }

    private IEnumerator Timer()
    {
        var duration = 15f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            CarObj.transform.localPosition = new Vector3(Mathf.Lerp(-0.0625f, 0.025f, elapsed / duration), 0f, 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
    }
}

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
    public KMSelectable[] LetterSels;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

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
        
    }
}

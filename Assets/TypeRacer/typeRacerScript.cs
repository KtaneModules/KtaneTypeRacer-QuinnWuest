using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class typeRacerScript : MonoBehaviour
{
    public class ModSettingsJSON
    {
        public int countdownTime;
        public string note;
    }
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public AudioClip[] sounds;
    public KMModSettings modSettings;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;
    public KMSelectable goButton;
    public KMSelectable deleteButton;
    public KMSelectable[] letteredButtons;
    public TextMesh[] lettersTextMesh;
    public GameObject car;
	public string[] letters;
    public List<int> chosenLettersIndex;
    public TextMesh screenTextMesh;
    public string screenText = "";
    public int aIndex;
    public int aRow;
    public int tempRow;
    public int tempCol;
    public bool goButtonPressed;
    private bool isCarMoving;
    public float carTimer;
    public string correctWord;
    public string tempScreenText;
    public string[] randomizedLetters = new string[26];
    public string[][] wordList = new string[10][]
    {
        new string[8]
        {
			"equator",
			"grounds",
			"nursery",
			"confine",
			"rainbow",
			"fantasy",
			"assault",
			"payment"
		},
		new string[8]
		{
			"nervous",
			"distort",
			"concern",
			"academy",
			"loyalty",
			"compact",
			"impound",
			"tourist"
		},
		new string[8]
		{
			"inflate",
			"funeral",
			"breathe",
			"project",
			"storage",
			"primary",
			"attract",
			"horizon"
		},
		new string[8]
		{
			"sweater",
			"passive",
			"capture",
			"graphic",
			"forward",
			"holiday",
			"descent",
			"section"
		},
		new string[8]
		{
			"ceiling",
			"install",
			"mislead",
			"retired",
			"provoke",
			"thirsty",
			"freedom",
			"abandon"
		},
		new string[8]
		{
			"confuse",
			"kitchen",
			"recruit",
			"related",
			"exploit",
			"scatter",
			"battery",
			"wrestle"
		},
		new string[8]
		{
			"musical",
			"elegant",
			"extract",
			"tension",
			"serious",
			"perfect",
			"laundry",
			"illness"
		},
		new string[8]
		{
			"benefit",
			"peasant",
			"welfare",
			"strange",
			"history",
			"thought",
			"harvest",
			"teacher"
		},
		new string[8]
		{
			"harmful",
			"reptile",
			"impress",
			"wording",
			"soldier",
			"highway",
			"contain",
			"brother"
		},
		new string[8]
		{
			"alcohol",
			"healthy",
			"lecture",
			"venture",
			"startup",
			"miracle",
			"virtual",
			"caramel"
		}
	};

	void Awake()
    {
		moduleId = moduleIdCounter++;
		foreach (KMSelectable letteredButton in letteredButtons)
        {
			KMSelectable kMselectable = letteredButton;
			letteredButton.OnInteract += delegate
			{
				LetteredButtonPress(letteredButton);
				letteredButton.AddInteractionPunch();
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
				return false;
			};
        }
		goButton.OnInteract += delegate
		{
			goButtonPress();
			goButton.AddInteractionPunch();
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			return false;
		};
		deleteButton.OnInteract += delegate
		{
			deleteButtonPress();
			deleteButton.AddInteractionPunch();
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			return false;
		};
    }

    void Start() {
		screenTextMesh.text = "";
		generateLetters();
		getRandomizedLetters();
		findCorrectWord();
	}

	private void checkForAnswer()
    {
		if (screenTextMesh.text.Length == 7)
        {
			tempScreenText = screenTextMesh.text.ToLowerInvariant();
			if (tempScreenText.Equals(correctWord))
            {
				Debug.LogFormat("[Type Racer #{0}] Inputted {1}. Correct. Module solved!", moduleId, tempScreenText);
				Audio.PlaySoundAtTransform("carSolve", car.transform);
				GetComponent<KMBombModule>().HandlePass();
				goButtonPressed = false;
				isCarMoving = false;
				StartCoroutine(moveCarToFinish());
				moduleSolved = true;
            }
			else if (!moduleSolved)
            {
				Debug.LogFormat("[Type Racer #{0}] Inputted {1}. Incorrect. Strike.", moduleId, tempScreenText);
				Audio.PlaySoundAtTransform("carStrike", car.transform);
				GetComponent<KMBombModule>().HandleStrike();
				goButtonPressed = false;
				isCarMoving = false;
				StartCoroutine(moveCarToStart());
				findCorrectWord();
            }
        }
    }

	private void generateLetters()
    {
		for (int i = 0; i <= 25; i++)
        {
			int num = UnityEngine.Random.Range(0, 26);
			while (chosenLettersIndex.Contains(num))
				num = UnityEngine.Random.Range(0, 26);
			chosenLettersIndex.Add(num);
			lettersTextMesh[i].text = letters[num];
			randomizedLetters[i] = lettersTextMesh[i].text;
        }
    }

	private void getRandomizedLetters()
    {
		for (int i = 0; i <= 25; i++)
			randomizedLetters[i] = lettersTextMesh[i].text;
    }

	private void LetteredButtonPress(KMSelectable letteredButton)
    {
		if (!moduleSolved && goButtonPressed)
        {
			if (screenTextMesh.text.Length != 7)
            {
				TextMesh obj = screenTextMesh;
				obj.text = obj.text + letteredButton.GetComponentInChildren<TextMesh>().text;
				chosenLettersIndex.Clear();
				generateLetters();
            }
			checkForAnswer();
        }
    }

	private void goButtonPress()
    {
		if (!moduleSolved)
        {
			Audio.PlaySoundAtTransform("carGoPress", car.transform);
			goButtonPressed = true;
			screenTextMesh.text = "";
			StartCoroutine(moveCar());
        }
    }

	private void deleteButtonPress()
    {
		if (!moduleSolved)
        {
			if (screenTextMesh.text.Length == 7)
				screenTextMesh.text = "";
			else if (screenTextMesh.text.Length > 0)
				screenTextMesh.text = screenTextMesh.text.Substring(0, screenTextMesh.text.Length - 1);
        }
    }

	private IEnumerator moveCar()
    {
		yield return null;
		isCarMoving = true;
		for (float i = 0; i <= 100; i += 1)
        {
			car.transform.localPosition = new Vector3(-0.045f + i * 0.00087f, 0.023f, 0.075f);
			yield return new WaitForSeconds(0.15f);
			if (Mathf.Approximately(i, 100f))
            {
				Audio.PlaySoundAtTransform("carStrike", car.transform);
				Debug.LogFormat("[Type Racer #{0}] Ran out of time. Strike.", moduleId);
				GetComponent<KMBombModule>().HandleStrike();
				StartCoroutine(moveCarToStart());
				chosenLettersIndex.Clear();
				generateLetters();
				findCorrectWord();
            }
			if (!isCarMoving)
				i = 101;
        }
    }

	private IEnumerator moveCarToStart()
    {
		float posX = car.transform.localPosition.x;
		yield return null;
		for (float i = 0; i <= 20; i += 1)
        {
			car.transform.localPosition = new Vector3(posX - i * (posX + 0.045f) / 20f, 0.023f, 0.075f);
			yield return new WaitForSeconds(0.05f);
        }
    }

	private IEnumerator moveCarToFinish()
    {
		yield return null;
		float posX = car.transform.localPosition.x;
		for (float i = 0; i <= 20; i += 1)
        {
			car.transform.localPosition = new Vector3(posX - i * (posX - 0.042f) / 20f, 0.023f, 0.075f);
			yield return new WaitForSeconds(0.05f);
        }
    }

	private void findRowOfA()
    {
		searchForA();
		if (aIndex >= 0 && aIndex <= 4)
        {
			aRow = 0;
			Debug.LogFormat("[Type Racer #{0}] Letter 'A' is in Row #1", moduleId);
        }
		else if (aIndex >= 5 && aIndex <= 9)
        {
			aRow = 1;
			Debug.LogFormat("[Type Racer #{0}] Letter 'A' is in Row #2", moduleId);
        }
		else if (aIndex >= 10 && aIndex <= 14)
		{
			aRow = 2;
			Debug.LogFormat("[Type Racer #{0}] Letter 'A' is in Row #3", moduleId);
		}
		else if (aIndex >= 14 && aIndex <= 19)
		{
			aRow = 3;
			Debug.LogFormat("[Type Racer #{0}] Letter 'A' is in Row #4", moduleId);
		}
		else
		{
			aRow = 4;
			Debug.LogFormat("[Type Racer #{0}] Letter 'A' is in Row #5", moduleId);
		}
	}

	private void searchForA()
    {
		TextMesh value = lettersTextMesh.First(x => x.text == "A");
		aIndex = Array.IndexOf(lettersTextMesh, value);
    }

	private void findCorrectWord()
    {
		findRowOfA();
		findTempRow();
		if (tempRow == 0)
		{
			if (aRow == 0)
			{
				correctWord = wordList[0][5];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[5][0];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[1][1];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[9][3];
			}
			else
			{
				correctWord = wordList[6][1];
			}
		}
		else if (tempRow == 1)
		{
			if (aRow == 0)
			{
				correctWord = wordList[7][0];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[1][7];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[1][3];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[4][5];
			}
			else
			{
				correctWord = wordList[0][3];
			}
		}
		else if (tempRow == 2)
		{
			if (aRow == 0)
			{
				correctWord = wordList[0][2];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[3][1];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[4][6];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[0][4];
			}
			else
			{
				correctWord = wordList[1][6];
			}
		}
		else if (tempRow == 3)
		{
			if (aRow == 0)
			{
				correctWord = wordList[9][4];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[7][2];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[6][2];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[2][3];
			}
			else
			{
				correctWord = wordList[4][2];
			}
		}
		else if (tempRow == 4)
		{
			if (aRow == 0)
			{
				correctWord = wordList[5][3];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[6][5];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[3][0];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[6][4];
			}
			else
			{
				correctWord = wordList[3][6];
			}
		}
		else if (tempRow == 5)
		{
			if (aRow == 0)
			{
				correctWord = wordList[6][6];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[2][4];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[4][7];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[4][0];
			}
			else
			{
				correctWord = wordList[0][0];
			}
		}
		else if (tempRow == 6)
		{
			if (aRow == 0)
			{
				correctWord = wordList[4][1];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[2][2];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[1][4];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[7][3];
			}
			else
			{
				correctWord = wordList[7][4];
			}
		}
		else if (tempRow == 7)
		{
			if (aRow == 0)
			{
				correctWord = wordList[7][5];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[1][5];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[0][7];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[0][6];
			}
			else
			{
				correctWord = wordList[2][5];
			}
		}
		else if (tempRow == 8)
		{
			if (aRow == 0)
			{
				correctWord = wordList[3][7];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[2][0];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[5][2];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[9][0];
			}
			else
			{
				correctWord = wordList[3][5];
			}
		}
		else if (tempRow == 9)
		{
			if (aRow == 0)
			{
				correctWord = wordList[7][6];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[6][3];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[9][6];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[9][5];
			}
			else
			{
				correctWord = wordList[8][1];
			}
		}
		else if (tempRow == 10)
		{
			if (aRow == 0)
			{
				correctWord = wordList[9][1];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[4][4];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[8][5];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[7][1];
			}
			else
			{
				correctWord = wordList[8][3];
			}
		}
		else if (tempRow == 11)
		{
			if (aRow == 0)
			{
				correctWord = wordList[7][7];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[6][0];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[0][1];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[6][7];
			}
			else
			{
				correctWord = wordList[2][6];
			}
		}
		else if (tempRow == 12)
		{
			if (aRow == 0)
			{
				correctWord = wordList[3][4]; // case A of [3][4] -> one of these should be [2][7]
            }
			else if (aRow == 1)
			{
				correctWord = wordList[5][6];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[3][3];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[1][0];
			}
			else
			{
				correctWord = wordList[8][7];
			}
		}
		else if (tempRow == 13)
		{
			if (aRow == 0)
			{
				correctWord = wordList[8][6];
			}
			else if (aRow == 1)
			{
				correctWord = wordList[1][2];
			}
			else if (aRow == 2)
			{
				correctWord = wordList[5][5];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[9][7];
			}
			else
			{
				correctWord = wordList[8][0];
			}
		}
		else if (tempRow == 14)
		{
			if (aRow == 0)
			{
				correctWord = wordList[8][2]; // case A of [2][7] -> should be C9 -> [8][2]
			}
			else if (aRow == 1)
			{
				correctWord = wordList[2][7]; // case B of [3][4] -> one of these should be [2][7]
			}
			else if (aRow == 2)
			{
				correctWord = wordList[5][4];
			}
			else if (aRow == 3)
			{
				correctWord = wordList[5][7];
			}
			else
			{
				correctWord = wordList[8][4];
			}
		}
		else if (aRow == 0)
		{
			correctWord = wordList[2][1];
		}
		else if (aRow == 1)
		{
			correctWord = wordList[4][3];
		}
		else if (aRow == 2)
		{
			correctWord = wordList[5][1];
		}
		else if (aRow == 3)
		{
			correctWord = wordList[3][2];
		}
		else
		{
			correctWord = wordList[9][2];
		}
		Debug.LogFormat("[Type Racer #{0}] The correct word is {1}", moduleId, correctWord);
	}
	private void findTempRow()
	{
		if (chosenLettersIndex[5 * aRow] < chosenLettersIndex[5 * aRow + 1])
		{
			if (chosenLettersIndex[5 * aRow + 1] < chosenLettersIndex[5 * aRow + 2])
			{
				if (chosenLettersIndex[5 * aRow + 2] < chosenLettersIndex[5 * aRow + 3])
				{
					if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
					{
						tempRow = 0;
						Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
						Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
						Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
						Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
					}
					else
					{
						tempRow = 1;
						Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
						Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
						Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
						Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
					}
				}
				else if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
				{
					tempRow = 2;
					Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
					Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
					Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
					Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
				}
				else
				{
					tempRow = 3;
					Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
					Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
					Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
					Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
				}
			}
			else if (chosenLettersIndex[5 * aRow + 2] < chosenLettersIndex[5 * aRow + 3])
			{
				if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
				{
					tempRow = 4;
					Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
					Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
					Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
					Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
				}
				else
				{
					tempRow = 5;
					Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
					Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
					Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
					Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
				}
			}
			else if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
			{
				tempRow = 6;
				Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
				Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
				Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
				Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
			}
			else
			{
				tempRow = 7;
				Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes earlier than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
				Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
				Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
				Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
			}
		}
		else if (chosenLettersIndex[5 * aRow + 1] < chosenLettersIndex[5 * aRow + 2])
		{
			if (chosenLettersIndex[5 * aRow + 2] < chosenLettersIndex[5 * aRow + 3])
			{
				if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
				{
					tempRow = 8;
					Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
					Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
					Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
					Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
				}
				else
				{
					tempRow = 9;
					Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
					Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
					Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
					Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
				}
			}
			else if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
			{
				tempRow = 10;
				Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
				Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
				Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
				Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
			}
			else
			{
				tempRow = 11;
				Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
				Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes earlier than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
				Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
				Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
			}
		}
		else if (chosenLettersIndex[5 * aRow + 2] < chosenLettersIndex[5 * aRow + 3])
		{
			if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
			{
				tempRow = 12;
				Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
				Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
				Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
				Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
			}
			else
			{
				tempRow = 13;
				Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
				Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
				Debug.LogFormat("[Type Racer #{0}] 3, {1}, comes earlier than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
				Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
			}
		}
		else if (chosenLettersIndex[5 * aRow + 3] < chosenLettersIndex[5 * aRow + 4])
		{
			tempRow = 14;
			Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
			Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
			Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
			Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes earlier than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
		}
		else
		{
			tempRow = 15;
			Debug.LogFormat("[Type Racer #{0}] Button 1, {1}, comes later than Button 2, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow]], letters[chosenLettersIndex[5 * aRow + 1]]);
			Debug.LogFormat("[Type Racer #{0}] Button 2, {1}, comes later than Button 3, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 1]], letters[chosenLettersIndex[5 * aRow + 2]]);
			Debug.LogFormat("[Type Racer #{0}] Button 3, {1}, comes later than Button 4, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 2]], letters[chosenLettersIndex[5 * aRow + 3]]);
			Debug.LogFormat("[Type Racer #{0}] Button 4, {1}, comes later than Button 5, {2}.", moduleId, letters[chosenLettersIndex[5 * aRow + 3]], letters[chosenLettersIndex[5 * aRow + 4]]);
		}
	}

	private readonly string TwitchHelpMessage = "Submit an answer with “!{0} submit <answer>”.";
	private IEnumerator ProcessTwitchCommand(string command)
	{
		string[] TPCmds2 = new string[2];
		yield return null;
		command = command.ToLowerInvariant();
		TPCmds2 = command.Split(' ');
		if (TPCmds2[0] == "submit")
		{
			yield return null;
			yield return null;
			goButton.OnInteract();
			yield return new WaitForSeconds(0.1f);
			if (TPCmds2[1].Length != 7)
			{
				yield return "sendtochaterror Not a valid submission.";
				yield break;
			}
			for (int i = 0; i < 7; i++)
			{
				yield return null;
				yield return null;
				TPCmds2[1] = TPCmds2[1].ToUpper();
				TextMesh obj = screenTextMesh;
				obj.text = obj.text + TPCmds2[1][i];
				chosenLettersIndex.Clear();
				generateLetters();
				goButton.AddInteractionPunch();
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
				yield return new WaitForSeconds(0.1f);
				if (i == 6)
				{
					checkForAnswer();
				}
			}
		}
		else
		{
			yield return "sendtochaterror Invalid command. Submit an answer with “!{0} submit <answer>”.";
		}
	}
}

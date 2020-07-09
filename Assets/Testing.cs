using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Testing : MonoBehaviour
{
    public TextMeshProUGUI CapMapText;
    public TextMeshProUGUI CurrIntText;
    public TextMeshProUGUI CollectedText;

    private Grid grid;
    private int currInt;
    private AudioSource audioSource;
    private Dictionary<string, float> notes;
    private Dictionary<string, int> collected;

    private float soundDuration = 0.2f;
    private float waitInterval = 0.3f; // no less than soundDuration
    private float soundDurationInside = 0.3f;
    private float waitIntervalInside = 0.4f; // no less than soundDurationInside
    private float soundDurationChord = 0.5f;

    private Queue myQueue;
    private Queue intQueue;

    string[] chords = new string[] {
                                    "",
                                    "",
                                    "",
                                    "",
                                    "",
                                    "",
                                    };
    string[] chordsWhole = new string[] {
                                    "246",
                                    "461",
                                    "613",
                                    "135",
                                    "357",
                                    "572",
                                    // "135",
                                    // "246",
                                    // "357",
                                    // "461",
                                    // "572",
                                    // "613",
                                    };
    string[] triples = new string[] {
                                    "111",
                                    "222",
                                    "333",
                                    "444",
                                    "555",
                                    "666",
                                    "777",
                                    };
    private int count = 0;
    private int level = 1;
    private int countThres = 10; //9 //8

    private bool isChecking = false;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(3, 3, 10f, Vector3.zero);

        audioSource = GetComponent<AudioSource>();

        notes = new Dictionary<string, float>();
        notes["0"]= 0.0f;
        // notes["6"]= 440.0f;
        // notes["7"]= 493.88f;
        notes["1"]= 523.25f/2;
        notes["2"]= 587.33f/2;
        notes["3"]= 659.25f/2;
        notes["4"]= 698.46f/2;
        notes["5"]= 783.99f/2;
        notes["6"]= 880.0f/2;
        notes["7"]= 987.77f/2;

        // chordsWhole = triples;
        chords[0] = chordsWhole[0];

        collected = new Dictionary<string, int>();
        foreach(string chord in chords)
        {
            collected[chord] = 0;
        }

        myQueue = new Queue();

        intQueue = new Queue();

        for (int i = 0; i < 6; i++) //4
        {
            intQueue.Enqueue(getRandInt());
        }

        CurrIntText.text = getIntQueueStr();
        currInt = getNextInt();
    }

    // Update is called once per frame
    void Update()
    {
        if((Input.GetMouseButtonDown(0)) && (isChecking != true))
        {
            Vector3 thisPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            grid.SetValue(thisPos, currInt);
            playMySound(notes[currInt.ToString()],soundDuration);
            myQueue.Enqueue(thisPos);
            StartCoroutine(checkMatchQueue());
            count += 1; // shouldnot add if mouse clicked outside grid
            Debug.Log(count+"/"+countThres+", @"+level+", Max:"+(1+level/2));
        }
        if(count == countThres)
        {
            count = 0;
            level += 1;
            countThres = 10+(level-1)*2;
            chords[level-1] = chordsWhole[level-1];
            collected[chords[level-1]] = 0;
            Debug.Log(count+"/"+countThres+", @"+level+", Max:"+(1+level/2));
        }
        CapMapText.text = grid.GetCapMapText();
        ShowCollected();
    }

    int getNextInt()
    {
        int res = (int) intQueue.Dequeue();
        intQueue.Enqueue(getRandInt());
        return res;
    }

    int getRandInt()
    {
        // int upper = 8;
        // int upper = Mathf.Clamp(4+level-1, 1, 8);
        // int upper = Mathf.Clamp(9-level, 4, 8);
        // return Random.Range(1,upper);

        int res = -1;
        int upper = Mathf.Clamp(3+level-1, 0, 7);
        switch (Random.Range(0,upper))
        {
            case 0:
                res = 2;
                break;
            case 1:
                res = 4;
                break;
            case 2:
                res = 6;
                break;
            case 3:
                res = 1;
                break;
            case 4:
                res = 3;
                break;
            case 5:
                res = 5;
                break;
            case 6:
                res = 7;
                break;
        }
        return res;
    }

    string getIntQueueStr()
    {
        string res = "";
        for (int i = 0; i < intQueue.Count; i++)
        {
            int thisInt = (int) intQueue.Dequeue();
            res += thisInt.ToString();
            res += " ";
            intQueue.Enqueue(thisInt);
        }
        return res;
    }

    IEnumerator checkMatch(Vector3 thisPos)
    {

        int x, y;
        grid.GetXY(thisPos, out x, out y);

        string ansRow = "";
        string ansCol = "";
        for (int i = 0; i < grid.GetWidth(); i++)
        {
            ansRow += grid.GetValue(i,y).ToString();
        }
        for (int j = 0; j < grid.GetHeight(); j++)
        {
            ansCol += grid.GetValue(x,j).ToString();
        }

        string chordRow = checkPattern(SortString(ansRow), chords, true);
        string chordCol = checkPattern(SortString(ansCol), chords, true);

        if ((chordRow != "") || (chordCol != ""))
        {
            yield return StartCoroutine(showChord(x, y, chordRow, chordCol));
        }

    }

    string SortString(string s)
    {
        char[] temp = s.ToCharArray();
        System.Array.Sort(temp);
        return new string(temp);
    }

    IEnumerator showChord(int x, int y, string chordRow, string chordCol)
    {

        if (chordRow != "")
        {
            yield return new WaitForSeconds(waitInterval);
            yield return showChordList(chordRow, y, -1);
            playChord(chordRow);
            collected[chordRow] += 1;
        }
        if (chordCol != "")
        {
            yield return new WaitForSeconds(waitInterval);
            yield return showChordList(chordCol, -1, x);
            playChord(chordCol);
            collected[chordCol] += 1;
        }

        yield return new WaitForSeconds(waitInterval);
        cleanMatch(x, y, chordRow, chordCol);

    }

    private void playMySound(float frequency, float duration)
    {
        int sampleFreq = 44100;
        int samplesLength = Mathf.CeilToInt(sampleFreq * duration);
        float[] samples = new float[samplesLength];
        for(int i = 0; i < samplesLength; i++)
        {
            // sin
            samples[i] = Mathf.Sin(Mathf.PI*2*i*frequency/sampleFreq);
        }

        AudioClip ac = AudioClip.Create("Test", samplesLength, 1, sampleFreq, false);
        ac.SetData(samples, 0);
        audioSource.PlayOneShot(ac, 1.0f);
    }

    string checkPattern(string thisStr, string[] patterns, bool toSort = false)
    {
        foreach (string pattern in patterns)
        {
            string thisPattern = pattern;
            if (toSort)
            {
                thisPattern = SortString(pattern);
            }
            else
            {
                thisPattern = pattern;
            }

            if (thisStr == thisPattern)
            {
              return pattern;
            }
        }
        return "";
    }

    void playChord(string thisChord)
    {
        foreach (char temp in thisChord.ToCharArray())
        {
            playMySound(notes[temp.ToString()], soundDurationChord);
        }
    }

    void cleanMatch(int x, int y, string chordRow, string chordCol)
    {
        if (chordRow != "")
        {
            for (int i = 0; i < grid.GetWidth(); i++)
            {
                if (i == x)
                {
                    continue;
                }
                grid.PopValue(i,y);
                myQueue.Enqueue(grid.GetPos(i,y));
            }
        }
        if (chordCol != "")
        {
            for (int j = 0; j < grid.GetHeight(); j++)
            {
                if (j == y)
                {
                    continue;
                }
                grid.PopValue(x,j);
                myQueue.Enqueue(grid.GetPos(x,j));
            }
        }
        if ((chordRow != "") || (chordCol != ""))
        {
            grid.PopValue(x,y);
        }
    }

    IEnumerator showChordList(string thisChord, int rowIndex, int colIndex)
    {
        int[] thisList;
        bool isRow;
        if (rowIndex < 0)
        {
            thisList = grid.getCol(colIndex);
            isRow = false;
        }
        else
        {
            thisList = grid.getRow(rowIndex);
            isRow = true;
        }

        bool isTriple = false;
        foreach (string pattern in triples)
        {
            if (thisChord == pattern)
            {
                isTriple = true;
            }
        }

        float prevFreq = 0;
        float scale = 1.0f;
        foreach (char temp in thisChord.ToCharArray())
        {
            for (int i = 0; i < thisList.Length; i++)
            {
                // Debug.Log(thisList[i].ToString()[0]);
                if (thisList[i].ToString()[0] == temp)
                {
                    float thisFreq = notes[temp.ToString()];
                    if ( thisFreq < prevFreq)
                    {
                        scale *= 2;
                    }
                    prevFreq = thisFreq;
                    playMySound(thisFreq*scale,soundDurationInside);

                    if (isRow)
                    {
                        grid.GetTextObj(i,rowIndex).text += "!!!";
                    }
                    else
                    {
                        grid.GetTextObj(colIndex,i).text += "!!!";
                    }

                    yield return new WaitForSeconds(waitIntervalInside);
                }
            }
            if (isTriple)
            {
                break;
            }
        }
    }

    void ShowCollected()
    {
        string result = "";
        foreach(string chord in chords)
        {
            result += chord + ": " + collected[chord].ToString();
            result += "\n";
        }
        CollectedText.text = result;
    }

    IEnumerator checkMatchQueue()
    {
        isChecking = true;
        CurrIntText.text = "...";

        while (myQueue.Count != 0)
        {
            yield return checkMatch((Vector3) myQueue.Dequeue());
        }

        CurrIntText.text = getIntQueueStr();
        currInt = getNextInt();
        isChecking = false;
    }
}

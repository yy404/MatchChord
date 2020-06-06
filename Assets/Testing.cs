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

    string[] chords = new string[] {
                                    "135",
                                    "246",
                                    "357",
                                    "461",
                                    "572",
                                    "613",
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
    // private int count = 0;
    // private int level = 1;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(3, 3, 10f, Vector3.zero);
        currInt = getNextInt();
        CurrIntText.text = "ToAdd: " + currInt.ToString();

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

        collected = new Dictionary<string, int>();
        foreach(string chord in chords)
        {
            collected[chord] = 0;
        }

        myQueue = new Queue();
    }

    // Update is called once per frame
    void Update()
    {
        if((Input.GetMouseButtonDown(0)) && (currInt>0))
        {
            Vector3 thisPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            grid.SetValue(thisPos, currInt);
            playMySound(notes[currInt.ToString()],soundDuration);
            currInt = -1;
            myQueue.Enqueue(thisPos);
            StartCoroutine(checkMatchQueue());
            // count += 1; // shouldnot add if mouse clicked outside grid
            // Debug.Log(count+"/"+(8+(level-1)*2)+", @"+level);
        }
        // if(Input.GetMouseButtonDown(1))
        // {
        //     count = 0;
        //     level += 1;
        //     Debug.Log(count);
        // }
        CapMapText.text = grid.GetCapMapText();
        ShowCollected();
    }

    int getNextInt()
    {
        return Random.Range(1,8);
    }

    IEnumerator checkMatch(Vector3 thisPos)
    {
        CurrIntText.text = "...";

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
        else
        {
            currInt = getNextInt();
            CurrIntText.text = currInt.ToString();
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
            // yield return showChordList(chordRow, y, -1);
            playChord(chordRow);
            collected[chordRow] += 1;
        }
        if (chordCol != "")
        {
            yield return new WaitForSeconds(waitInterval);
            // yield return showChordList(chordCol, -1, x);
            playChord(chordCol);
            collected[chordCol] += 1;
        }

        yield return new WaitForSeconds(waitInterval);
        cleanMatch(x, y, chordRow, chordCol);

        currInt = getNextInt();
        CurrIntText.text = currInt.ToString();
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
            // break;
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
        while (myQueue.Count != 0)
        {
            yield return checkMatch((Vector3) myQueue.Dequeue());
        }
    }
}

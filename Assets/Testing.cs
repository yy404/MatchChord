using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{

    private Grid grid;
    private int currInt;
    private AudioSource audioSource;
    private Dictionary<string, float> notes;

    private float waitInterval = 0.5f;
    private float soundDuration = 0.3f;
    private float soundDurationLong = 0.7f;

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

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(3, 3, 10f, Vector3.zero);
        currInt = getNextInt();
        Debug.Log(currInt);

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
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 thisPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            grid.SetValue(thisPos, currInt);
            checkMatch(thisPos);
        }
        if(Input.GetMouseButtonDown(1))
        {
            Vector3 thisPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            grid.SetValue(thisPos, 0);
        }
    }

    int getNextInt()
    {
        return Random.Range(1,8);
    }

    void checkMatch(Vector3 thisPos)
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

        StartCoroutine(showChord(x, y, ansRow, ansCol));
    }

    string SortString(string s)
    {
        char[] temp = s.ToCharArray();
        System.Array.Sort(temp);
        return new string(temp);
    }

    IEnumerator showChord(int x, int y, string ansRow, string ansCol)
    {
        playMySound(notes[currInt.ToString()],soundDuration);
        // yield return new WaitForSeconds(waitInterval);

        string chordRow = checkPattern(SortString(ansRow), chords, true);
        string chordCol = checkPattern(SortString(ansCol), chords, true);

        if (chordRow != "")
        {
            yield return new WaitForSeconds(waitInterval);
            yield return showChordList(chordRow, y, -1);
            playChord(chordRow);
        }
        if (chordCol != "")
        {
            yield return new WaitForSeconds(waitInterval);
            yield return showChordList(chordCol, -1, x);
            playChord(chordCol);
        }

        yield return new WaitForSeconds(waitInterval);
        if ((chordRow != "") || (checkPattern(SortString(ansRow), triples) != ""))
        {
            for (int i = 0; i < grid.GetWidth(); i++)
            {
                grid.SetValue(i,y,0);
            }
        }
        if ((chordCol != "") || (checkPattern(SortString(ansCol), triples) != ""))
        {
            for (int j = 0; j < grid.GetHeight(); j++)
            {
                grid.SetValue(x,j,0);
            }
        }

        currInt = getNextInt();
        Debug.Log(currInt);
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
            playMySound(notes[temp.ToString()], soundDurationLong);
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
                Debug.Log(thisList[i].ToString()[0]);
                if (thisList[i].ToString()[0] == temp)
                {
                    float thisFreq = notes[temp.ToString()];
                    if ( thisFreq < prevFreq)
                    {
                        scale *= 2;
                    }
                    prevFreq = thisFreq;
                    playMySound(thisFreq*scale,soundDuration);

                    if (isRow)
                    {
                        grid.GetTextObj(i,rowIndex).text += "!!!";
                    }
                    else
                    {
                        grid.GetTextObj(colIndex,i).text += "!!!";
                    }

                    yield return new WaitForSeconds(waitInterval);
                }
            }
        }
    }

}

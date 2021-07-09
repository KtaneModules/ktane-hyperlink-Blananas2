using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class hyperlinkScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] Arrows;
    public KMSelectable Square;
    public GameObject Circle;
    public MeshRenderer ConnectionLED;
    public TextMesh TopNumber;
    public TextMesh MainText;
    public GameObject Back;
    public GameObject veryBack;
    public GameObject Outer;
    public GameObject[] Frame;
    public Material[] OOOMats;
    public Material[] OtherMats;
    public Material[] IconMats;
    public Font[] Fonts;
    public Material[] FontMats;
    public GameObject[] RGB;

    //Logging
    static int moduleIdCounter = 1;
    public int moduleId;
    public bool moduleSolved;

    private Coroutine buttonHold;
	private bool holding;
    private bool interactable;
    private int btn;

    int selectedID;
    int anchor;
    int currentScreen;
    string selectedString = "";
    public List<string> charList = new List<string>();
    public List<string> screwList = new List<string>();
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string nonsense = "⅓⅔⅛⅜⅝⅞ↄ←↑→↓↔↨∂∆∏∑−∕∙√∞∟∩∫≈";
    string screwString = "";
    string connectionLink;
    int step;
    int selectedIcon = 60;
    public List<string> colorString = new List<string> { "0FF", "08F", "888", "F80", "FFF", "8F0", "00F", "FF8", "808", "F0F", "0F0", "F8F", "080", "FF0", "800", "8F8", "008", "880", "88F", "F08", "F88", "F00", "0F8", "000", "088", "8FF", "80F" };

    public List<int> index = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
    public List<string> encodings = new List<string> { "Alphabetic Position", "American Sign Language", "Binary", "Boozleglyphs", "Braille", "Cube Symbols", "\"Deaf\" Semaphore Telegraph", "Elder Futhark", "14-Segment Display", "Lombax", "Maritime Flags", "Moon Type", "Morse Code", "Necronomicon", "Ogham", "Pigpen", "Semaphore", "Standard", "Standard Galactic Alphabet", "SYNC-125 [3]", "Tap Code", "Unown", "Webdings", "Wingdings", "Wingdings 2", "Wingdings 3", "Zoni"};

    public List<string> alphabeticPosition = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26" };
    public List<string> binary = new List<string> { "00001", "00010", "00011", "00100", "00101", "00110", "00111", "01000", "01001", "01010", "01011", "01100", "01101", "01110", "01111", "10000", "10001", "10010", "10011", "10100", "10101", "10110", "10111", "11000", "11001", "11010" };
    public List<string> elderFuthark = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "cc", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "uu", "x", "y", "z" };
    public List<string> morse = new List<string> { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
    public List<string> ogham = new List<string> { "┼", "┬", "╨╨", "╨", "╫╫", "┬┬┬", "//", "┴", "╫┼╫", "#", "₩", "╥", "/", "╥┬╥", "╫", "═", "╨┴╨", "/////", "╥╥", "┴┴┴", "┼┼┼", "□", "◊", "X", "///", "////" };
    public List<string> sync = new List<string> { "a", "p'", "C", "t'", "e", "f", "k'", "h", "i", "j'", "k", "r'", "m", "n", "o", "p", "?", "r", "s", "t", "u", "f'", "w", "!", "y", "s'" };
    public List<string> tapCode = new List<string> { "11", "12", "13", "14", "15", "21", "22", "23", "24", "25", "66", "31", "32", "33", "34", "35", "41", "42", "43", "44", "45", "51", "52", "53", "54", "55" };

    public List<string> holdingList = new List<string>();

    private hyperlinkWebSocketManager _webSocketManager;

    private bool connectionSuccessful;
    
    private const int numberOfLetters = 11;

    void Awake () {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable arrow in Arrows) {
            KMSelectable pressedArrow = arrow;
            arrow.OnInteract += delegate { arrowPress(pressedArrow); return false; };
            arrow.OnInteractEnded += arrowRelease;
        }

        Square.OnInteract += delegate { squarePress(); return false; };

    }

    // Use this for initialization
    void Start () {
	    selectedID = Random.Range(0, 121);
        anchor = 2 * selectedID;
        _webSocketManager = new hyperlinkWebSocketManager(this, GetData(selectedID));
    }

    private string GetData(int selectedId)
    {
        var link = new List<char>();
        var redirect = IDList.phrases[selectedId * 2];
        for (var i = 0; i < numberOfLetters; ++i)
        {
            link.Add("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-_"[Random.Range(0, 54)]);
        }

        connectionLink = new string(link.ToArray());
        return string.Format("Connection|Addsocket|{0}|{1}", connectionLink, redirect);
    }

    void arrowPress (KMSelectable arrow)
    {
        if (!interactable)
        {
            return;
        }
        if (step == 0)
        {
            if (arrow == Arrows[0])
            {
                if (currentScreen != 0)
                {
                    Audio.PlaySoundAtTransform("button", transform);
                    arrow.AddInteractionPunch();
                    currentScreen -= 1;
                }
            }
            else if (arrow == Arrows[1])
            {
                if (currentScreen != 10)
                {
                    Audio.PlaySoundAtTransform("button", transform);
                    arrow.AddInteractionPunch();
                    currentScreen += 1;
                }
            }

            if (currentScreen == 0)
            {
                Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
                Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[0];
            }
            else if (currentScreen == 10)
            {
                Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[0];
                Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[1];
            }
            else
            {
                Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[0];
                Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[0];
            }

            TopNumber.text = " " + (currentScreen + 1) + " ";
            UpdateText();
        } else if (step == 1)
        {
            Audio.PlaySoundAtTransform("button", transform);
            arrow.AddInteractionPunch();
            if (arrow == Arrows[0])
            {
                selectedIcon = (selectedIcon - 1);
                btn = 0;
            }
            else if (arrow == Arrows[1])
            {
                selectedIcon = (selectedIcon + 1);
                btn = 1;
            }

            if (selectedIcon == -1) { selectedIcon = 120; }
            if (selectedIcon == 121) { selectedIcon = 0; }
            Back.GetComponent<Renderer>().material = IconMats[selectedIcon];
            buttonHold = StartCoroutine(HoldChecker());
        }
    }

    void arrowRelease () {
        if (!interactable)
        {
            return;
        }
        if (step == 1) {
            StopCoroutine(buttonHold);
        }
    }
    
    private void OnDestroy()
    {
        _webSocketManager.Stop();
    }

    public IEnumerator ConnectionError()
    {
        Startmod(true);
        yield return null;
    }
    
    public IEnumerator ConnectionReady()
    {
        Startmod(false);
        yield return null;
    }

    private void Startmod(bool error)
    {
        //Start the module
        if (step == 1 && error)
        {
            currentScreen = 0;
            TopNumber.text = " " + (currentScreen + 1) + " ";
            Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
            Back.GetComponent<Renderer>().material = OtherMats[2];
            step = 0;
            UpdateText();
        }
        ConnectionLED.material = error ? OtherMats[6] : OtherMats[4];
        connectionSuccessful = !error;
        charList.Clear();
        screwList.Clear();
        holdingList.Clear();
        selectedString = error ? IDList.phrases[anchor] : connectionLink;
        StringToLetters(selectedString);

        index.Shuffle();

        LettersToScrew();

        UpdateText();

        Debug.LogFormat(
            error
                ? "[The Hyperlink #{0}] The module failed to connect the the server, use the youtube link instead."
                : "[The Hyperlink #{0}] The module connected to the server.", moduleId);
        for (int a = 0; a < 11; a++)
        {
            Debug.LogFormat("[The Hyperlink #{0}] Encoding {1}: '{2}' in {3} => '{4}' => {5}", moduleId, a + 1, screwList[a].Replace("\n", ""), encodings[index[a]], charList[a].Replace("\n", ""), selectedString[a] );
        }
        Debug.LogFormat("[The Hyperlink #{0}] Video {3}{1} references {2}.", moduleId, selectedString, IDList.phrases[anchor + 1], error ? "https://www.youtube.com/watch?v=" : "https://marksam32.github.io/hl/?link=");
        interactable = true;
        Circle.GetComponent<MeshRenderer>().material = OtherMats[0];
        Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[0];
        Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
        currentScreen = 0;
        TopNumber.text = "1";
    }

    public IEnumerator ConnectionLost()
    {
        Debug.LogFormat("[The Hyperlink #{0}] The module has lost its connection to the server and will now use the youtube link instead", moduleId);
        ConnectionLED.material = OtherMats[6];
        StartCoroutine(ConnectionError());
        yield return null;
    }

    IEnumerator HoldChecker()
	{
		yield return new WaitForSeconds(.4f);
        holding = true;
        if (step == 1 && holding) {
            backHere:
            Audio.PlaySoundAtTransform("button", transform);
            if (btn == 0) {
                selectedIcon = (selectedIcon - 1);
            } else {
                selectedIcon = (selectedIcon + 1);
            }
            if (selectedIcon == -1) { selectedIcon = 120; }
            if (selectedIcon == 121) { selectedIcon = 0; }
            Back.GetComponent<Renderer>().material = IconMats[selectedIcon];
            yield return new WaitForSeconds(0.075f);
            goto backHere;
        }
    }

    void squarePress ()
    {
        if (!interactable)
        {
            return;
        }
        if (moduleSolved == false)
        {
            step = (step + 1) % 2;
            if (step == 1)
            {
                Audio.PlaySoundAtTransform("button", transform);
                Square.AddInteractionPunch();
                MainText.text = "";
                TopNumber.text = "?";
                if(currentScreen == 0)
                {
                    Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[0];
                }
                else if (currentScreen == 10)
                {
                    Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[0];
                }
                Frame[0].GetComponent<Renderer>().material = OtherMats[2];
                Frame[1].GetComponent<Renderer>().material = OtherMats[2];
                Frame[2].GetComponent<Renderer>().material = OtherMats[2];
                Frame[3].GetComponent<Renderer>().material = OtherMats[2];
                RGB[0].GetComponent<Renderer>().material = OtherMats[2];
                RGB[1].GetComponent<Renderer>().material = OtherMats[2];
                RGB[2].GetComponent<Renderer>().material = OtherMats[2];
                selectedIcon = 60;
                Back.GetComponent<Renderer>().material = IconMats[60];
            }
            else if (step == 0)
            {
                if (selectedIcon == selectedID)
                {
                    Debug.LogFormat("[The Hyperlink #{0}] Icon for {1} selected, module solved.", moduleId, IDList.phrases[anchor + 1]);
                    Audio.PlaySoundAtTransform("solve", transform);
                    GetComponent<KMBombModule>().HandlePass();
                    if (connectionSuccessful)
                    {
                        _webSocketManager.Solve();
                    }
                    moduleSolved = true;
                    Square.AddInteractionPunch();
                    step = 999;
                    TopNumber.text = "   ";
                    ConnectionLED.material = OtherMats[2];
                    Frame[0].GetComponent<Renderer>().material = OtherMats[2];
                    Frame[1].GetComponent<Renderer>().material = OtherMats[2];
                    Frame[2].GetComponent<Renderer>().material = OtherMats[2];
                    Frame[3].GetComponent<Renderer>().material = OtherMats[2];
                    RGB[0].GetComponent<Renderer>().material = OtherMats[2];
                    RGB[1].GetComponent<Renderer>().material = OtherMats[2];
                    RGB[2].GetComponent<Renderer>().material = OtherMats[2];
                    Back.GetComponent<Renderer>().material = OtherMats[2];
                    veryBack.GetComponent<Renderer>().material = OtherMats[2];
                    Square.GetComponent<MeshRenderer>().material = OtherMats[2];
                    Circle.GetComponent<MeshRenderer>().material = OtherMats[2];
                    Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[2];
                    Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[2];
                    Outer.GetComponent<Renderer>().material = OtherMats[9];
                }
                else
                {
                    Debug.LogFormat("[The Hyperlink #{0}] Icon for {1} selected, module striked.", moduleId, IDList.phrases[selectedIcon * 2 + 1]);
                    GetComponent<KMBombModule>().HandleStrike();
                    Square.AddInteractionPunch();
                    currentScreen = 0;
                    TopNumber.text = " " + (currentScreen + 1) + " ";
                    Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
                    Back.GetComponent<Renderer>().material = OtherMats[2];
                    UpdateText();
                }
            }
        }
    }

    void UpdateText ()
    {
        MainText.text = screwList[currentScreen];

        switch (index[currentScreen]) {
            case 1:
                MainText.font = Fonts[3];
                MainText.GetComponent<Renderer>().material = FontMats[3];
                break;
            case 2:
                MainText.font = Fonts[22];
                MainText.GetComponent<Renderer>().material = FontMats[22];
                break;
            case 3:
                MainText.font = Fonts[4];
                MainText.GetComponent<Renderer>().material = FontMats[4];
                break;
            case 4:
                MainText.font = Fonts[1];
                MainText.GetComponent<Renderer>().material = FontMats[1];
                break;
            case 5:
                MainText.font = Fonts[10];
                MainText.GetComponent<Renderer>().material = FontMats[10];
                break;
            case 6:
                MainText.font = Fonts[23];
                MainText.GetComponent<Renderer>().material = FontMats[23];
                break;
            case 7:
                MainText.font = Fonts[12];
                MainText.GetComponent<Renderer>().material = FontMats[12];
                break;
            case 8:
                MainText.font = Fonts[9];
                MainText.GetComponent<Renderer>().material = FontMats[9];
                break;
            case 9:
                MainText.font = Fonts[8];
                MainText.GetComponent<Renderer>().material = FontMats[8];
                break;
            case 10:
                MainText.font = Fonts[7];
                MainText.GetComponent<Renderer>().material = FontMats[7];
                break;
            case 11:
                MainText.font = Fonts[11];
                MainText.GetComponent<Renderer>().material = FontMats[11];
                break;
            case 13:
                MainText.font = Fonts[5];
                MainText.GetComponent<Renderer>().material = FontMats[5];
                break;
            case 15:
                MainText.font = Fonts[6];
                MainText.GetComponent<Renderer>().material = FontMats[6];
                break;
            case 16:
                MainText.font = Fonts[14];
                MainText.GetComponent<Renderer>().material = FontMats[14];
                break;
            case 18:
                MainText.font = Fonts[15];
                MainText.GetComponent<Renderer>().material = FontMats[15];
                break;
            case 19:
                MainText.font = Fonts[13];
                MainText.GetComponent<Renderer>().material = FontMats[13];
                break;
            case 21:
                MainText.font = Fonts[16];
                MainText.GetComponent<Renderer>().material = FontMats[16];
                break;
            case 22:
                MainText.font = Fonts[21];
                MainText.GetComponent<Renderer>().material = FontMats[21];
                break;
            case 23:
                MainText.font = Fonts[18];
                MainText.GetComponent<Renderer>().material = FontMats[18];
                break;
            case 24:
                MainText.font = Fonts[19];
                MainText.GetComponent<Renderer>().material = FontMats[19];
                break;
            case 25:
                MainText.font = Fonts[20];
                MainText.GetComponent<Renderer>().material = FontMats[20];
                break;
            case 26:
                MainText.font = Fonts[17];
                MainText.GetComponent<Renderer>().material = FontMats[17];
                break;
            default:
                MainText.font = Fonts[0];
                MainText.GetComponent<Renderer>().material = FontMats[0];
                break;
        }

        Frame[0].GetComponent<Renderer>().material = OOOMats[index[currentScreen]];
        Frame[1].GetComponent<Renderer>().material = OOOMats[index[currentScreen]];
        Frame[2].GetComponent<Renderer>().material = OOOMats[index[currentScreen]];
        Frame[3].GetComponent<Renderer>().material = OOOMats[index[currentScreen]];

        if (colorString[index[currentScreen]][0] == '0')
        {
            RGB[0].GetComponent<Renderer>().material = OtherMats[2];
        }
        else if (colorString[index[currentScreen]][0] == '8')
        {
            RGB[0].GetComponent<Renderer>().material = OtherMats[3];
        }
        else
        {
            RGB[0].GetComponent<Renderer>().material = OtherMats[6];
        }

        if (colorString[index[currentScreen]][1] == '0')
        {
            RGB[1].GetComponent<Renderer>().material = OtherMats[2];
        }
        else if (colorString[index[currentScreen]][1] == '8')
        {
            RGB[1].GetComponent<Renderer>().material = OtherMats[4];
        }
        else
        {
            RGB[1].GetComponent<Renderer>().material = OtherMats[7];
        }

        if (colorString[index[currentScreen]][2] == '0')
        {
            RGB[2].GetComponent<Renderer>().material = OtherMats[2];
        }
        else if (colorString[index[currentScreen]][2] == '8')
        {
            RGB[2].GetComponent<Renderer>().material = OtherMats[5];
        }
        else
        {
            RGB[2].GetComponent<Renderer>().material = OtherMats[8];
        }

        MainText.GetComponent<TextMesh>().fontSize = charList[currentScreen].Length == 3 ? 288 : 144;
    }

    void StringToLetters (string s)
    {
        for (int i = 0; i < 11; i++)
        {
            switch (s[i])
            {
                case '0': charList.Add(" Z E \n\n R O "); break;
                case '1': charList.Add(" O N \n\n E "); break;
                case '2': charList.Add(" T W \n\n O "); break;
                case '3': charList.Add(" T H \n\n R \n\n E E "); break;
                case '4': charList.Add(" F O \n\n U R "); break;
                case '5': charList.Add(" F I \n\n V E "); break;
                case '6': charList.Add(" S I \n\n X "); break;
                case '7': charList.Add(" S E \n\n V \n\n E N "); break;
                case '8': charList.Add(" E I \n\n G \n\n H T "); break;
                case '9': charList.Add(" N I \n\n N E "); break;
                case 'A': charList.Add(" A L \n\n F A "); break;
                case 'B': charList.Add(" B R \n\n A \n\n V O "); break;
                case 'C': charList.Add(" C H \n\n A R L \n\n I E "); break;
                case 'D': charList.Add(" D E \n\n L \n\n T A "); break;
                case 'E': charList.Add(" E C \n\n H O "); break;
                case 'F': charList.Add(" F O \n\n X T R \n\n O T "); break;
                case 'G': charList.Add(" G O \n\n L F "); break;
                case 'H': charList.Add(" H O \n\n T \n\n E L "); break;
                case 'I': charList.Add(" I N \n\n D \n\n I A "); break;
                case 'J': charList.Add(" J U \n\n L I E \n\n T T "); break;
                case 'K': charList.Add(" K I \n\n L O "); break;
                case 'L': charList.Add(" L I \n\n M A "); break;
                case 'M': charList.Add(" M I \n\n K E "); break;
                case 'N': charList.Add(" N O V \n\n E M \n\n B E R "); break;
                case 'O': charList.Add(" O S \n\n C \n\n A R "); break;
                case 'P': charList.Add(" P A \n\n P A "); break;
                case 'Q': charList.Add(" Q U \n\n E B \n\n E C "); break;
                case 'R': charList.Add(" R O \n\n M \n\n E O "); break;
                case 'S': charList.Add(" S I \n\n E R \n\n R A "); break;
                case 'T': charList.Add(" T A \n\n N \n\n G O "); break;
                case 'U': charList.Add(" U N \n\n I F O \n\n R M "); break;
                case 'V': charList.Add(" V I \n\n C T \n\n O R "); break;
                case 'W': charList.Add(" W H \n\n I S K \n\n E Y "); break;
                case 'X': charList.Add(" X R \n\n A Y "); break;
                case 'Y': charList.Add(" Y A \n\n N K \n\n E E "); break;
                case 'Z': charList.Add(" Z U \n\n L U "); break;
                case 'a': charList.Add(" A "); break;
                case 'b': charList.Add(" B "); break;
                case 'c': charList.Add(" C "); break;
                case 'd': charList.Add(" D "); break;
                case 'e': charList.Add(" E "); break;
                case 'f': charList.Add(" F "); break;
                case 'g': charList.Add(" G "); break;
                case 'h': charList.Add(" H "); break;
                case 'i': charList.Add(" I "); break;
                case 'j': charList.Add(" J "); break;
                case 'k': charList.Add(" K "); break;
                case 'l': charList.Add(" L "); break;
                case 'm': charList.Add(" M "); break;
                case 'n': charList.Add(" N "); break;
                case 'o': charList.Add(" O "); break;
                case 'p': charList.Add(" P "); break;
                case 'q': charList.Add(" Q "); break;
                case 'r': charList.Add(" R "); break;
                case 's': charList.Add(" S "); break;
                case 't': charList.Add(" T "); break;
                case 'u': charList.Add(" U "); break;
                case 'v': charList.Add(" V "); break;
                case 'w': charList.Add(" W "); break;
                case 'x': charList.Add(" X "); break;
                case 'y': charList.Add(" Y "); break;
                case 'z': charList.Add(" Z "); break;
                case '-': charList.Add(" D A \n\n S H "); break;
                case '_': charList.Add(" U N D \n\n E R L \n\n I N E "); break; 
            }
        }
    }

    void LettersToScrew()
    {
        for (int j = 0; j < 11; j++)
        {
            screwString = "";

            switch(index[j])
            {
                case 0:
                    BiggerSwitch(0, j);
                    break;
                case 2:
                    BiggerSwitch(1, j);
                    break;
                case 7:
                    BiggerSwitch(2, j);
                    break;
                case 12:
                    BiggerSwitch(3, j);
                    break;
                case 13:
                    LetterSwitch("abcdefghijklmnopqrstuvwxyz", j);
                    break;
                case 14:
                    BiggerSwitch(4, j);
                    break;
                case 15:
                    LetterSwitch("acegikmoqbdfhjlnprsuwytvxz", j);
                    break;
                case 16:
                    LetterSwitch("ABCDEFGHIJKLM\\OPQRSTUVWXYZ", j);
                    break;
                case 19:
                    BiggerSwitch(6, j);
                    break;
                case 20:
                    BiggerSwitch(5, j);
                    break;
                default:
                    screwString = charList[j];
                    break;
            }

            screwList.Add(screwString);
        }
    }

    void BiggerSwitch(int l, int w)
    {
        switch (l)
        {
            case 0:
                for (int ap = 0; ap < 26; ap++)
                {
                    holdingList.Add(alphabeticPosition[ap]);
                }
                break;
            case 1:
                for (int bi = 0; bi < 26; bi++)
                {
                    holdingList.Add(binary[bi]);
                }
                break;
            case 2:
                for (int ef = 0; ef < 26; ef++)
                {
                    holdingList.Add(elderFuthark[ef]);
                }
                break;
            case 3:
                for (int mo = 0; mo < 26; mo++)
                {
                    holdingList.Add(morse[mo]);
                }
                break;
            case 4:
                for (int og = 0; og < 26; og++)
                {
                    holdingList.Add(ogham[og]);
                }
                break;
            case 5:
                for (int tc = 0; tc < 26; tc++)
                {
                    holdingList.Add(tapCode[tc]);
                }
                break;
            case 6:
                for (int sy = 0; sy < 26; sy++)
                {
                    holdingList.Add(sync[sy]);
                }
                break;
        }

        screwString = charList[w];

        //AAAAAAA MESSY CODE NO LOOK HERE AAAAAAA
        for (int k = 0; k < 26; k++)
        {
            screwString = screwString.Replace(" " + alphabet[k] + " ", " " + nonsense[k] + " ");
            screwString = screwString.Replace(" " + nonsense[k] + " ", " " + holdingList[k] + " ");
        }
        for (int n = 0; n < 26; n++)
        {
            screwString = screwString.Replace(" " + alphabet[n] + " ", " " + nonsense[n] + " ");
            screwString = screwString.Replace(" " + nonsense[n] + " ", " " + holdingList[n] + " ");
        }

        if (l == 4) {
            screwString = ">" + screwString + "<";
            screwString = screwString.Replace("> ", ">");
            screwString = screwString.Replace(" <", "<");
        }

        holdingList.Clear();
    }

    void LetterSwitch(string o, int v)
    {
         screwString = charList[v];

         for (int m = 0; m < 26; m++)
         {
             screwString = screwString.Replace(" " + alphabet[m] + " ", " " + nonsense[m] + " ");
             screwString = screwString.Replace(" " + nonsense[m] + " ", " " + o[m] + " ");
         }
         for (int p = 0; p < 26; p++)
         {
             screwString = screwString.Replace(" " + alphabet[p] + " ", " " + nonsense[p] + " ");
             screwString = screwString.Replace(" " + nonsense[p] + " ", " " + o[p] + " ");
         }
    }

    //twitch plays
    private bool moveValid(string s)
    {
        string[] valids = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"};
        if (valids.Contains(s))
        {
            return true;
        }
        return false;
    }

    private bool listContains(string s)
    {
        List<string> names = new List<string>();
        for (int i = 0; i < IDList.phrases.Length; i++)
        {
            if (i % 2 != 0)
            {
                names.Add(IDList.phrases[i].ToLower());
            }
        }
        if (names.Contains(s))
            return true;
        return false;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} left/right (#) [Press the left/right arrow (optionally '#' times)] | !{0} submit <module> [Submits the specified module]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (!TopNumber.text.Equals("?"))
            {
                yield return null;
                if (currentScreen == 0)
                {
                    yield return "sendtochaterror I cannot scroll to the left anymore!";
                    yield break;
                }
                Arrows[0].OnInteract();
                Arrows[0].OnInteractEnded();
            }
            else
            {
                yield return "sendtochaterror I cannot scroll when in the submission page!";
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (!TopNumber.text.Equals("?"))
            {
                yield return null;
                if (currentScreen == 10)
                {
                    yield return "sendtochaterror I cannot scroll to the right anymore!";
                    yield break;
                }
                Arrows[1].OnInteract();
                Arrows[1].OnInteractEnded();
            }
            else
            {
                yield return "sendtochaterror I cannot scroll when in the submission page!";
            }
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (!TopNumber.text.Equals("?"))
            {
                if (parameters.Length == 2)
                {
                    if (moveValid(parameters[1]))
                    {
                        yield return null;
                        int temp;
                        int.TryParse(parameters[1], out temp);
                        for (int i = 0; i < temp; i++)
                        {
                            if (parameters[0].ToLower().Equals("left"))
                            {
                                if (currentScreen == 0)
                                {
                                    yield break;
                                }
                                Arrows[0].OnInteract();
                                Arrows[0].OnInteractEnded();
                            }
                            else if (parameters[0].ToLower().Equals("right"))
                            {
                                if (currentScreen == 10)
                                {
                                    yield break;
                                }
                                Arrows[1].OnInteract();
                                Arrows[1].OnInteractEnded();
                            }
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror '" + parameters[1] + "' is not a valid number! Numbers 1-10 are valid for scrolling.";
                    }
                }
            }
            else
            {
                yield return "sendtochaterror I cannot scroll when in the submission page!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length >= 2)
            {
                yield return null;
                if (!TopNumber.text.Equals("?"))
                {
                    Square.OnInteract();
                }
                string module = "";
                for (int i = 1; i < parameters.Length; i++)
                {
                    module += parameters[i] + " ";
                }
                module = module.Trim();
                module = module.ToLower();
                if (!listContains(module))
                {
                    string modname = Back.GetComponent<Renderer>().material.name.ToLower();
                    modname = modname.Replace(" (instance)", "");
                    int rando = Random.Range(0, 2);
                    int counter = 0;
                    while (!module.Equals(modname))
                    {
                        Arrows[rando].OnInteract();
                        Arrows[rando].OnInteractEnded();
                        modname = Back.GetComponent<Renderer>().material.name.ToLower();
                        modname = modname.Replace(" (instance)", "");
                        yield return new WaitForSeconds(0.05f);
                        counter++;
                        if (counter == 121)
                        {
                            yield return "sendtochaterror '" + module + "' is not a valid module name!";
                            yield break;
                        }
                    }
                }
                else
                {
                    int ctleft = 0;
                    int ctright = 0;
                    int curindex = 121;
                    while (!module.Equals(IDList.phrases[curindex].ToLower()))
                    {
                        curindex -= 2;
                        if (curindex < 1)
                            curindex = 245;
                        ctleft++;
                    }
                    curindex = 121;
                    while (!module.Equals(IDList.phrases[curindex].ToLower()))
                    {
                        curindex += 2;
                        if (curindex > 245)
                            curindex = 1;
                        ctright++;
                    }
                    if (ctleft > ctright)
                    {
                        for (int i = 0; i < ctright; i++)
                        {
                            Arrows[1].OnInteract();
                            Arrows[1].OnInteractEnded();
                            yield return new WaitForSeconds(0.05f);
                        }
                    }
                    else if (ctleft < ctright)
                    {
                        for (int i = 0; i < ctleft; i++)
                        {
                            Arrows[0].OnInteract();
                            Arrows[0].OnInteractEnded();
                            yield return new WaitForSeconds(0.05f);
                        }
                    }
                    else
                    {
                        int rando = Random.Range(0, 2);
                        for (int i = 0; i < ctleft; i++)
                        {
                            Arrows[rando].OnInteract();
                            Arrows[rando].OnInteractEnded();
                            yield return new WaitForSeconds(0.05f);
                        }
                    }
                    if (ctleft == 0 && ctright == 0)
                        yield return new WaitForSeconds(0.05f);
                    Square.OnInteract();
                }
            }
        }
    }

    //tp force solve handler
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return ProcessTwitchCommand("submit "+ IDList.phrases[anchor + 1]);
    }
}

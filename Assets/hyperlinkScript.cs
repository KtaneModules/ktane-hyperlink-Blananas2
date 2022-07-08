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
    public MeshRenderer Frame;
    public Material[] OOOMats;
    public Material[] OtherMats;
    public Material[] IconMats;
    public Font[] Fonts;
    public Material[] FontMats;
    public GameObject[] RGB;
    public SpriteRenderer[] MariPos;
    public Sprite[] MariFlags;

    //Logging
    static int moduleIdCounter = 1;
    private int moduleId;
    public bool moduleSolved;

	private bool holding;
    private bool interactable;
    private int btn;

    int selectedID;
    int anchor;
    int moduleIndex { get { return anchor + 1; } }
    int currentScreen;
    string selectedString = "";
    private readonly List<string> charList = new List<string>();
    private readonly List<string> screwList = new List<string>();
    readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly string nonsense = "⅓⅔⅛⅜⅝⅞ↄ←↑→↓↔↨∂∆∏∑−∕∙√∞∟∩∫≈";
    string screwString = "";
    string connectionLink;
    int step;
    int selectedIcon = 60;
    private readonly List<string> colorString = new List<string> { "0FF", "08F", "888", "F80", "FFF", "8F0", "00F", "FF8", "808", "F0F", "0F0", "F8F", "080", "FF0", "800", "8F8", "008", "880", "88F", "F08", "F88", "F00", "0F8", "000", "088", "8FF", "80F" };

    private readonly List<int> index = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
    private readonly List<string> encodings = new List<string> { "Alphabetic Position", "American Sign Language", "Binary", "Boozleglyphs", "Braille", "Cube Symbols", "\"Deaf\" Semaphore Telegraph", "Elder Futhark", "14-Segment Display", "Lombax", "Maritime Flags", "Moon Type", "Morse Code", "Necronomicon", "Ogham", "Pigpen", "Semaphore", "Standard", "Standard Galactic Alphabet", "SYNC-125 [3]", "Tap Code", "Unown", "Webdings", "Wingdings", "Wingdings 2", "Wingdings 3", "Zoni"};

    private readonly List<string> alphabeticPosition = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26" };
    private readonly List<string> binary = new List<string> { "00001", "00010", "00011", "00100", "00101", "00110", "00111", "01000", "01001", "01010", "01011", "01100", "01101", "01110", "01111", "10000", "10001", "10010", "10011", "10100", "10101", "10110", "10111", "11000", "11001", "11010" };
    private readonly List<string> elderFuthark = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "cc", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "uu", "x", "y", "z" };
    private readonly List<string> morse = new List<string> { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
    private readonly List<string> ogham = new List<string> { "┼", "┬", "╨╨", "╨", "╫╫", "┬┬┬", "//", "┴", "╫┼╫", "#", "₩", "╥", "/", "╥┬╥", "╫", "═", "╨┴╨", "/////", "╥╥", "┴┴┴", "┼┼┼", "□", "◊", "X", "///", "////" };
    private readonly List<string> sync = new List<string> { "a", "p'", "C", "t'", "e", "f", "k'", "h", "i", "j'", "k", "r'", "m", "n", "o", "p", "?", "r", "s", "t", "u", "f'", "w", "!", "y", "s'" };
    private readonly List<string> tapCode = new List<string> { "11", "12", "13", "14", "15", "21", "22", "23", "24", "25", "66", "31", "32", "33", "34", "35", "41", "42", "43", "44", "45", "51", "52", "53", "54", "55" };

    private readonly List<string> holdingList = new List<string>();

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

            switch (currentScreen)
            {
                case 0:
                    Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
                    Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[0];
                    break;
                case 10:
                    Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[0];
                    Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[1];
                    break;
                default:
                    Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[0];
                    Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[0];
                    break;
            }

            TopNumber.text = string.Format(" {0} ", currentScreen + 1);
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
            StartCoroutine(HoldChecker());
        }
    }

    void arrowRelease () {
        if (!interactable)
        {
            return;
        }
        holding = false;
        StopAllCoroutines();
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
            TopNumber.text = string.Format(" {0} ", currentScreen + 1);
            Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
            Back.GetComponent<Renderer>().material = OtherMats[2];
            step = 0;
            UpdateText();
        }
        ConnectionLED.material = error ? OtherMats[3] : OtherMats[7];
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
                ? "[The Hyperlink #{0}] The module failed to connect the the server, use the YouTube link instead."
                : "[The Hyperlink #{0}] The module connected to the server.", moduleId);
        for (int a = 0; a < 11; a++)
        {
            Debug.LogFormat("[The Hyperlink #{0}] Encoding {1}: '{2}' in {3} => '{4}' => {5}", moduleId, a + 1, screwList[a].Replace("\n", ""), encodings[index[a]], charList[a].Replace("\n", ""), selectedString[a] );
        }
        Debug.LogFormat("[The Hyperlink #{0}] Link {3}{1} references {2}.", moduleId, selectedString, IDList.phrases[moduleIndex], error ? "https://www.youtube.com/watch?v=" : "https://hyperlink.marksam.net/?link=");
        interactable = true;
        Circle.GetComponent<MeshRenderer>().material = OtherMats[0];
        Arrows[1].GetComponent<MeshRenderer>().material = OtherMats[0];
        Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
        currentScreen = 0;
        TopNumber.text = "1";
    }

    public IEnumerator ConnectionLost()
    {
        Debug.LogFormat("[The Hyperlink #{0}] The module has lost its connection to the server and will now use the YouTube link instead.", moduleId);
        ConnectionLED.material = OtherMats[7];
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
                selectedIcon = selectedIcon - 1;
            } else {
                selectedIcon = selectedIcon + 1;
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
                HideMaritime();
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
                Frame.material = OtherMats[2];
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
                    Debug.LogFormat("[The Hyperlink #{0}] Icon for {1} selected, module solved.", moduleId, IDList.phrases[moduleIndex]);
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
                    Frame.material = OtherMats[2];
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
                    TopNumber.text = string.Format(" {0} ", currentScreen + 1);
                    Arrows[0].GetComponent<MeshRenderer>().material = OtherMats[1];
                    Back.GetComponent<Renderer>().material = OtherMats[2];
                    UpdateText();
                }
            }
        }
    }

    void UpdateText ()
    {
        HideMaritime();
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
                MainText.text = "";
                MainText.font = Fonts[7];
                MainText.GetComponent<Renderer>().material = FontMats[7];
                ShowMaritime(selectedString[currentScreen]);
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
        Frame.material = OOOMats[index[currentScreen]];
        for(var i = 0; i < 3; ++i)
        {
            switch (colorString[index[currentScreen]][i])
            {
                case '0':
                    RGB[i].GetComponent<Renderer>().material = OtherMats[2];
                    break;
                case '8':
                    RGB[i].GetComponent<Renderer>().material = OtherMats[i + 3];
                    break;
                default:
                    RGB[i].GetComponent<Renderer>().material = OtherMats[i + 6];
                    break;
            }
        }

        MainText.GetComponent<TextMesh>().fontSize = charList[currentScreen].Length == 3 ? 288 : 144;
    }

    void HideMaritime () {
        for (int g = 0; g < 20; g++) {
            MariPos[g].sprite = MariFlags[0];
        }
    }

    void ShowMaritime(char c) {
        const int one = 4;
        int[] three = new[] { 15, 16, 19 };
        int[] four = new[] { 15, 16, 17, 18 };
        int[] five = new[] { 9, 10, 4, 13, 14 };
        int[] six = new[] { 9, 10, 11, 12, 13, 14 };
        int[] seven = new[] { 9, 10, 3, 4, 5, 13, 14 };
        int[] eight = new[] { 0, 1, 2, 11, 12, 6, 7, 8 };
        int[] nine = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        switch (c) {
            case '0': MariPos[four[0]].sprite = MariFlags[26]; MariPos[four[1]].sprite = MariFlags[5]; MariPos[four[2]].sprite = MariFlags[18]; MariPos[four[3]].sprite = MariFlags[15]; break;     
            case '1': MariPos[three[0]].sprite = MariFlags[15]; MariPos[three[1]].sprite = MariFlags[14]; MariPos[three[2]].sprite = MariFlags[5]; break;      
            case '2': MariPos[three[0]].sprite = MariFlags[20]; MariPos[three[1]].sprite = MariFlags[23]; MariPos[three[2]].sprite = MariFlags[15]; break;      
            case '3': MariPos[five[0]].sprite = MariFlags[20]; MariPos[five[1]].sprite = MariFlags[8]; MariPos[five[2]].sprite = MariFlags[18]; MariPos[five[3]].sprite = MariFlags[5]; MariPos[five[4]].sprite = MariFlags[5]; break;    
            case '4': MariPos[four[0]].sprite = MariFlags[6]; MariPos[four[1]].sprite = MariFlags[15]; MariPos[four[2]].sprite = MariFlags[21]; MariPos[four[3]].sprite = MariFlags[18]; break;     
            case '5': MariPos[four[0]].sprite = MariFlags[6]; MariPos[four[1]].sprite = MariFlags[9]; MariPos[four[2]].sprite = MariFlags[22]; MariPos[four[3]].sprite = MariFlags[5]; break;     
            case '6': MariPos[three[0]].sprite = MariFlags[19]; MariPos[three[1]].sprite = MariFlags[9]; MariPos[three[2]].sprite = MariFlags[24]; break;      
            case '7': MariPos[five[0]].sprite = MariFlags[19]; MariPos[five[1]].sprite = MariFlags[5]; MariPos[five[2]].sprite = MariFlags[22]; MariPos[five[3]].sprite = MariFlags[5]; MariPos[five[4]].sprite = MariFlags[14]; break;    
            case '8': MariPos[five[0]].sprite = MariFlags[5]; MariPos[five[1]].sprite = MariFlags[9]; MariPos[five[2]].sprite = MariFlags[7]; MariPos[five[3]].sprite = MariFlags[8]; MariPos[five[4]].sprite = MariFlags[20]; break;    
            case '9': MariPos[four[0]].sprite = MariFlags[14]; MariPos[four[1]].sprite = MariFlags[9]; MariPos[four[2]].sprite = MariFlags[14]; MariPos[four[3]].sprite = MariFlags[5]; break;     
            case 'A': MariPos[four[0]].sprite = MariFlags[1]; MariPos[four[1]].sprite = MariFlags[12]; MariPos[four[2]].sprite = MariFlags[6]; MariPos[four[3]].sprite = MariFlags[1]; break;     
            case 'B': MariPos[five[0]].sprite = MariFlags[2]; MariPos[five[1]].sprite = MariFlags[18]; MariPos[five[2]].sprite = MariFlags[1]; MariPos[five[3]].sprite = MariFlags[22]; MariPos[five[4]].sprite = MariFlags[15]; break;    
            case 'C': MariPos[seven[0]].sprite = MariFlags[3]; MariPos[seven[1]].sprite = MariFlags[8]; MariPos[seven[2]].sprite = MariFlags[1]; MariPos[seven[3]].sprite = MariFlags[18]; MariPos[seven[4]].sprite = MariFlags[12]; MariPos[seven[5]].sprite = MariFlags[9]; MariPos[seven[6]].sprite = MariFlags[5]; break;  
            case 'D': MariPos[five[0]].sprite = MariFlags[4]; MariPos[five[1]].sprite = MariFlags[5]; MariPos[five[2]].sprite = MariFlags[12]; MariPos[five[3]].sprite = MariFlags[20]; MariPos[five[4]].sprite = MariFlags[1]; break;    
            case 'E': MariPos[four[0]].sprite = MariFlags[5]; MariPos[four[1]].sprite = MariFlags[3]; MariPos[four[2]].sprite = MariFlags[8]; MariPos[four[3]].sprite = MariFlags[15]; break;     
            case 'F': MariPos[seven[0]].sprite = MariFlags[6]; MariPos[seven[1]].sprite = MariFlags[15]; MariPos[seven[2]].sprite = MariFlags[24]; MariPos[seven[3]].sprite = MariFlags[20]; MariPos[seven[4]].sprite = MariFlags[18]; MariPos[seven[5]].sprite = MariFlags[15]; MariPos[seven[6]].sprite = MariFlags[20]; break;  
            case 'G': MariPos[four[0]].sprite = MariFlags[7]; MariPos[four[1]].sprite = MariFlags[15]; MariPos[four[2]].sprite = MariFlags[12]; MariPos[four[3]].sprite = MariFlags[6]; break;     
            case 'H': MariPos[five[0]].sprite = MariFlags[8]; MariPos[five[1]].sprite = MariFlags[15]; MariPos[five[2]].sprite = MariFlags[20]; MariPos[five[3]].sprite = MariFlags[5]; MariPos[five[4]].sprite = MariFlags[12]; break;    
            case 'I': MariPos[five[0]].sprite = MariFlags[9]; MariPos[five[1]].sprite = MariFlags[14]; MariPos[five[2]].sprite = MariFlags[4]; MariPos[five[3]].sprite = MariFlags[9]; MariPos[five[4]].sprite = MariFlags[1]; break;    
            case 'J': MariPos[seven[0]].sprite = MariFlags[10]; MariPos[seven[1]].sprite = MariFlags[21]; MariPos[seven[2]].sprite = MariFlags[12]; MariPos[seven[3]].sprite = MariFlags[9]; MariPos[seven[4]].sprite = MariFlags[5]; MariPos[seven[5]].sprite = MariFlags[20]; MariPos[seven[6]].sprite = MariFlags[20]; break;  
            case 'K': MariPos[four[0]].sprite = MariFlags[11]; MariPos[four[1]].sprite = MariFlags[9]; MariPos[four[2]].sprite = MariFlags[12]; MariPos[four[3]].sprite = MariFlags[15]; break;     
            case 'L': MariPos[four[0]].sprite = MariFlags[12]; MariPos[four[1]].sprite = MariFlags[9]; MariPos[four[2]].sprite = MariFlags[13]; MariPos[four[3]].sprite = MariFlags[1]; break;     
            case 'M': MariPos[four[0]].sprite = MariFlags[13]; MariPos[four[1]].sprite = MariFlags[9]; MariPos[four[2]].sprite = MariFlags[11]; MariPos[four[3]].sprite = MariFlags[5]; break;     
            case 'N': MariPos[eight[0]].sprite = MariFlags[14]; MariPos[eight[1]].sprite = MariFlags[15]; MariPos[eight[2]].sprite = MariFlags[22]; MariPos[eight[3]].sprite = MariFlags[5]; MariPos[eight[4]].sprite = MariFlags[13]; MariPos[eight[5]].sprite = MariFlags[2]; MariPos[eight[6]].sprite = MariFlags[5]; MariPos[eight[7]].sprite = MariFlags[18]; break; 
            case 'O': MariPos[five[0]].sprite = MariFlags[15]; MariPos[five[1]].sprite = MariFlags[19]; MariPos[five[2]].sprite = MariFlags[3]; MariPos[five[3]].sprite = MariFlags[1]; MariPos[five[4]].sprite = MariFlags[18]; break;    
            case 'P': MariPos[four[0]].sprite = MariFlags[16]; MariPos[four[1]].sprite = MariFlags[1]; MariPos[four[2]].sprite = MariFlags[16]; MariPos[four[3]].sprite = MariFlags[1]; break;     
            case 'Q': MariPos[six[0]].sprite = MariFlags[17]; MariPos[six[1]].sprite = MariFlags[21]; MariPos[six[2]].sprite = MariFlags[5]; MariPos[six[3]].sprite = MariFlags[2]; MariPos[six[4]].sprite = MariFlags[5]; MariPos[six[5]].sprite = MariFlags[3]; break;   
            case 'R': MariPos[five[0]].sprite = MariFlags[18]; MariPos[five[1]].sprite = MariFlags[15]; MariPos[five[2]].sprite = MariFlags[13]; MariPos[five[3]].sprite = MariFlags[5]; MariPos[five[4]].sprite = MariFlags[15]; break;    
            case 'S': MariPos[six[0]].sprite = MariFlags[19]; MariPos[six[1]].sprite = MariFlags[9]; MariPos[six[2]].sprite = MariFlags[5]; MariPos[six[3]].sprite = MariFlags[18]; MariPos[six[4]].sprite = MariFlags[18]; MariPos[six[5]].sprite = MariFlags[1]; break;   
            case 'T': MariPos[five[0]].sprite = MariFlags[20]; MariPos[five[1]].sprite = MariFlags[1]; MariPos[five[2]].sprite = MariFlags[14]; MariPos[five[3]].sprite = MariFlags[7]; MariPos[five[4]].sprite = MariFlags[15]; break;    
            case 'U': MariPos[seven[0]].sprite = MariFlags[21]; MariPos[seven[1]].sprite = MariFlags[14]; MariPos[seven[2]].sprite = MariFlags[9]; MariPos[seven[3]].sprite = MariFlags[6]; MariPos[seven[4]].sprite = MariFlags[15]; MariPos[seven[5]].sprite = MariFlags[18]; MariPos[seven[6]].sprite = MariFlags[13]; break;  
            case 'V': MariPos[six[0]].sprite = MariFlags[22]; MariPos[six[1]].sprite = MariFlags[9]; MariPos[six[2]].sprite = MariFlags[3]; MariPos[six[3]].sprite = MariFlags[20]; MariPos[six[4]].sprite = MariFlags[15]; MariPos[six[5]].sprite = MariFlags[18]; break;   
            case 'W': MariPos[seven[0]].sprite = MariFlags[23]; MariPos[seven[1]].sprite = MariFlags[8]; MariPos[seven[2]].sprite = MariFlags[9]; MariPos[seven[3]].sprite = MariFlags[19]; MariPos[seven[4]].sprite = MariFlags[11]; MariPos[seven[5]].sprite = MariFlags[5]; MariPos[seven[6]].sprite = MariFlags[25]; break;  
            case 'X': MariPos[four[0]].sprite = MariFlags[24]; MariPos[four[1]].sprite = MariFlags[18]; MariPos[four[2]].sprite = MariFlags[1]; MariPos[four[3]].sprite = MariFlags[25]; break;     
            case 'Y': MariPos[six[0]].sprite = MariFlags[25]; MariPos[six[1]].sprite = MariFlags[1]; MariPos[six[2]].sprite = MariFlags[14]; MariPos[six[3]].sprite = MariFlags[11]; MariPos[six[4]].sprite = MariFlags[5]; MariPos[six[5]].sprite = MariFlags[5]; break;   
            case 'Z': MariPos[four[0]].sprite = MariFlags[26]; MariPos[four[1]].sprite = MariFlags[21]; MariPos[four[2]].sprite = MariFlags[12]; MariPos[four[3]].sprite = MariFlags[21]; break;     
            case 'a': MariPos[one].sprite = MariFlags[1]; break;        
            case 'b': MariPos[one].sprite = MariFlags[2]; break;        
            case 'c': MariPos[one].sprite = MariFlags[3]; break;        
            case 'd': MariPos[one].sprite = MariFlags[4]; break;        
            case 'e': MariPos[one].sprite = MariFlags[5]; break;        
            case 'f': MariPos[one].sprite = MariFlags[6]; break;        
            case 'g': MariPos[one].sprite = MariFlags[7]; break;        
            case 'h': MariPos[one].sprite = MariFlags[8]; break;        
            case 'i': MariPos[one].sprite = MariFlags[9]; break;        
            case 'j': MariPos[one].sprite = MariFlags[10]; break;        
            case 'k': MariPos[one].sprite = MariFlags[11]; break;        
            case 'l': MariPos[one].sprite = MariFlags[12]; break;        
            case 'm': MariPos[one].sprite = MariFlags[13]; break;        
            case 'n': MariPos[one].sprite = MariFlags[14]; break;        
            case 'o': MariPos[one].sprite = MariFlags[15]; break;        
            case 'p': MariPos[one].sprite = MariFlags[16]; break;        
            case 'q': MariPos[one].sprite = MariFlags[17]; break;        
            case 'r': MariPos[one].sprite = MariFlags[18]; break;        
            case 's': MariPos[one].sprite = MariFlags[19]; break;        
            case 't': MariPos[one].sprite = MariFlags[20]; break;        
            case 'u': MariPos[one].sprite = MariFlags[21]; break;        
            case 'v': MariPos[one].sprite = MariFlags[22]; break;        
            case 'w': MariPos[one].sprite = MariFlags[23]; break;        
            case 'x': MariPos[one].sprite = MariFlags[24]; break;        
            case 'y': MariPos[one].sprite = MariFlags[25]; break;        
            case 'z': MariPos[one].sprite = MariFlags[26]; break;        
            case '-': MariPos[four[0]].sprite = MariFlags[4]; MariPos[four[1]].sprite = MariFlags[1]; MariPos[four[2]].sprite = MariFlags[19]; MariPos[four[3]].sprite = MariFlags[8]; break;     
            case '_': MariPos[nine[0]].sprite = MariFlags[21]; MariPos[nine[1]].sprite = MariFlags[14]; MariPos[nine[2]].sprite = MariFlags[4]; MariPos[nine[3]].sprite = MariFlags[5]; MariPos[nine[4]].sprite = MariFlags[18]; MariPos[nine[5]].sprite = MariFlags[12]; MariPos[nine[6]].sprite = MariFlags[9]; MariPos[nine[7]].sprite = MariFlags[14]; MariPos[nine[8]].sprite = MariFlags[5]; break;
        }
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
            screwString = screwString.Replace(string.Format(" {0} ", alphabet[k]), string.Format(" {0} ", nonsense[k]));
            screwString = screwString.Replace(string.Format(" {0} ", nonsense[k]), string.Format(" {0} ", holdingList[k]));
        }
        for (int n = 0; n < 26; n++)
        {
            screwString = screwString.Replace(string.Format(" {0} ", alphabet[n]), string.Format(" {0} ", nonsense[n]));
            screwString = screwString.Replace(string.Format(" {0} ", nonsense[n]), string.Format(" {0} ", holdingList[n]));
        }

        if (l == 4) {
            screwString = string.Format(">{0}<", screwString);
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
             screwString = screwString.Replace(string.Format(" {0} ", alphabet[m]), string.Format(" {0} ", nonsense[m]));
             screwString = screwString.Replace(string.Format(" {0} ", nonsense[m]), string.Format(" {0} ", o[m]));
         }
         for (int p = 0; p < 26; p++)
         {
             screwString = screwString.Replace(string.Format(" {0} ", alphabet[p]), string.Format(" {0} ", nonsense[p]));
             screwString = screwString.Replace(string.Format(" {0} ", nonsense[p]), string.Format(" {0} ", o[p]));
         }
    }

    //twitch plays
    private bool moveValid(string s) 
    {
        return new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }.Contains(s);
    } 

    private bool listContains(string s)
    {
        return Enumerable.Range(0, IDList.phrases.Length).Where(x => x % 2 != 0).Select(x => IDList.phrases[x].ToLowerInvariant()).Contains(s);
    }

    #pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} left/right (#) [Press the left/right arrow (optionally '#' times)] | !{0} submit <module> [Submits the specified module]";
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
                            if (parameters[0].ToLowerInvariant().Equals("left"))
                            {
                                if (currentScreen == 0)
                                {
                                    yield break;
                                }
                                Arrows[0].OnInteract();
                                Arrows[0].OnInteractEnded();
                            }
                            else if (parameters[0].ToLowerInvariant().Equals("right"))
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
                        yield return string.Format("sendtochaterror '{0}' is not a valid number! Numbers 1-10 are valid for scrolling.", parameters[1]);
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
                module = module.ToLowerInvariant();
                if (!listContains(module))
                {
                    string modname = Back.GetComponent<Renderer>().material.name.ToLowerInvariant();
                    modname = modname.Replace(" (instance)", "");
                    int rando = Random.Range(0, 2);
                    int counter = 0;
                    while (!module.Equals(modname))
                    {
                        Arrows[rando].OnInteract();
                        Arrows[rando].OnInteractEnded();
                        modname = Back.GetComponent<Renderer>().material.name.ToLowerInvariant();
                        modname = modname.Replace(" (instance)", "");
                        yield return new WaitForSeconds(0.05f);
                        counter++;
                        if (counter == 121)
                        {
                            yield return string.Format("sendtochaterror '{0}' is not a valid module name!", module);
                            yield break;
                        }
                    }
                }
                else
                {
                    int ctleft = 0;
                    int ctright = 0;
                    int curindex = 121;
                    while (!module.Equals(IDList.phrases[curindex].ToLowerInvariant()))
                    {
                        curindex -= 2;
                        if (curindex < 1)
                            curindex = 245;
                        ctleft++;
                    }
                    curindex = 121;
                    while (!module.Equals(IDList.phrases[curindex].ToLowerInvariant()))
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
        yield return ProcessTwitchCommand(string.Format("submit {0}", IDList.phrases[moduleIndex]));
    }
}

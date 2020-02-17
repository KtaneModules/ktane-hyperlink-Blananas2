using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class hyperlinkScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] Arrows;
    public KMSelectable Square;
    public GameObject Circle;
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
    int moduleId;
    private bool moduleSolved;

    int selectedID = 0;
    int anchor = 0;
    int currentScreen = 0;
    string selectedString = "";
    public List<string> charList = new List<string> { };
    public List<string> screwList = new List<string> { };
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string nonsense = "⅓⅔⅛⅜⅝⅞ↄ←↑→↓↔↨∂∆∏∑−∕∙√∞∟∩∫≈";
    string screwString = "";
    int step = 0;
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

    public List<string> holdingList = new List<string> { };

    void Awake () {
        moduleId = moduleIdCounter++;
        
        foreach (KMSelectable arrow in Arrows) {
            KMSelectable pressedArrow = arrow;
            arrow.OnInteract += delegate () { arrowPress(pressedArrow); return false; };
        }
        
        Square.OnInteract += delegate () { squarePress(); return false; };
        
    }

    // Use this for initialization
    void Start () {
	    selectedID = UnityEngine.Random.Range(0, 121);
        anchor = 2 * selectedID;
        selectedString = IDList.phrases[anchor];
        StringToLetters(selectedString);
        
        index.Shuffle();

        LettersToScrew();

        UpdateText();

        for (int a = 0; a < 11; a++)
        {
            Debug.LogFormat("[The Hyperlink #{0}] Encoding {1}: '{2}' in {3} => '{4}' => {5}", moduleId, a + 1, screwList[a].Replace("\n", ""), encodings[index[a]], charList[a].Replace("\n", ""), selectedString[a] );
        }
        Debug.LogFormat("[The Hyperlink #{0}] Video https://www.youtube.com/watch?v={1} references {2}.", moduleId, selectedString, IDList.phrases[anchor + 1]);
    }

    void arrowPress (KMSelectable arrow)
    {
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
            }
            else if (arrow == Arrows[1])
            {
                selectedIcon = (selectedIcon + 1);
            }

            if (selectedIcon == -1) { selectedIcon = 120; }
            if (selectedIcon == 121) { selectedIcon = 0; }
            Back.GetComponent<Renderer>().material = IconMats[selectedIcon];
        }
    }

    void squarePress ()
    {
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
                    moduleSolved = true;
                    Square.AddInteractionPunch();
                    step = 999;
                    TopNumber.text = "   ";
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

        if (charList[currentScreen].Length == 3) {
            MainText.GetComponent<TextMesh>().fontSize = 288;
        } else
        {
            MainText.GetComponent<TextMesh>().fontSize = 144;
        }
    }

    void StringToLetters (string s)
    {
        for (int i = 0; i < 11; i++)
        {
            if (s[i] == '0') { charList.Add(" Z E \n\n R O "); };
            if (s[i] == '1') { charList.Add(" O N \n\n E "); };
            if (s[i] == '2') { charList.Add(" T W \n\n O "); };
            if (s[i] == '3') { charList.Add(" T H \n\n R \n\n E E "); };
            if (s[i] == '4') { charList.Add(" F O \n\n U R "); };
            if (s[i] == '5') { charList.Add(" F I \n\n V E "); };
            if (s[i] == '6') { charList.Add(" S I \n\n X "); };
            if (s[i] == '7') { charList.Add(" S E \n\n V \n\n E N "); };
            if (s[i] == '8') { charList.Add(" E I \n\n G \n\n H T "); };
            if (s[i] == '9') { charList.Add(" N I \n\n N E "); };
            if (s[i] == 'A') { charList.Add(" A L \n\n F A "); };
            if (s[i] == 'B') { charList.Add(" B R \n\n A \n\n V O "); };
            if (s[i] == 'C') { charList.Add(" C H \n\n A R L \n\n I E "); };
            if (s[i] == 'D') { charList.Add(" D E \n\n L \n\n T A "); };
            if (s[i] == 'E') { charList.Add(" E C \n\n H O "); };
            if (s[i] == 'F') { charList.Add(" F O \n\n X T R \n\n O T "); };
            if (s[i] == 'G') { charList.Add(" G O \n\n L F "); };
            if (s[i] == 'H') { charList.Add(" H O \n\n T \n\n E L "); };
            if (s[i] == 'I') { charList.Add(" I N \n\n D \n\n I A "); };
            if (s[i] == 'J') { charList.Add(" J U \n\n L I E \n\n T T "); };
            if (s[i] == 'K') { charList.Add(" K I \n\n L O "); };
            if (s[i] == 'L') { charList.Add(" L I \n\n M A "); };
            if (s[i] == 'M') { charList.Add(" M I \n\n K E "); };
            if (s[i] == 'N') { charList.Add(" N O V \n\n E M \n\n B E R "); };
            if (s[i] == 'O') { charList.Add(" O S \n\n C \n\n A R "); };
            if (s[i] == 'P') { charList.Add(" P A \n\n P A "); };
            if (s[i] == 'Q') { charList.Add(" Q U \n\n E B \n\n E C "); };
            if (s[i] == 'R') { charList.Add(" R O \n\n M \n\n E O "); };
            if (s[i] == 'S') { charList.Add(" S I \n\n E R \n\n R A "); };
            if (s[i] == 'T') { charList.Add(" T A \n\n N \n\n G O "); };
            if (s[i] == 'U') { charList.Add(" U N \n\n I F O \n\n R M "); };
            if (s[i] == 'V') { charList.Add(" V I \n\n C T \n\n O R "); };
            if (s[i] == 'W') { charList.Add(" W H \n\n I S K \n\n E Y "); };
            if (s[i] == 'X') { charList.Add(" X R \n\n A Y "); };
            if (s[i] == 'Y') { charList.Add(" Y A \n\n N K \n\n E E "); };
            if (s[i] == 'Z') { charList.Add(" Z U \n\n L U "); };
            if (s[i] == 'a') { charList.Add(" A "); };
            if (s[i] == 'b') { charList.Add(" B "); };
            if (s[i] == 'c') { charList.Add(" C "); };
            if (s[i] == 'd') { charList.Add(" D "); };
            if (s[i] == 'e') { charList.Add(" E "); };
            if (s[i] == 'f') { charList.Add(" F "); };
            if (s[i] == 'g') { charList.Add(" G "); };
            if (s[i] == 'h') { charList.Add(" H "); };
            if (s[i] == 'i') { charList.Add(" I "); };
            if (s[i] == 'j') { charList.Add(" J "); };
            if (s[i] == 'k') { charList.Add(" K "); };
            if (s[i] == 'l') { charList.Add(" L "); };
            if (s[i] == 'm') { charList.Add(" M "); };
            if (s[i] == 'n') { charList.Add(" N "); };
            if (s[i] == 'o') { charList.Add(" O "); };
            if (s[i] == 'p') { charList.Add(" P "); };
            if (s[i] == 'q') { charList.Add(" Q "); };
            if (s[i] == 'r') { charList.Add(" R "); };
            if (s[i] == 's') { charList.Add(" S "); };
            if (s[i] == 't') { charList.Add(" T "); };
            if (s[i] == 'u') { charList.Add(" U "); };
            if (s[i] == 'v') { charList.Add(" V "); };
            if (s[i] == 'w') { charList.Add(" W "); };
            if (s[i] == 'x') { charList.Add(" X "); };
            if (s[i] == 'y') { charList.Add(" Y "); };
            if (s[i] == 'z') { charList.Add(" Z "); };
            if (s[i] == '-') { charList.Add(" D A \n\n S H "); };
            if (s[i] == '_') { charList.Add(" U N D \n\n E R L \n\n I N E "); };
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
            default:
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
        for(int i = 0; i < IDList.phrases.Length; i++)
        {
            if (IDList.phrases[i].ToLower().Equals(s.ToLower()))
            {
                return true;
            }
        }
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
                        int temp = 0;
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
                            }
                            else if (parameters[0].ToLower().Equals("right"))
                            {
                                if (currentScreen == 10)
                                {
                                    yield break;
                                }
                                Arrows[1].OnInteract();
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
                if (IDList.phrases[anchor + 1].ToLower().Equals(module))
                {
                    yield return "solve";
                }
                else if(listContains(module))
                {
                    yield return "strike";
                }
                string modname = Back.GetComponent<Renderer>().material.name.ToLower();
                modname = modname.Replace(" (instance)", "");
                int rando = UnityEngine.Random.Range(0, 2);
                int counter = 0;
                while (!module.Equals(modname))
                {
                    Arrows[rando].OnInteract();
                    modname = Back.GetComponent<Renderer>().material.name.ToLower();
                    modname = modname.Replace(" (instance)", "");
                    yield return new WaitForSeconds(0.05f);
                    counter++;
                    if(counter == 121)
                    {
                        yield return "sendtochaterror '"+module+"' is not a valid module name!";
                        yield break;
                    }
                }
                Square.OnInteract();
            }
        }
    }

    //tp force solve handler
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return ProcessTwitchCommand("submit "+ IDList.phrases[anchor + 1]);
    }
}

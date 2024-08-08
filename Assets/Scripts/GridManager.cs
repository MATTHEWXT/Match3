using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public int width = 12;
    public int height = 8;
    public GameObject buttonPrefab;
    public Transform gridPanel;
    public Sprite[] icons;
    public Sprite[] blockedIcon;
    private Button[,] buttons;

    private GridAnimation gridAnimation;
    void Start()
    {
        GenerateGrid();
    }

    public void Initialize(GridAnimation gridAnimation)
    {
        this.gridAnimation = gridAnimation;
    }

    private void GenerateGrid()
    {
        buttons = new Button[height, width];

        GenerateBlockedTileWay();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (buttons[y, x] == null)
                {
                    int spriteIndex = Random.Range(0, icons.Length);

                    int? prevTileIdX = -1;
                    int? prevTileIdY = -1;

                    if (x - 1 >= 0)
                    {
                        prevTileIdX = buttons[y, x - 1].gameObject.GetComponent<TileIdentifier>().identifier;
                    }
                    if (y - 1 >= 0)
                    {
                        prevTileIdY = buttons[y - 1, x].gameObject.GetComponent<TileIdentifier>().identifier;
                    }

                    bool isDiffPrevIdY = false;
                    bool isDiffPrevIdX = false;
                    while (!isDiffPrevIdY || !isDiffPrevIdX)
                    {
                        if (prevTileIdY == spriteIndex)
                        {
                            spriteIndex = Random.Range(0, icons.Length);
                            isDiffPrevIdY = false;
                        }
                        else
                        {
                            isDiffPrevIdY = true;
                        }

                        if (prevTileIdX == spriteIndex)
                        {
                            spriteIndex = Random.Range(0, icons.Length);
                            isDiffPrevIdX = false;
                        }
                        else
                        {
                            isDiffPrevIdX = true;
                        }
                    }

                    int idnex = y * width + x;
                    GameObject buttonObject = Instantiate(buttonPrefab, gridPanel);
                    Button button = buttonObject.GetComponent<Button>();

                    buttonObject.name = $"Tile {idnex}";
                    buttonObject.transform.GetChild(0).GetComponent<Image>().sprite = icons[spriteIndex]; // Назначение иконки

                    TileIdentifier tileIdentifier = buttonObject.GetComponent<TileIdentifier>();
                    tileIdentifier.identifier = spriteIndex;

                    int mainId = spriteIndex;
                    tileIdentifier.mainId = mainId;

                    ButtonClickHandler clickHandler = buttonObject.AddComponent<ButtonClickHandler>();
                    button.onClick.AddListener(() => clickHandler.OnButtonClick(button));
                    buttons[y, x] = button;
                }
                else
                {
                    GameObject buttonObject = buttons[y, x].gameObject;
                    buttonObject.transform.SetParent(gridPanel, false);
                }
            }
        }
    }

    private void GenerateBlockedTileWay()
    {
        int blockedCount = 7;
        for (int i = 0; i < blockedIcon.Length; i++)
        {
            Vector2Int currentPosition = new Vector2Int(Random.Range(0, width - 1), Random.Range(0, height - 1));
            GenerateBlockedObject(currentPosition.y, currentPosition.x);

            int indexEl = 0;
            bool isDeadEnd = false;
            while (indexEl < blockedCount)
            {
                int selectDirection = Random.Range(1, 4);
                //Right
                if (selectDirection == 1 && currentPosition.x + 1 < width)
                {
                    Vector2Int nextElement = new Vector2Int(currentPosition.x + 1, currentPosition.y);

                    if (isDeadEnd && buttons[nextElement.y, nextElement.x] != null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        isDeadEnd = false;
                        indexEl++;
                        continue;
                    }
                    if (buttons[nextElement.y, nextElement.x] == null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        indexEl++;
                    }
                    else
                    {
                        isDeadEnd = true;
                        continue;
                    }
                }
                //Bottom
                else if (selectDirection == 2 && currentPosition.y + 1 < height)
                {
                    Vector2Int nextElement = new Vector2Int(currentPosition.x, currentPosition.y + 1);
                    if (isDeadEnd && buttons[nextElement.y, nextElement.x] != null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        isDeadEnd = false;
                        indexEl++;
                        continue;
                    }
                    if (buttons[nextElement.y, nextElement.x] == null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        indexEl++;
                    }
                    else
                    {
                        isDeadEnd = true;
                        continue;
                    }
                }
                //Left
                else if (selectDirection == 3 && currentPosition.x - 1 > 0)
                {
                    Vector2Int nextElement = new Vector2Int(currentPosition.x - 1, currentPosition.y);
                    if (isDeadEnd && buttons[nextElement.y, nextElement.x] != null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        isDeadEnd = false;
                        indexEl++;
                        continue;
                    }
                    if (buttons[nextElement.y, nextElement.x] == null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        indexEl++;
                    }
                    else
                    {
                        isDeadEnd = true;
                        continue;
                    }
                }
                //Up
                else if (selectDirection == 4 && currentPosition.y - 1 > height)
                {
                    Vector2Int nextElement = new Vector2Int(currentPosition.x, currentPosition.y - 1);
                    if (isDeadEnd && buttons[nextElement.y, nextElement.x] != null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        isDeadEnd = false;
                        indexEl++;
                        continue;
                    }
                    if (buttons[nextElement.y, nextElement.x] == null)
                    {
                        GenerateBlockedObject(nextElement.y, nextElement.x);
                        currentPosition = nextElement;
                        indexEl++;
                    }
                    else
                    {
                        isDeadEnd = true;
                        continue;
                    }
                }
            }
        }
    }

    private void GenerateBlockedObject(int y, int x)
    {
        int spriteIndex = Random.Range(0, blockedIcon.Length);
        int idnex = y * width + x;
        GameObject buttonObject = Instantiate(buttonPrefab);
        Button button = buttonObject.GetComponent<Button>();

        buttonObject.name = $"Tile {idnex}";
        buttonObject.transform.GetChild(0).GetComponent<Image>().sprite = blockedIcon[spriteIndex]; // Назначение иконки

        TileIdentifier tileIdentifier = buttonObject.GetComponent<TileIdentifier>();
        tileIdentifier.mainId = spriteIndex;
        tileIdentifier.identifier = -1;

        buttons[y, x] = button;
    }

    private List<(Button button, Vector2Int position)> CheckMatchesOfBtn1(Button button1)
    {
        List<(Button button, Vector2Int position)> matches = new List<(Button button, Vector2Int position)>();

        Vector2Int pos = GetButtonPosition(button1);
        matches.Add((button1, pos));

        int counterY = 0;
        int counterX = 0;
        Button buttonCheck;
        //Checker Y Down
        for (int y = 1; y < height - pos.y; y++)
        {
            buttonCheck = buttons[(int)pos.y + y, (int)pos.x];

            if (CheckEqualButton(button1, buttonCheck))
            {
                matches.Add((buttonCheck, new Vector2Int(pos.x, pos.y + y)));
                counterY++;
            }
            else
            {
                break;
            }
        }
        //Checker Y Up
        for (int y = 1; y <= pos.y; y++)
        {
            buttonCheck = buttons[(int)pos.y - y, (int)pos.x];

            if (CheckEqualButton(button1, buttonCheck))
            {
                matches.Add((buttonCheck, new Vector2Int(pos.x, pos.y - y)));
                counterY++;
            }
            else
            {
                break;
            }
        }

        if (counterY == 1)
        {
            matches.RemoveRange(1, 1);
            counterY = 0;
        }

        //Checker X Right
        for (int x = 1; x < width - pos.x; x++)
        {
            buttonCheck = buttons[(int)pos.y, (int)pos.x + x];

            if (CheckEqualButton(button1, buttonCheck))
            {
                matches.Add((buttonCheck, new Vector2Int(pos.x + x, pos.y)));
                counterX++;
            }
            else
            {
                break;
            }
        }
        //Checker X Left
        for (int x = 1; x <= pos.x; x++)
        {
            buttonCheck = buttons[(int)pos.y, (int)pos.x - x];

            if (CheckEqualButton(button1, buttonCheck))
            {
                matches.Add((buttonCheck, new Vector2Int(pos.x - x, pos.y)));
                counterX++;
            }
            else
            {
                break;
            }
        }

        if (counterX == 1)
        {
            matches.RemoveRange(counterY + 1, 1);
        }
        if (matches.Count > 2)
        {
            CheckMatchesWithBlocked(matches);
            return matches;
        }
        return null;
    }

    private void CheckMatchesWithBlocked(List<(Button button, Vector2Int position)> matches)
    {
        var mainXValues = matches.Select(m => m.position.x).Distinct().ToList();
        var allYForXValues = mainXValues.Select(x => new
        {
            X = x,
            MaxY = matches.Where(m => m.position.x == x).Max(m => m.position.y),
            MinY = matches.Where(m => m.position.x == x).Min(m => m.position.y)

        }).ToList();

        var leftX = allYForXValues.Max(item => item.X);
        var rightX = allYForXValues.Min(item => item.X);

        TileIdentifier tileObj;
        var sortedYForXValues = allYForXValues.OrderBy(item => item.X).ToList();

        foreach (var tile in sortedYForXValues)
        {
            tileObj = buttons[tile.MaxY, tile.X].gameObject.GetComponent<TileIdentifier>();
            bool isCheckerUpDown = false;
            for (int i = 0; i <= tile.MaxY - tile.MinY; i++)
            {
                //Check Up Down
                if (isCheckerUpDown == false)
                {
                    if (tile.MinY - i - 1 >= 0)
                    {
                        TileIdentifier blockedObjUnder = buttons[tile.MinY - i - 1, tile.X].gameObject.GetComponent<TileIdentifier>();
                        if (blockedObjUnder.mainId == tileObj.mainId && blockedObjUnder.GetComponent<ButtonClickHandler>() == null)
                        {
                            Button blockedBtnDown = buttons[tile.MinY - i - 1, tile.X].GetComponent<Button>();
                            Vector2Int posBtnDown = new Vector2Int(tile.X, tile.MinY - i - 1);
                            matches.Add((blockedBtnDown, posBtnDown));
                        }
                    }
                    if (tile.MaxY + i + 1 < height)
                    {
                        TileIdentifier blockedObjAbove = buttons[tile.MaxY + i + 1, tile.X].gameObject.GetComponent<TileIdentifier>();
                        if (blockedObjAbove.mainId == tileObj.mainId && blockedObjAbove.GetComponent<ButtonClickHandler>() == null)
                        {
                            Button blockedBtnUp = buttons[tile.MaxY + i + 1, tile.X].GetComponent<Button>();
                            Vector2Int posBtnUp = new Vector2Int(tile.X, tile.MaxY + i + 1);
                            matches.Add((blockedBtnUp, posBtnUp));
                        }
                    }
                    isCheckerUpDown = true;
                }
                //Check left
                if (tile.X == leftX && (tile.X + 1) < width)
                {
                    TileIdentifier blockedObjLeft = buttons[tile.MaxY - i, tile.X + 1].gameObject.GetComponent<TileIdentifier>();
                    if (blockedObjLeft.mainId == tileObj.mainId && blockedObjLeft.GetComponent<ButtonClickHandler>() == null)
                    {
                        Button blockedBtnLeft = buttons[tile.MaxY - i, tile.X + 1].GetComponent<Button>();
                        Vector2Int posBtnLeft = new Vector2Int(tile.X + 1, tile.MaxY - i);
                        matches.Add((blockedBtnLeft, posBtnLeft));
                    }
                }
                //Check right
                if (tile.X == rightX && (tile.X - 1) >= 0)
                {
                    TileIdentifier blockedObjRight = buttons[tile.MaxY - i, tile.X - 1].gameObject.GetComponent<TileIdentifier>();
                    if (blockedObjRight.mainId == tileObj.mainId && blockedObjRight.GetComponent<ButtonClickHandler>() == null)
                    {
                        Button blockedBtnRight = buttons[tile.MaxY - i, tile.X - 1].GetComponent<Button>();
                        Vector2Int posBtnRight = new Vector2Int(tile.X - 1, tile.MaxY - i);
                        matches.Add((blockedBtnRight, posBtnRight));
                    }
                }
            }
        }
    }

    bool isActionFindMatches = true;
    private async Task FindMatches()
    {
        bool isActionX = false;
        bool isActionY = false;
        var matches = new List<(Button button, Vector2Int position)>();
        // Поиск горизонтальных матчей
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                int? current = buttons[y, x].gameObject.GetComponent<TileIdentifier>().identifier;
                if (current != -1 &&
                    current == buttons[y, x + 1].gameObject.GetComponent<TileIdentifier>().identifier &&
                    current == buttons[y, x + 2].gameObject.GetComponent<TileIdentifier>().identifier)
                {
                    matches.Add((buttons[y, x], new Vector2Int(x, y)));
                    matches.Add((buttons[y, x + 1], new Vector2Int(x + 1, y)));
                    matches.Add((buttons[y, x + 2], new Vector2Int(x + 2, y)));
                    // Проверяем дополнительные элементы в горизонтальном ряду
                    for (int k = x + 3; k < width && buttons[y, k].gameObject.GetComponent<TileIdentifier>().identifier == current; k++)
                    {
                        matches.Add((buttons[y, k], new Vector2Int(k, y)));
                    }
                }
                if (matches.Count > 2)
                {
                    CheckMatchesWithBlocked(matches);
                    await Task.Delay(300);
                    await UpdateGrid(matches);
                    matches.Clear();
                    isActionX = true;
                }
                else
                {
                    isActionX = false;
                }
            }
        }
        for (int y = 0; y < height - 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int? current = buttons[y, x].gameObject.GetComponent<TileIdentifier>().identifier;
                if (current != -1 &&
                    current == buttons[y + 1, x].gameObject.GetComponent<TileIdentifier>().identifier &&
                    current == buttons[y + 2, x].gameObject.GetComponent<TileIdentifier>().identifier)
                {
                    matches.Add((buttons[y, x], new Vector2Int(x, y)));
                    matches.Add((buttons[y + 1, x], new Vector2Int(x, y + 1)));
                    matches.Add((buttons[y + 2, x], new Vector2Int(x, y + 2)));
                    // Проверяем дополнительные элементы в горизонтальном ряду
                    for (int k = y + 3; k < height && buttons[k, x].gameObject.GetComponent<TileIdentifier>().identifier == current; k++)
                    {
                        matches.Add((buttons[k, x], new Vector2Int(x, k)));
                    }
                }
                if (matches.Count > 2)
                {
                    CheckMatchesWithBlocked(matches);
                    await Task.Delay(300);
                    await UpdateGrid(matches);
                    matches.Clear();
                    isActionX = true;
                }
                else
                {
                    isActionX = false;
                }
            }
        }
        if (isActionY || isActionX)
        {
            isActionFindMatches = true;
        }
    }

    private async Task UpdateGrid(List<(Button button, Vector2Int position)> matches)
    {
        isSwapping = true;
        float time = 0;
        float inNextAction = 0;
        var mainXValues = matches.Select(m => m.position.x).Distinct().ToList();
        var allYForXValues = mainXValues.Select(x => new
        {
            X = x,
            MaxY = matches.Where(m => m.position.x == x).Max(m => m.position.y),
            MinY = matches.Where(m => m.position.x == x).Min(m => m.position.y)

        }).ToList();

        var sortedYForXValues = allYForXValues.OrderBy(item => item.X).ToList();
        foreach (var obj in sortedYForXValues)
        {
            int posX = obj.X;
            int upperPosY = obj.MaxY;
            int lowestPosY = obj.MinY;
            int heightReplace = upperPosY - lowestPosY + 1;
            GameObject replaceButton = null;
            GameObject remainsButton = null;
            float duration = heightReplace * 0.2f;
            inNextAction = duration;

            GameObject tmpBlockedButton = null;
            bool isHaveBlockedInColumn = false;
            for (int replace = 0; replace < height - lowestPosY; replace++)
            {
                replaceButton = buttons[lowestPosY + replace, posX].gameObject;
                bool isHaveBtnClickHandler = true;
                if (upperPosY + replace + 1 < height)
                {
                    if (isHaveBlockedInColumn == false)
                    {
                        remainsButton = buttons[upperPosY + replace + 1, posX].gameObject;
                        if (remainsButton.GetComponent<ButtonClickHandler>() == null)
                        {
                            isHaveBtnClickHandler = false;
                            tmpBlockedButton = buttons[upperPosY + replace + 1, posX].gameObject;
                        }
                    }
                }
                else
                {
                    remainsButton = null;
                }

                if (isHaveBtnClickHandler == false)
                {
                    isHaveBlockedInColumn = true;
                }
                //анимация и перестаноака элемета 
                if (isHaveBlockedInColumn != true && remainsButton != null)
                {
                    TileIdentifier tileIdentifier = remainsButton.GetComponent<TileIdentifier>();

                    replaceButton.transform.GetChild(0).GetComponent<Image>().sprite = remainsButton.transform.GetChild(0).GetComponent<Image>().sprite;
                    replaceButton.GetComponent<TileIdentifier>().identifier = tileIdentifier.identifier;
                    replaceButton.GetComponent<TileIdentifier>().mainId = tileIdentifier.mainId;

                    GameObject buttonObject = replaceButton.gameObject;
                    Button button = buttonObject.GetComponent<Button>();
                    if (buttonObject.GetComponent<ButtonClickHandler>() == null)
                    {
                        ButtonClickHandler clickHandler = buttonObject.AddComponent<ButtonClickHandler>();
                        button.onClick.AddListener(() => clickHandler.OnButtonClick(button));
                    }

                    RectTransform replaceBtnPos = replaceButton.transform.Find("Icon").GetComponent<RectTransform>();
                    Vector2 startPos = replaceBtnPos.anchoredPosition;
                    startPos.y = heightReplace * 100;
                    replaceBtnPos.anchoredPosition = startPos;

                    StartCoroutine(gridAnimation.AnimateUpdategrid(replaceBtnPos, time, duration));
                    time += 0.025f;
                }
                //анимация и создание элемета 
                if (remainsButton == null || isHaveBlockedInColumn == true)
                {
                    if (replaceButton == tmpBlockedButton)
                    {
                        break;
                    }
                    Button button = replaceButton.GetComponent<Button>();
                    int spriteIndex = Random.Range(0, icons.Length);
                    replaceButton.transform.GetChild(0).GetComponent<Image>().sprite = icons[spriteIndex];

                    TileIdentifier tileIdentifier = replaceButton.GetComponent<TileIdentifier>();
                    tileIdentifier.identifier = spriteIndex;
                    tileIdentifier.mainId = spriteIndex;

                    if (replaceButton.GetComponent<ButtonClickHandler>() == null)
                    {
                        ButtonClickHandler clickHandler = replaceButton.AddComponent<ButtonClickHandler>();
                        button.onClick.AddListener(() => clickHandler.OnButtonClick(button));
                    }
                    RectTransform replaceBtnPos = replaceButton.transform.Find("Icon").GetComponent<RectTransform>();
                    replaceBtnPos.anchoredPosition = new Vector2(0, heightReplace * 100);

                    StartCoroutine(gridAnimation.AnimateUpdategrid(replaceBtnPos, time, duration));
                    time += 0.025f;
                }
            }
        }
        await Task.Delay((int)(inNextAction * 1000) + 200);
        if (isActionFindMatches)
        {
            await FindMatches();
        }
        isSwapping = false;
        isActionFindMatches = true;
    }

    private bool isSwapping = false;
    public async Task SwapButtons(Button button1, Button button2)
    {
        if (isSwapping) return;
        List<(Button button, Vector2Int position)> resaultMatches = new List<(Button button, Vector2Int position)>();
        isSwapping = true;
        if (IsDifferentIcon(button1, button2))
        {
            Sprite tempSprite = button1.transform.Find("Icon").GetComponent<Image>().sprite;
            int? tempId = button1.GetComponent<TileIdentifier>().identifier;
            int? tempMainId = button1.GetComponent<TileIdentifier>().mainId;

            button1.GetComponent<TileIdentifier>().identifier = button2.GetComponent<TileIdentifier>().identifier;
            button2.GetComponent<TileIdentifier>().identifier = tempId;

            button1.GetComponent<TileIdentifier>().mainId = button2.GetComponent<TileIdentifier>().mainId;
            button2.GetComponent<TileIdentifier>().mainId = tempMainId;

            RectTransform iconRectTransform1 = button1.transform.Find("Icon").GetComponent<RectTransform>();
            RectTransform iconRectTransform2 = button2.transform.Find("Icon").GetComponent<RectTransform>();

            button1.transform.Find("Icon").GetComponent<Image>().sprite = button2.transform.Find("Icon").GetComponent<Image>().sprite;
            button2.transform.Find("Icon").GetComponent<Image>().sprite = tempSprite;

            StartCoroutine(gridAnimation.AnimateSwap(iconRectTransform2, iconRectTransform1, 0.3f));
            await Task.Delay(300);

            List<(Button button, Vector2Int position)> checkBtn1 = CheckMatchesOfBtn1(button1);
            List<(Button button, Vector2Int position)> checkBtn2 = CheckMatchesOfBtn1(button2);
            if (checkBtn1 != null)
            {
                resaultMatches.AddRange(checkBtn1);
            }
            if (checkBtn2 != null)
            {
                resaultMatches.AddRange(checkBtn2);
            }
            if (checkBtn1 != null || checkBtn2 != null)
            {
                await UpdateGrid(resaultMatches);
            }
            if (checkBtn1 == null && checkBtn2 == null)
            {
                Sprite tempSpriteRec = button1.transform.Find("Icon").GetComponent<Image>().sprite;
                int? tempIdRec = button1.GetComponent<TileIdentifier>().identifier;
                int? tempMainIdRec = button1.GetComponent<TileIdentifier>().mainId;

                button1.GetComponent<TileIdentifier>().identifier = button2.GetComponent<TileIdentifier>().identifier;
                button2.GetComponent<TileIdentifier>().identifier = tempIdRec;

                button1.GetComponent<TileIdentifier>().mainId = button2.GetComponent<TileIdentifier>().mainId;
                button2.GetComponent<TileIdentifier>().mainId = tempMainIdRec;

                RectTransform iconRectTransform1Rec = button1.transform.Find("Icon").GetComponent<RectTransform>();
                RectTransform iconRectTransform2Rec = button2.transform.Find("Icon").GetComponent<RectTransform>();

                button1.transform.Find("Icon").GetComponent<Image>().sprite = button2.transform.Find("Icon").GetComponent<Image>().sprite;
                button2.transform.Find("Icon").GetComponent<Image>().sprite = tempSpriteRec;

                StartCoroutine(gridAnimation.AnimateSwap(iconRectTransform2Rec, iconRectTransform1Rec, 0.2f));
                await Task.Delay(300);
            }
        }
        isSwapping = false;
    }

    private bool IsDifferentIcon(Button button1, Button button2)
    {
        TileIdentifier tileIdentifier1 = button1.GetComponent<TileIdentifier>();
        TileIdentifier tileIdentifier2 = button2.GetComponent<TileIdentifier>();
        return tileIdentifier1.identifier != tileIdentifier2.identifier;
    }

    private bool CheckEqualButton(Button buttonClicked, Button buttonCheck)
    {
        int? tileIdentifier1 = buttonClicked?.GetComponent<TileIdentifier>().identifier;
        int? tileIdentifier2 = buttonCheck?.GetComponent<TileIdentifier>().identifier;
        return tileIdentifier2 != null && (tileIdentifier1 == tileIdentifier2);
    }

    public bool AreNeighbors(Button button1, Button button2)
    {
        Vector2Int pos1 = GetButtonPosition(button1);
        Vector2Int pos2 = GetButtonPosition(button2);
        return (Mathf.Abs(pos1.y - pos2.y) == 1 && pos1.x == pos2.x) ||
               (Mathf.Abs(pos1.x - pos2.x) == 1 && pos1.y == pos2.y);
    }

    private Vector2Int GetButtonPosition(Button button)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (buttons[y, x] == button)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.zero;
    }
}

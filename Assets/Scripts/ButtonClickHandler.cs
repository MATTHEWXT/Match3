using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ButtonClickHandler : MonoBehaviour
{
    private static Button previousButton;
    private GridManager gridManager;
    private GridAnimation gridAnimation = new GridAnimation();
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        gridManager.Initialize(gridAnimation);
    }
    public async void OnButtonClick(Button clickedButton)
    {
        if (previousButton == null)
        {
            previousButton = clickedButton;
            return;
        }
        if (gridManager.AreNeighbors(previousButton, clickedButton))
        {
           await gridManager.SwapButtons(previousButton, clickedButton);
            previousButton = null;
        }
        else
        {
            previousButton = clickedButton;
        }
    }
}

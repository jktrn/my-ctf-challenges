using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RequestClasses;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI crystalText;
    public Button onePullButton;
    public Button tenPullButton;
    public Canvas splashArtCanvas;
    public Image splashArtImage;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI nameText;

    private GameState gameState;
    private GachaManager gachaManager;

    void Start()
    {
        gameState = FindObjectOfType<GameState>();
        gachaManager = FindObjectOfType<GachaManager>();
        UpdateUI();

        onePullButton.onClick.AddListener(() => gachaManager.OnPullButtonClick(100, 1));
        tenPullButton.onClick.AddListener(() => gachaManager.OnPullButtonClick(1000, 10));
    }

    public void UpdateUI()
    {
        crystalText.text = $"Crystals: {gameState.crystals}";
    }

    public IEnumerator DisplaySplashArt(Character[] characters)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            DisplayCharacter(characters[i]);
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }

        splashArtCanvas.gameObject.SetActive(false);
    }

    void DisplayCharacter(Character character)
    {
        rarityText.text = character.rarity;
        cardNameText.text = character.cardName;
        nameText.text = character.name;

        string imagePath = "Gacha/" + character.splashArt;
        Texture2D texture = Resources.Load<Texture2D>(imagePath);
        splashArtImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RequestClasses;

public class UIManager : MonoBehaviour
{
    [Header("Main Gacha")]
    public TextMeshProUGUI crystalText;
    public Button onePullButton;
    public Button tenPullButton;

    [Header("Splash Art")]
    public Canvas splashArtCanvas;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI nameText;
    public Image splashArtImage;
    public Button skipButton;

    [Header("Gacha Overview")]
    public Canvas gachaOverviewCanvas;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject avatarPrefab;
    public Button nextButton;

    private GameState gameState;
    private GachaManager gachaManager;
    private List<GameObject> avatarObjects = new List<GameObject>();

    void Start()
    {
        gameState = FindObjectOfType<GameState>();
        gachaManager = FindObjectOfType<GachaManager>();
        UpdateUI();

        onePullButton.onClick.AddListener(() => OnPullButtonClick(100, 1));
        tenPullButton.onClick.AddListener(() => OnPullButtonClick(1000, 10));
        skipButton.onClick.AddListener(OnSkipButtonClick);
        nextButton.onClick.AddListener(OnNextButtonClick);
    }

    public void UpdateUI()
    {
        crystalText.text = $"Crystals: {gameState.crystals}";
    }

    public void OnPullButtonClick(int cost, int numPulls)
    {
        if (gameState.crystals < cost)
        {
            Debug.Log("Not enough crystals!");
            return;
        }

        StartCoroutine(gachaManager.SendGachaRequest(numPulls));
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
        DisplayAvatars(characters);
        gachaOverviewCanvas.gameObject.SetActive(true);
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

    void DisplayAvatars(Character[] characters)
    {
        foreach (GameObject avatarObject in avatarObjects)
        {
            Destroy(avatarObject);
        }
        avatarObjects.Clear();

        foreach (Character character in characters)
        {
            GameObject avatarObject = Instantiate(avatarPrefab, gridLayoutGroup.transform);
            avatarObjects.Add(avatarObject);

            Image avatarImage = avatarObject.GetComponent<Image>();
            string imagePath = "Gacha/" + character.avatar;
            Texture2D texture = Resources.Load<Texture2D>(imagePath);
            avatarImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    void OnSkipButtonClick()
    {
        StopAllCoroutines();
        splashArtCanvas.gameObject.SetActive(false);
        DisplayAvatars(gachaManager.lastGachaResponse.characters);
        gachaOverviewCanvas.gameObject.SetActive(true);
    }

    void OnNextButtonClick()
    {
        gachaOverviewCanvas.gameObject.SetActive(false);
    }
}
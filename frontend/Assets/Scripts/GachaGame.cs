using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class GachaGame : MonoBehaviour
{
    public TextMeshProUGUI crystalText;
    public Button onePullButton;
    public Button tenPullButton;
    public Canvas splashArtCanvas;
    public Image splashArtImage;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI nameText;
    public Button skipButton;
    public GridLayoutGroup overviewGrid;

    private const int OnePullCost = 100;
    private const int TenPullCost = 1000;

    public int crystals = 1000;
    private int pulls = 0;

    private Character[] pulledCharacters;

    void Start()
    {
        UpdateUI();
        onePullButton.onClick.AddListener(() => OnPullButtonClick(OnePullCost, 1));
        tenPullButton.onClick.AddListener(() => OnPullButtonClick(TenPullCost, 10));
        skipButton.onClick.AddListener(OnSkipButtonClick);
    }

    void UpdateUI()
    {
        crystalText.text = $"Crystals: {crystals}";
    }

    void OnPullButtonClick(int cost, int numPulls)
    {
        if (crystals < cost)
        {
            Debug.Log("Not enough crystals!");
            return;
        }

        StartCoroutine(SendGachaRequest(numPulls));
    }

    IEnumerator SendGachaRequest(int numPulls)
    {
        string json = JsonUtility.ToJson(new GachaRequest(crystals, pulls, numPulls));
        UnityWebRequest request = new UnityWebRequest("http://localhost:3000/gacha", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("User-Agent", "SekaiCTF");

        yield return request.SendWebRequest();
        request.uploadHandler.Dispose();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            GachaResponse response = JsonUtility.FromJson<GachaResponse>(request.downloadHandler.text);

            if (response.flag != null)
            {
                Debug.Log($"Flag: {response.flag}");
            }

            crystals -= numPulls == 1 ? OnePullCost : TenPullCost;
            pulls += numPulls;
            UpdateUI();

            pulledCharacters = response.characters;
            splashArtCanvas.gameObject.SetActive(true);
            StartCoroutine(DisplaySplashArt(pulledCharacters));
        }
        else
        {
            Debug.Log($"Error: {request.error}");
        }
    }

    IEnumerator DisplaySplashArt(Character[] characters)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            Character character = characters[i];
            rarityText.text = character.rarity;
            cardNameText.text = character.cardName;
            nameText.text = character.name;

            string imagePath = "Gacha/" + character.splashArt;
            Texture2D texture = Resources.Load<Texture2D>(imagePath);
            splashArtImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }

        splashArtCanvas.gameObject.SetActive(false);
    }

    // void DisplayOverview(Character[] characters)
    // {
    //     for (int i = 0; i < characters.Length; i++)
    //     {
    //         Character character = characters[i];
    //         GameObject characterAvatar = new GameObject();
    //         Image avatarImage = characterAvatar.AddComponent<Image>();

    //         StartCoroutine(LoadImageFromUrl(character.avatar, (sprite) => {
    //             avatarImage.sprite = sprite;
    //         }));

    //         characterAvatar.transform.SetParent(overviewGrid.transform);
    //     }
    // }

    void OnSkipButtonClick()
    {
        StopAllCoroutines();
        splashArtCanvas.gameObject.SetActive(false);
        // DisplayOverview(pulledCharacters);
    }
}

[System.Serializable]
public class GachaRequest
{
    public int crystals;
    public int pulls;
    public int numPulls;

    public GachaRequest(int crystals, int pulls, int numPulls)
    {
        this.crystals = crystals;
        this.pulls = pulls;
        this.numPulls = numPulls;
    }
}

[System.Serializable]
public class GachaResponse
{
    public Character[] characters;
    public string flag;
}

[System.Serializable]
public class Character
{
    public string name;
    public string cardName;
    public string rarity;
    public string attribute;
    public string splashArt;
    public string avatar;
}

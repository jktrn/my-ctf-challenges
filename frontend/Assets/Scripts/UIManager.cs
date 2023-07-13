using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RequestClasses;
using UnityEngine.EventSystems;

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
    public Image splashArtBackground;
    public Image splashArtImage;
    public Image splashArtImageMask;
    public Image silhouetteImage;
    public RawImage movingTriangles;
    public Button skipButton;

    [Header("Gacha Overview")]
    public Canvas gachaOverviewCanvas;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject avatarPrefab;
    public Button nextButton;
    public Button returnButton;
    public Button pullAgainButton;

    private GameState gameState;
    private GachaManager gachaManager;
    private Animator animator;
    private List<GameObject> avatarObjects = new List<GameObject>();
    private bool skipClicked = false;

    void Start()
    {
        gameState = FindObjectOfType<GameState>();
        gachaManager = FindObjectOfType<GachaManager>();
        animator = GetComponent<Animator>();
        UpdateUI();

        onePullButton.onClick.AddListener(() => OnPullButtonClick(100, 1));
        tenPullButton.onClick.AddListener(() => OnPullButtonClick(1000, 10));
        skipButton.onClick.AddListener(OnSkipButtonClick);
        nextButton.onClick.AddListener(OnNextButtonClick);
        returnButton.onClick.AddListener(OnNextButtonClick);
        pullAgainButton.onClick.AddListener(OnPullAgainButtonClick);
    }

    public void UpdateUI()
    {
        crystalText.text = gameState.crystals.ToString();
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
            if (skipClicked)
            {
                skipClicked = false;
                break;
            }

            StartCoroutine(DisplayCharacter(characters[i]));
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() =>
            {
                if (skipClicked)
                {
                    return true;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current.currentSelectedGameObject != skipButton.gameObject)
                    {
                        return true;
                    }
                }
                return false;
            });

            if (!skipClicked)
            {
                AudioController.Instance.PlaySFX("Touch");
            }
        }

        DisplayGachaOverview();
    }

    IEnumerator DisplayCharacter(Character character)
    {
        UpdateCharacterText(character);

        var silhouetteImageAnimation = silhouetteImage.GetComponent<Animation>();
        var splashArtBackgroundAnimation = splashArtBackground.GetComponent<Animation>();
        var splashArtImageMaskAnimation = splashArtImageMask.GetComponent<Animation>();
        var movingTrianglesAnimation = movingTriangles.GetComponent<Animation>();

        ResetAnimations();
        UpdateSplashArtImage(character.splashArt);
        UpdateSilhouetteImage(character.splashArt);

        silhouetteImageAnimation.Play();
        yield return new WaitForSeconds(silhouetteImageAnimation.clip.length);
        StartCoroutine(FadeMaterial(splashArtBackground.material, 0.5f));
        movingTrianglesAnimation.Play();
        splashArtBackgroundAnimation.Play();
        splashArtImageMaskAnimation.Play();
    }

    IEnumerator FadeMaterial(Material material, float duration)
    {
        float startValue = material.GetFloat("_GrayscaleAmount");

        float targetValue = 0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float value = Mathf.Lerp(startValue, targetValue, t / duration);
            material.SetFloat("_GrayscaleAmount", value);
            yield return null;
        }

        material.SetFloat("_GrayscaleAmount", targetValue);
    }

    void UpdateCharacterText(Character character)
    {
        rarityText.text = character.rarity;
        cardNameText.text = character.cardName;
        nameText.text = character.name;
    }

    void ResetAnimations()
    {
        RectTransform splashArtImageMaskRectTransform = splashArtImageMask.GetComponent<RectTransform>();
        splashArtImageMaskRectTransform.sizeDelta = new Vector2(splashArtImageMaskRectTransform.sizeDelta.x, 0);
        splashArtBackground.material.SetFloat("_GrayscaleAmount", 1f);

        var movingTrianglesImage = movingTriangles.GetComponent<RawImage>();
        Color movingTrianglesColor = movingTrianglesImage.color;
        movingTrianglesColor.a = 0;
        movingTrianglesImage.color = movingTrianglesColor;
    }

    void UpdateSplashArtImage(string splashArt)
    {
        string imagePath = "Gacha/" + splashArt;
        Texture2D texture = Resources.Load<Texture2D>(imagePath);
        splashArtImage.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    void UpdateSilhouetteImage(string splashArt)
    {
        string imagePath = "Gacha/" + splashArt;
        Texture2D texture = Resources.Load<Texture2D>(imagePath);

        Texture2D silhouetteTexture = new Texture2D(texture.width, texture.height);
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a > 0)
            {
                pixels[i] = new Color(1, 103 / 255f, 154 / 255f, pixels[i].a);
            }
        }
        silhouetteTexture.SetPixels(pixels);
        silhouetteTexture.Apply();
        silhouetteImage.sprite = Sprite.Create(
            silhouetteTexture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    void DisplayGachaOverview()
    {
        splashArtCanvas.gameObject.SetActive(false);
        AudioController.Instance.FadeMusic("Gacha");
        DisplayAvatars(gachaManager.lastGachaResponse.characters);
        gachaOverviewCanvas.gameObject.SetActive(true);
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
        skipClicked = true;
        StopAllCoroutines();
        DisplayGachaOverview();
    }

    void OnNextButtonClick()
    {
        gachaOverviewCanvas.gameObject.SetActive(false);
    }

    void OnPullAgainButtonClick()
    {
        StartCoroutine(gachaManager.SendGachaRequest(10));
        gachaOverviewCanvas.gameObject.SetActive(false);
    }
}
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
    public Image backgroundTint;
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
    private bool isAnimating = false;

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
                    if (isAnimating)
                    {
                        StopAllCoroutines();
                        StartCoroutine(DisplayCharacterInstantly(characters[i]));
                        return false;
                    }

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
        UpdateSplashArtImage(character.splashArt);
        UpdateSilhouetteImage(character.splashArt);
        ResetAnimations();

        var silhouetteImageAnimation = silhouetteImage.GetComponent<Animation>();
        var splashArtBackgroundAnimation = splashArtBackground.GetComponent<Animation>();
        var backgroundTintAnimation = backgroundTint.GetComponent<Animation>();
        var splashArtImageMaskAnimation = splashArtImageMask.GetComponent<Animation>();
        var movingTrianglesAnimation = movingTriangles.GetComponent<Animation>();

        isAnimating = true;
        silhouetteImageAnimation.Play();
        splashArtBackgroundAnimation.Play();

        yield return new WaitForSeconds(silhouetteImageAnimation.clip.length);

        movingTrianglesAnimation.Play();
        splashArtImageMaskAnimation.Play();

        yield return new WaitForSeconds(1.5f);

        backgroundTintAnimation.Play();

        yield return new WaitUntil(
            () =>
                !splashArtBackgroundAnimation.isPlaying
                && !splashArtImageMaskAnimation.isPlaying
                && !movingTrianglesAnimation.isPlaying
        );

        isAnimating = false;
    }

    IEnumerator DisplayCharacterInstantly(Character character)
    {
        UpdateCharacterText(character);
        UpdateSplashArtImage(character.splashArt);
        UpdateSilhouetteImage(character.splashArt);

        var splashArtBackgroundAnimation = splashArtBackground.GetComponent<Animation>();
        RectTransform splashArtImageMaskRectTransform =
            splashArtImageMask.GetComponent<RectTransform>();

        SetImageOpacity(silhouetteImage.GetComponent<Image>(), 1f);
        SetImageOpacity(backgroundTint.GetComponent<Image>(), 0.6f);
        SetImageOpacity(movingTriangles.GetComponent<RawImage>(), 0.6f);

        splashArtImageMaskRectTransform.sizeDelta = new Vector2(
            splashArtImageMaskRectTransform.sizeDelta.x,
            1207.6f
        );

        splashArtBackgroundAnimation["SplashArtBackground"].normalizedTime = 1f;

        isAnimating = false;
        yield break;
    }

    void UpdateCharacterText(Character character)
    {
        rarityText.text = character.rarity;
        cardNameText.text = character.cardName;
        nameText.text = character.name;
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
            avatarImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }

    void SetImageOpacity(MaskableGraphic image, float targetOpacity)
    {
        Color imageColor = image.color;
        imageColor.a = targetOpacity;
        image.color = imageColor;
    }

    void DisplayGachaOverview()
    {
        splashArtCanvas.gameObject.SetActive(false);
        AudioController.Instance.FadeMusic("Gacha");
        DisplayAvatars(gachaManager.lastGachaResponse.characters);
        gachaOverviewCanvas.gameObject.SetActive(true);
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

    void ResetAnimations()
    {
        RectTransform splashArtImageMaskRectTransform =
            splashArtImageMask.GetComponent<RectTransform>();
        splashArtImageMaskRectTransform.sizeDelta = new Vector2(
            splashArtImageMaskRectTransform.sizeDelta.x,
            0.0f
        );

        SetImageOpacity(silhouetteImage.GetComponent<Image>(), 0f);
        SetImageOpacity(backgroundTint.GetComponent<Image>(), 0f);
        SetImageOpacity(movingTriangles.GetComponent<RawImage>(), 0f);
    }
}

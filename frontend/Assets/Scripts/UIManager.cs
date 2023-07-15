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
    public Image threeStarSplash;
    public Image silhouetteImage;
    public Image namecardMask;
    public Image flagImage;
    public GameObject stars;
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

            if (characters[i].rarity == "4*")
            {
                StartCoroutine(DisplayFourStarCharacter(characters[i]));
            }
            else if (characters[i].rarity == "3*")
            {
                StartCoroutine(DisplayThreeStarCharacter(characters[i]));
            }
            else
            {
                StartCoroutine(DisplayTwoStarCharacter(characters[i]));
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => CheckForSkipOrClick(characters, i));

            if (!skipClicked)
            {
                AudioController.Instance.PlaySFX("Touch");
            }
        }

        DisplayGachaOverview();
    }

    private IEnumerator DisplayTwoStarCharacter(Character character)
    {
        ResetAnimations();
        UpdateCharacterText(character);
        UpdateSplashArtImage(character.splashArt, splashArtImage);
        UpdateSilhouetteImage(character.splashArt);

        Animation silhouetteImageAnimation = silhouetteImage.GetComponent<Animation>();
        Animation splashArtBackgroundAnimation = splashArtBackground.GetComponent<Animation>();
        Animation backgroundTintAnimation = backgroundTint.GetComponent<Animation>();
        Animation splashArtImageMaskAnimation = splashArtImageMask.GetComponent<Animation>();
        Animation movingTrianglesAnimation = movingTriangles.GetComponent<Animation>();
        Animation starsAnimation = stars.GetComponent<Animation>();
        Animation namecardMaskAnimation = namecardMask.GetComponent<Animation>();

        isAnimating = true;
        silhouetteImageAnimation.Play();
        splashArtBackgroundAnimation.Play();

        yield return new WaitForSeconds(1f);

        movingTrianglesAnimation.Play();
        splashArtImageMaskAnimation.Play();
        starsAnimation.Play("TwoStarRarity");

        yield return new WaitForSeconds(0.5f);

        namecardMaskAnimation.Play();

        yield return new WaitForSeconds(1f);

        backgroundTintAnimation.Play();

        yield return new WaitUntil(
            () =>
                !splashArtBackgroundAnimation.isPlaying
                && !splashArtImageMaskAnimation.isPlaying
                && !movingTrianglesAnimation.isPlaying
        );

        isAnimating = false;
    }

    private IEnumerator DisplayTwoStarCharacterInstant(Character character)
    {
        UpdateCharacterText(character);
        UpdateSplashArtImage(character.splashArt, splashArtImage);
        UpdateSilhouetteImage(character.splashArt);

        SkipAnimation(silhouetteImage.gameObject, "SilhouetteImage", 3f);
        SkipAnimation(splashArtBackground.gameObject, "SplashArtBackground");
        SkipAnimation(backgroundTint.gameObject, "BackgroundTint");
        SkipAnimation(splashArtImageMask.gameObject, "SplashImage");
        SkipAnimation(movingTriangles.gameObject, "MovingTriangles");
        SkipAnimation(stars, "TwoStarRarity");
        SkipAnimation(namecardMask.gameObject, "Namecard");

        isAnimating = false;
        yield break;
    }

    private IEnumerator DisplayThreeStarCharacter(Character character)
    {
        ResetAnimations();
        UpdateCharacterText(character);
        UpdateSplashArtImage(character.splashArt, threeStarSplash);
        threeStarSplash.gameObject.SetActive(true);

        Animation threeStarSplashAnimation = threeStarSplash.GetComponent<Animation>();
        Animation starsAnimation = stars.GetComponent<Animation>();
        Animation namecardMaskAnimation = namecardMask.GetComponent<Animation>();

        isAnimating = true;
        threeStarSplashAnimation.Play();

        yield return new WaitForSeconds(1f);

        starsAnimation.Play("ThreeStarRarity");

        yield return new WaitForSeconds(1f);

        namecardMaskAnimation.Play();

        yield return new WaitUntil(() => !threeStarSplashAnimation.isPlaying);

        isAnimating = false;
        yield break;
    }

    private IEnumerator DisplayThreeStarCharacterInstant(Character character)
    {
        UpdateCharacterText(character);
        UpdateSplashArtImage(character.splashArt, threeStarSplash);
        threeStarSplash.gameObject.SetActive(true);

        SkipAnimation(threeStarSplash.gameObject, "ThreeStarSplash");
        SkipAnimation(stars, "ThreeStarRarity");
        SkipAnimation(namecardMask.gameObject, "Namecard");

        isAnimating = false;
        yield break;
    }

    private IEnumerator DisplayFourStarCharacter(Character character)
    {
        ResetAnimations();
        UpdateCharacterText(character);
        UpdateSplashArtImage(character.splashArt, threeStarSplash);
        threeStarSplash.gameObject.SetActive(true);

        Animation threeStarSplashAnimation = threeStarSplash.GetComponent<Animation>();
        Animation starsAnimation = stars.GetComponent<Animation>();
        Animation namecardMaskAnimation = namecardMask.GetComponent<Animation>();

        string flag = gachaManager.lastGachaResponse.flag;

        if (flag != null)
        {
            byte[] imageBytes = System.Convert.FromBase64String(flag);
            Texture2D flagTexture = new Texture2D(2, 2);
            flagTexture.LoadImage(imageBytes);

            Rect rect = new Rect(0, 0, flagTexture.width, flagTexture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite flagSprite = Sprite.Create(flagTexture, rect, pivot);

            flagImage.sprite = flagSprite;
        }

        isAnimating = true;
        threeStarSplashAnimation.Play();

        yield return new WaitForSeconds(1f);

        starsAnimation.Play("ThreeStarRarity");

        yield return new WaitForSeconds(1f);

        flagImage.gameObject.SetActive(true);
        namecardMaskAnimation.Play();

        yield return new WaitUntil(() => !threeStarSplashAnimation.isPlaying);

        isAnimating = false;
        yield break;
    }

    private IEnumerator DisplayFourStarCharacterInstant(Character character)
    {
        ResetAnimations();
        UpdateCharacterText(character);
        UpdateSplashArtImage(character.splashArt, threeStarSplash);
        threeStarSplash.gameObject.SetActive(true);

        SkipAnimation(threeStarSplash.gameObject, "ThreeStarSplash");
        SkipAnimation(stars, "ThreeStarRarity");
        SkipAnimation(namecardMask.gameObject, "Namecard");

        isAnimating = false;
        yield break;
    }

    private void UpdateCharacterText(Character character)
    {
        cardNameText.text = character.cardName;
    }

    private void UpdateSplashArtImage(string splashArt, Image image)
    {
        string imagePath = "Gacha/" + splashArt;
        Texture2D texture = Resources.Load<Texture2D>(imagePath);
        image.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    private void UpdateSilhouetteImage(string splashArt)
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

    private void DisplayAvatars(Character[] characters)
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

    private void SkipAnimation(
        GameObject gameObject,
        string clipName,
        float startTime = float.MaxValue
    )
    {
        Animation animation = gameObject.GetComponent<Animation>();
        animation.Play(clipName);
        animation[clipName].time =
            startTime == float.MaxValue ? animation[clipName].clip.length : startTime;
    }

    private void SetImageOpacity(MaskableGraphic image, float targetOpacity)
    {
        Color imageColor = image.color;
        imageColor.a = targetOpacity;
        image.color = imageColor;
    }

    private void DisplayGachaOverview()
    {
        splashArtCanvas.gameObject.SetActive(false);
        AudioController.Instance.FadeMusic("Gacha");
        DisplayAvatars(gachaManager.lastGachaResponse.characters);
        gachaOverviewCanvas.gameObject.SetActive(true);
    }

    private void OnSkipButtonClick()
    {
        skipClicked = true;
        StopAllCoroutines();
        ResetAnimations();
        DisplayGachaOverview();
    }

    private void OnNextButtonClick()
    {
        gachaOverviewCanvas.gameObject.SetActive(false);
    }

    private void OnPullAgainButtonClick()
    {
        StartCoroutine(gachaManager.SendGachaRequest(10));
        StartCoroutine(SetInactiveAfterDelay());
    }

    private IEnumerator SetInactiveAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        gachaOverviewCanvas.gameObject.SetActive(false);
    }

    private void ResetAnimations()
    {
        Animation silhouetteImageAnimation = silhouetteImage.GetComponent<Animation>();
        Animation starsAnimation = stars.GetComponent<Animation>();
        silhouetteImageAnimation.Stop();
        starsAnimation.Stop();

        RectTransform splashArtImageMaskRectTransform =
            splashArtImageMask.GetComponent<RectTransform>();
        splashArtImageMaskRectTransform.sizeDelta = new Vector2(
            splashArtImageMaskRectTransform.sizeDelta.x,
            0.0f
        );

        RectTransform namecardMaskRectTransform = namecardMask.GetComponent<RectTransform>();
        namecardMaskRectTransform.sizeDelta = new Vector2(
            namecardMaskRectTransform.sizeDelta.x,
            0.0f
        );

        SetImageOpacity(silhouetteImage.GetComponent<Image>(), 0f);
        SetImageOpacity(backgroundTint.GetComponent<Image>(), 0f);
        SetImageOpacity(movingTriangles.GetComponent<RawImage>(), 0f);

        foreach (Transform child in stars.transform)
        {
            Image childImage = child.GetComponent<Image>();
            SetImageOpacity(childImage, 0f);
        }

        flagImage.gameObject.SetActive(false);
    }

    private bool CheckForSkipOrClick(Character[] characters, int index)
    {
        if (skipClicked)
        {
            threeStarSplash.gameObject.SetActive(false);
            return true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (isAnimating)
            {
                StopAllCoroutines();
                if (characters[index].rarity == "4*")
                {
                    StartCoroutine(DisplayFourStarCharacterInstant(characters[index]));
                }
                else if (characters[index].rarity == "3*")
                {
                    StartCoroutine(DisplayThreeStarCharacterInstant(characters[index]));
                }
                else
                {
                    StartCoroutine(DisplayTwoStarCharacterInstant(characters[index]));
                }
                return false;
            }

            if (EventSystem.current.currentSelectedGameObject != skipButton.gameObject)
            {
                if (characters[index].rarity == "3*" || characters[index].rarity == "4*")
                {
                    threeStarSplash.gameObject.SetActive(false);
                }

                return true;
            }
        }
        return false;
    }
}

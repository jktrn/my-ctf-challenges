using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using RequestClasses;

public class GachaManager : MonoBehaviour
{
    public UIManager uiManager;
    public GachaResponse lastGachaResponse;

    private GameState gameState;

    void Start()
    {
        gameState = FindObjectOfType<GameState>();
    }

    public void OnPullButtonClick(int cost, int numPulls)
    {
        if (gameState.crystals < cost)
        {
            Debug.Log("Not enough crystals!");
            return;
        }

        StartCoroutine(SendGachaRequest(numPulls));
    }

    public IEnumerator SendGachaRequest(int numPulls)
    {
        GachaRequest gachaRequest = new GachaRequest(gameState.crystals, gameState.pulls, numPulls);
        string json = JsonUtility.ToJson(gachaRequest);

        using (UnityWebRequest request = CreateGachaWebRequest(json))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleGachaResponse(request.downloadHandler.text, numPulls);
                GachaResponse response = JsonUtility.FromJson<GachaResponse>(
                    request.downloadHandler.text
                );
                StartCoroutine(uiManager.DisplaySplashArt(response.characters));
            }
            else
            {
                uiManager.GenericModalHandler(
                    uiManager.failedConnectionModal,
                    uiManager.failedConnectionModalCloseButton
                );
            }
        }
    }

    UnityWebRequest CreateGachaWebRequest(string json)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest("http://localhost:3000/gacha", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("User-Agent", "SekaiCTF");

        return request;
    }

    void HandleGachaResponse(string responseText, int numPulls)
    {
        GachaResponse response = JsonUtility.FromJson<GachaResponse>(responseText);
        lastGachaResponse = response;

        gameState.SpendCrystals(numPulls);
        uiManager.UpdateUI();
        uiManager.splashArtCanvas.gameObject.SetActive(true);
        AudioController.Instance.FadeMusic("Pulling");
    }
}

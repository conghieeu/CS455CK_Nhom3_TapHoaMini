 using UnityEngine;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
    [SerializeField] Button _btnNewGame;
    [SerializeField] Button _btnTiepTuc; 

    DataManager _SAE => DataManager.Instance;
    ScenesManager scenesManager;

    private void Start()
    {  
        _btnTiepTuc.gameObject.SetActive(_SAE.GameData._gamePlayData.IsInitialized);
        _btnNewGame.onClick.AddListener(OnClickNewGame);
        scenesManager = ScenesManager.Instance;
    }

    public void OnClickNewGame()
    {
        _SAE.OnStartNewGame();
        scenesManager.LoadSceneDemo();

    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller of the data and the views.
/// </summary>
public class DataHandler : MonoBehaviour
{
    //View with the scroll panel that contains the items.
    [SerializeField] private ItemsScrollView _scrollView;
    //iew with the progress bar that display the progress of the models download.
    [SerializeField] private ProgressBarView _progressView;
    //List of items data.
    [SerializeField] private List<Item> _itemsList = new List<Item>();
    //List of the items view.
    private List<ItemView> itemsViewList = new List<ItemView>();
    
    private JsonReader _jsonData;

    //Callback for the finish of all item data download.
    private Action ITEMS_DATA_DOWNLOADED;

    //Game object parent of the model downloaded.
    [SerializeField] private GameObject itemFBX;
    //Container dictionary of model textures.
    private Dictionary<string, Texture2D> modelTextures = new Dictionary<string, Texture2D>();

    private Shader _itemShader;

    private void OnEnable()
    {
        ITEMS_DATA_DOWNLOADED += ShowAllDataInView;
    }

    private void OnDisable()
    {
        ITEMS_DATA_DOWNLOADED -= ShowAllDataInView;
    }

    private void Start()
    {
        _jsonData = gameObject.GetComponent<JsonReader>();
        GetModelsData();
    }

    private void Update()
    {
        //Check if items list is full of data and start the callback for display de data in the view
        if (_itemsList.Count == _jsonData.UidsFromJson.uids.Length && itemsViewList.Count == 0)
            ITEMS_DATA_DOWNLOADED();
    }

    /// <summary>
    /// Get the item's data.
    /// </summary>
    private void GetModelsData()
    {
        foreach (var uid in _jsonData.UidsFromJson.uids)
        {
            StartCoroutine(LoadAndSaveData(uid));
        }
    }

    /// <summary>
    /// Coroutine that get the item data from web request and save the data into a list.
    /// </summary>
    /// <param name="uid">Item Uid that identify it.</param>
    /// <returns>Wait for the web request answer.</returns>
    private IEnumerator LoadAndSaveData(string uid)
    {
        //create the web request for download the item data.
        Item itemData = new Item();
        var webRequest = UnityWebRequest.Get("https://universal-s35ndifg4a-uc.a.run.app/model/read/" + uid);
        yield return webRequest.SendWebRequest();

        //check for any error in the web request.
        if (webRequest.result != UnityWebRequest.Result.ConnectionError ||
            webRequest.result != UnityWebRequest.Result.ProtocolError)
        {
            //Get item data from web request and save it into items list.
            itemData = JsonUtility.FromJson<Item>(webRequest.downloadHandler.text);
            _itemsList.Add(itemData);
        }
        else
        {
            Debug.LogWarning(
                "A web connection error occurred while trying to obtain the data. Check the server url and try again");
        }
    }

    public void ShowAllDataInView()
    {
        foreach (Item item in _itemsList)
        {
            ShowItemDataInView(item);
        }
    }

    /// <summary>
    /// Display the item data in the view.
    /// </summary>
    /// <param name="itemData">Item data that want to display it.</param>
    public void ShowItemDataInView(Item itemData)
    {
        //Create a new item view for that item data and set the data in the view.
        ItemView newItemView = _scrollView.CreateItem();
        newItemView.SetName(itemData.name);
        newItemView.DisplayButton(IsCompleteForShow(itemData));
        itemsViewList.Add(newItemView);
        
        //Add button listener for download model
        AddButtonListener(newItemView);
        
        //Call the coroutine for get the image and display in the view.
        StartCoroutine(GetItemImage(itemData.imgArray[0], newItemView));
    }
    
    /// <summary>
    /// Coroutine that get the item image trough a web request and display the image in the item view.
    /// </summary>
    /// <param name="uri">Uri of the item image tha want to download.</param>
    /// <param name="itemViewParam">Item view where you want to display the image.</param>
    /// <returns>Wait for the web request answer.</returns>
    private IEnumerator GetItemImage(string uri, ItemView itemViewParam)
    {
        //create the web request for download the item image.
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(uri);
        yield return webRequest.SendWebRequest();
    
        //check for any error in the web request.
        if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
            webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            //Get the image in a texture file.
            Texture2D myTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
            //Create a sprite with the created texture.
            Sprite newSprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(.5f, .5f));
            //DIsplay the new sprite in the item view image.
            itemViewParam.SetImage(newSprite);
        }
    }

    /// <summary>
    /// Verify that the minimum fields required to display the 3D model are not empty.
    /// </summary>
    /// <param name="itemData">Item data that want to check.</param>
    /// <returns>True if can display the 3D model, False if any of the fields are empty.</returns>
    private bool IsCompleteForShow(Item itemData)
    {
        if (itemData.HighPoly.Model == null || itemData.HighPoly.DiffuseMap == null ||
            itemData.HighPoly.MetallicMap == null || itemData.HighPoly.NormalMap == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Adds the click listener to the button with the method for downloading the model.
    /// </summary>
    /// <param name="itemView">Item view of the article from which you want to download the model.</param>
    public void AddButtonListener(ItemView itemView)
    {
        itemView.ShowButton.onClick.AddListener(() => {DownloadModel(itemView); });
    }

    /// <summary>
    /// Search the item from wich you want to download the model and check if the item data contains the zip
    /// path and download it, if the item don't contains the zip path, download the fbx of the item and all
    /// the textures.
    /// </summary>
    /// <param name="itemView">Item view of the article from which you want to download the model.</param>
    public void DownloadModel(ItemView itemView)
    {
        Item itemData = null;
        
        //Search the item data in the list of items and compare it with the name of the item.
        foreach (Item item in _itemsList)
        {
            if (item.name == itemView.ItemName.text)
            {
                itemData = item;
            }
        }
        
        if(itemData == null)
            Debug.LogWarning("There was a problem and the selected item is not found.");

        Dictionary<string, string> downloadPath = new Dictionary<string, string>();

        //If item data don't contains the zip path for download, then download the fbx object and all the textures.
        if (itemData.zipPath != "")
        {
            //Add the model and textures to dictionary with his name of the file for download and save it.
            downloadPath.Add("Model", itemData.HighPoly.Model);
            downloadPath.Add("DiffuseMap", itemData.HighPoly.DiffuseMap);
            downloadPath.Add("MetallicMap", itemData.HighPoly.MetallicMap);
            downloadPath.Add("NormalMap", itemData.HighPoly.NormalMap);
            DownLoadTexturesAndModel(downloadPath);
        }
        else
        {
            //download de zip file and add it into a scene
            downloadPath.Add("Zip", itemData.zipPath);
            DownloadZipModel(downloadPath);
        }
    }

    /// <summary>
    /// Download the zip file of the model and add the object into a scene.
    /// </summary>
    /// <param name="pathsList">Dictionary with the paths for download and his name.</param>
    private void DownloadZipModel(Dictionary<string, string> pathsList)
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        var webRequest = AssetDownloader.CreateWebRequest(pathsList["Zip"]);
        AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, itemFBX, assetLoaderOptions);
    }
    
    /// <summary>
    /// Download the fbx object and all the textures and add it into a dictionary for next use.
    /// </summary>
    /// <param name="pathsList">Dictionary with the paths for download and his name.</param>
    private void DownLoadTexturesAndModel(Dictionary<string, string> pathsList)
    {
        //Download de fbx object.
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        var webRequest = AssetDownloader.CreateWebRequest(pathsList["Model"]);
        AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, 
            itemFBX, assetLoaderOptions, null, "fbx", false);

        //Download all the textures and add it into a dictionary.
        foreach (KeyValuePair<string, string> path in pathsList)
        {
            StartCoroutine(DownloadItemTextures(path.Key, path.Value));
        }
    }

    /// <summary>
    /// Coroutine that get the item's textures from web request and save it into a dictionary.
    /// </summary>
    /// <param name="textureName">Corresponding texture name.</param>
    /// <param name="uri">Item texture download path.</param>
    /// <returns>Wait for the web request answer.</returns>
    private IEnumerator DownloadItemTextures(string textureName, string uri)
    {
        //create the web request for download the item texture.
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(uri);
        yield return webRequest.SendWebRequest();
    
        //check for any error in the web request.
        if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
            webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            //Get the image in a texture file.
            Texture2D myTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
            modelTextures.Add(textureName, myTexture);
        }
    } 

    #region LoadModelsMethods

    /// <summary>
    /// Called when any error occurs.
    /// </summary>
    /// <param name="obj">The contextualized error, containing the original exception and the context passed to the method where the error was thrown.</param>
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
    }

    /// <summary>
    /// Called when the Model loading progress changes.
    /// </summary>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    /// <param name="progress">The loading progress.</param>
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
        float progressBarValue = Mathf.Clamp01(progress / .9f);
        if(!_progressView.gameObject.activeInHierarchy)
            _progressView.gameObject.SetActive(true);
        _progressView.UpdateBar(progressBarValue);
    }

    /// <summary>
    /// Called when the Model (including Textures and Materials) has been fully loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Materials loaded. Model fully loaded.");
        _progressView.gameObject.SetActive(false);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        GameObject itemObject = itemFBX.transform.GetChild(0).gameObject;
        SceneManager.MoveGameObjectToScene(itemObject, SceneManager.GetSceneAt(1));
    }

    /// <summary>
    /// Called when the Model Meshes and hierarchy are loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model loaded. Loading materials.");
    }

    #endregion
}

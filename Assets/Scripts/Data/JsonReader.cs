using UnityEngine;

/// <summary>
/// Class in charge of reading the json file and putting it into the respective data structure after converting it to text and separating it.
/// </summary>
public class JsonReader : MonoBehaviour
{
    [SerializeField]
    [Tooltip("This field is for selecting the .json file containing the uids of the items to search for.")]
    private TextAsset jsonFile;
    
    //List of data extracted from the json file
    private UidList uidsFromJson = new UidList();

    public UidList UidsFromJson => uidsFromJson;

    private void Start()
    {
        //Extracts and separates the data from the json file and saves it in the created list.
        uidsFromJson = JsonUtility.FromJson<UidList>(jsonFile.text);
    }
}

/// <summary>
/// The struct containing the list of uids to be extracted from the json file.
/// </summary>
public struct UidList
{
    public string[] uids;
}

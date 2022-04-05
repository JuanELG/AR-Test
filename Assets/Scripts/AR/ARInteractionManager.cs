using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Vector2 = UnityEngine.Vector2;

/// <summary>
/// Handling of AR functionalities
/// </summary>
public class ARInteractionManager : MonoBehaviour
{
    private ARRaycastManager _raycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    
    //Determines whether the item is already in the initial position or whether the item position must be initialized.
    private bool _isInitialPosition;
    //Determines if the item has already been placed on the plane.
    private bool _isPlaced = false;

    //Indicator showing where the camera is pointing at the moment.
    private GameObject _arPointer;
    //Model of the item that was selected in the menu.
    private GameObject _objectToPlace;

    //Initial position in which the screen was touched to rotate the object.
    private Vector2 _initialTouchPosition;

    /// <summary>
    /// Object containing the tutorial panel.
    /// </summary>
    [SerializeField] private GameObject tutorialPanel; 

    private void Start()
    {
        //Components are initialized.
        _raycastManager = FindObjectOfType<ARRaycastManager>();
        _arPointer = transform.GetChild(0).gameObject;
        _objectToPlace = GameObject.Find("1");
        SceneManager.UnloadSceneAsync(0);
    }

    private void Update()
    {
        //Instantiate the item object received from the previous screen and unload the previous scene.
        if (_objectToPlace != null && SceneManager.sceneCount > 1)
        {
            _objectToPlace.transform.SetParent(_arPointer.transform);
            _objectToPlace.transform.position = Vector3.zero;
            _isInitialPosition = true;
        }
        
        //If the tutorial is open, you should not do anything AR.
        if (tutorialPanel.activeInHierarchy)
            return;

        //If it is in the initial position and the tutorial is not open, the movement of the item is initialized.
        if (_isInitialPosition  && !tutorialPanel.activeInHierarchy)
        {
            SetItemInitialPosition();
        }
        
        if(!_isInitialPosition && !_isPlaced  && !tutorialPanel.activeInHierarchy)
            UpdateItemPositionInPlane();
        
        //Touching the screen determines what action to take.
        if (Input.touchCount > 0 && !tutorialPanel.activeInHierarchy)
        {
            Touch touchOne = Input.GetTouch(0);
            
            //If it is a single touch and the object is not placed in the plane, it places the object in the plane.
            if (touchOne.phase == TouchPhase.Began && !_isPlaced)
                PlaceItem();

            //If it is a single touch and the object is placed in the plane, it changes the position of the item.
            if (touchOne.phase == TouchPhase.Began && _isPlaced)
                MovePlacedItem();

            //If they touch the plane with two fingers, the item is rotated.
            if (Input.touchCount == 2)
            {
                Touch touchTwo = Input.GetTouch(1);
                RotateItem(touchOne, touchTwo);
            }
        }
    }

    /// <summary>
    /// Set the initial position to the object of the instantiated item in one of the detected planes.
    /// </summary>
    private void SetItemInitialPosition()
    {
        Vector2 middlePointScreen = new Vector2(Screen.width / 2, Screen.height / 2);
        _raycastManager.Raycast(middlePointScreen, hits, TrackableType.Planes);
        if (hits.Count > 0)
        {
            var transform1 = transform;
            transform1.position = hits[0].pose.position;
            transform1.rotation = hits[0].pose.rotation;
            _arPointer.SetActive(true);
            _isInitialPosition = false;
        }
    }

    /// <summary>
    /// Place the object in the plane at the position where the plane was touched according to the indicator.
    /// </summary>
    private void PlaceItem()
    {
        _objectToPlace.transform.SetParent(null);
        _arPointer.SetActive(false);
        _isPlaced = true;
    }

    /// <summary>
    /// Changes the position of an item placed on the plane
    /// </summary>
    private void MovePlacedItem()
    {
        _objectToPlace.transform.SetParent(_arPointer.transform);
        _objectToPlace.transform.position = _arPointer.transform.position;
        _arPointer.SetActive(true);
        _isPlaced = false;
    }

    /// <summary>
    /// Rotates the item according to the direction of the touches in which it moves .
    /// </summary>
    /// <param name="touchOne">Touch one of the touches list.</param>
    /// <param name="touchTwo">Touch Two of the touches list.</param>
    private void RotateItem(Touch touchOne, Touch touchTwo)
    {
        //(DONT WORK)
        //If touch one and two is a touch that remains, the positions of the touches are saved.
        if (touchOne.phase == TouchPhase.Began || touchTwo.phase == TouchPhase.Began && _isPlaced)
        {
            _initialTouchPosition = touchTwo.position - touchOne.position;
        }
        
        //Depending on the position in which the touches move, the angle is determined and rotated.
        if (touchOne.phase == TouchPhase.Moved && touchTwo.phase == TouchPhase.Moved && _isPlaced)
        {
            Vector2 currentTouchPosition = touchTwo.position - touchOne.position;
            float angle = Vector2.SignedAngle(_initialTouchPosition, currentTouchPosition);
            _objectToPlace.transform.rotation = Quaternion.Euler(0, _objectToPlace.transform.eulerAngles.y - angle, 0);
            _initialTouchPosition = currentTouchPosition;
        }
    }

    /// <summary>
    /// Updates the item position when the camera is moved
    /// </summary>
    private void UpdateItemPositionInPlane()
    {
        //The center of the screen is determined and if it is possible to put in the plane the object changes its position.
        Vector2 middlePointScreen = new Vector2(Screen.width / 2, Screen.height / 2);
        _raycastManager.Raycast(middlePointScreen, hits, TrackableType.Planes);
        transform.position = hits[0].pose.position;
    }

    /// <summary>
    /// Button event to return to the menu.
    /// </summary>
    public void OnClickReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}

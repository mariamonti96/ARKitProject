using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ARKitProjectUI : MonoBehaviour
{

    #region PUBLIC_MEMBERS
    [Header("UI Buttons")]
    public Toggle m_TGOToggle;
    public Toggle m_ESAToggle;

    #endregion //PUBLIC_MEMBERS

    //#region PRIVATE_MEMBERS
    //#endregion //PRIVATE_MEMBERS



    // Use this for initialization
    void Start()
    {
        m_TGOToggle.interactable = true;
        m_TGOToggle.isOn = true;

        m_ESAToggle.interactable = true;
        m_ESAToggle.isOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        // When to enable the toggle?

        //Check if ObjectPlacement.isPlaced is true. If it is then enable the reset button.
    }

    #region PUBLIC_METHODS
    public void Reset()
    {
        Debug.Log("Reset() called");

        //When to reset? 
        //When reset button is clicked!!

    }

    public bool InitializeUI()
    {
        m_TGOToggle.interactable = true;

        //Should you turn the toggle on? Which one? 
        m_TGOToggle.isOn = true;
        return true;
    }

    //maybe add "public bool IsCanvasButtonPressed" to make sure that the objects are not moved when pressing a canvas button?

    #endregion

}

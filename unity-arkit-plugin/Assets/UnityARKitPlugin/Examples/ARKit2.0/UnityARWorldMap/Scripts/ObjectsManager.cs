﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.iOS
{
    public class ObjectsManager : MonoBehaviour
    {

        public enum ObjectMode
        {
            TGO,
            NOTHING,
            ESA,
            DEL
        }

        #region PUBLIC_MEMBERS
        public Transform m_HitTransform;
        public float maxRayDistance = 30.0f;
        public LayerMask collisionLayer = 1 << 10; //ARKitPlane layer


        [Header("Objects augmentations")]
        public GameObject ESA_icon_prefab;
        public GameObject TGO_prefab;
        public static ObjectMode objectMode = ObjectMode.TGO;

        #endregion //PUBLIC_MEMBERS

        #region PRIVATE_MEMBERS
        ObjectPlacement m_TGOPlacement;
        ObjectPlacement m_ESAPlacement;

        ARKitProjectUI m_ARKitProjectUI;

        bool uiHasBeenInitialized;
        #endregion //PRIVATE_MEMBERS

        #region MONOBEHAVIOUR_METHODS

        // Use this for initialization
        void Start()
        {
            //m_ObjectPlacement = FindObjectOfType<ObjectPlacement>();
            //m_ESAPlacement = FindObjectOfType<TGOPlacement>();
            //m_TGOPlacement = GameObject.Find("TGO/default").GetComponent<ObjectPlacement>();
            //m_ESAPlacement = GameObject.Find("ESA_icon").GetComponent<ObjectPlacement>();
            
            m_ARKitProjectUI = FindObjectOfType<ARKitProjectUI>();
            
        }

        // Update is called once per frame
        void Update()
        {
            #if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                //we will try to hit one of the plane collider gameobjects that were generated by the plugin
                //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
                if (Physics.Raycast(ray, out hit, maxRayDistance, collisionLayer))
                {
                    //we are going to get the position from the contact point
                    m_HitTransform.position = hit.point;
                    Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

                    //and the rotation from the transform of the plane collider
                    m_HitTransform.rotation = hit.transform.rotation;
                }
            }
            #else
            //if(Input.touchCount >0 && m_HitTransform != null)
            if(Input.touchCount >0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    if (!m_ARKitProjectUI.IsCanvasButtonPressed())
                    {
                        var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                        ARPoint point = new ARPoint
                        {
                            x = screenPosition.x,
                            y = screenPosition.y
                        };

                        //prioritize results types
                        ARHitTestResultType[] resultTypes =
                        {
                            //ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
                            ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent,
                            // if you want to use infinite planes use this:
                            //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                            //ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane,
                            //ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane,
                            //ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                        };

                        foreach (ARHitTestResultType resultType in resultTypes)
                            if (HitTestWithResultType(point, resultType))
                            {
                                return;
                            }
                    }
                }
            }
            #endif


        }
#endregion //MONOBEHAVIOUR_METHODS

#region HIT_TEST_METHODS
        bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultType)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultType);
            if(hitResults.Count > 0)
            {
                foreach(var hitResult in hitResults)
                {
                    Debug.Log("Got Hit!");
                    //This was used in the original UnityARHitTestExample,
                    //but we want to have multiple objects placed based on toggles
                    // so we will change it
                    
                    //m_HitTransform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                    //m_HitTransform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    //Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

                    var position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                    var rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    switch (objectMode)
                    {
                        case ObjectMode.TGO:
                            if (m_ARKitProjectUI.GetGameObjectPressed() == null)
                            {
                                GameObject TGO = (GameObject)Instantiate(TGO_prefab);
                                m_TGOPlacement = TGO.GetComponentInChildren<ObjectPlacement>();
                                m_TGOPlacement.placeObject(position, rotation);
                            }
                            break;

                        case ObjectMode.ESA:
                            
                            if (m_ARKitProjectUI.GetGameObjectPressed() == null)
                            {
                                GameObject ESA_icon = (GameObject)Instantiate(ESA_icon_prefab);
                                m_ESAPlacement = ESA_icon.GetComponent<ObjectPlacement>();
                                m_ESAPlacement.placeObject(position, rotation);
                            }
                            break;

                        case ObjectMode.DEL:
                            GameObject gameObjectDel = m_ARKitProjectUI.GetGameObjectPressed();
                            if(gameObjectDel != null)
                            {
                                Destroy(gameObjectDel);
                            }
                            break;
                    }
                    return true;
                    
                    
                }
            }
            return false;
        }
#endregion //HIT_TEST_METHODS 
       
#region PUBLIC_BUTTON_METHODS

        //TGOToggle -> TGO Mode
        //Adds one instance of the TGO spacecraft
        //in the position the user touched
        public void SetTGOMode(bool active)
        {
            if (active)
            {
                Debug.Log("Setting Object Mode To TGO");
                objectMode = ObjectMode.TGO;
                m_ARKitProjectUI.m_ESAToggle.isOn = false;
                m_ARKitProjectUI.m_DELToggle.isOn = false;
                //something else? 
            }
        }


        //ESAToggle -> ESA Mode
        //Adds one instance of the ESA logo in the position
        //the user touched
        public void SetESAMode(bool active)
        {
            if (active)
            {
                Debug.Log("Setting Object Mode to TGO");
                objectMode = ObjectMode.ESA;
                m_ARKitProjectUI.m_TGOToggle.isOn = false;
                m_ARKitProjectUI.m_DELToggle.isOn = false;
            }
        }

        //DELToggle -> Delete Mode
        //Destroys game objects when the user touches them
        public void SetDELMode(bool active)
        {
            if (active)
            {

                Debug.Log("Setting Object Mode to DEL");
                objectMode = ObjectMode.DEL;
                m_ARKitProjectUI.m_TGOToggle.isOn = false;
                m_ARKitProjectUI.m_ESAToggle.isOn = false;

         
                
            }
        }
        //Add public void ResetScene() and ResetTrackers()?


        #endregion //PUBLIC_BUTTON_METHODS


        //#region PRIVATE_BUTTON_METHODS
        //private GameObject SpawnESA_icon()
        //{
        //    return (GameObject)Instantiate(ESA_icon_prefab);
        //}

        //#endregion //PRIVATE_BUTTON_METHODS


    }
}

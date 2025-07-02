using System.Collections;
using System.Collections.Generic;
using SoulGames.EasyGridBuilderPro;
using SoulGames.Utilities;
using UnityEngine;

namespace FixedRotationSystem
{
    /// <summary>
    /// Fixed version of the BuildableObjectSelector rotation logic
    /// This fixes the position bug during object rotation
    /// </summary>
    public class FixedBuildableObjectSelector : MonoBehaviour
    {
        [Header("Fixed Rotation Settings")]
        [SerializeField] private bool useFixedRotation = true;
        [SerializeField] private bool debugMode = false;
        
        private BuildableObjectSelector originalSelector;
        private GridManager gridManager;

        private void Start()
        {
            originalSelector = GetComponent<BuildableObjectSelector>();
            gridManager = GridManager.Instance;
        }

        /// <summary>
        /// Fixed version of the grid object rotation that preserves exact position
        /// </summary>
        public void RotateSelectedBuildableGridObjectFixed(BuildableObjectDestroyer buildableObjectDestroyer, 
            BuildableGridObject buildableGridObject, bool rotateClockwise, ref List<BuildableObject> newSelectedObjectList)
        {
            if (!useFixedRotation) return;

            BuildableGridObjectSO buildableGridObjectSO = (BuildableGridObjectSO)buildableGridObject.GetBuildableObjectSO();
            if (!buildableGridObjectSO.isSelectedObjectRotatable) return;

            // Store the exact world position and vertical index BEFORE destroying
            Vector3 exactWorldPosition = buildableGridObject.transform.position;
            EasyGridBuilderPro easyGridBuilderPro = buildableGridObject.GetOccupiedGridSystem();
            int verticalGridIndex = buildableGridObject.GetOccupiedVerticalGridIndex();
            
            // Store other properties
            FourDirectionalRotation currentFourDirectionalRotation = buildableGridObject.GetObjectFourDirectionalRotation();
            BuildableObjectSO.RandomPrefabs objectRandomPrefab = buildableGridObject.GetBuildableObjectSORandomPrefab();

            if (debugMode)
            {
                Debug.Log($"[FixedRotation] Original Position: {exactWorldPosition}, VerticalIndex: {verticalGridIndex}");
            }

            // Destroy the original object
            buildableObjectDestroyer.SetInputDestroyBuildableObject(buildableGridObject, true, true, true);

            // Try to place the new rotated object at the EXACT same position
            bool initializationSuccessful = false;
            int iterationCount = 0;
            BuildableGridObject newBuildableGridObject = null;
            
            while (!initializationSuccessful && iterationCount < 4)
            {
                FourDirectionalRotation nextFourDirectionalRotation = rotateClockwise ? 
                    buildableGridObjectSO.GetNextDirectionClockwise(currentFourDirectionalRotation) : 
                    buildableGridObjectSO.GetNextDirectionCounterClockwise(currentFourDirectionalRotation);

                // Use the exact world position instead of calculated cell position
                if (easyGridBuilderPro.TryInitializeBuildableGridObjectSinglePlacement(
                    exactWorldPosition, 
                    buildableGridObjectSO, 
                    nextFourDirectionalRotation, 
                    true, true, 
                    verticalGridIndex, 
                    true,
                    out newBuildableGridObject, 
                    objectRandomPrefab, 
                    buildableGridObject))
                {
                    initializationSuccessful = true;
                    
                    // Force the exact position after creation to ensure it's correct
                    if (newBuildableGridObject != null)
                    {
                        newBuildableGridObject.transform.position = exactWorldPosition;
                        
                        if (debugMode)
                        {
                            Debug.Log($"[FixedRotation] New Position: {newBuildableGridObject.transform.position}");
                        }
                    }
                }
                
                currentFourDirectionalRotation = nextFourDirectionalRotation;
                iterationCount++;
            }

            if (newBuildableGridObject != null)
            {
                newSelectedObjectList.Add(newBuildableGridObject);
            }
            else
            {
                Debug.LogWarning("[FixedRotation] Failed to rotate object - could not place in any rotation");
            }
        }

        /// <summary>
        /// Fixed version for edge objects
        /// </summary>
        public void RotateSelectedBuildableEdgeObjectFixed(BuildableObjectDestroyer buildableObjectDestroyer, 
            BuildableEdgeObject buildableEdgeObject, bool rotateClockwise, ref List<BuildableObject> newSelectedObjectList)
        {
            if (!useFixedRotation) return;

            BuildableEdgeObjectSO buildableEdgeObjectSO = (BuildableEdgeObjectSO)buildableEdgeObject.GetBuildableObjectSO();
            if (!buildableEdgeObjectSO.isSelectedObjectRotatable) return;

            // Store exact position
            Vector3 exactWorldPosition = buildableEdgeObject.transform.position;
            EasyGridBuilderPro easyGridBuilderPro = buildableEdgeObject.GetOccupiedGridSystem();
            int verticalGridIndex = buildableEdgeObject.GetOccupiedVerticalGridIndex();
            
            FourDirectionalRotation currentFourDirectionalRotation = buildableEdgeObject.GetObjectFourDirectionalRotation();
            bool isObjectFlipped = buildableEdgeObject.GetIsObjectFlipped();
            BuildableObjectSO.RandomPrefabs objectRandomPrefab = buildableEdgeObject.GetBuildableObjectSORandomPrefab();

            if (debugMode)
            {
                Debug.Log($"[FixedRotation] Edge Object Original Position: {exactWorldPosition}");
            }

            buildableObjectDestroyer.SetInputDestroyBuildableObject(buildableEdgeObject, true, true, true);

            bool initializationSuccessful = false;
            int iterationCount = 0;
            BuildableEdgeObject newBuildableEdgeObject = null;
            
            while (!initializationSuccessful && iterationCount < 4)
            {
                FourDirectionalRotation nextFourDirectionalRotation = rotateClockwise ? 
                    buildableEdgeObjectSO.GetNextDirectionClockwise(currentFourDirectionalRotation) : 
                    buildableEdgeObjectSO.GetNextDirectionCounterClockwise(currentFourDirectionalRotation);

                if (easyGridBuilderPro.TryInitializeBuildableEdgeObjectSinglePlacement(
                    exactWorldPosition, 
                    buildableEdgeObjectSO, 
                    nextFourDirectionalRotation, 
                    isObjectFlipped, 
                    true, true, 
                    verticalGridIndex, 
                    true, 
                    out newBuildableEdgeObject, 
                    objectRandomPrefab, 
                    buildableEdgeObject))
                {
                    initializationSuccessful = true;
                    
                    // Force exact position
                    if (newBuildableEdgeObject != null)
                    {
                        newBuildableEdgeObject.transform.position = exactWorldPosition;
                    }
                }
                
                currentFourDirectionalRotation = nextFourDirectionalRotation;
                iterationCount++;
            }

            if (newBuildableEdgeObject != null)
            {
                newSelectedObjectList.Add(newBuildableEdgeObject);
            }
        }

        /// <summary>
        /// Public method to trigger fixed rotation from UI
        /// </summary>
        public void TriggerFixedRotation(bool clockwise)
        {
            if (!originalSelector) return;

            var selectedObjects = originalSelector.GetSelectedObjectsList();
            if (selectedObjects.Count == 0) return;

            if (!gridManager.TryGetBuildableObjectDestroyer(out BuildableObjectDestroyer buildableObjectDestroyer)) return;

            List<BuildableObject> newSelectedObjectList = new List<BuildableObject>();

            foreach (BuildableObject selectedObject in selectedObjects)
            {
                switch (selectedObject)
                {
                    case BuildableGridObject buildableGridObject:
                        RotateSelectedBuildableGridObjectFixed(buildableObjectDestroyer, buildableGridObject, clockwise, ref newSelectedObjectList);
                        break;
                    case BuildableEdgeObject buildableEdgeObject:
                        RotateSelectedBuildableEdgeObjectFixed(buildableObjectDestroyer, buildableEdgeObject, clockwise, ref newSelectedObjectList);
                        break;
                    case BuildableCornerObject buildableCornerObject:
                        // Corner objects don't have the position bug, use original method
                        newSelectedObjectList.Add(buildableCornerObject);
                        RotateCornerObjectInPlace(buildableCornerObject, clockwise);
                        break;
                    case BuildableFreeObject buildableFreeObject:
                        // Free objects don't have the position bug, use original method  
                        newSelectedObjectList.Add(buildableFreeObject);
                        RotateFreeObjectInPlace(buildableFreeObject, clockwise);
                        break;
                }
            }

            // Update selection
            StartCoroutine(UpdateSelectionCoroutine(newSelectedObjectList));
        }

        private void RotateCornerObjectInPlace(BuildableCornerObject buildableCornerObject, bool rotateClockwise)
        {
            BuildableCornerObjectSO buildableCornerObjectSO = (BuildableCornerObjectSO)buildableCornerObject.GetBuildableObjectSO();
            if (!buildableCornerObjectSO.isSelectedObjectRotatable) return;

            EasyGridBuilderPro easyGridBuilderPro = buildableCornerObject.GetOccupiedGridSystem();

            switch (buildableCornerObjectSO.rotationType)
            {
                case CornerObjectRotationType.FourDirectionalRotation:
                    FourDirectionalRotation currentFourDirectionalRotation = buildableCornerObject.GetObjectFourDirectionalRotation();
                    FourDirectionalRotation nextFourDirectionalRotation = rotateClockwise ? 
                        buildableCornerObjectSO.GetNextFourDirectionalDirectionClockwise(currentFourDirectionalRotation) : 
                        buildableCornerObjectSO.GetNextFourDirectionalDirectionCounterClockwise(currentFourDirectionalRotation);
                    
                    buildableCornerObject.SetObjectFourDirectionalRotation(nextFourDirectionalRotation);
                    float rotation = buildableCornerObjectSO.GetFourDirectionalRotationAngle(nextFourDirectionalRotation);
                    
                    if (easyGridBuilderPro is EasyGridBuilderProXZ) 
                        buildableCornerObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
                    else 
                        buildableCornerObject.transform.rotation = Quaternion.Euler(0, 0, rotation);
                    break;
            }
        }

        private void RotateFreeObjectInPlace(BuildableFreeObject buildableFreeObject, bool rotateClockwise)
        {
            BuildableFreeObjectSO buildableFreeObjectSO = (BuildableFreeObjectSO)buildableFreeObject.GetBuildableObjectSO();
            if (!buildableFreeObjectSO.isSelectedObjectRotatable) return;

            EasyGridBuilderPro easyGridBuilderPro = buildableFreeObject.GetOccupiedGridSystem();

            switch (buildableFreeObjectSO.rotationType)
            {
                case FreeObjectRotationType.FourDirectionalRotation:
                    FourDirectionalRotation currentFourDirectionalRotation = buildableFreeObject.GetObjectFourDirectionalRotation();
                    FourDirectionalRotation nextFourDirectionalRotation = rotateClockwise ? 
                        buildableFreeObjectSO.GetNextFourDirectionalDirectionClockwise(currentFourDirectionalRotation) : 
                        buildableFreeObjectSO.GetNextFourDirectionalDirectionCounterClockwise(currentFourDirectionalRotation);
                    
                    buildableFreeObject.SetObjectFourDirectionalRotation(nextFourDirectionalRotation);
                    float rotation = buildableFreeObjectSO.GetFourDirectionalRotationAngle(nextFourDirectionalRotation);
                    
                    if (easyGridBuilderPro is EasyGridBuilderProXZ) 
                        buildableFreeObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
                    else 
                        buildableFreeObject.transform.rotation = Quaternion.Euler(0, 0, rotation);
                    break;
            }
        }

        private IEnumerator UpdateSelectionCoroutine(List<BuildableObject> newSelectedObjectList)
        {
            // Clear current selection
            originalSelector.SetInputAreaSelectionReset();
            
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Select new objects
            foreach (BuildableObject selectedObject in newSelectedObjectList)
            {
                if (selectedObject != null)
                {
                    originalSelector.SetInputSelectBuildableObject(selectedObject);
                }
            }
        }
    }
}
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GrowthManager))]
public class GrowthManagerEditor : Editor {
  // Algorithm parameters
  SerializedProperty attractionDistanceProp;
  SerializedProperty killDistanceProp;
  SerializedProperty segmentLengthProp;

  // Branch rendering
  SerializedProperty materialProp;
  SerializedProperty enableCanalizationProp;
  SerializedProperty minimumRadiusProp;
  SerializedProperty maximumRadiusProp;
  SerializedProperty radiusIncrementProp;
  SerializedProperty constantRadiusProp;

  // Attractor generation
  SerializedProperty attractorsTypeProp;
  SerializedProperty gridDimensionsProp;
  SerializedProperty gridResolutionProp;
  SerializedProperty gridJitterAmountProp;
  SerializedProperty attractorSphereCountProp;
  SerializedProperty attractorSphereRadiusProp;
  SerializedProperty targetMeshProp;
  SerializedProperty attractorRaycastingTypeProp;
  SerializedProperty attractorRaycastAttemptsProp;
  SerializedProperty attractorSurfaceOffsetProp;
  SerializedProperty attractorGizmoRadiusProp;

  // Root node(s) generation
  SerializedProperty rootNodeTypeProp;
  SerializedProperty inputRootNodeProp;
  SerializedProperty numRootNodesProp;

  // Bounding mesh
  SerializedProperty enableBoundsProp;
  SerializedProperty boundsMeshProp;

  // Obstacles
  SerializedProperty enableObstaclesProp;
  SerializedProperty obstacleMeshListProp;

  public void OnEnable() {
    attractionDistanceProp = serializedObject.FindProperty("AttractionDistance");
    killDistanceProp = serializedObject.FindProperty("KillDistance");
    segmentLengthProp = serializedObject.FindProperty("SegmentLength");

    materialProp = serializedObject.FindProperty("BranchMaterial");
    enableCanalizationProp = serializedObject.FindProperty("EnableCanalization");
    minimumRadiusProp = serializedObject.FindProperty("MinimumRadius");
    maximumRadiusProp = serializedObject.FindProperty("MaximumRadius");
    radiusIncrementProp = serializedObject.FindProperty("RadiusIncrement");
    constantRadiusProp = serializedObject.FindProperty("ConstantRadius");

    attractorsTypeProp = serializedObject.FindProperty("attractorsType");
    gridDimensionsProp = serializedObject.FindProperty("GridDimensions");
    gridResolutionProp = serializedObject.FindProperty("GridResolution");
    gridJitterAmountProp = serializedObject.FindProperty("GridJitterAmount");
    attractorSphereCountProp = serializedObject.FindProperty("AttractorSphereCount");
    attractorSphereRadiusProp = serializedObject.FindProperty("AttractorSphereRadius");
    targetMeshProp = serializedObject.FindProperty("TargetMesh");
    attractorRaycastingTypeProp = serializedObject.FindProperty("attractorRaycastingType");
    attractorRaycastAttemptsProp = serializedObject.FindProperty("AttractorRaycastAttempts");
    attractorSurfaceOffsetProp = serializedObject.FindProperty("AttractorSurfaceOffset");
    attractorGizmoRadiusProp = serializedObject.FindProperty("AttractorGizmoRadius");

    rootNodeTypeProp = serializedObject.FindProperty("rootNodeType");
    inputRootNodeProp = serializedObject.FindProperty("InputRootNode");
    numRootNodesProp = serializedObject.FindProperty("NumRootNodes");

    enableBoundsProp = serializedObject.FindProperty("EnableBounds");
    boundsMeshProp = serializedObject.FindProperty("BoundingMesh");

    enableObstaclesProp = serializedObject.FindProperty("EnableObstacles");
    obstacleMeshListProp = serializedObject.FindProperty("Obstacles");
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();
    GrowthManager manager = (GrowthManager)target;

    //=======================================
    //  Algorithm parameters
    //=======================================
    EditorGUILayout.LabelField("Algorithm parameters", EditorStyles.boldLabel);
    EditorGUI.indentLevel++;

      attractionDistanceProp.floatValue = EditorGUILayout.FloatField("Attraction distance", attractionDistanceProp.floatValue);
      killDistanceProp.floatValue = EditorGUILayout.FloatField("Kill distance", killDistanceProp.floatValue);
      segmentLengthProp.floatValue = EditorGUILayout.FloatField("Segment length", segmentLengthProp.floatValue);

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    EditorGUILayout.Space();


    //=======================================
    //  Branch rendering
    //=======================================
    EditorGUILayout.LabelField("Branch rendering", EditorStyles.boldLabel);
    EditorGUI.indentLevel++;

      materialProp.objectReferenceValue = (Material)EditorGUILayout.ObjectField("Material", materialProp.objectReferenceValue, typeof(Material), true);

      enableCanalizationProp.boolValue = EditorGUILayout.Toggle("Enable vein thickening", enableCanalizationProp.boolValue);

      if(enableCanalizationProp.boolValue) {
        minimumRadiusProp.floatValue = EditorGUILayout.FloatField("Minimum radius", minimumRadiusProp.floatValue);
        maximumRadiusProp.floatValue = EditorGUILayout.FloatField("Maximum radius", maximumRadiusProp.floatValue);
        radiusIncrementProp.floatValue = EditorGUILayout.FloatField("Radius increment", radiusIncrementProp.floatValue);
      } else {
        constantRadiusProp.floatValue = EditorGUILayout.FloatField("Radius", constantRadiusProp.floatValue);
      }

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    EditorGUILayout.Space();


    //=======================================
    //  Attractor generation
    //=======================================
    EditorGUILayout.LabelField("Attractor generation", EditorStyles.boldLabel);
    EditorGUI.indentLevel++;

      EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Type of attractors");
        EditorGUI.indentLevel--;
          attractorsTypeProp.enumValueIndex = EditorGUILayout.Popup(attractorsTypeProp.enumValueIndex, attractorsTypeProp.enumDisplayNames);
        EditorGUI.indentLevel++;

      EditorGUILayout.EndHorizontal();

      switch(attractorsTypeProp.enumValueIndex) {
        case (int)GrowthManager.AttractorsType.GRID:
          gridDimensionsProp.vector3Value = EditorGUILayout.Vector3Field("Dimensions", gridDimensionsProp.vector3Value);
          gridResolutionProp.vector3IntValue = EditorGUILayout.Vector3IntField("Resolution", gridResolutionProp.vector3IntValue);
          gridJitterAmountProp.floatValue = EditorGUILayout.FloatField("Jitter amount", gridJitterAmountProp.floatValue);
          break;

        case (int)GrowthManager.AttractorsType.SPHERE:
          attractorSphereRadiusProp.floatValue = EditorGUILayout.FloatField("Radius", attractorSphereRadiusProp.floatValue);
          attractorSphereCountProp.intValue = EditorGUILayout.IntField("Attractor count", attractorSphereCountProp.intValue);
          break;

        case (int)GrowthManager.AttractorsType.MESH:
          targetMeshProp.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Target mesh", targetMeshProp.objectReferenceValue, typeof(GameObject), true);
          attractorRaycastAttemptsProp.intValue = EditorGUILayout.IntField("Raycast attempts", attractorRaycastAttemptsProp.intValue);

          EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("Raycasting direction");
            EditorGUI.indentLevel--;
              attractorRaycastingTypeProp.enumValueIndex = EditorGUILayout.Popup(attractorRaycastingTypeProp.enumValueIndex, attractorRaycastingTypeProp.enumDisplayNames);
            EditorGUI.indentLevel++;

          EditorGUILayout.EndHorizontal();
          break;
      }

      attractorGizmoRadiusProp.floatValue = EditorGUILayout.FloatField("Attractor dot radius", attractorGizmoRadiusProp.floatValue);

      EditorGUILayout.Space();
      EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Actions");

        if(GUILayout.Button("Generate")) {
          manager.ResetScene();
        }

        if(GUILayout.Button("Clear")) {
          manager.ResetScene();
        }

      EditorGUILayout.EndHorizontal();

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    EditorGUILayout.Space();


    //=======================================
    //  Root nodes
    //=======================================
    EditorGUILayout.LabelField("Root nodes", EditorStyles.boldLabel);
    EditorGUI.indentLevel++;

      EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Type of root node(s)");
        EditorGUI.indentLevel--;
          rootNodeTypeProp.enumValueIndex = EditorGUILayout.Popup(rootNodeTypeProp.enumValueIndex, rootNodeTypeProp.enumDisplayNames);
        EditorGUI.indentLevel++;

      EditorGUILayout.EndHorizontal();

      switch(rootNodeTypeProp.enumValueIndex) {
        case (int)GrowthManager.RootNodeType.INPUT:
          inputRootNodeProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Root node object", inputRootNodeProp.objectReferenceValue, typeof(Transform), true);
          break;

        case (int)GrowthManager.RootNodeType.MESH:
          targetMeshProp.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Bounding mesh", targetMeshProp.objectReferenceValue, typeof(GameObject), true);
          numRootNodesProp.intValue = EditorGUILayout.IntSlider("Number of root nodes", numRootNodesProp.intValue, 1, 10);
          break;
      }

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    EditorGUILayout.Space();


    //=======================================
    //  Bounds
    //=======================================
    EditorGUILayout.LabelField("Bounds", EditorStyles.boldLabel);
    EditorGUI.indentLevel++;

      enableBoundsProp.boolValue = EditorGUILayout.Toggle("Use bounds", enableBoundsProp.boolValue);

      using(new EditorGUI.DisabledScope(enableBoundsProp.boolValue == false)) {
        boundsMeshProp.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Bounding mesh", boundsMeshProp.objectReferenceValue, typeof(GameObject), true);
      }

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    EditorGUILayout.Space();


    //=======================================
    //  Obstacles
    //=======================================
    EditorGUILayout.LabelField("Obstacles", EditorStyles.boldLabel);
    EditorGUI.indentLevel++;

      enableObstaclesProp.boolValue = EditorGUILayout.Toggle("Use obstacles", enableObstaclesProp.boolValue);

      using(new EditorGUI.DisabledScope(enableObstaclesProp.boolValue == false)) {
        EditorGUI.indentLevel++;
          EditorGUILayout.PropertyField(obstacleMeshListProp, new GUIContent("Obstacle meshes"), true);
        EditorGUI.indentLevel--;
      }

    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    EditorGUILayout.Space();


    EditorGUILayout.BeginHorizontal();

      if(GUILayout.Button("Grow")) {
        manager.GrowInEditor();
      }

      if(GUILayout.Button("Export OBJ")) {
        manager.ExportOBJ();
      }

    EditorGUILayout.EndHorizontal();

    serializedObject.ApplyModifiedProperties();
  }
}
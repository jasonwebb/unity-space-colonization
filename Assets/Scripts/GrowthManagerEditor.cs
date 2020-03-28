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

  // Iteration limits
  SerializedProperty iterationsToRunProp;

  // Export
  SerializedProperty exportFilenameProp;

  // Foldouts
  private bool showAlgorithmParameters = true;
  private bool showBranchRendering = true;
  private bool showAttractorGeneration = true;
  private bool showRootNodes = true;
  private bool showBounds = true;
  private bool showObstacles = true;
  private bool showRunControls = true;
  private bool showExport = true;

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

    iterationsToRunProp = serializedObject.FindProperty("IterationsToRun");

    exportFilenameProp = serializedObject.FindProperty("ExportFilename");
  }

  public override void OnInspectorGUI() {
    serializedObject.Update();
    GrowthManager manager = (GrowthManager)target;

    GUIStyle boldFoldoutLabel = new GUIStyle(EditorStyles.foldout);
    boldFoldoutLabel.fontStyle = FontStyle.Bold;

    //=======================================
    //  Algorithm parameters
    //=======================================
    showAlgorithmParameters = EditorGUILayout.Foldout(showAlgorithmParameters, "Algorithm parameters", boldFoldoutLabel);

    if(showAlgorithmParameters) {
      EditorGUI.indentLevel++;

        attractionDistanceProp.floatValue = EditorGUILayout.FloatField("Attraction distance", attractionDistanceProp.floatValue);
        killDistanceProp.floatValue = EditorGUILayout.FloatField("Kill distance", killDistanceProp.floatValue);
        segmentLengthProp.floatValue = EditorGUILayout.FloatField("Segment length", segmentLengthProp.floatValue);

      EditorGUI.indentLevel--;
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }


    //=======================================
    //  Branch rendering
    //=======================================
    showBranchRendering = EditorGUILayout.Foldout(showBranchRendering, "Branch rendering", boldFoldoutLabel);

    if(showBranchRendering) {
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
    }


    //=======================================
    //  Attractor generation
    //=======================================
    showAttractorGeneration = EditorGUILayout.Foldout(showAttractorGeneration, "Attractor generation", boldFoldoutLabel);

    if(showAttractorGeneration) {
      EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();

          EditorGUILayout.PrefixLabel("Attractor placement");
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

        attractorGizmoRadiusProp.floatValue = EditorGUILayout.FloatField("Attractor gizmo radius", attractorGizmoRadiusProp.floatValue);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

          EditorGUILayout.PrefixLabel("Actions");

          if(GUILayout.Button("Generate attractors")) {
            if(enableBoundsProp.boolValue && boundsMeshProp.objectReferenceValue == null) {
              Debug.LogError("Bounding mesh must be provided when bounds are enabled.");
              return;
            }

            manager.CreateAttractors();
            EditorWindow.GetWindow<SceneView>().Repaint();
          }

          if(GUILayout.Button("Clear")) {
            manager.ClearAttractors();
            EditorWindow.GetWindow<SceneView>().Repaint();
          }

        EditorGUILayout.EndHorizontal();

      EditorGUI.indentLevel--;
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }


    //=======================================
    //  Root node(s)
    //=======================================
    showRootNodes = EditorGUILayout.Foldout(showRootNodes, "Root node(s)", boldFoldoutLabel);

    if(showRootNodes) {
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
            targetMeshProp.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Target mesh", targetMeshProp.objectReferenceValue, typeof(GameObject), true);
            numRootNodesProp.intValue = EditorGUILayout.IntSlider("Number of root nodes", numRootNodesProp.intValue, 1, 10);
            break;
        }

      EditorGUI.indentLevel--;
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }


    //=======================================
    //  Bounds
    //=======================================
    showBounds = EditorGUILayout.Foldout(showBounds, "Bounds", boldFoldoutLabel);

    if(showBounds) {
      EditorGUI.indentLevel++;

        enableBoundsProp.boolValue = EditorGUILayout.Toggle("Use bounds", enableBoundsProp.boolValue);

        using(new EditorGUI.DisabledScope(enableBoundsProp.boolValue == false)) {
          boundsMeshProp.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Bounding mesh", boundsMeshProp.objectReferenceValue, typeof(GameObject), true);
        }

      EditorGUI.indentLevel--;
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }


    //=======================================
    //  Obstacles
    //=======================================
    showObstacles = EditorGUILayout.Foldout(showObstacles, "Obstacles", boldFoldoutLabel);

    if(showObstacles) {
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
    }


    //=======================================
    //  Run controls
    //=======================================
    showRunControls = EditorGUILayout.Foldout(showRunControls, "Run controls", boldFoldoutLabel);

    if(showRunControls) {
      EditorGUI.indentLevel++;

        iterationsToRunProp.intValue = EditorGUILayout.IntField("Iterations to run", iterationsToRunProp.intValue);

      EditorGUI.indentLevel--;
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;

        if(GUILayout.Button("▶ Run", buttonStyle, GUILayout.Height(40))) {
          manager.Unpause();

          bool attractorsReady = true,
               rootNodesReady = true,
               meshesReady = true;

          // Check that attractors are generated
          if(manager.GetAttractorCount() == 0) {
            attractorsReady = manager.CreateAttractors();
          }

          // Check that root nodes are generated
          if(manager.GetNodeCount() == 0) {
            rootNodesReady = manager.CreateRootNodes();
          }

          // Check that meshes are set up
          if(!manager.GetMeshesReady()) {
            meshesReady = manager.SetupMeshes();
          }

          if(attractorsReady && rootNodesReady) {
            manager.BuildSpatialIndex();

            for(int i=0; i<iterationsToRunProp.intValue; i++) {
              manager.Update();
            }
          }
        }

        if(GUILayout.Button("Reset", buttonStyle, GUILayout.Height(40))) {
          manager.ResetScene();
        }

      EditorGUILayout.EndHorizontal();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }


    //=======================================
    //  Export
    //=======================================
    showExport = EditorGUILayout.Foldout(showExport, "Export", boldFoldoutLabel);

    if(showExport) {
      EditorGUI.indentLevel++;

      EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Filename");

        exportFilenameProp.stringValue = EditorGUILayout.TextField(exportFilenameProp.stringValue);

        if(GUILayout.Button("Export")) {
          manager.ExportOBJ();
        }

      EditorGUILayout.EndHorizontal();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }

    serializedObject.ApplyModifiedProperties();
  }
}
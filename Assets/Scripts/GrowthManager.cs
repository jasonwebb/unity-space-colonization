using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.Profiling;
using DataStructures.ViliWonka.KDTree;

public class GrowthManager : MonoBehaviour {
  //========================================================
  //  Configurable parameters
  //========================================================
  // Algorithm parameters
  public float AttractionDistance;
  public float KillDistance;
  public float SegmentLength;

  // Branch rendering parameters
  public Material BranchMaterial;
  public bool EnableCanalization;
  public float MinimumRadius;
  public float MaximumRadius;
  public float RadiusIncrement;
  public float ConstantRadius;

  // Attractors type parameters
  public enum AttractorsType {SPHERE, GRID, MESH};
  public AttractorsType attractorsType;

    // GRID
    public Vector3 GridDimensions;
    public Vector3Int GridResolution;
    public float GridJitterAmount;

    // SPHERE
    public float AttractorSphereRadius;
    public int AttractorSphereCount;

    // MESH
    public GameObject TargetMesh;

  // Attractor generation parameters
  public int AttractorRaycastAttempts;
  public float AttractorSurfaceOffset;
  public float AttractorGizmoRadius;

  public enum AttractorRaycastingType {INWARDS, OUTWARDS, DOME};
  public AttractorRaycastingType attractorRaycastingType;

  // Root node(s) parameters
  public enum RootNodeType {INPUT, MESH};
  public RootNodeType rootNodeType;

  public Transform InputRootNode;
  public int NumRootNodes;

  // Bounds parameters
  public GameObject BoundingMesh;
  public bool EnableBounds;

  // Obstacles parameters
  public bool EnableObstacles;
  public List<GameObject> Obstacles = new List<GameObject>();
  private RaycastHit[] hits;  // results of raycast hit-tests against bounds and obstacles

  // Iteration limit
  public int IterationsToRun;

  // Export parameters
  public string ExportFilename;

  // Public state flags
  public bool isPaused = false;


  //========================================================
  //  Internal variables
  //========================================================
  // Attractors
  private List<Attractor> _attractors = new List<Attractor>();
  private List<Attractor> _attractorsToRemove = new List<Attractor>();

  // Nodes
  private List<Node> _rootNodes = new List<Node>();
  private List<Node> _nodes = new List<Node>();
  private List<int> _nodesInAttractionZone = new List<int>();
  private List<Node> _nodesToAdd = new List<Node>();

  private KDTree _nodeTree;                // spatial index of vein nodes
  private KDQuery _query = new KDQuery();  // query object for spatial indices

  // Branch meshes and data
  private List<List<Vector3>> _branches = new List<List<Vector3>>();
  private List<List<float>> _branchRadii = new List<List<float>>();
  private List<CombineInstance> _branchMeshes = new List<CombineInstance>();

  // Patch meshes and data
  private List<List<Vector3>> _patches = new List<List<Vector3>>();
  private List<List<float>> _patchRadii = new List<List<float>>();
  private List<CombineInstance> _patchMeshes = new List<CombineInstance>();

  // Tube renderer and output mesh
  private TubeRenderer _tube;
  private GameObject _veinsMesh;
  private MeshFilter _filter;


  /*
  =================================
    RESET
    Called when script component
    is reset in Editor
  =================================
  */
  void Reset() {
    AttractionDistance = 10f;
    KillDistance = .5f;
    SegmentLength = .5f;

    EnableCanalization = true;
    MinimumRadius = .005f;
    MaximumRadius = .15f;
    RadiusIncrement = .001f;
    ConstantRadius = .01f;

    attractorsType = AttractorsType.GRID;

    GridDimensions = new Vector3(20f,20f,20f);
    GridResolution = new Vector3Int(5,5,5);
    GridJitterAmount = 1f;

    AttractorSphereRadius = 10f;
    AttractorSphereCount = 1000;

    AttractorRaycastAttempts = 200000;
    attractorRaycastingType = AttractorRaycastingType.INWARDS;
    AttractorSurfaceOffset = .01f;
    AttractorGizmoRadius = .05f;

    rootNodeType = RootNodeType.INPUT;
    NumRootNodes = 3;

    EnableBounds = false;
    EnableObstacles = false;

    Obstacles = new List<GameObject>();

    IterationsToRun = 10;

    ExportFilename = "veins.obj";
  }


  /*
  ========================
    INITIAL SETUP
  ========================
  */
  void Start() {
    ResetScene();
  }

    /*
    ========================
      MESHES
    ========================
    */
    public bool SetupMeshes() {
      Profiler.BeginSample("SetupMeshes");

      // Remove all children (veins/tube) that can build up when switching between Editor and Game modes
      while(transform.childCount > 0) {
        DestroyImmediate(transform.GetChild(0).gameObject);
      }

      // Set up a separate GameObject to render the veins to
      _veinsMesh = new GameObject("Veins");
      _veinsMesh.transform.SetParent(gameObject.transform);
      _veinsMesh.AddComponent<MeshRenderer>();

      _filter = _veinsMesh.AddComponent<MeshFilter>();
      _filter.sharedMesh = new Mesh();
      _filter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
      _veinsMesh.GetComponent<Renderer>().material = BranchMaterial;

      // Set up the tube renderer
      _tube = new GameObject("(Temporary) Tubes").AddComponent<TubeRenderer>();
      _tube.transform.SetParent(gameObject.transform);

      Profiler.EndSample();

      return true;
    }


    /*
    ========================
      ATTRACTORS
    ========================
    */
    public bool CreateAttractors() {
      Profiler.BeginSample("CreateAttractors");

      bool attractorsReady = false;
      _attractors.Clear();

      switch(attractorsType) {
        // Create attractors on target mesh(es) using raycasting ------------------------------------
        case AttractorsType.MESH:
          attractorsReady = CreateAttractorsOnMeshSurface();
          break;

        // Points in a 3D grid ----------------------------------------------------------------------
        case AttractorsType.GRID:
          for(int x = 0; x <= GridResolution.x; x++) {
            for(int y = 0; y <= GridResolution.y; y++) {
              for(int z = 0; z <= GridResolution.z; z++) {
                Vector3 attractorPosition = new Vector3(
                  x * (GridDimensions.x/GridResolution.x) - GridDimensions.x/2 + UnityEngine.Random.Range(-GridJitterAmount, GridJitterAmount),
                  y * (GridDimensions.y/GridResolution.y) - GridDimensions.y/2 + UnityEngine.Random.Range(-GridJitterAmount, GridJitterAmount),
                  z * (GridDimensions.z/GridResolution.z) - GridDimensions.z/2 + UnityEngine.Random.Range(-GridJitterAmount, GridJitterAmount)
                );

                AddAttractor(attractorPosition);
              }
            }
          }

          attractorsReady = true;

          break;

        // Points in a sphere ----------------------------------------------------------------------
        case AttractorsType.SPHERE:
          for (int i = 0; i < AttractorSphereCount; i++) {
            AddAttractor(UnityEngine.Random.insideUnitSphere * AttractorSphereRadius);
          }

          attractorsReady = true;

          break;
      }

      Profiler.EndSample();

      return attractorsReady;
    }

      public bool CreateAttractorsOnMeshSurface() {
        Profiler.BeginSample("CreateAttractorsOnMeshSurface");

        bool attractorsReady = false;

        if(TargetMesh != null) {
          int hitCount = 0;

          for(int i=0; i<AttractorRaycastAttempts; i++) {
            RaycastHit hitInfo;

            Vector3 startingPoint = Vector3.zero;
            Vector3 targetPoint = Vector3.zero;

            switch(attractorRaycastingType) {
              // Inside-out raycasting
              case AttractorRaycastingType.OUTWARDS:
                startingPoint = new Vector3(0f,.5f,0);
                targetPoint = UnityEngine.Random.onUnitSphere * 100f;
                break;

              // Outside-in raycasting
              case AttractorRaycastingType.INWARDS:
                startingPoint = UnityEngine.Random.onUnitSphere * 100f;
                targetPoint = UnityEngine.Random.onUnitSphere * .1f;
                break;

              // Dome (upper hemisphere of a sphere)
              case AttractorRaycastingType.DOME:
                startingPoint = UnityEngine.Random.onUnitSphere * 100f;
                targetPoint = UnityEngine.Random.onUnitSphere * .1f;

                if(startingPoint.y < 0) {
                  continue;
                }

                break;
            }

            bool bHit = Physics.Raycast(
              startingPoint,
              targetPoint,
              out hitInfo,
              Mathf.Infinity,
              LayerMask.GetMask("Targets"),
              QueryTriggerInteraction.Ignore
            );

            if(bHit) {
              AddAttractor(hitInfo.point + (hitInfo.normal * AttractorSurfaceOffset));
              hitCount++;
            }
          }

          attractorsReady = true;
        } else {
          Debug.LogError("Target mesh must be provided to generator attractors.");
        }

        Profiler.EndSample();

        return attractorsReady;
      }

      public void ClearAttractors() {
        _attractors.Clear();
      }


    /*
    ========================
      ROOT NODES
    ========================
    */
    public bool CreateRootNodes() {
      Profiler.BeginSample("CreateRootNodes");

      bool rootNodesReady = false;
      _nodes.Clear();
      _rootNodes.Clear();

      switch(rootNodeType) {
        // Root node from provided transform ------------------------------------------------------
        case RootNodeType.INPUT:
          if(InputRootNode != null) {
            _rootNodes.Add(
              new Node(
                InputRootNode.position,
                null,
                true,
                MinimumRadius
              )
            );

            rootNodesReady = true;
          } else {
            Debug.LogError("Input root node must be provided.");
          }

          break;

        // Random point(s) on mesh ----------------------------------------------------------------
        case RootNodeType.MESH:
          if(TargetMesh != null) {
            bool isHit = false;
            RaycastHit hitInfo;

            for(int i=0; i<NumRootNodes; i++) {
              do {
                Vector3 startingPoint = UnityEngine.Random.onUnitSphere * 5;
                Vector3 targetPoint = UnityEngine.Random.onUnitSphere * .5f;

                isHit = Physics.Raycast(
                  startingPoint,
                  targetPoint,
                  out hitInfo,
                  Mathf.Infinity,
                  LayerMask.GetMask("Targets"),
                  QueryTriggerInteraction.Ignore
                );

                if(isHit) {
                  _rootNodes.Add(
                    new Node(
                      hitInfo.point,
                      null,
                      true,
                      MinimumRadius
                    )
                  );
                }
              } while(!isHit);
            }

            rootNodesReady = true;
          } else {
            Debug.LogError("Target mesh must be provided to generate root nodes.");
          }

          break;
      }

      // Add root nodes to node tree
      foreach(Node rootNode in _rootNodes) {
        _nodes.Add(rootNode);
      }

      Profiler.EndSample();

      return rootNodesReady;
    }


  /*
  ========================
    MAIN PROGRAM LOOP
  ========================
  */
  public void Update() {
    // Toggle pause on "space"
    if(Input.GetKeyUp("space")) { isPaused = !isPaused; }

    // Reload the scene when "r" is pressed
    if(Input.GetKeyUp("r")) { ResetScene(); }

    // Stop iterating when paused or if there are no attractors
    if(isPaused) { return; }

    // Reset lists of attractors that vein nodes were attracted to last cycle
    foreach(Node node in _nodes) {
      node.influencedBy.Clear();
    }

    // 1. Associate attractors with vein nodes =============================================================================
    AssociateAttractors();

    // 2. Add vein nodes onto every vein node that is being influenced ====================================================
    GrowNetwork();

    // 3. Remove attractors that have been reached by their vein nodes =====================================================
    PruneAttractors();

    // 4. Rebuild vein node spatial index with latest vein nodes ===========================================================
    BuildSpatialIndex();

    // 5. Generate tube meshes to render the vein network ==================================================================
    CreateMeshes();
  }

    void AssociateAttractors() {
      Profiler.BeginSample("AssociateAttractors");

      foreach(Attractor attractor in _attractors) {
        attractor.isInfluencing.Clear();
        attractor.isReached = false;
        _nodesInAttractionZone.Clear();

        // a. Open venation = closest vein node only ---------------------------------------------------------------------
        _query.ClosestPoint(_nodeTree, attractor.position, _nodesInAttractionZone);

        // ii. If a vein node is found, associate it by pushing attractor ID to _nodeInfluencedBy
        if(_nodesInAttractionZone.Count > 0) {
          Node closestNode = _nodes[_nodesInAttractionZone[0]];
          float distance = (attractor.position - closestNode.position).sqrMagnitude;

          if(distance <= AttractionDistance * AttractionDistance) {
            closestNode.influencedBy.Add(attractor);

            if(distance > KillDistance * KillDistance) {
              attractor.isReached = false;
            } else {
              attractor.isReached = true;
            }
          }
        }

        // b. Closed venation = all vein nodes in relative neighborhood

      }

      Profiler.EndSample();
    }

    void GrowNetwork() {
      Profiler.BeginSample("GrowNetwork");

      _nodesToAdd.Clear();

      foreach(Node node in _nodes) {
        if(node.influencedBy.Count > 0) {
          // Calculate the average direction of the influencing attractors
          Vector3 averageDirection = GetAverageDirection(node, node.influencedBy);

          // Calculate a new node position
          Vector3 newNodePosition = node.position + averageDirection * SegmentLength;

          // Add a random jitter to reduce split sources
          // newNodePosition += new Vector3(Random.Range(-.0001f,.0001f), Random.Range(-.0001f,.0001f), Random.Range(-.0001f,.0001f));

          if(
            (EnableBounds && IsInsideBounds(newNodePosition)) &&
            (EnableObstacles && !IsInsideAnyObstacle(newNodePosition))
          ) {
            // Since this vein node is spawning a new one, it is no longer a tip
            node.isTip = false;

            // Create the new node
            Node newNode = new Node(
              newNodePosition,
              node,
              true,
              MinimumRadius
            );

            node.children.Add(newNode);
            _nodesToAdd.Add(newNode);
          }
        }
      }

      // Add in the new vein nodes that have been produced
      for(int i=0; i<_nodesToAdd.Count; i++) {
        Node currentNode = _nodesToAdd[i];

        _nodes.Add(currentNode);

        // Thicken the radius of every parent Node
        if(EnableCanalization) {
          Profiler.BeginSample("Canalization");

          while(currentNode.parent != null) {
            if(currentNode.parent.radius + RadiusIncrement <= MaximumRadius) {
              currentNode.parent.radius += RadiusIncrement;
            }

            currentNode = currentNode.parent;
          }

          Profiler.EndSample();
        }
      }

      Profiler.EndSample();
    }

    void PruneAttractors() {
      Profiler.BeginSample("PruneAttractors");

      _attractorsToRemove.Clear();

      foreach(Attractor attractor in _attractors) {
        // a. Open venation = as soon as the closest vein node enters KillDistance
        if(attractor.isReached) {
          _attractorsToRemove.Add(attractor);
        }

        // b. Closed venation = only when all vein nodes in relative neighborhood enter KillDistance
      }

      // Remove any attractors that were flagged
      foreach(Attractor attractor in _attractorsToRemove) {
        _attractors.Remove(attractor);
      }

      Profiler.EndSample();
    }

    void CreateMeshes() {
      Profiler.BeginSample("CreateMeshes");

      List<CombineInstance> branchMeshes = GetBranchMeshes();
      List<CombineInstance> patchMeshes = GetPatchMeshes();
      List<CombineInstance> allMeshes = new List<CombineInstance>();

      allMeshes.AddRange(branchMeshes);
      allMeshes.AddRange(patchMeshes);

      _filter.sharedMesh.CombineMeshes(allMeshes.ToArray());

      Profiler.EndSample();
    }


      /*
      ========================
        BRANCH MESHES
      ========================
      */
      List<CombineInstance> GetBranchMeshes() {
        Profiler.BeginSample("GetBranchMeshes");

        _branches.Clear();
        _branchRadii.Clear();

        // Recursively populate the _branches array
        foreach(Node rootNode in _rootNodes) {
          GetBranch(rootNode);
        }

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        int t = 0;

        // Create continuous tube meshes for each branch
        foreach(List<Vector3> branch in _branches) {
          _tube.points = new Vector3[branch.Count];
          _tube.radiuses = new float[branch.Count];

          for(int j=0; j<branch.Count; j++) {
            _tube.points[j] = branch[j];
            _tube.radiuses[j] = _branchRadii[t][j];
          }

          _tube.ForceUpdate();

          CombineInstance cb = new CombineInstance();
          cb.mesh = Instantiate(_tube.mesh);  // Instantiate is expensive AF - needs improvement!
          cb.transform = _tube.transform.localToWorldMatrix;
          combineInstances.Add(cb);

          t++;
        }

        Profiler.EndSample();

        return combineInstances;
      }

        private void GetBranch(Node startingNode) {
          Profiler.BeginSample("GetBranch");

          List<Vector3> thisBranch = new List<Vector3>();
          List<float> thisRadii = new List<float>();
          Node currentNode = startingNode;

          if(currentNode.parent != null) {
            thisBranch.Add(currentNode.parent.position);
            thisRadii.Add(currentNode.parent.radius);
          }

          thisBranch.Add(currentNode.position);
          thisRadii.Add(currentNode.radius);

          while(currentNode != null && currentNode.children.Count > 0) {
            if(currentNode.children.Count == 1) {
              thisBranch.Add(currentNode.children[0].position);
              thisRadii.Add(currentNode.children[0].radius);

              currentNode = currentNode.children[0];
            } else {
              foreach(Node childNode in currentNode.children) {
                GetBranch(childNode);
              }

              currentNode = null;
            }
          }

          _branches.Add(thisBranch);
          _branchRadii.Add(thisRadii);

          Profiler.EndSample();
        }


      /*
      ========================
        PATCH MESHES
      ========================
      */
      List<CombineInstance> GetPatchMeshes() {
        Profiler.BeginSample("GetPatchMeshes");

        _patches.Clear();
        _patchRadii.Clear();

        // Recursively populate the _patches array
        foreach(Node rootNode in _rootNodes) {
          GetPatches(rootNode);
        }

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        int t = 0;

        // Create continuous tube meshes for each patch
        foreach(List<Vector3> patch in _patches) {
          _tube.points = new Vector3[patch.Count];
          _tube.radiuses = new float[patch.Count];

          for(int j=0; j<patch.Count; j++) {
            _tube.points[j] = patch[j];
            _tube.radiuses[j] = _patchRadii[t][j];
          }

          _tube.ForceUpdate();

          CombineInstance cb = new CombineInstance();
          cb.mesh = Instantiate(_tube.mesh);  // Instantiate is expensive AF - needs improvement!
          cb.transform = _tube.transform.localToWorldMatrix;
          combineInstances.Add(cb);

          t++;
        }

        Profiler.EndSample();

        return combineInstances;
      }

        private void GetPatches(Node startingNode) {
          Profiler.BeginSample("GetPatches");

          Node currentNode = startingNode;
          List<Vector3> thisPatch;
          List<float> thisRadii;

          while(currentNode != null && currentNode.children.Count > 0) {
            if(currentNode.children.Count == 1) {
              currentNode = currentNode.children[0];

            } else if(currentNode.children.Count > 1) {
              Node previousNode = currentNode.parent == null ? currentNode : currentNode.parent;

              foreach(Node nextNode in currentNode.children) {
                thisPatch = new List<Vector3>();
                thisRadii = new List<float>();

                thisPatch.Add(previousNode.position);
                thisPatch.Add(currentNode.position);
                thisPatch.Add(nextNode.position);

                thisRadii.Add(previousNode.radius);
                thisRadii.Add(currentNode.radius);
                thisRadii.Add(nextNode.radius);

                _patches.Add(thisPatch);
                _patchRadii.Add(thisRadii);

                GetPatches(nextNode);
              }

              currentNode = null;
            }
          }

          Profiler.EndSample();
        }


  /*
  ========================
    GIZMOS
  ========================
  */
  void OnDrawGizmos() {
    Profiler.BeginSample("OnDrawGizmos");

    // Draw a spheres for all attractors
    Gizmos.color = Color.yellow;
    foreach(Attractor attractor in _attractors) {
      Gizmos.DrawSphere(attractor.position, AttractorGizmoRadius);
    }

    // Draw lines to connect each vein node
    // Gizmos.color = Random.ColorHSV();
    Gizmos.color = Color.white;
    foreach(Node node in _nodes) {
      if(node.parent != null) {
        Gizmos.DrawLine(node.parent.position, node.position);
      }

      // Gizmos.DrawSphere(node.position, 1);
    }

    Profiler.EndSample();
  }


  /*
  ========================
    SPATIAL INDEX
  ========================
  */
  public void BuildSpatialIndex() {
    Profiler.BeginSample("BuildSpatialIndex");

    // Create spatial index using _nodePositions
    List<Vector3> nodePositions = new List<Vector3>();

    foreach(Node node in _nodes) {
      nodePositions.Add(node.position);
    }

    _nodeTree = new KDTree(nodePositions.ToArray());

    Profiler.EndSample();
  }


  /*
  ========================
    HELPER FUNCTIONS
  ========================
  */
  private Vector3 GetAverageDirection(Node node, List<Attractor> attractors) {
    Profiler.BeginSample("GetAverageDirection");

    Vector3 averageDirection = new Vector3(0,0,0);

    foreach(Attractor attractor in attractors) {
      Vector3 direction = attractor.position - node.position;
      direction.Normalize();

      averageDirection += direction;
    }

    averageDirection /= attractors.Count;
    averageDirection.Normalize();

    Profiler.EndSample();

    return averageDirection;
  }

  public void AddAttractor(Vector3 position) {
    bool passedBoundsCheck = EnableBounds ? IsInsideBounds(position) : true;
    bool passedObstaclesCheck = EnableObstacles ? !IsInsideAnyObstacle(position) : true;

    if(passedBoundsCheck && passedObstaclesCheck) {
      _attractors.Add(new Attractor(position));
    }
  }

  public int GetAttractorCount() {
    return _attractors.Count;
  }

  public int GetNodeCount() {
    return _nodes.Count;
  }

  public bool GetMeshesReady() {
    return _veinsMesh ? true : false &&
           _tube ? true : false &&
           _filter ? true : false;
  }

  public bool IsInsideBounds(Vector3 point) {
    bool isHit = false;
    RaycastHit hitInfo;

    isHit = Physics.Raycast(
      point,
      (BoundingMesh.transform.position - point).normalized,
      out hitInfo,
      Mathf.Infinity,
      LayerMask.GetMask("Bounds"),
      QueryTriggerInteraction.Ignore
    );

    return !isHit;
  }

  public bool IsInsideAnyObstacle(Vector3 point) {
    bool isInsideObstacle = false;

    foreach(GameObject obstacle in Obstacles) {
      if(obstacle.activeInHierarchy) {
        // Cast a ray from the test point to the center of this obstacle mesh
        hits = Physics.RaycastAll(
          point,  // starting point
          (obstacle.transform.position - point).normalized,   // direction
          (int)Mathf.Ceil(Vector3.Distance(point, obstacle.transform.position)),  // maximum distance
          LayerMask.GetMask("Obstacles")  // layer containing obstacles
        );

        // 0 = point is inside the obstacle
        if(hits.Length == 0) {
          isInsideObstacle = true;
        }
      }
    }

    return isInsideObstacle;
  }


  /*
  ========================
    SCENE
  ========================
  */
  public void ResetScene() {
    // Reset nodes
    _nodes.Clear();
    _rootNodes.Clear();
    CreateRootNodes();
    BuildSpatialIndex();

    // Reset and generate new attractors
    CreateAttractors();

    // Setup meshes
    SetupMeshes();
    CreateMeshes();
  }

    public void Pause() {
      isPaused = true;
    }

    public void Unpause() {
      isPaused = false;
    }


  /*
  =====================
    OBJ EXPORT
    https://forum.unity.com/threads/export-obj-while-runtime.252262/
  =====================
  */
  struct ObjMaterial {
    public string name;
    public string textureName;
  }

  private int vertexOffset = 0;
  private int normalOffset = 0;
  private int uvOffset = 0;

  string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList) {
    Mesh m = mf.mesh;
    Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;
    StringBuilder sb = new StringBuilder();

    sb.Append("g ").Append(mf.name).Append("\n");

    foreach (Vector3 lv in m.vertices) {
      Vector3 wv = mf.transform.TransformPoint(lv);

      //This is sort of ugly - inverting x-component since we're in
      //a different coordinate system than "everyone" is "used to".
      sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
    }

    sb.Append("\n");

    foreach (Vector3 lv in m.normals) {
      Vector3 wv = mf.transform.TransformDirection(lv);
      sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
    }

    sb.Append("\n");

    foreach (Vector3 v in m.uv) {
      sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
    }

    for (int material = 0; material < m.subMeshCount; material++) {
      sb.Append("\n");
      sb.Append("usemtl ").Append(mats[material].name).Append("\n");
      sb.Append("usemap ").Append(mats[material].name).Append("\n");

      //See if this material is already in the materiallist.
      try {
        ObjMaterial objMaterial = new ObjMaterial();

        objMaterial.name = mats[material].name;

        if (mats[material].mainTexture) {
          // objMaterial.textureName = EditorUtility.GetAssetPath(mats[material].mainTexture);
        } else {
          objMaterial.textureName = null;
        }

        materialList.Add(objMaterial.name, objMaterial);
      } catch (ArgumentException) {
        //Already in the dictionary
      }


      int[] triangles = m.GetTriangles(material);
      for (int i = 0; i < triangles.Length; i += 3) {
        //Because we inverted the x-component, we also needed to alter the triangle winding.
        sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                               triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
      }
    }

    vertexOffset += m.vertices.Length;
    normalOffset += m.normals.Length;
    uvOffset += m.uv.Length;

    return sb.ToString();
  }

  void Clear() {
    vertexOffset = 0;
    normalOffset = 0;
    uvOffset = 0;
  }

  Dictionary<string, ObjMaterial> PrepareFileWrite() {
    Clear();
    return new Dictionary<string, ObjMaterial>();
  }

  void MeshToFile(MeshFilter mf) {
    Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

    using (StreamWriter sw = new StreamWriter(Application.dataPath + "/ExportedModels/" + ExportFilename)) {
      sw.Write("mtllib ./veins.mtl\n");
      sw.Write(MeshToString(mf, materialList));
    }
  }

  public void ExportOBJ() {
    MeshToFile(_filter);
  }
}

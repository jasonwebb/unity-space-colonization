using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor {
  public Vector3 position;
  public List<Node> isInfluencing;
  public bool isFresh;
  public bool isReached;

  public Attractor(Vector3 _position) {
    position = _position;

    isInfluencing = new List<Node>();
    isFresh = true;
    isReached = false;
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
  public Vector3 position;
  public bool isTip;
  public Node parent;
  public List<Node> children;
  public float radius;

  public List<Attractor> influencedBy;

  public Node(Vector3 _position, Node _parent, bool _isTip, float _radius) {
    position = _position;
    parent = _parent;
    isTip = _isTip;
    radius = _radius;

    influencedBy = new List<Attractor>();
    children = new List<Node>();
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaporationMapOverride : MonoBehaviour {
    [SerializeField] Texture _map;

    public Texture map => _map;
}

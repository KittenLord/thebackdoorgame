using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Current { get; private set; }
    [field:SerializeField] public Canvas Canvas { get; private set; }
    [field:SerializeField] public Window WindowPrefab { get; private set; }

    void Awake() { Current = this; }
    void Start() {}
}
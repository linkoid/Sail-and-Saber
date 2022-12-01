using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISong
{
    string Name          { get; }
    string Identity      { get; }
    int    IDNumber      { get; }
    int    LoopbackPoint { get; }
    //Object Prefab   { get; }
}

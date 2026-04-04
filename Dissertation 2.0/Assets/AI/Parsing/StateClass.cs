using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public int stateID;
    public int frequency = 1;
    public float trade = 0;
    public float wave = 0;
    public float tower = 0;
    public float back = 0;
}

[Serializable]
public class GameStatesList
{
    public GameState[] states;
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class MGR_game : MonoBehaviour
{
    public static MGR_game game { get; private set; }
    public static MGR_mtx mtx;
    public static MGR_visuals visuals;
    public static MGR_input input;
    public static MGR_levelUI levelUI;

    [SerializeField] MGR_mtx instanceMtx;
    [SerializeField] MGR_visuals instanceVisuals;
    [SerializeField] MGR_input instanceInput;
    [SerializeField] MGR_levelUI instanceLevelUI;

    private void Awake()
    {
        if (game != null) Debug.LogWarning("Replacing game manager");
        game = this;
        mtx = game.instanceMtx;
        visuals = game.instanceVisuals;
        input = game.instanceInput;
        levelUI = game.instanceLevelUI;
    }
}

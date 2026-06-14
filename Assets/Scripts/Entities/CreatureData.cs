using UnityEngine;

[CreateAssetMenu(menuName = "Valenthia/Creatures/Creature Data")]
public class CreatureData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public GameObject Model   => ModelPrefab;
    public float      Speed   => MoveSpeed;
    public float      Radius  => WanderRadius;
    public float      MinIdle => IdleTimeMin;
    public float      MaxIdle => IdleTimeMax;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Visual")]
    [SerializeField] private GameObject ModelPrefab;

    [Header("Movement")]
    [SerializeField] private float MoveSpeed    = 3.5f;
    [SerializeField] private float WanderRadius = 10.0f;
    [SerializeField] private float IdleTimeMin  = 2.0f;
    [SerializeField] private float IdleTimeMax  = 5.0f;
}

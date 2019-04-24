using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    public enum State
    {
        Normal, //Normal Default state
        Impaired, //Deal half damage next attack
        Steamed, //Dealing burn damage for a one turn
        Frozen, //Skip next attack
    }

    public State Current_State;

    public enum Affinity
    {
        None,
        Fire,
        Water,
        Earth,
        Air,
    }

    public Affinity affinity;
    public Affinity weakness;

    public int HP = 0;
    public int Dmg = 0;

    public void setState(State new_State)
    {
        Current_State = new_State;
    }
    public State getState()
    {
        return Current_State;
    }

    public void Start()
    {
        Current_State = State.Normal;
    }
}

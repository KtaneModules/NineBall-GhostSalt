using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class NineBallScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] OtherBalls;
    public KMSelectable One;
    public KMSelectable Nine;
    public KMSelectable[] Balls;
    public Material[] Materials;

    private int[] RndBallNums = { 1, 2, 3, 4, 5, 6, 7 };
    private bool[] BreakSinks = new bool[9];
    private bool[] Potted = new bool[9];
    private int[] Ref = new int[9];
    private int[] RefDos = new int[9];

    private KMSelectable.OnInteractHandler BallInteract(int pos)
    {
        return delegate
        {
            BallPot(pos);
            Balls[pos].AddInteractionPunch(0.5f);
            Audio.PlaySoundAtTransform("press", Balls[pos].transform);
            return false;
        };
    }
    // Hotel? Trivago.
    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < Balls.Length; i++)
        {
            Balls[i].OnInteract += BallInteract(i);
        };
        RndBallNums = RndBallNums.Shuffle();
        for (int i = 0; i < OtherBalls.Length; i++)
        {
            OtherBalls[i].GetComponent<MeshRenderer>().material = Materials[RndBallNums[i]];
        }
    }

    // caleb carlton <3 the jewel vault o.o
    void Start() {
        CalcSinks();
    }

    void CalcSinks()
    {
        RefDos = (new int[] { 0 }).Concat(RndBallNums).ToArray().Concat(new int[] { 8 }).ToArray();
        for (int i = 0; i < 7; i++)
        {
            Ref[RndBallNums[i]] = i + 1;
        }
        Ref[8] = 8;
        BreakSinks = new bool[9];
        if (RndBallNums[0] > 4)
        {
            BreakSinks[1] = true;
        }
        if (RndBallNums[0] > RndBallNums[1])
        {
            BreakSinks[2] = true;
        }
        if (RndBallNums[2] > (Bomb.GetSerialNumberNumbers().Last() - 1))
        {
            BreakSinks[3] = true;
        }
        if (RndBallNums[3] == 1 || RndBallNums[3] == 2 || RndBallNums[3] == 4 || RndBallNums[3] == 6)
        {
            BreakSinks[4] = true;
        }
        if (RndBallNums[2] == (RndBallNums[4] + 1) || RndBallNums[2] == (RndBallNums[4] - 1) || RndBallNums[6] == (RndBallNums[4] + 1) || RndBallNums[6] == (RndBallNums[4] - 1))
        {
            BreakSinks[5] = true;
        }
        if ((RndBallNums[3] - RndBallNums[6]) > 2 || (RndBallNums[6] - RndBallNums[3]) > 2)
        {
            BreakSinks[6] = true;
        }
        if (!(RndBallNums[4] > 5) && !(RndBallNums[5] > 5))
        {
            BreakSinks[7] = true;
        }
        if ((Bomb.GetSerialNumberNumbers().Last() % 2) == 1)
        {
            BreakSinks[0] = true;
        }
        Debug.LogFormat("[9-Ball #{0}] The array of balls in reading order is {1}, 9, {2} and 1.", _moduleID, RndBallNums.Select(x => x + 1).ToArray().Take(4).Join(", "), RndBallNums.Select(x => x + 1).ToArray().Skip(4).Join(", "));
        if (BreakSinks.Count(x => x) == 0)
        {
            Debug.LogFormat("[9-Ball #{0}] There are no valid break balls.", _moduleID);
        }
        Debug.LogFormat("[9-Ball #{0}] The valid break ball(s) is/are {1}.", _moduleID, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.Where(x => BreakSinks[Ref[x]]).Select(x => x + 1).Join(", "));
    }
    void BallPot(int pos)
    {
        if (BreakSinks.Count(x => x) <= Potted.Count(x => x))
        {
            bool Memory = false;
            for (int i = 0; i < RefDos[pos]; i++)
            {
                if (!Potted[Ref[i]])
                {
                    Memory = true;
                }
            }
            if (Memory)
            {
                Module.HandleStrike();
                Debug.LogFormat("[9-Ball #{0}] You have potted a ball out of order, causing the module to strike and reset. :(", _moduleID);
                for (int i = 0; i < Balls.Length; i++)
                {
                    Balls[i].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                }
                RndBallNums = RndBallNums.Shuffle();
                for (int i = 0; i < OtherBalls.Length; i++)
                {
                    OtherBalls[i].GetComponent<MeshRenderer>().material = Materials[RndBallNums[i]];
                }
                CalcSinks();
                Potted = new bool[9];
            }
            else
            {
                Balls[pos].transform.localScale = new Vector3(0f, 0f, 0f);
                Potted[pos] = true;
                if (Potted.Count(x => x) == 9)
                {
                    Module.HandlePass();
                    Debug.LogFormat("[9-Ball #{0}] You have cleared the table, so the module is now solved. Poggers!", _moduleID);
                }
            }
        }
        else
        {
            if (BreakSinks[pos])
            {
                Balls[pos].transform.localScale = new Vector3(0f, 0f, 0f);
                Potted[pos] = true;
            }
            else
            {
                Module.HandleStrike();
                Debug.LogFormat("[9-Ball #{0}] You have potted an incorrect ball, causing the module to strike and reset. :(", _moduleID);
                for (int i = 0; i < Balls.Length; i++)
                {
                    Balls[i].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                }
                RndBallNums = RndBallNums.Shuffle();
                for (int i = 0; i < OtherBalls.Length; i++)
                {
                    OtherBalls[i].GetComponent<MeshRenderer>().material = Materials[RndBallNums[i]];
                }
                CalcSinks();
                Potted = new bool[9];
            }
        }
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} pot 3 5' to pot the balls labelled 3 and 5.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] CommandArray = command.Split(' ');
        if (CommandArray[0] != "pot" || CommandArray.Length == 1)
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        for (int i = 1; i < CommandArray.Length; i++)
        {
            if (!"123456789".Contains(CommandArray[i][0]) || CommandArray[i].Length > 1)
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
        }
        yield return null;
        for (int i = 1; i < CommandArray.Length; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if ("123456789"[j] == CommandArray[i][0])
                {
                    if (!Potted[Ref[j]])
                    {
                        Balls[Ref[j]].OnInteract();
                        yield return new WaitForSeconds(0.1f); ;
                    }
                    else
                    {
                        yield return "sendtochaterror You have tried to pot a non-existent ball. Please try again.";
                        yield break;
                    }
                }
            }
        }
        yield return null;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        for (int i = 0; i < 9; i++)
        {
            if (BreakSinks[i] && !Potted[i])
            {
                Balls[i].OnInteract();
                yield return true;
            }
        }
        for (int i = 0; i < 9; i++)
        {
            if (!Potted[Ref[i]])
            {
                Balls[Ref[i]].OnInteract();
                yield return true;
            }
        }
    }
    // pog
}

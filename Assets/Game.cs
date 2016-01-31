using UnityEngine;
using System.Collections.Generic;
using FableLabs.Anim;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Linq;

public enum RuneColor
{
    Red,
    Blue,
    Green,
    Orange
}


public class Card
{
    public static string Symbols = "-|<>v^ZO";
    public static string Colors = "rbgo";

    private static Color[] _colors = { 
                                         new Color(1.0f, 0.0f, 0.0f, 1.0f),
                                         new Color(0.0f, 1.0f, 0.0f, 1.0f),
                                         new Color(0.0f, 0.0f, 1.0f, 1.0f),
                                         new Color(1.0f, 0.7f, 0.0f, 1.0f),
                                     };
    public static RuneColor[] _symbolColor = 
                                    { 
                                        RuneColor.Red, RuneColor.Red, 
                                        RuneColor.Blue, RuneColor.Blue, 
                                        RuneColor.Green, RuneColor.Green, 
                                        RuneColor.Orange, RuneColor.Orange
                                    };
    public static Color GetColor (RuneColor rc) { return _colors[(int)rc]; }
    public static string GetColorName (RuneColor rc) { return new string(new char[] { Colors[(int)rc]} ); }

    public string Symbol;
    public RuneColor Color;
    public Card(string sym)
    {
        Symbol = sym;
        Color = _symbolColor[Symbols.IndexOf(sym)];
    }
}

public static class Spell
{
    public static Dictionary<string, Action<Game>> _spells = new Dictionary<string, Action<Game>>
    {
        { "r", Fire },
        { "b", Counter },
        { "g", Grow },
        { "og", BoostGrow },
        { "ooog", BoostGrow },
    };

    public static void Fire(Game game)
    {
        var leaves = game.Enemy.Root.Nodes.Where(n => n.IsLeaf).ToArray();
        if(leaves.Length == 0) { return; }
        var leaf = leaves[Random.Range(0, leaves.Length)];
        leaf.Destroy();
    }

    public static void Grow(Game game)
    {
        //game.Self.Root.Grow(new Card(game.Self.Selection.Symbol));
    }

    public static void Counter(Game game)
    {

    }

    public static void BoostGrow(Game game)
    {
        Grow(game);
        Grow(game);
    }
}

public class Deck
{
    public List<Card> _cards = new List<Card>();
    public Deck(int size)
    {
        for(int i=0;i<size;i++)
        {
            var c = new Card(new string( new char[] { Card.Symbols[Random.Range(0, Card.Symbols.Length)] }));
            _cards.Add(c);
        }
    }

    public Card Draw()
    {
        var c = _cards[0];
        _cards.RemoveAt(0);
        return c;
    }
}

public class AI
{
    public void Choose(GamePlayer player)
    {
        player.Selection = player.Hand[Random.Range(0, player.Hand.Count)];
    }
}

public class TreeNode
{
    public TreeNode Parent;
    public List<TreeNode> Children = new List<TreeNode>();
    public Card Symbol;
    public int Index = 0;
    public int Depth = 0;

    public TreeNode()
    {
        Depth = -1;
    }

    public TreeNode(TreeNode parent, int index)
    {
        Parent = parent;
        Parent.Children.Add(this);
        Index = index;
        Depth = Parent.Depth + 1;
    }

    public void Destroy()
    {
        if (Parent == null) { return; }
        Parent.Children.Remove(this);
    }

    public bool IsLeaf { get { return Children.Count == 0; }}

    public IEnumerable<TreeNode> Nodes
    {
        get
        {
            yield return this;
            foreach(var c in Children)
            {
                foreach(var n in c.Nodes)
                {
                    yield return n;
                }
            }
        }
    }
}

public class GamePlayer
{
    public Deck Pool;
    public List<Card> Hand = new List<Card>();
    public TreeNode Root;
    public TreeNode Cursor;
    public Transform Tree;
    public Card Selection;

    public Card RootCard { get { return Nodes.Count > 0 ? Nodes[0].Symbol : null; } }

    public List<TreeNode> Nodes;

    private void CreateTree()
    {
        Root = new TreeNode();
        int i=0;
        var n0 = new TreeNode(Root, i++);
        var n1 = new TreeNode(n0, i++);
        var n2 = new TreeNode(n0, i++);
        var n3 = new TreeNode(n1, i++);
        var n4 = new TreeNode(n1, i++);
        var n5 = new TreeNode(n2, i++);
        var n6 = new TreeNode(n2, i++);
        var n7 = new TreeNode(n3, i++);
        var n8 = new TreeNode(n3, i++);
        var n9 = new TreeNode(n4, i++);
        var n10 = new TreeNode(n4, i++);
        var n11 = new TreeNode(n5, i++);
        var n12 = new TreeNode(n5, i++);
        var n13 = new TreeNode(n6, i++);
        var n14 = new TreeNode(n6, i++);
        Nodes = new List<TreeNode> { n0, n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14 };
    }

    public TreeNode GetFirstOpenNode()
    {
        for(int i=0;i<Nodes.Count;i++)
        {
            if (Nodes[i].Symbol == null) { return Nodes[i]; }
        }
        return null;
    }

    public void Draw()
    {
        Hand.Clear();
        for (int i = 0; i < Game.HandSize; i++)
        {
            var c = Pool.Draw();
            bool repeat = false;
            if (RootCard != null && c.Symbol == RootCard.Symbol) { repeat = true; }
            for (int j = 0; j < Hand.Count; j++)
            {
                if (Hand[j].Symbol == c.Symbol) { repeat = true; }
            }
            if (repeat) { i--; continue; }
            Hand.Add(c);
            if(Id == "Player")
            {
                var cui = GameObject.Find(string.Format("Card{0}", Hand.Count));
                var cuit = cui.transform.FindChild("Text").GetComponent<Text>();
                cuit.text = c.Symbol;
            }
        }
    }

    public Card GetHandCard(string sym)
    {
        for (int i = 0; i < Hand.Count; i++)
        {
            if (Hand[i].Symbol == sym) { return Hand[i]; }
        }
        return null;
    }

    public void ChooseIndex(int i)
    {
        // TODO: turn to spell
        Selection = Hand[i];
        Debug.Log("Selected " + i+ " "+Selection.Symbol);
    }

    public void Build(Card c)
    {
        var prefab = Resources.Load<GameObject>("Card");
        var ci = Object.Instantiate<GameObject>(prefab);

        var n = GetFirstOpenNode();
        n.Symbol = c;
        //Debug.Log("Node ID " + id);
        ci.transform.SetParent(Tree.Find(n.Index.ToString()));

        var rt = ci.GetComponent<RectTransform>();
        var cit = ci.GetComponentInChildren<Text>();
        cit.text = c.Symbol;
        rt.offsetMax = new Vector2(20f, 30f);
        rt.offsetMin = new Vector2(-20f, -30f);
    }

    public void DoMove()
    {
        if (Selection != null)
        {
            Debug.Log(Id + " Cast " + Selection.Symbol);
            Build(Selection);
        }
        Selection = null;
    }

    public string Id;
    public GamePlayer(string id)
    {
        Id = id;
        Pool = new Deck(250);
        CreateTree();
    }
}

public class Game : MonoBehaviour 
{
    public Slider Timer;
    public static float TurnTime = 4.0f;
    public static int HandSize = 3;

    public Transform SelfTree;
    public Transform EnemyTree;
    public GamePlayer Self;
    public GamePlayer Enemy;
    public int Phase;
    public Action<Action>[] Phases;
    public AI Computer = new AI();

    public void Start()
    {
        Phase = 0;
        Self = new GamePlayer("Player");
        Enemy = new GamePlayer("Enemy");
        Self.Tree = SelfTree;
        Enemy.Tree = EnemyTree;
        Phases = new Action<Action>[] { StartTurn, StartMove, EndMove, EndTurn };
        NextPhase();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) { ChooseIndex(0); }
        if (Input.GetKeyDown(KeyCode.S)) { ChooseIndex(1); }
        if (Input.GetKeyDown(KeyCode.D)) { ChooseIndex(2); }
    }

    public void ChooseIndex(int i)
    {
        Self.ChooseIndex(i);
    }

    private void StartTurn(Action next)
    {
        Self.Draw();
        Enemy.Draw();
        next();
    }

    private void StartMove(Action next)
    {
        Tween.FromTo<float>(Timer, f => Timer.value = f, 1.0f, 0.0f, 4.0f)
            .Curve(null)
            .OnComplete(next);
        Computer.Choose(Enemy);
    }

    private void EndMove(Action next)
    {
        //Enemy.ChooseIndex(Random.Range(0, 3));
        Self.DoMove();
        Enemy.DoMove();
        
        next();
    }

    private void EndTurn(Action next)
    {
        next();
    }

    private void NextPhase()
    {
        var cp = Phases[Phase++ % Phases.Length];
        var np = Phases[Phase % Phases.Length];
        cp(NextPhase);
    }
}

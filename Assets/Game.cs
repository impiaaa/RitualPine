using UnityEngine;
using System.Collections.Generic;
using FableLabs.Anim;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine.Networking;

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
                                         new Color(0.0f, 0.0f, 1.0f, 1.0f),
                                         new Color(0.0f, 1.0f, 0.0f, 1.0f),
                                         new Color(1.0f, 0.7f, 0.0f, 1.0f),
                                     };
    public static RuneColor[] _symbolColor = 
                                    { 
                                        RuneColor.Red, RuneColor.Red, 
                                        RuneColor.Blue, RuneColor.Blue, 
                                        RuneColor.Green, RuneColor.Green, 
                                        RuneColor.Orange, RuneColor.Orange
                                    };
    public static Color GetColor(string sym) { return GetColor(GetSymbolColor(sym)); }
    public static Color GetColor(RuneColor rc) { return _colors[(int)rc]; }
    public static string GetColorName(RuneColor rc) { return new string(new char[] { Colors[(int)rc] }); }
    public static RuneColor GetSymbolColor(string sym) {
        return _symbolColor[Symbols.IndexOf(sym)];
    }

    public string Symbol;
    public RuneColor Color;

    private Rune _rune;
    public Rune Rune { get { return _rune; } set { _rune = value; if (_rune != null) { _rune.SetGlyph(this); _rune.Selected = _active; } } }
    private bool _active;
    public bool Active { get { return _active; } set { _active = value; if (_rune != null) { _rune.Selected = _active; } } }
    public Card(string sym)
    {
        Symbol = sym;
        Color = _symbolColor[Symbols.IndexOf(sym)];
    }


}

public static class Spell
{
    public static Dictionary<string, Action<Env>> _spells = new Dictionary<string, Action<Env>>
    {
        { "r", Fire() },  { "b", Counter() },  { "g", Grow() },  { "o", Fire().Repeat(1) },
        { "rr", Fire() },  { "rb", Fire() },  { "rg", Grow() },  { "ro", Fire() },
        { "br", Fire().And(Counter()) },  { "bb", Counter() },  { "bg", Grow() },  { "bo", Fire() },
        { "gr", Fire() },  { "gb", Counter() },  { "gg", Grow() },  { "go", Fire() },
        { "or", Fire() },  { "ob", Counter() },  { "og", Grow() },  { "oo", Fire() },
        { "rrr", Fire() },  { "rrb", Counter() },  { "rrg", Grow() },  { "rro", Fire() },
        { "rbr", Fire() },  { "rbb", Counter() },  { "rbg", Grow() },  { "rbo", Fire() },
        { "rgr", Fire() },  { "rgb", Counter() },  { "rgg", Grow() },  { "rgo", Fire() },
        { "ror", Fire() },  { "rob", Counter() },  { "rog", Grow() },  { "roo", Fire() },
        { "brr", Fire() },  { "brb", Counter() },  { "brg", Grow() },  { "bro", Fire() },
        { "bbr", Fire() },  { "bbb", Counter() },  { "bbg", Grow() },  { "bbo", Fire() },
        { "bgr", Fire() },  { "bgb", Counter() },  { "bgg", Grow() },  { "bgo", Fire() },
        { "bor", Fire() },  { "bob", Counter() },  { "bog", Grow() },  { "boo", Fire() },
        { "grr", Fire() },  { "grb", Counter() },  { "grg", Grow() },  { "gro", Fire() },
        { "gbr", Fire() },  { "gbb", Counter() },  { "gbg", Grow() },  { "gbo", Fire() },
        { "ggr", Fire() },  { "ggb", Counter() },  { "ggg", Grow() },  { "ggo", Fire() },
        { "gor", Fire() },  { "gob", Counter() },  { "gog", Grow() },  { "goo", Fire() },
        { "orr", Fire() },  { "orb", Counter() },  { "org", Grow() },  { "oro", Fire() },
        { "obr", Fire() },  { "obb", Counter() },  { "obg", Grow() },  { "obo", Fire() },
        { "ogr", Fire() },  { "ogb", Counter() },  { "ogg", Grow() },  { "ogo", Fire() },
        { "oor", Fire() },  { "oob", Counter() },  { "oog", Grow() },  { "ooo", Fire() },
    };

    public static void Cast(Env env, string spell)
    {
        if (spell.Length > 3) { spell = spell.Substring(spell.Length - 3); }
        var sp = _spells[spell];
        Debug.Log(env.Self.Id + " Cast " + spell);
        sp(env);
    }

    public static Action<Env> And(this Action<Env> a, Action<Env> b)
    {
        return env => { a(env); b(env); };
    }

    public static Action<Env> Repeat(this Action<Env> a, int count = 1)
    {
        return env =>
        {
            for (int i = 0; i < count; i++) { a(env); }
        };
    }

    public static Action<Env> Boost(int count = 0)
    {
        return env => 
        {
            AudioBank.PlayGlobal("buzz"); // backlash, raze
        };
    }

    public static Action<Env> Fire()
    {
        return env => 
        {
            var leaves = env.Enemy.Nodes.Where(n => n.IsLeaf).ToArray();
            if(leaves.Length == 0) { return; }
            var leaf = leaves[Random.Range(0, leaves.Length)];
            env.Enemy.Destroy(leaf);
            AudioBank.PlayGlobal("hit");
        };
    }

    public static Action<Env> Grow()
    {
        return env =>
        {
            env.Self.Build(env.Self.FirstCard);
            AudioBank.PlayGlobal("grow");
        };
        
    }

    public static Action<Env> Counter()
    {
        return env =>
        {
            AudioBank.PlayGlobal("counter");
        };
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
        player.Selection.Add(player.Hand[Random.Range(0, player.Hand.Count)]);
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

    public bool IsLeaf { get { return Symbol != null && (Children.Count == 0 || Children.All(cn => cn.Symbol == null)); }}

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
    public Transform HandTransform;
    public List<Card> Selection = new List<Card>();
    public bool SpellClosed = false;
    public Env View;

    public Card RootCard { get { return Nodes.Count > 0 ? Nodes[0].Symbol : null; } }
    public Card FirstCard { get { return Selection.Count > 0 ? Selection[0] : null; } }
    public Card LastCard { get { return Selection.Count > 0 ? Selection[Selection.Count-1] : null; } }

    public List<TreeNode> Nodes;

    public string SelectionSpell 
    {
        get 
        {
            string sp = "";
            for(int i=0;i<Selection.Count;i++)
            {
                sp += Card.GetColorName(Selection[i].Color);
            }
            return sp;
        }
    }
    
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
                var cui = HandTransform.GetChild(i);
                c.Rune = cui.GetComponent<Rune>();
            }
        }
        for (int i = 0; i < Nodes.Count; i++) { if(Nodes[i].Symbol != null) { Nodes[i].Symbol.Active = false; } }
        Cursor = Root;
    }

    public void Destroy(TreeNode n)
    {
        Debug.Log("Destroying " + n.Index);
        n.Symbol = null;
        var t = Tree.Find(n.Index.ToString());
        for(int i=0;i<t.childCount;i++)
        {
            var ti = t.GetChild(i);
            Tween.FromTo<float>(ti, f => ti.localEulerAngles = new Vector3(0, 0, f * 45f), 0f, 5f, 1f);
            Tween.FromTo<float>(ti, f => ti.localScale = new Vector3(f, f, f), 1f, 0f, 1f).OnComplete(() =>
            {
                Object.Destroy(ti.gameObject);
            });
        }
    }

    public void Build(Card c)
    {

        var n = GetFirstOpenNode();
        if (n == null) { return; }
        var nc = new Card(c.Symbol);
        n.Symbol = nc;
        //Debug.Log("Node ID " + id);
        UpdateNodeVisual(n);
    }

    public void UpdateNodeVisual(TreeNode n)
    {
        var prefab = Resources.Load<GameObject>("Card");
        var ci = Object.Instantiate<GameObject>(prefab);

        var nc = n.Symbol;
        ci.transform.SetParent(Tree.Find(n.Index.ToString()));

        nc.Active = false;
        var rt = ci.GetComponent<RectTransform>();
        nc.Rune = ci.GetComponentInChildren<Rune>();
        rt.offsetMax = new Vector2(20f, 30f);
        rt.offsetMin = new Vector2(-20f, -30f);
        Tween.FromTo<float>(rt, f => rt.localScale = new Vector3(f, f, f), 0.0f, 1.0f, 0.3f);
    }

    public void DoMove()
    {
        if (Selection.Count > 0)
        {
            Spell.Cast(View, SelectionSpell);
            Build(LastCard);
        }
        else
        {
            AudioBank.PlayGlobal("fail");
        }
        Clear();
    }

    public void Clear()
    {
        Selection.Clear();
        SpellClosed = false;

    }

    public void CastGlyph(string str)
    {
        var sc = Card.GetSymbolColor(str);
        var tn = Cursor.Children.FirstOrDefault(nc => nc.Symbol != null && nc.Symbol.Color == sc);
        if (tn != null) { CastGlyph(tn.Symbol); }
        else { CastGlyph(Hand.FirstOrDefault(h => h.Color == sc)); }
    }

    public void CastGlyph(Card c)
    {
        if (c == null) { return; }
        if (SpellClosed) { return; }
        var valid = false;
        if (Hand.Contains(c) && Selection.Count == 0) { SpellClosed = true; valid = true; }
        else
        {
            var tn = Cursor.Children.FirstOrDefault(n => n.Symbol == c);
            if (tn != null) { Cursor = tn; valid = true; }
        }
        if (valid) 
        {
            if (Cursor.Children.Count == 0) { SpellClosed = true; }
            Selection.Add(c); 
            c.Active = true;
        }
    }
    
    public string Id;
    public GamePlayer(string id)
    {
        Id = id;
        Pool = new Deck(250);
        CreateTree();
    }
}

public class Env
{
    public GamePlayer Self;
    public GamePlayer Enemy;
}

public class NodeMessage : MessageBase
{
    public int Index;
    public string Symbol;
}

public class Game : MonoBehaviour 
{
    public Slider Timer;
    public static float TurnTime = 4.0f;
    public static int HandSize = 3;

    public Transform SelfTree;
    public Transform EnemyTree;
    public Transform SelfHand;
    public Transform EnemyHand;
    public GamePlayer Self;
    public GamePlayer Enemy;
    public int Phase;
    public Action<Action>[] Phases;
    public AI Computer = new AI();
    public bool Networked = true;

	public enum PhaseNames {
		START_TURN_PHASE = 0,
		START_MOVE_PHASE = 1,
		END_MOVE_PHASE = 2,
		END_TURN_PHASE = 3
	}

    public void Start()
    {
        Phase = -1;
        Self = new GamePlayer("Player");
        Enemy = new GamePlayer("Enemy");
        Self.Tree = SelfTree;
        Enemy.Tree = EnemyTree;
        Self.HandTransform = SelfHand;
        Enemy.HandTransform = EnemyHand;
        Self.View = new Env { Self = Self, Enemy = Enemy };
        Enemy.View = new Env { Self = Enemy, Enemy = Self };
        AudioBank.PlayGlobal("bgm");

        Phases = new Action<Action>[] { StartTurnPhase, StartMovePhase, EndMovePhase, EndTurnPhase };
        if (!Networked) { OnPhaseChange(0); }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { Self.CastGlyph("|"); }
        if (Input.GetKeyDown(KeyCode.B)) { Self.CastGlyph("<"); }
        if (Input.GetKeyDown(KeyCode.G)) { Self.CastGlyph("^"); }
        if (Input.GetKeyDown(KeyCode.O)) { Self.CastGlyph("O"); }

        if (Input.GetKeyDown(KeyCode.Space)) { OnPhaseChange((Phase+1) % Phases.Length); }
    }

    private void StartTurnPhase(Action next)
    {
        Self.Draw();
        Enemy.Draw();
        next();
    }

    #region Networking
    public void OnTimerUpdate(float time)
    {
        Timer.value = time;
    }
    public void OnPhaseChange(int phase)
    {
        Debug.Log("Phase change " + phase);
        Phase = phase % Phases.Length;
        var cp = Phases[Phase];
        cp(NextPhase);
    }
    public void OnEnemyMove(string spell)
    {
        for(int i=0;i<spell.Length;i++)
        {
            Enemy.CastGlyph(spell.Substring(i, 1));
        }
    }
    public void OnNodeChange(NodeMessage msg)
    {
        var n = Self.Nodes[msg.Index];
        n.Symbol = new Card(msg.Symbol);
        Self.UpdateNodeVisual(n);
    }
    #endregion

    private void StartMovePhase(Action next)
    {
        Tween.FromTo<float>(Timer, f => Timer.value = f, 1.0f, 0.0f, 4.0f)
            .Curve(null)
            .OnComplete(next);
        if (!Networked) { Computer.Choose(Enemy); }
    }

    private void EndMovePhase(Action next)
    {
        Self.DoMove();
        //Enemy.Clear();
        Enemy.DoMove();
        next();
    }

    private void EndTurnPhase(Action next)
    {
        // test win condition here
        next();
    }

    private void NextPhase()
    {
        if (Networked) { Debug.Log("Phase done, waiting... "); }
        else { OnPhaseChange(Phase+1 % Phases.Length); }
    }
}

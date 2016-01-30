using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkedPlayer : NetworkBehaviour
{
    [Command]
    void CmdSubmitSpell(string spell)
    {
        print(spell);
    }
}

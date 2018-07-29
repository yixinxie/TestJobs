using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface INeutralObject {
    void onServerInitialized();
}
public class PendingNetworkObjects : MonoBehaviour {
    public static PendingNetworkObjects self;
    List<INeutralObject> npcs;
    private void Awake() {
        self = this;
        npcs = new List<INeutralObject>();
    }
    public void registerNPC(INeutralObject _char) {
        npcs.Add(_char);
    }
    public List<INeutralObject> getNPCs() {
        return npcs;
    }
}

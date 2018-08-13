using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControll : MonoBehaviour {
    public static UIControll self;
    public RectTransform group;

    public RectTransform revealButton;
    public GameObject structureGhost;
    public RectTransform beltButton;
    public RectTransform beltExitButton;
    byte buildPhase;
    int buildStructureId;
    private void Awake() {
        self = this;
        group.gameObject.SetActive(false);
        structureGhost.SetActive(false);
        beltExitButton.gameObject.SetActive(false);

    }
    public void onBuildPressed() {
        revealButton.gameObject.SetActive(false);
        group.gameObject.SetActive(true);
        buildPhase = 1;
    }
    public void onBuildReleased() {
        revealButton.gameObject.SetActive(true);
        group.gameObject.SetActive(false);
        structureGhost.SetActive(true);
        if (buildPhase == 1)
            buildPhase = 2;
    }
    public byte getBuildPhase() {
        return buildPhase;
    }
    public void resetBuildEvent() {
        buildPhase = 0;
        structureGhost.SetActive(false);
    }
    public void setBuildStructure(int idx) {
        buildStructureId = idx;
    }
    public int getBuildStructure() {
        return buildStructureId;
    }
    public void onBeltPressed() {
        beltButton.gameObject.SetActive(false);
        beltExitButton.gameObject.SetActive(true);
        buildPhase = 3;
    }
    public void onBeltModeEnded() {
        beltButton.gameObject.SetActive(true);
        beltExitButton.gameObject.SetActive(false);
        buildPhase = 0;
    }
    private void Update() {
        if(buildPhase == 2) {

            structureGhost.transform.position = CamControl.self.pointedAt;
        }
    }

}

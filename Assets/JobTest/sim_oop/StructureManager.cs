using Simulation_OOP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// one or more of this form the branches of an output.
public class StructureLayout {
    public int x_max;
    public int y_max;
    public int z_max;
    public ISimData[] list;
}
public class StructureLayoutManager {
    List<StructureLayout> layouts;
    public StructureLayoutManager() {
        layouts = new List<StructureLayout>();
    }


}
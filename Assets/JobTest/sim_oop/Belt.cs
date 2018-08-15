using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Belt : MonoBehaviour {
        public BeltData target;
        public Vector3[] path;
        public MeshFilter mfilter;
        public Transform[] targets;
        private void Awake() {

            path = new Vector3[16];
        }
        private void Start() {

        }
        Vector3[] four = new Vector3[4];
        public void setPath(Vector3[] positions, int positionsLength, float smoothness, Vector3 basePos, List<Vector3> verts, List<int> indices) {
            int steps = 10;
            Vector3[] ctrlPos;
            
            setPathPoints(positions, positionsLength, smoothness, out ctrlPos);
            Vector3 lastleft = Vector3.zero;
            Vector3 lastright = Vector3.zero;
            int vert_inc = 0;
            int index_inc = 0;
            verts.Clear();
            indices.Clear();

            for (int j = 0; j < ctrlPos.Length / 2; ++j) {
                four[0] = positions[j] - basePos;
                four[1] = ctrlPos[j * 2] - basePos;
                four[2] = ctrlPos[j * 2 + 1] - basePos;
                four[3] = positions[j + 1] - basePos;

                Vector3 lastPos = BezierStatic.evalBezier(four[0], four[1], four[2], four[3], 0);

                for (int i = 1; i <= steps; i++) {
                    float t = (float)i / (float)steps;
                    Vector3 thisPos = BezierStatic.evalBezier(four[0], four[1], four[2], four[3], t);
                    Vector3 dir = thisPos - lastPos;
                    dir.Normalize();
                    dir = Vector3.Cross(dir, Vector3.up);
                    dir.Normalize();
                    Vector3 right = thisPos - dir;
                    Vector3 left = thisPos + dir;
                    if (i == 1 && j == 0) {
                        lastright = lastPos - dir;
                        lastleft = lastPos + dir;

                        verts.Add(lastleft);
                        verts.Add(lastright);
                        vert_inc += 2;
                    }

                    verts.Add(left);
                    verts.Add(right);

                    indices.Add(vert_inc - 1);
                    indices.Add(vert_inc - 2);
                    indices.Add(vert_inc);
                    indices.Add(vert_inc + 1);
                    indices.Add(vert_inc - 1);
                    indices.Add(vert_inc);

                    vert_inc += 2;
                    index_inc += 6;

                    //Debug.DrawLine(lastPos, thisPos, Color.black);
                    //Debug.DrawLine(lastleft, left, Color.green);
                    //Debug.DrawLine(lastright, right, Color.green);

                    lastleft = left;
                    lastright = right;
                    lastPos = thisPos;

                }
            }

        }
        public static void setPathPoints(Vector3[] pts, int positionsLength, float smoothness, out Vector3[] controlPts) {

            controlPts = new Vector3[(positionsLength - 1) * 2];
            Vector3 curPos = pts[0];
            Vector3 nextPos = pts[1];
            Vector3 lastdiff = nextPos - curPos;
            lastdiff.Normalize();
            controlPts[0] = pts[0] + lastdiff * smoothness;
            for (int i = 1; i < positionsLength - 1; ++i) {
                curPos = pts[i];
                nextPos = pts[i + 1];
                Vector3 diff = nextPos - curPos;
                diff.Normalize();
                Vector3 avg = (diff + lastdiff) / 2f;
                avg.Normalize();
                controlPts[i * 2 - 1] = pts[i] - avg * smoothness;
                controlPts[i * 2] = pts[i] + avg * smoothness;

                //Debug.DrawLine(controlPts[i * 2 - 1], controlPts[i * 2], Color.red);
                lastdiff = diff;

            }
            controlPts[(positionsLength - 1) * 2 - 1] = pts[positionsLength - 1] - lastdiff * smoothness;
        }
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();
        int[] tmpIndices;
        public void Update() {

            for (int i = 0; i < targets.Length; ++i) {
                path[i] = targets[i].position;
            }
            mfilter.mesh.GetVertices(verts);

            mfilter.mesh.GetIndices(indices, 0);
            setPath(path, targets.Length, 2f, path[0], verts, indices);
            mfilter.mesh.SetVertices(verts);
            if (tmpIndices == null || tmpIndices.Length != indices.Count) {
                tmpIndices = new int[indices.Count];
            }
            for(int i = 0; i < indices.Count; ++i) {
                tmpIndices[i] = indices[i];
            }
            mfilter.mesh.SetIndices(tmpIndices, MeshTopology.Triangles, 0);
        }
    }
}
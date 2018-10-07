using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Belt : MonoBehaviour, ISimView {
        public BeltData target;
        public List<Vector3> path;
        public MeshFilter mfilter;
        public Mesh itemMesh;
        public Material itemMat;
        public MeshCollider meshCol;
        Matrix4x4[] matrices;
        // debug
        public float[] debugPos;
        public int count;
        Vector3[] four = new Vector3[4];
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();
        int[] tmpIndices;
        private void Awake() {

            path = new List<Vector3>();
        }
        private void Start() {

        }
        public ISimData getTarget() {
            return target;
        }
        private void Update() {
            debugPos = target.positions;
            count = target.count;
            Vector3 selfPos = transform.position;
            if (count == 0) return;

            if(matrices == null || matrices.Length < count) {
                matrices = new Matrix4x4[count * 2];
            }
            
            float[] positions = target.positions;
            
            //Vector3 from = path[0];
            //Vector3 to = path[path.Count - 1];
            //for (int j = 0; j < count; ++j) {
            //    Vector3 thisPos = Vector3.Lerp(from, to, positions[j]/ target.tubeLength) + selfPos;
            //    matrices[j] = Matrix4x4.TRS(thisPos, Quaternion.identity, Vector3.one);
            //}
            int j = 0;
            float accum = 0f;
            for (int i = 0; i < count; ++i) {
                float pos = positions[i] - accum;
                for(; j < path.Count - 1; j++) {
                    float thisDist = Vector3.Distance(path[j], path[j + 1]);
                    if (pos <= thisDist) {
                        Vector3 thisPos = Vector3.Lerp(path[j], path[j + 1], pos / thisDist) + selfPos;
                        //Debug.DrawLine(path[j] + Vector3.up * 3f, path[j + 1] + Vector3.up * 3f, Color.red);
                        //Debug.DrawLine(thisPos, thisPos + Vector3.up * 3f, Color.red);
                        matrices[i] = Matrix4x4.TRS(thisPos, Quaternion.identity, Vector3.one);
                        break;

                    }
                    else {
                        accum += thisDist;
                        pos -= thisDist;
                    }
                }
            }
                
            //for (int j = 0; j < path.Count - 1; ++j) {
            //    float thisDist = Vector3.Distance(path[j], path[j + 1]);
            //    if (pos <= thisDist) {
            //        Vector3 thisPos = Vector3.Lerp(path[j], path[j + 1], pos / thisDist) + selfPos;
            //        Debug.DrawLine(path[j] + Vector3.up * 3f, path[j + 1] + Vector3.up * 3f, Color.red);
            //        //Debug.DrawLine(thisPos, thisPos + Vector3.up * 3f, Color.red);
            //        matrices[i] = Matrix4x4.TRS(thisPos, Quaternion.identity, Vector3.one);
            //        thisDist -= pos;
            //        i++;
            //        if (i >= count) break;
            //        pos = positions[i];
            //        pos -= thisDist;
            //    }
            //    else {
            //        pos -= thisDist;
            //    }
            //}
            Graphics.DrawMeshInstanced(itemMesh, 0, itemMat, matrices, count);
        }
        float getPathDistance() {
            float sum = 0.0f;
            for(int i = 0; i < path.Count - 1; ++i) {
                sum += Vector3.Distance(path[i], path[i + 1]);
            }
            return sum;
        }

        void setPathGrid(Vector3[] positions, int positionsLength, float smoothness, Vector3 basePos, List<Vector3> verts, List<int> indices) {

        }

        void setPathBezier(Vector3[] positions, int positionsLength, float smoothness, Vector3 basePos, List<Vector3> verts, List<int> indices) {
            int steps = 10;
            Vector3[] ctrlPos;
            
            genBezierControlPoints(positions, positionsLength, smoothness, out ctrlPos);
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
                if(j == 0)
                    path.Add(lastPos);
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
                    path.Add(thisPos);
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
        static void genBezierControlPoints(Vector3[] pts, int positionsLength, float smoothness, out Vector3[] controlPts) {

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
        
        public void refreshMesh(Vector3[] pathpoints) {
            //for (int i = 0; i < pathpoints.Length; ++i) {
            //    path[i] = pathpoints[i];
            //}
            path.Clear();
            mfilter.mesh.GetVertices(verts);

            mfilter.mesh.GetIndices(indices, 0);
            setPathBezier(pathpoints, pathpoints.Length, 2f, pathpoints[0], verts, indices);
            transform.position = pathpoints[0];
            mfilter.mesh.Clear(true);
            mfilter.mesh.vertices = verts.ToArray();
            
            if (tmpIndices == null || tmpIndices.Length != indices.Count) {
                tmpIndices = new int[indices.Count];
            }
            for(int i = 0; i < indices.Count; ++i) {
                tmpIndices[i] = indices[i];
            }
            mfilter.mesh.SetIndices(tmpIndices, MeshTopology.Triangles, 0);
            target.tubeLength = getPathDistance();
            //meshCol.me = mfilter.mesh;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class Belt : MonoBehaviour {
        public BeltData target;
        public List<Vector3> path;
        public MeshFilter mfilter;
        private void Awake() {
            path = new List<Vector3>(16);
        }
        public static void setPath(Vector3[] positions, float smoothness) {
            int steps = 10;
            Vector3[] ctrlPos;
            Vector3[] four = new Vector3[4];
            setPathPoints(positions, smoothness, out ctrlPos);
            Vector3 lastleft = Vector3.zero;
            Vector3 lastright = Vector3.zero;
            for (int j = 0; j < ctrlPos.Length / 2; ++j) {
                four[0] = positions[j];
                four[1] = ctrlPos[j * 2];
                four[2] = ctrlPos[j * 2 + 1];
                four[3] = positions[j + 1];
                Vector3 left, right;
               
                Vector3 lastPos = BezierStatic.evalBezier(four[0], four[1], four[2], four[3], 0);
                for (int i = 1; i <= steps; i++) {
                    float t = (float)i / (float)steps;
                    Vector3 thisPos = BezierStatic.evalBezier(four[0], four[1], four[2], four[3], t);
                    Debug.DrawLine(lastPos, thisPos, Color.black);
                    Vector3 dir = thisPos - lastPos;
                    dir.Normalize();
                    dir = Vector3.Cross(dir, Vector3.up);
                    dir.Normalize();
                    //dir *=
                    right = thisPos - dir;
                    left = thisPos + dir;
                    if (i == 1 && j == 0) {
                        lastright = lastPos - dir;
                        lastleft = lastPos + dir;
                    }

                    Debug.DrawLine(lastleft, left, Color.green);
                    Debug.DrawLine(lastright, right, Color.green);
                    lastleft = left;
                    lastright = right;
                    lastPos = thisPos;

                }
            }

        }
        public static void setPathPoints(Vector3[] pts, float smoothness, out Vector3[] controlPts) {
            controlPts = new Vector3[(pts.Length - 1) * 2];
            Vector3 curPos = pts[0];
            Vector3 nextPos = pts[1];
            Vector3 lastdiff = nextPos - curPos;
            lastdiff.Normalize();
            controlPts[0] = pts[0] + lastdiff * smoothness;
            for (int i = 1; i < pts.Length - 1; ++i) {
                curPos = pts[i];
                nextPos = pts[i + 1];
                Vector3 diff = nextPos - curPos;
                diff.Normalize();
                Vector3 avg = (diff + lastdiff) / 2f;
                avg.Normalize();
                controlPts[i * 2 - 1] = pts[i] - avg * smoothness;
                controlPts[i * 2] = pts[i] + avg * smoothness;
                lastdiff = diff;
            }
            lastdiff = pts[pts.Length - 1] - pts[pts.Length - 2];
            controlPts[(pts.Length - 1) * 2 - 1] = pts[pts.Length - 1] - lastdiff * smoothness;

            //mfilter.mesh.ver
        }
        
    }
}
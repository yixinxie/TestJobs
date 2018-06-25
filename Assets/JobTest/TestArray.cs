using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

public class TestArray : MonoBehaviour {

    NativeArray<Vector3> m_Velocities;
    TransformAccessArray m_TransformsAccessArray;

    PositionUpdateJob m_Job;
    //AccelerationJob m_AccelJob;

    JobHandle m_PositionJobHandle;
    JobHandle m_AccelJobHandle;
    public int m_ObjectCount;
    public float m_ObjectPlacementRadius;
    GameObject[] m_Objects;
    Renderer[] m_Renderers;
    Transform[] m_Transforms;
    public Object prototype;
    float stepRadian;
    float elapsed;
    protected void Start() {
        //m_Velocities = new NativeArray<Vector3>(m_ObjectCount, Allocator.Persistent);
        stepRadian = Mathf.PI * 2f / m_ObjectCount;
        m_Objects = PlaceRandomCubes(m_ObjectCount, m_ObjectPlacementRadius);
        m_Transforms = new Transform[m_ObjectCount];
        m_Renderers = new Renderer[m_ObjectCount];
        for (int i = 0; i < m_Objects.Length; i++) {
            GameObject obj = m_Objects[i];
            m_Transforms[i] = obj.transform;
            m_Renderers[i] = obj.GetComponent<Renderer>();
        }

        m_TransformsAccessArray = new TransformAccessArray(m_Transforms);
        
    }
    GameObject[] PlaceRandomCubes(int count, float radius) {
        GameObject[] ret = new GameObject[count];
        
        for(int i = 0; i <count; ++i) {
            float x = radius * Mathf.Cos(stepRadian * i);
            float y = radius * Mathf.Sin(stepRadian * i);
            ret[i] = GameObject.Instantiate(prototype, new Vector3(x, y, 0f), Quaternion.identity, transform) as GameObject;
        }
        return ret;
    }

    struct PositionUpdateJob : IJobParallelForTransform {
        //[ReadOnly]
        //public NativeArray<Vector3> velocity;  // the velocities from AccelerationJob

        public float elapsedTime;
        public float step;
        public float radius;

        public void Execute(int i, TransformAccess transform) {
            transform.position = radius * new Vector3(Mathf.Cos(step* (i + elapsedTime)) , Mathf.Sin(step * (i + elapsedTime)), 0f);
        }
    }

    //struct AccelerationJob : IJobParallelFor {
    //    public NativeArray<Vector3> velocity;

    //    public Vector3 acceleration;
    //    public Vector3 accelerationMod;

    //    public float deltaTime;

    //    public void Execute(int i) {
    //        // here, i'm intentionally using the index to affect acceleration (it looks cool),
    //        // but generating velocities probably wouldn't be tied to index normally.
    //        velocity[i] += (acceleration + i * accelerationMod) * deltaTime;
    //    }
    //}
    
    public void Update() {
        //m_AccelJob = new AccelerationJob() {
        //    deltaTime = Time.deltaTime,
        //    velocity = m_Velocities,
        //    acceleration = m_Acceleration,
        //    accelerationMod = m_AccelerationMod
        //};
        elapsed += Time.deltaTime;
        m_Job = new PositionUpdateJob() {
            elapsedTime = elapsed,
            step = stepRadian,
            radius = m_ObjectPlacementRadius,
            //velocity = m_Velocities,
        };

       // m_AccelJobHandle = m_AccelJob.Schedule(m_ObjectCount, 64);
        //m_PositionJobHandle = m_Job.Schedule(m_TransformsAccessArray, m_AccelJobHandle);
        m_PositionJobHandle = m_Job.Schedule(m_TransformsAccessArray);
    }

    public void LateUpdate() {
        m_PositionJobHandle.Complete();
    }

    private void OnDestroy() {
        //m_Velocities.Dispose();
        m_TransformsAccessArray.Dispose();
    }
}
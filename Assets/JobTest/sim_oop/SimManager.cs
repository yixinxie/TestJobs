using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Simulation_OOP {
    public class SimManager : MonoBehaviour {
        public static SimManager self;
        public Object producerPrefab;
        public Object assemblerPrefab;
        public Object storagePrefab;
        public Object beltPrefab;
        public Object inserterPrefab;
        List<ProducerData> producers;
        List<AssemblerData> assemblers;
        List<StorageData> storages;
        List<BeltData> belts;
        List<InserterData> inserters;

        List<Producer> producerGO;
        List<Assembler> assemblerGO;
        List<Storage> storageGO;
        List<Belt> beltGO;
        List<Inserter> inserterGO;

        void Awake() {
            self = this;
            producers = new List<ProducerData>();
            assemblers = new List<AssemblerData>();
            storages = new List<StorageData>();
            belts = new List<BeltData>();
            inserters = new List<InserterData>();

            producerGO = new List<Producer>();
            assemblerGO = new List<Assembler>();
            storageGO = new List<Storage>();
            beltGO = new List<Belt>();
            inserterGO = new List<Inserter>();
        }
        
        public ProducerData addGenerator(Vector3 pos) {
            RaycastHit hitInfo;
            if(Physics.Raycast(pos + Vector3.up, Vector3.down, out hitInfo, 2f) == false) {
                return null;
            }
            ResourceNode rnode = hitInfo.collider.gameObject.GetComponent<ResourceNode>();
            if (rnode == null)
                return null;

            ProducerData p = new ProducerData();
            p.itemId = 1;
            producers.Add(p);



            GameObject go = GameObject.Instantiate(producerPrefab, pos, Quaternion.identity) as GameObject;
            Producer comp = go.GetComponent<Producer>();
            
            comp.target = p;
            producerGO.Add(comp);
            comp.initialize(rnode);
            return p;
        }
        public BeltData addBelt (Vector3 fromPos, Vector3 toPos) {
            BeltData p = new BeltData();
            belts.Add(p);

            GameObject go = GameObject.Instantiate(beltPrefab, (fromPos+toPos) / 2f, Quaternion.identity) as GameObject;
            Belt comp = go.GetComponent<Belt>();

            comp.target = p;
            beltGO.Add(comp);
            Vector3[] temp = new Vector3[2];
            temp[0] = fromPos;
            temp[1] = toPos;
            comp.refreshMesh(temp);

            return p;
        }
        public void addStraightBelt(Vector3 fromPos, Vector3 toPos) {
            BeltData p = new BeltData();
            belts.Add(p);

            GameObject go = GameObject.Instantiate(beltPrefab, (fromPos + toPos) / 2f, Quaternion.identity) as GameObject;
            Belt comp = go.GetComponent<Belt>();

            comp.target = p;
            beltGO.Add(comp);
            Vector3[] temp = new Vector3[2];
            temp[0] = fromPos;
            temp[1] = toPos;
            comp.refreshMesh(temp);
        }
        public InserterData addInserter(Vector3 pos) {
            InserterData ins = new InserterData();
            inserters.Add(ins);

            GameObject go = GameObject.Instantiate(inserterPrefab, pos, Quaternion.identity) as GameObject;
            Inserter comp = go.GetComponent<Inserter>();

            comp.target = ins;
            inserterGO.Add(comp);

            //surroundingCheck(pos, p, comp);
            return ins;
        }
        void surroundingCheck(Vector3 pos, InserterData inserterData, Inserter inserter) {
            // inserter check
            // generator to belt
            // belt to storage
            // generator to storage
            // assembler to storage
            // generator to assembler
            // get inserter's surrounding
            Vector3[] offsets = new Vector3[4] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            RaycastHit hit;
            ISimData[] adjacentTypes = new ISimData[4];
            for (int i = 0; i < 4; ++i) {
                if (Physics.Raycast(pos + offsets[i] + Vector3.up * 2f, Vector3.down, out hit, 2f, LayerMask.GetMask("Default" ))) {
                    ISimView simData = hit.collider.GetComponent<ISimView>();
                    adjacentTypes[i] = simData.getTarget();

                }
            }
            for(int i = 0; i < 4; ++i) {
                ProducerData generator = adjacentTypes[i] as ProducerData;
                if(generator != null) {
                    BeltData belt = adjacentTypes[(i + 2) % 4] as BeltData;
                    if(belt != null) {
                        
                    }
                    inserterData.expectedItemId = generator.itemId;
                    inserterData.source = generator;
                    inserterData.target = belt;
                    inserterData.targetPos = 0f;
                    inserterData.source = (ISimData)generator;
                    inserterData.target = (ISimData)belt;
                    inserterData.targetPos = 0f;
                    inserter.head = generator.ToString();
                    if(belt != null)
                        inserter.head = belt.ToString();
                }
            }
        }
        public void addAssemblerArray(int x_max, int y_max, int z_max, Vector3 basePos) {
            float spacing = 4f;
            for (int z = 0; z < z_max; ++z) {
                for (int y = 0; y < y_max; ++y) {
                    for (int x = 0; x < x_max; ++x) {
                        addAssembler(basePos + new Vector3(x, y, z) * spacing);
                    }
                }
            }
        }
        public void addAssembler(Vector3 pos) {
            AssemblerData p = new AssemblerData();
            assemblers.Add(p);

            GameObject go = GameObject.Instantiate(assemblerPrefab, pos, Quaternion.identity) as GameObject;
            Assembler comp = go.GetComponent<Assembler>();

            comp.target = p;
            assemblerGO.Add(comp);
        }
        public StorageData addStorage(Vector3 pos) {
            StorageData stor = new StorageData();
            storages.Add(stor);

            GameObject go = GameObject.Instantiate(storagePrefab, pos, Quaternion.identity) as GameObject;
            Storage comp = go.GetComponent<Storage>();

            comp.target = stor;
            storageGO.Add(comp);
            return stor;
        }
        private void Start() {
            ProducerData gen = addGenerator(new Vector3(1f, 0f, 13f));
            InserterData ins = addInserter(new Vector3(3f, 0, 13f));
            StorageData stor = addStorage(new Vector3(5f, 0, 13f));
            BeltData belt = addBelt(new Vector3(1f, 0f, 13f),
                new Vector3(5f, 0f, 13f));
            ins.source = gen;
            ins.target = belt;
            ins.targetPos = 0f;
            ins.expectedItemId = 1;
            

            InserterData ins2 = addInserter(new Vector3(3f, 0, 13f));
            ins2.source = belt;
            ins2.sourcePos = 4f;
            ins2.target = stor;
            ins2.expectedItemId = 1;

        }
        void FixedUpdate() {
            float dt = Time.fixedDeltaTime;
            for (int i = 0; i < producers.Count; ++i) {
                producers[i].update(dt);
            }
            for (int i = 0; i < assemblers.Count; ++i) {
                assemblers[i].update(dt);
            }
            for (int i = 0; i < belts.Count; ++i) {
                belts[i].update(dt);
            }

            for (int i = 0; i < inserters.Count; ++i) {
                inserters[i].update(dt);
            }
        }
    }
}
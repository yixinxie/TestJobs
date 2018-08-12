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
        public void addGenerator(Vector3 pos) {
            ProducerData p = new ProducerData();
            p.itemId = 1;
            producers.Add(p);

            GameObject go = GameObject.Instantiate(producerPrefab, pos, Quaternion.identity) as GameObject;
            Producer comp = go.GetComponent<Producer>();
            
            comp.target = p;
            producerGO.Add(comp);
        }
        public void addBelt (Vector3 pos) {
            BeltData p = new BeltData();
            belts.Add(p);

            GameObject go = GameObject.Instantiate(beltPrefab, pos, Quaternion.identity) as GameObject;
            Belt comp = go.GetComponent<Belt>();

            comp.target = p;
            beltGO.Add(comp);
        }
        public void addInserter(Vector3 pos) {
            InserterData p = new InserterData();
            inserters.Add(p);

            GameObject go = GameObject.Instantiate(inserterPrefab, pos, Quaternion.identity) as GameObject;
            Inserter comp = go.GetComponent<Inserter>();

            comp.target = p;
            inserterGO.Add(comp);
        }
        void surroundingCheck() {

        }
        public void addAssembler(Vector3 pos) {
            AssemblerData p = new AssemblerData();
            assemblers.Add(p);

            GameObject go = GameObject.Instantiate(assemblerPrefab, pos, Quaternion.identity) as GameObject;
            Assembler comp = go.GetComponent<Assembler>();

            comp.target = p;
            assemblerGO.Add(comp);
        }
        public void addStorage(Vector3 pos) {
            StorageData stor = new StorageData();
            storages.Add(stor);

            GameObject go = GameObject.Instantiate(storagePrefab, pos, Quaternion.identity) as GameObject;
            Storage comp = go.GetComponent<Storage>();

            comp.target = stor;
            storageGO.Add(comp);
        }
        private void Start() {
            //ProducerData p = new ProducerData();
            //p.itemId = 1;
            //producers.Add(p);

            //StorageData stor = new StorageData();
            //storages.Add(stor);

            //InserterData ins = new InserterData();
            //inserters.Add(ins);
            //ins.source = p;
            //ins.target = stor;
            //ins.expectedItemId = 1;

            for (int i = 0; i < belts.Count; ++i) {
                GameObject go = GameObject.Instantiate(beltPrefab) as GameObject;
                Belt comp = go.GetComponent<Belt>();
                comp.target = belts[i];
                beltGO.Add(comp);
            }

            for (int i = 0; i < storages.Count; ++i) {
                GameObject go = GameObject.Instantiate(storagePrefab) as GameObject;
                Storage comp = go.GetComponent<Storage>();
                comp.target = storages[i];
                storageGO.Add(comp);
            }

            for (int i = 0; i < assemblers.Count; ++i) {
                GameObject go = GameObject.Instantiate(assemblerPrefab) as GameObject;
                Assembler comp = go.GetComponent<Assembler>();
                comp.target = assemblers[i];
                assemblerGO.Add(comp);
            }

            for (int i = 0; i < producers.Count; ++i) {
                GameObject go = GameObject.Instantiate(producerPrefab) as GameObject;
                Producer comp = go.GetComponent<Producer>();
                comp.target = producers[i];
                producerGO.Add(comp);
            }

            for (int i = 0; i < inserters.Count; ++i) {
                GameObject go = GameObject.Instantiate(inserterPrefab) as GameObject;
                Inserter comp = go.GetComponent<Inserter>();
                comp.target = inserters[i];
                inserterGO.Add(comp);
            }
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
        private void LateUpdate() {
            
        }
    }
}
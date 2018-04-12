using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHandler : MonoBehaviour {
    
    class NPCInfo
    {
        GameObject           gameObject;
        bool                 isLeader;
        NPC.BehaviourType    behaviour;

        public GameObject GetGameObject() { return gameObject; }
        public void SetGameObject(GameObject obj) { gameObject = obj; }

        public bool IsLeader() { return isLeader; }
        public void SetLeader(bool leader) { isLeader = leader; }

        public NPC.BehaviourType GetBehaviour() { return behaviour; }
        public void SetBehaviour(NPC.BehaviourType newBehaviour) { behaviour = newBehaviour; }
    };

    Vector3 LeftLine = new Vector3(-10, 0, 0);
    Vector3 RightLine = new Vector3(10, 0, 0);
    Vector3 LeftTrackV = new Vector3(-5, 0, 5);
    Vector3 RightTrackV = new Vector3(5, 0, 5);
    Vector3 LeftAttackV = new Vector3(-10, 0, 5);
    Vector3 RightAttackV = new Vector3(10, 0, 5);

    List<NPCInfo> NPCInfos = new List<NPCInfo>();

    private NPC.BehaviourType generalBehavior;

	void Start ()
    {
        GameObject[] NPCs = GameObject.FindGameObjectsWithTag("NPC");

        foreach (GameObject obj in NPCs)
        {
            NPCInfo temp = new NPCInfo();
            temp.SetGameObject(obj);
            temp.SetLeader(false);
            temp.SetBehaviour(NPC.BehaviourType.Wander);
            NPCInfos.Add(temp);
        }
        generalBehavior = NPC.BehaviourType.Wander;

        SetFormation(NPCInfos[0]);
	}

    private void FixedUpdate()
    {
        foreach(NPCInfo inf in NPCInfos)
        {
            if(inf.GetBehaviour() != NPC.BehaviourType.Formation && !inf.IsLeader())
            {
                ResetFormation();
            }
        }
    }

    private void ResetFormation()
    {
        foreach (NPCInfo inf in NPCInfos)
        {
            if (inf.IsLeader())
                SetFormation(inf);
        }
    }

    private void SetFormation(NPCInfo leader)
    {
        if (generalBehavior == NPC.BehaviourType.Wander)
        {
            GameObject temp = GetMiddleNPC();
            foreach (NPCInfo inf in NPCInfos)
            {
                if (inf.GetGameObject() == temp)
                {
                    inf.SetLeader(true);
                    inf.GetGameObject().GetComponent<NPC>().setFormation(temp, Vector3.zero);
                    SetBestNPCForOffset(temp, LeftLine);
                    SetBestNPCForOffset(temp, RightLine);
                }
            }
        }
        else if(generalBehavior == NPC.BehaviourType.Track)
        {
            leader.SetLeader(true);
            leader.GetGameObject().GetComponent<NPC>().setFormation(leader.GetGameObject(), Vector3.zero);
            SetBestNPCForOffset(leader.GetGameObject(), LeftTrackV);
            SetBestNPCForOffset(leader.GetGameObject(), RightTrackV);
        }
        else if (generalBehavior == NPC.BehaviourType.Attack)
        {
            leader.SetLeader(true);
            leader.GetGameObject().GetComponent<NPC>().setFormation(leader.GetGameObject(), Vector3.zero);
            SetBestNPCForOffset(leader.GetGameObject(), LeftAttackV);
            SetBestNPCForOffset(leader.GetGameObject(), RightAttackV);
        }
    }

    private GameObject GetMiddleNPC()
    {
        Vector3 average = Vector3.zero;
        for(int i = 0; i < NPCInfos.Count; ++i)
        {
            average += NPCInfos[i].GetGameObject().transform.position;
        }
        average /= NPCInfos.Count;
        GameObject res = NPCInfos[0].GetGameObject();
        for (int i = 0; i < NPCInfos.Count; ++i)
        {
            float dist0 = Mathf.Sqrt(Mathf.Pow(average.x - res.transform.position.x, 2) + Mathf.Pow(average.z - res.transform.position.z, 2));
            float dist1 = Mathf.Sqrt(Mathf.Pow(average.x - NPCInfos[i].GetGameObject().transform.position.x, 2) + Mathf.Pow(average.z - NPCInfos[i].GetGameObject().transform.position.z, 2));
            if (dist1 < dist0)
                res = NPCInfos[i].GetGameObject();
        }

        return res;
    }

    private void SetBestNPCForOffset(GameObject leader, Vector3 offset)
    {
        Vector3 target = leader.transform.TransformPoint(offset);
        GameObject res = NPCInfos[0].GetGameObject();
        for (int i = 0; i < NPCInfos.Count; ++i)
        {
            if (NPCInfos[i].IsLeader())
                continue;
            float dist0 = Mathf.Sqrt(Mathf.Pow(target.x - res.transform.position.x, 2) + Mathf.Pow(target.z - res.transform.position.z, 2));
            float dist1 = Mathf.Sqrt(Mathf.Pow(target.x - NPCInfos[i].GetGameObject().transform.position.x, 2) + Mathf.Pow(target.z - NPCInfos[i].GetGameObject().transform.position.z, 2));
            if (dist1 < dist0)
                res = NPCInfos[i].GetGameObject();
        }
        if (res != leader)
            res.GetComponent<NPC>().setFormation(leader, offset);
    }

    public void SetNPCBehavior(GameObject npc, NPC.BehaviourType newBehaviour)
    {
        foreach(NPCInfo inf in NPCInfos)
        {
            if (inf.GetGameObject() == npc)
            {
                inf.SetBehaviour(newBehaviour);
                CheckBehaviour(npc, newBehaviour);
                return;
            }
        }
    }

    private void CheckBehaviour(GameObject npc, NPC.BehaviourType bhv)
    {
        bool leaderOrder = false;
        foreach (NPCInfo inf in NPCInfos)
        {
            if (inf.GetGameObject() == npc)
            {
                leaderOrder = inf.IsLeader();
                break;
            }
        }
        if ((bhv > generalBehavior && bhv != NPC.BehaviourType.Bush) || leaderOrder)
        {
            generalBehavior = bhv;
            foreach(NPCInfo inf in NPCInfos)
            {
                if(inf.GetGameObject() == npc)
                    SetFormation(inf);
            }
        }
    }

    public NPC.BehaviourType GetGeneralBehaviour()
    {
        return generalBehavior;
    }
}

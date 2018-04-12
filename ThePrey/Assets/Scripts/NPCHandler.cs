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

    Vector3 LeftLine = new Vector3(-6, 0, 0);
    Vector3 RightLine = new Vector3(6, 0, 0);
    Vector3 LeftTrackV = new Vector3(-5, 0, 5);
    Vector3 RightTrackV = new Vector3(5, 0, 5);
    Vector3 LeftAttackV = new Vector3(-2, 0, 2);
    Vector3 RightAttackV = new Vector3(2, 0, 2);

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

        SetFormation(GetMiddleNPC());
	}

    private void ResetFormation()
    {
        foreach (NPCInfo inf in NPCInfos)
        {
            if (inf.IsLeader())
                SetFormation(inf);
        }
    }

    private void SetFormation(NPCInfo newLeader)
    {
        foreach (NPCInfo inf in NPCInfos)
            inf.SetLeader(false);
        if (generalBehavior == NPC.BehaviourType.Wander)
        {
            newLeader.SetLeader(true);
            newLeader.GetGameObject().GetComponent<NPC>().setFormation(newLeader.GetGameObject(), Vector3.zero);
            SetBestNPCForOffset(newLeader.GetGameObject(), LeftLine, RightLine);
        }
        else if(generalBehavior == NPC.BehaviourType.Track)
        {
            newLeader.SetLeader(true);
            newLeader.GetGameObject().GetComponent<NPC>().setFormation(newLeader.GetGameObject(), Vector3.zero);
            SetBestNPCForOffset(newLeader.GetGameObject(), LeftTrackV, RightTrackV);
        }
        else if (generalBehavior == NPC.BehaviourType.Attack)
        {
            newLeader.SetLeader(true);
            newLeader.GetGameObject().GetComponent<NPC>().setFormation(newLeader.GetGameObject(), Vector3.zero);
            SetBestNPCForOffset(newLeader.GetGameObject(), LeftAttackV, RightAttackV);
        }
    }

    private NPCInfo GetMiddleNPC()
    {
        Vector3 average = Vector3.zero;
        for(int i = 0; i < NPCInfos.Count; ++i)
        {
            average += NPCInfos[i].GetGameObject().transform.position;
        }
        average /= NPCInfos.Count;
        NPCInfo res = NPCInfos[0];
        for (int i = 0; i < NPCInfos.Count; ++i)
        {
            float dist0 = Mathf.Sqrt(Mathf.Pow(average.x - res.GetGameObject().transform.position.x, 2) + Mathf.Pow(average.z - res.GetGameObject().transform.position.z, 2));
            float dist1 = Mathf.Sqrt(Mathf.Pow(average.x - NPCInfos[i].GetGameObject().transform.position.x, 2) + Mathf.Pow(average.z - NPCInfos[i].GetGameObject().transform.position.z, 2));
            if (dist1 < dist0)
                res = NPCInfos[i];
        }

        return res;
    }

    private void SetBestNPCForOffset(GameObject leader, Vector3 offsetLeft, Vector3 offsetRight)
    {
        Vector3 target = leader.transform.TransformPoint(offsetLeft);
        GameObject res = NPCInfos[0].GetGameObject();
        if(NPCInfos.Count > 1)
            foreach(NPCInfo inf in NPCInfos)
            {
                if(!inf.IsLeader())
                {
                    res = inf.GetGameObject();
                    break;
                }
            }
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
        {
            res.GetComponent<NPC>().setFormation(leader, offsetLeft);
            if (NPCInfos.Count == 3)
            {
                foreach (NPCInfo inf in NPCInfos)
                {
                    if(inf.GetGameObject() != res && !inf.IsLeader())
                        inf.GetGameObject().GetComponent<NPC>().setFormation(leader, offsetRight);
                }
            }
        }
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

        if ((bhv > generalBehavior && bhv != NPC.BehaviourType.Bush && bhv != NPC.BehaviourType.Flee) || leaderOrder)
        {
            generalBehavior = bhv;
            foreach (NPCInfo inf in NPCInfos)
            {
                if (inf.GetGameObject() == npc)
                    SetFormation(inf);
            }
        }
        else if(bhv != NPC.BehaviourType.Bush && bhv != NPC.BehaviourType.Flee)
            ResetFormation();
    }

    public NPC.BehaviourType GetGeneralBehaviour()
    {
        return generalBehavior;
    }

    public void removeNPC(GameObject npc)
    {
        int i = 0;
        while(i < NPCInfos.Count)
        {
            if (NPCInfos[i].GetGameObject() == npc)
                break;
            i++;
        }
        NPCInfos.RemoveAt(i);
        SetFormation(GetMiddleNPC());
    }
}

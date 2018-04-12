using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

    private class Cell
    {
        Vector3 _offset;
        float _weight;
        List<Cell> _neighbors;

        public Cell(Vector3 o, float w)
        {
            _offset = o;
            _weight = w;
            _neighbors = new List<Cell>();
        }

        public void sw(float w) { _weight = w; }
        public float gw() { return (_weight); }
        public void so(Vector3 o) { _offset = o; }
        public Vector3 getPosition(Vector3 pos) { return (pos + _offset); }
        public void link(Cell n) { _neighbors.Add(n); }
        public List<Cell> gn() { return (_neighbors); }
        public float gow() { return (Mathf.Sqrt(Mathf.Pow(_offset.x, 2) + Mathf.Pow(_offset.z, 2))); }
    }

    List<Cell> _g;
    public float _distBetweenNodes;
    public int _rowNumber;
    public int _nodeNumber;

    // Use this for initialization
    void Start () {
        _g = new List<Cell>();
        _g.Add(new Cell(new Vector3(0, 50, 0), 0));
        for (int j = 0; j < _rowNumber; j++) // number of rows
        {
            for (int i = 0; i < _nodeNumber * (j + 1); i++) // number of node of the initial row
            {
                _g.Add(new Cell(new Vector3(Mathf.Cos(Mathf.PI * 2f * i / (_nodeNumber * (j + 1))) * _distBetweenNodes * (j + 1), 50, Mathf.Sin(Mathf.PI * 2f * i / (_nodeNumber * (j + 1))) * _distBetweenNodes * (j + 1)), 0));
                if (j == 0)
                    _g[(_nodeNumber * (j + 1)) / 2 * j + i + 1].link(_g[0]);
                else
                {
                    float tmp = _nodeNumber * j / 2 * (j - 1) + (i * j) / (j + 1) + 1;
                    for (int n = (int)tmp; n <= (int)tmp + ((i % (j + 1) != 0) ? (1) : (0)); n++)
                        _g[_nodeNumber * (j + 1) / 2 * j + i + 1].link(_g[(n == _nodeNumber * (j + 1) + 1) ? (n) : n]);
                }
                if (i != 0)
                    _g[_nodeNumber * (j + 1) / 2 * j + i + 1].link(_g[_nodeNumber * (j + 1) / 2 * j + i]);
            }
            _g[_nodeNumber * (j + 1) / 2 * j + _nodeNumber * (j + 1)].link(_g[_nodeNumber * (j + 1) / 2 * j + 1]);
        }
    }

    private void addWeights(Vector3 pos, Vector3 target)
    {
        RaycastHit hit;
        foreach (Cell c in _g)
            if (c == _g[0] || (Physics.Raycast(c.getPosition(pos), -transform.up, out hit, 300) && hit.collider.tag != "Untagged" && hit.collider.tag != "Player" && hit.collider.tag != "Visible"))
                c.sw(10000);// Collided below
            else
            {
                float w = _rowNumber * _distBetweenNodes - c.gow();
                Vector3 check = new Vector3(c.getPosition(pos).x, 1, c.getPosition(pos).z) - pos;
                float res = Mathf.Sqrt(check.x * check.x + check.z * check.z);
                if (Physics.Raycast(pos, check, out hit, res) && hit.collider.tag != "Untagged" && hit.collider.tag != "Player" && hit.collider.tag != "Visible")
                    w += res;// Collided between pos and node
                w += res;

                check = target - new Vector3(c.getPosition(pos).x, 1, c.getPosition(pos).z);
                res = Mathf.Sqrt(check.x * check.x + check.z * check.z);
                if (Physics.Raycast(pos, check, out hit, res) && hit.collider.tag != "Untagged" && hit.collider.tag != "Player" && hit.collider.tag != "Visible")
                    w += res;// Collided between target and node
                w += res;

                c.sw(w);
            }
    }

    public Vector3 getPosition(Vector3 pos, Vector3 target)
    {
        addWeights(pos, target);
        Cell min = _g[0];
        foreach (Cell c in _g)
            if (min.gw() > c.gw())
                min = c;
        return (new Vector3(min.getPosition(pos).x, 1, min.getPosition(pos).z));
    }
}

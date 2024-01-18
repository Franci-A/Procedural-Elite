using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeHandler : MonoBehaviour
{
    [SerializeField] private GameObject ropePointPrefab;
    [SerializeField] private int numberOfPoints = 3;
    private List<Transform> ropePoints;
    [SerializeField] private LineRenderer ropeVisual;
    [SerializeField] private AnimationCurve positionOffsetCurve;

    public void Init(BossHandler boss ,BossPlug attackedPlug)
    {
        attackedPlug.onDestroy.AddListener(PlugDestroyed);
        ropePoints = new List<Transform>();
        ropePoints.Add(boss.transform);
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (i + 1f) / (numberOfPoints + 2f);
            Vector2 bossPlugVec = boss.transform.position - attackedPlug.transform.position;
            if (boss.transform.position.x < attackedPlug.transform.position.x)
                bossPlugVec = attackedPlug.transform.position - boss.transform.position;
            Vector2 perpendicularOffset = Vector2.Perpendicular(bossPlugVec) * positionOffsetCurve.Evaluate(t);

            Vector2 position = Vector2.Lerp(boss.transform.position, attackedPlug.transform.position, t) + perpendicularOffset;
            ropePoints.Add(Instantiate(ropePointPrefab, position, Quaternion.identity, transform).transform);
        }
        
        ropePoints[1].GetComponent<HingeJoint2D>().connectedBody = boss.GetComponent<Rigidbody2D>();
        ropePoints.Add(attackedPlug.transform);
        for (int i = 2; i < ropePoints.Count; i++)
        {
            ropePoints[i].GetComponent<HingeJoint2D>().connectedBody = ropePoints[i -1].GetComponent<Rigidbody2D>();
        }
        ropeVisual.positionCount = ropePoints.Count;
        UpdateRenderer();
    }

    private void UpdateRenderer()
    {
        for (int i = 0; i < ropePoints.Count; i++)
        {
            if (ropePoints[i] == null)
            {
                ropePoints.RemoveAt(i);
                return;
            }
            ropeVisual.SetPosition(i, ropePoints[i].position);
        }
    }

    private void Update()
    {
        UpdateRenderer();
    }

    private void PlugDestroyed(BossPlug obj)
    {
        ropePoints.RemoveAt(ropePoints.Count - 1);
    }
    
}

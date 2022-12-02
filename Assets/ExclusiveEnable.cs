using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExclusiveEnable : MonoBehaviour
{
    [SerializeField]
    private GameObject[] m_Targets = new GameObject[0];

    void OnEnable()
    {
        int enabledIndex = Random.Range(0, m_Targets.Length - 1);
        for (int i = 0; i < m_Targets.Length; i++)
        {
            m_Targets[i].SetActive(i == enabledIndex); 
		}
    }
}

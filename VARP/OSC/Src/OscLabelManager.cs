using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VARP.OSC;

public class OscLabelManager : MonoBehaviour
{
	public OscChannelLabel label;
	public int labelsNumber = 16;

	private List<OscChannelLabel> freeLabels;

	private void OnEnable()
	{
		freeLabels = new List<OscChannelLabel>(labelsNumber);
		label.visible = false;
		freeLabels.Add(label);
		for (var i = 1; i < labelsNumber; i++)
		{
			var instance = Instantiate(label.gameObject) as GameObject;
			instance.transform.SetParent(transform, true);
			instance.transform.localScale = new Vector3(1,1,1);
			var instanceLabel = instance.GetComponent<OscChannelLabel>();
			instanceLabel.visible = false;
			freeLabels.Add(instanceLabel);
		}
	}

	public OscChannelLabel SpawnLabel()
	{
		if (freeLabels.Count == 0)
		{
			throw new SystemException("OscLabelManager.SpawnLabel reached limit if labels quantity.");
		}
		else
		{
			var last = freeLabels.Count - 1;
			var label = freeLabels[last];
			freeLabels.RemoveAt(freeLabels.Count - 1);

			return label;
		}
	}
	
	public void Release(OscChannelLabel label)
	{
		label.visible = false;
		if (!freeLabels.Contains(label))
			freeLabels.Add(label);
	}

}

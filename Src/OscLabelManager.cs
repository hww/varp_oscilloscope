// =============================================================================
// MIT License
// 
// Copyright (c) 2018 Valeriya Pudova (hww.github.io)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// =============================================================================

using System;
using System.Collections.Generic;
using VARP.OSC;
using UnityEngine;

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
			var instance = Instantiate(label.gameObject);
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
			throw new SystemException("OscLabelManager.SpawnLabel reached limit if labels quantity.");

		var last = freeLabels.Count - 1;
		var label = freeLabels[last];
		freeLabels.RemoveAt(freeLabels.Count - 1);
		return label;
	}
	
	public void Release(OscChannelLabel label)
	{
		label.visible = false;
		if (!freeLabels.Contains(label))
			freeLabels.Add(label);
	}

}

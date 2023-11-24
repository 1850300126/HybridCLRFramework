﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUpdateDemoSDK
{

	public class NoDrawingRayCast : Graphic
	{
		public override void SetMaterialDirty()
		{
		}

		public override void SetVerticesDirty()
		{
		}


		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
			return;
		}
	}

}


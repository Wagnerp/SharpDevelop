﻿/*
 * Created by SharpDevelop.
 * User: Peter Forstmeier
 * Date: 19.03.2013
 * Time: 20:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using ICSharpCode.Reporting.Interfaces;

namespace ICSharpCode.Reporting.Items
{
	/// <summary>
	/// Description of BaseSection.
	/// </summary>
	public class BaseSection:ReportItem
	{
		#region Constructors
		
		public BaseSection()
		{}
		
		public BaseSection (string sectionName) 
		{
			base.Name = sectionName;
		}
		
		#endregion
	}
}

﻿/*
 * Created by SharpDevelop.
 * User: Peter Forstmeier
 * Date: 17.03.2013
 * Time: 17:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using ICSharpCode.Reporting.Globals;
using ICSharpCode.Reporting.Interfaces;
using ICSharpCode.Reporting.Items;
using ICSharpCode.Reporting.PageBuilder;
using ICSharpCode.Reporting.Xml;

namespace ICSharpCode.Reporting
{
	/// <summary>
	/// Description of Reporting.
	/// </summary>
	public class ReportingFactory
	{
		public ReportingFactory()
		{
		}
		
		
		public IReportCreator ReportCreator (Stream stream)
		{
			IReportModel reportModel = LoadReportModel (stream);
			IReportCreator builder = null;
			if (reportModel.ReportSettings.DataModel == GlobalEnums.PushPullModel.FormSheet) {
				builder =  new FormPageBuilder(reportModel);
			}
//			else {
//				CheckForParameters(reportModel,reportParameters);
//				IDataManager dataMan  = DataManagerFactory.CreateDataManager(reportModel,reportParameters);
//				builder = DataPageBuilder.CreateInstance(reportModel, dataMan);
//			}
			return builder;
//			return null;
		}
			
		
		internal ReportModel LoadReportModel (Stream stream)
		{
			var doc = new XmlDocument();
			doc.Load(stream);
			var rm = LoadModel(doc);
			return rm;
		}
		
		static ReportModel LoadModel(XmlDocument doc)
		{
			var loader = new ModelLoader();
			object root = loader.Load(doc.DocumentElement);

			var model = root as ReportModel;
			if (model == null) {
//				throw new IllegalFileFormatException("ReportModel");
			}
			return model;
		}
	}
}

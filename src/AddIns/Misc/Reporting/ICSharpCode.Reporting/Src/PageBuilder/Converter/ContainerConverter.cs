﻿/*
 * Created by SharpDevelop.
 * User: Peter Forstmeier
 * Date: 08.04.2013
 * Time: 19:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;

using ICSharpCode.Reporting.Factories;
using ICSharpCode.Reporting.Interfaces;
using ICSharpCode.Reporting.Interfaces.Export;
using ICSharpCode.Reporting.Items;
using ICSharpCode.Reporting.PageBuilder.ExportColumns;

namespace ICSharpCode.Reporting.PageBuilder.Converter
{
	/// <summary>
	/// Description of SectionConverter.
	/// </summary>
	internal class ContainerConverter
	{
	
		public ContainerConverter(IReportContainer reportContainer,Point currentLocation )
		{
			Container = reportContainer;
			CurrentLocation = currentLocation;
		}
		
		
		public IExportContainer Convert() {
			Console.WriteLine("Convert section for location {0}",CurrentLocation);
			var strat = ((ReportContainer)Container).ArrangeStrategy;
			strat.Arrange(Container);
			
			var exportContainer = (ExportContainer)Container.CreateExportColumn();
			exportContainer.Location = CurrentLocation;
			var itemsList = new List<IExportColumn>();
			foreach (var element in Container.Items) {
				var item = ExportColumnFactory.CreateItem(element);
				itemsList.Add(item);
			}
			exportContainer.ExportedItems.AddRange(itemsList);
			return exportContainer;
		}
			
		internal IReportContainer Container {get; private set;}
		
		internal Point CurrentLocation {get; private set;}
	}
}
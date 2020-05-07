﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.DictionaryRendererGraphicsOverlay
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Graphics overlay (dictionary renderer)",
        "GraphicsOverlay",
        "This sample demonstrates applying a dictionary renderer to graphics, in order to display military symbology without the need for a feature table.",
        "Pan and zoom to explore military symbols on the map.",
        "defense", "military", "situational awareness", "tactical", "visualization")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("c78b149a1d52414682c86a5feeb13d30", "1e4ea99af4b440c092e7959cf3957bfa")]
    public partial class DictionaryRendererGraphicsOverlay
    {
        // Hold a reference to the graphics overlay for easy access.
        private GraphicsOverlay _tacticalMessageOverlay;

        public DictionaryRendererGraphicsOverlay()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                MyMapView.Map = new Map(Basemap.CreateTopographic());

                // Create an overlay for visualizing tactical messages and add it to the map.
                _tacticalMessageOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_tacticalMessageOverlay);

                // Prevent graphics from showing up when zoomed too far out.
                _tacticalMessageOverlay.MinScale = 1000000;

                // Create a symbol dictionary style following the mil2525d spec.
                string symbolFilePath = DataManager.GetDataFolder("c78b149a1d52414682c86a5feeb13d30", "mil2525d.stylx");
                DictionarySymbolStyle mil2525DStyle = await DictionarySymbolStyle.CreateFromFileAsync(symbolFilePath);

                // Use the dictionary symbol style to render graphics in the overlay.
                _tacticalMessageOverlay.Renderer = new DictionaryRenderer(mil2525DStyle);

                // Load the military messages and render them.
                LoadMilitaryMessages();

                // Get the extent of the graphics.
                Envelope graphicExtent = GeometryEngine.CombineExtents(_tacticalMessageOverlay.Graphics.Select(graphic => graphic.Geometry));

                // Zoom to the extent of the graphics.
                await MyMapView.SetViewpointGeometryAsync(graphicExtent, 10);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await new MessageDialog(e.ToString()).ShowAsync();
            }
        }

        private void LoadMilitaryMessages()
        {
            // Get the path to the messages file.
            string militaryMessagePath = DataManager.GetDataFolder("1e4ea99af4b440c092e7959cf3957bfa", "Mil2525DMessages.xml");

            // Load the XML document.
            XElement xmlRoot = XElement.Load(militaryMessagePath);

            // Get all of the messages.
            IEnumerable<XElement> messages = xmlRoot.Descendants("message");

            // Add a graphic for each message.
            foreach (var message in messages)
            {
                Graphic messageGraphic = GraphicFromAttributes(message.Descendants().ToList());
                _tacticalMessageOverlay.Graphics.Add(messageGraphic);
            }
        }

        private Graphic GraphicFromAttributes(List<XElement> graphicAttributes)
        {
            // Get the geometry and the spatial reference from the message elements.
            XElement geometryAttribute = graphicAttributes.First(attr => attr.Name == "_control_points");
            XElement spatialReferenceAttr = graphicAttributes.First(attr => attr.Name == "_wkid");

            // Split the geometry field into a list of points.
            Array pointStrings = geometryAttribute.Value.Split(';');

            // Create a point collection in the correct spatial reference.
            int wkid = Convert.ToInt32(spatialReferenceAttr.Value);
            SpatialReference pointSR = SpatialReference.Create(wkid);
            PointCollection graphicPoints = new PointCollection(pointSR);

            // Add a point for each point in the list.
            foreach (string pointString in pointStrings)
            {
                var coords = pointString.Split(',');
                graphicPoints.Add(Convert.ToDouble(coords[0]), Convert.ToDouble(coords[1]));
            }

            // Create a multipoint from the point collection.
            Multipoint graphicMultipoint = new Multipoint(graphicPoints);

            // Create the graphic from the multipoint.
            Graphic messageGraphic = new Graphic(graphicMultipoint);

            // Add all of the message's attributes to the graphic (some of these are used for rendering).
            foreach (XElement attr in graphicAttributes)
            {
                messageGraphic.Attributes[attr.Name.ToString()] = attr.Value;
            }

            return messageGraphic;
        }
    }
}
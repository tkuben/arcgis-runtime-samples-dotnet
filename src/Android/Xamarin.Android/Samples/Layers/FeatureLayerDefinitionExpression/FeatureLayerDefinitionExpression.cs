// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.FeatureLayerDefinitionExpression
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer definition expression",
        "Layers",
        "Limit the features displayed on a map with a definition expression.",
        "Press the 'Apply Expression' button to limit the features requested from the feature layer to those specified by the SQL query definition expression. Tap the 'Reset Expression' button to remove the definition expression on the feature layer, which returns all the records.",
        "SQL", "definition expression", "filter", "limit data", "query", "restrict data", "where clause")]
    public class FeatureLayerDefinitionExpression : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Feature layer definition expression";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create a mappoint the map should zoom to
            MapPoint mapPoint = new MapPoint(-13630484, 4545415, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(mapPoint, 90000);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Create the uri for the feature service
            Uri featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            ServiceFeatureTable featureTable = new ServiceFeatureTable(featureServiceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable);

            //Add the feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);
        }

        private void OnApplyExpressionClicked(object sender, EventArgs e)
        {
            // Adding definition expression to show specific features only
            _featureLayer.DefinitionExpression = "req_Type = 'Tree Maintenance or Damage'";
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            // Reset the definition expression to see all features again
            _featureLayer.DefinitionExpression = "";
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a button to reset the renderer
            Button resetButton = new Button(this)
            {
                Text = "Reset"
            };
            resetButton.Click += OnResetButtonClicked;

            // Create a button to apply new renderer
            Button overrideButton = new Button(this)
            {
                Text = "Apply expression"
            };
            overrideButton.Click += OnApplyExpressionClicked;

            // Add Reset Button to the layout
            layout.AddView(resetButton);

            // Add Override Button to the layout
            layout.AddView(overrideButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}
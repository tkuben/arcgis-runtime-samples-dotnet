// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using System;
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.OpenScene
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open a scene (portal item)",
        "Map",
        "Open a web scene from a portal item.",
        "When the sample opens, it will automatically display the scene from ArcGIS Online. Pan and zoom to explore the scene.",
        "portal", "scene", "web scene")]
    public partial class OpenScene
    {
        // Hold the ID of the portal item, which is a web scene.
        private const string ItemId = "c6f90b19164c4283884361005faea852";

        public OpenScene()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }
        
        private async void Initialize()
        {
            try
            {
                // Try to load the default portal, which will be ArcGIS Online.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Create the portal item.
                PortalItem websceneItem = await PortalItem.CreateAsync(portal, ItemId);

                // Create and show the scene.
                MySceneView.Scene = new Scene(websceneItem);
            }
            catch (Exception e)
            {
                await new MessageDialog(e.ToString(), "Error").ShowAsync();
            }
        }
    }
}

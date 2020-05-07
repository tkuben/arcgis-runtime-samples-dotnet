// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;

namespace ArcGISRuntime.WPF.Samples.SetInitialMapLocation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Set initial map location",
        "Map",
        "Display a basemap centered at an initial location and scale.",
        "When the map loads, note the specific location and scale of the initial map view.",
        "LOD", "basemap", "center", "envelope", "extent", "initial", "lat", "latitude", "level of detail", "location", "long", "longitude", "scale", "zoom level")]
    public partial class SetInitialMapLocation
    {
        public SetInitialMapLocation()
        {
            InitializeComponent();

            //initialize map with `imagery with labels` basemap and an initial location
            Map myMap = new Map(BasemapType.ImageryWithLabels, -33.867886, -63.985, 15);
            //assign the map to the map view
            MyMapView.Map = myMap;
        }
    }
}
// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.ShowLocationHistory
{
    class FakeLocationDataSource : LocationDataSource
    {
        public FakeLocationDataSource()
        {
            // Generate the points that will be used.

            // The location will walk around a circle around the point.
            Polygon outerCircle = (Polygon)GeometryEngine.BufferGeodetic(_circleRouteCenter, 1000, LinearUnits.Feet);

            // Get a list of points on the circle from the buffered point.
            _artificialMapPoints = outerCircle.Parts[0].Points.ToList();
        }

        ~FakeLocationDataSource()
        {
            if (IsStarted)
            {
                OnStopAsync().Start();
            }
        }

        private NmeaParser.NmeaFileDevice fileDevice = null;

        // Center around which the fake locations are centered.
        private readonly MapPoint _circleRouteCenter = new MapPoint(-117.195801, 34.056007, SpatialReferences.Wgs84);

        // List of points around the circle for use in generating fake locations.
        private readonly List<MapPoint> _artificialMapPoints;

        // Index keeps track of where on the fake track you are.
        private int _locationIndex = 0;

        private volatile object stateLock = new object();

        private bool isStarted = false;

        protected override Task OnStartAsync()
        {
            lock (stateLock)
            {
                if (!isStarted)
                {
                    isStarted = true;

                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GPS_V_Log.txt");

                    fileDevice = new NmeaParser.NmeaFileDevice(filePath, 400);
                    fileDevice.MessageReceived += OnMessageReceived;

                    return fileDevice.OpenAsync();
                }
                return Task.CompletedTask;
            }
        }

        protected override Task OnStopAsync()
        {
            lock (stateLock)
            {
                if (isStarted)
                {
                    isStarted = false;
                    if (fileDevice != null)
                    {
                        Task.Run(async () => await fileDevice.CloseAsync()).Wait();
                        fileDevice.MessageReceived -= OnMessageReceived;
                        fileDevice?.Dispose();
                        fileDevice = null;
                    }
                }
                return Task.CompletedTask;
            }
        }


        public int TrackedSatelliteCount { get; protected set; } = 0;
        protected double Course { get; set; } = 0;
        protected double Velocity { get; set; } = 0;


        protected virtual void OnMessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
        {
            const double knotsToMeterPerSecond = 0.5144;

            try
            {
                double? latitude = null;
                double? longitude = null;

                switch (args.Message)
                {
                    case NmeaParser.Messages.Gga gga:
                        TrackedSatelliteCount = gga.NumberOfSatellites;             // Satellites being tracked
                                                                                    // gga.Hdop                                                 // Horizontal dilution of position
                        if (gga.Quality != NmeaParser.Messages.Gga.FixQuality.Invalid)
                        {
                            latitude = gga.Latitude;
                            longitude = gga.Longitude;
                        }
                        break;

                    case NmeaParser.Messages.Gsa gsa:
                        TrackedSatelliteCount = (gsa.SatelliteIDs?.Count() ?? 0);   // Satellite IDs used for fix
                                                                                    // gsa.Fix                                                  // None; 2D; or, 3D
                                                                                    // gsa.Pdop
                                                                                    // gsa.Hdop
                                                                                    // gsa.Vdop
                        break;

                    case NmeaParser.Messages.Rma rma:
                        if (rma.Status != NmeaParser.Messages.Rma.PositioningStatus.Invalid)
                        {
                            latitude = rma.Latitude;
                            longitude = rma.Longitude;
                            if (!double.IsNaN(rma.Course))
                            {
                                Course = rma.Course;
                            }
                            Velocity = (rma.Speed * knotsToMeterPerSecond);
                        }
                        break;

                    case NmeaParser.Messages.Rmc rmc:
                        latitude = rmc.Latitude;
                        longitude = rmc.Longitude;
                        if (!double.IsNaN(rmc.Course))
                        {
                            Course = rmc.Course;
                        }
                        Velocity = (rmc.Speed * knotsToMeterPerSecond);
                        break;
                }

                var validLatitude = ((latitude != null) && (!double.IsNaN(latitude.Value)));
                var validLongitude = ((longitude != null) && (!double.IsNaN(longitude.Value)));

                if (validLatitude && validLongitude)
                {
                    var mapPoint = new MapPoint(longitude.Value, latitude.Value, SpatialReferences.Wgs84);
                    var location = new Esri.ArcGISRuntime.Location.Location(mapPoint, 2, Velocity, Course, false);
                    UpdateLocation(location);
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
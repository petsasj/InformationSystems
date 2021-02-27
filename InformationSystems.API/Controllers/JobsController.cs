using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using InformationSystems.API.Models;
using Newtonsoft.Json.Linq;
using GeoJSON.Net.Feature;

namespace InformationSystems.API.Controllers
{
    public class JobsController : ControllerBase
    {
        private const string _authCode = "3b9084e71efa496aac6e3d1693e3889e";
        private readonly UnitOfWork _unitOfWork;

        public JobsController(UnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Route("FailedRequestsNotification")]
        public async Task<IActionResult> FailedRequestsNotification()
        {
            var failedRequests = _unitOfWork
                .Query<InfrastructureModificationRequest>()
                .Where(mr => mr.Approved == false || mr.HasValidationConflicts == true)
                .Where(mr => mr.Company.ReceiveConflictNotification)
                .GroupBy(mr => new { mr.Company.Oid, mr.Company.Vat, mr.Company.ConflictCallbackUrl });

            _unitOfWork.BeginTransaction();

            foreach (var companyRequests in failedRequests)
            {
                using var httpClient = new HttpClient();

                // if provider has given a page that informs in batches
                if (!string.IsNullOrWhiteSpace(companyRequests.Key.ConflictCallbackUrl))
                {
                    var body = Newtonsoft.Json.JsonConvert.SerializeObject(
                        companyRequests.Select(mr => new { mr.InternalId, mr.RejectionReason }));

                    var content = new StringContent(body, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(companyRequests.Key.ConflictCallbackUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        companyRequests.ToList().ForEach(mr =>
                        {
                            mr.ProviderConflictNotified = true;
                            mr.DateNotified = DateTime.UtcNow;
                        });
                    }
                }
                // if provider wants unique notifications for each failed request
                else
                {
                    foreach (var modificationRequest in companyRequests)
                    {
                        if (!string.IsNullOrWhiteSpace(modificationRequest.CallbackUrl))
                        {
                            var body = Newtonsoft.Json.JsonConvert.SerializeObject(new
                            { modificationRequest.InternalId, modificationRequest.RejectionReason });

                            var content = new StringContent(body, Encoding.UTF8, "application/json");

                            var response = await httpClient.PostAsync(modificationRequest.CallbackUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                modificationRequest.ProviderConflictNotified = true;
                                modificationRequest.DateNotified = DateTime.UtcNow;
                            }
                        }
                    }
                }

            }

            await _unitOfWork.CommitTransactionAsync();

            return Ok();
        }


        private async Task<bool> ValidateAndInsertGeoJsonData()
        {
            var unhandledModificationRequests = await _unitOfWork
                .Query<InfrastructureModificationRequest>()
                .Where(imr => !imr.DateValidated.HasValue)
                .ToListAsync();

            foreach (var modificationRequest in unhandledModificationRequests)
            {
                var tdr =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<TelecommunicationDataRequest>(modificationRequest
                        .OriginalRequest);

                var geoJson = tdr.Data;
                var type = geoJson["type"]?.Value<string>();

                if (type == null)
                {
                    throw new NullReferenceException("Type of model is unknown");
                }

                GeoJSON.Net.GeoJSONObject model = type.ToLower() switch
                {
                    "geometrycollection" => geoJson.ToObject<GeometryCollection>(),
                    "linestring" => geoJson.ToObject<LineString>(),
                    "multilinestring" => geoJson.ToObject<MultiLineString>(),
                    "multipoint" => geoJson.ToObject<MultiPoint>(),
                    "multipolygon" => geoJson.ToObject<MultiPolygon>(),
                    "point" => geoJson.ToObject<Point>(),
                    "polygon" => geoJson.ToObject<Polygon>(),
                    "feature" => geoJson.ToObject<Feature>(),
                    "featurecollection" => geoJson.ToObject<FeatureCollection>(),
                    _ => throw new ArgumentOutOfRangeException()
                };


                var infrastructure = new Infrastructure(_unitOfWork)
                {
                    Company = modificationRequest.Company,
                    InfrastructureType = modificationRequest.InfrastructureType,
                    GeoJSONType = model?.Type.ToString(),
                    ModificationRequest = modificationRequest
                };

                var geoLocations = new List<GeoLocation>();

                // boxing and unboxing
                switch (model)
                {
                    case MultiPoint multiPoint:
                        geoLocations = multiPoint?.Coordinates?.Select(c => new GeoLocation(_unitOfWork)
                        {
                            Altitude = c.Coordinates.Altitude,
                            Latitude = c.Coordinates.Latitude,
                            Longitude = c.Coordinates.Longitude
                        }).ToList();
                        break;
                    case Point point:
                        geoLocations.Add(new GeoLocation(_unitOfWork)
                        {
                            Altitude = point.Coordinates.Altitude,
                            Latitude = point.Coordinates.Latitude,
                            Longitude = point.Coordinates.Longitude
                        });
                        break;
                    case LineString lineString:
                        geoLocations = lineString?.Coordinates?.Select(c => new GeoLocation(_unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                    case MultiLineString multiLineString:
                        geoLocations = multiLineString.Coordinates?.SelectMany(c => c.Coordinates).Select(c => new GeoLocation(_unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                    case Polygon polygon:
                        geoLocations = polygon.Coordinates?.SelectMany(c => c.Coordinates).Select(c => new GeoLocation(_unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                    case MultiPolygon multiPolygon:
                        geoLocations = multiPolygon.Coordinates?.SelectMany(c => c.Coordinates).SelectMany(c => c.Coordinates).Select(c => new GeoLocation(_unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                }

                if (geoLocations?.Any() == true)
                {
                    infrastructure.GeoLocations.AddRange(geoLocations);
                }

                if (infrastructure.GeoLocations.Any())
                {
                    await _unitOfWork.CommitChangesAsync();
                }
            }

            return true;
        }
    }
}

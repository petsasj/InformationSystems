using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using InformationSystems.API.AttributeFilters;
using InformationSystems.API.Models;
using InformationSystems.API.Models.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InformationSystems.API.Controllers
{
    [ApiController]
    [Route("telcomdata")]
    public class DataController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;

        public DataController(UnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Used for querying of data from other public services
        /// Or other providers
        /// Must be authorized via JWT
        /// </summary>
        /// <param name="providerVat"></param>
        /// <param name="infrastructureType"></param>
        /// <returns></returns>
        [Route("getdata")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetData(string providerVat, string infrastructureType)
        {
            var companiesWithInfrastructures = _unitOfWork.Query<Company>()
                .Where(c => c.Infrastructures.Any(i => i.ModificationRequest.Approved ?? false));

            if (!string.IsNullOrWhiteSpace(providerVat))
            {
                companiesWithInfrastructures = companiesWithInfrastructures.Where(c => c.Vat == providerVat);
            }

            var results = new QueryResults();

            foreach (var company in companiesWithInfrastructures
)
            {
                var queryResult = new QueryResult
                {
                    CompanyName = company.RegisteredName,
                    CompanyVat = company.Vat,
                };

                var infrastructures = company.Infrastructures.Where(i => i.ModificationRequest.Approved ?? false);
                if (!string.IsNullOrWhiteSpace(infrastructureType))
                {
                    infrastructures = infrastructures
                        .Where(i => i.InfrastructureType.ToLower() == infrastructureType.ToLower());
                }

                if (infrastructures.Any())
                {
                    foreach (var infrastructure in infrastructures)
                    {
                        var pocoInfrastructure = new PocoInfrastructure()
                        {
                            InfrastructureType = infrastructure.InfrastructureType,
                            GeoLocations = infrastructure.GeoLocations.Select(i => new PocoGeoLocation
                            {
                                Altitude = i.Altitude,
                                Latitude = i.Latitude,
                                Longitude = i.Longitude
                            }).ToList()
                        };

                        queryResult.Infrastructures.Add(pocoInfrastructure);
                    }
                    results.Results.Add(queryResult);
                }
            }

            if (!results.Results.Any())
                return NoContent();

            return Ok(results);
        }


        /// <summary>
        /// Submission of data
        /// Mainly used by providers to submit their data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("post")]
        [HttpPost]
        [ProviderAuthorization]
        public async Task<IActionResult> PostData([FromBody] TelecommunicationDataRequest model)
        {
            var cachedCompany = HttpContext.Items["Company"] as PocoCompany;

            if (cachedCompany == null)
            {
                // TODO Add Logging
                throw new NullReferenceException("Cached Company not found.");
            }

            var company = await _unitOfWork.Query<Company>()
                .SingleOrDefaultAsync(c => c.InternalId == cachedCompany.InternalId);

            // should never happen since all requests are authenticated
            if (company == null)
            {
                throw new NullReferenceException("Company not found.");
            }

            // Logging request and response
            var request = new InfrastructureModificationRequest(_unitOfWork)
            {
                RequestType = "Addition",
                Company = company,
                OriginalRequest = JsonConvert.SerializeObject(model),
                CallbackUrl = model.CallBackUrl,
                InternalId = model.InternalId,
                InfrastructureType = model.InfrastructureType
            };

            try
            {
                var statusMessage = string.Empty;
                var geoJson = model.Data;

                // if nested GeoJSON is missing
                if (geoJson == null)
                {
                    statusMessage = "GeoJSON is empty or invalid";
                    request.SubmitSuccess = false;
                    request.InitialResponse = statusMessage;
                    request.DateInitialResponse = DateTime.UtcNow;

                    await _unitOfWork.CommitChangesAsync();

                    return BadRequest(statusMessage);
                }

                var type = geoJson["type"]?.Value<string>();

                var acceptedTypes = new[] { "GeometryCollection", "LineString", "MultilineString", "Multipoint", "MultiPolygon", "Point", "Polygon", "Feature", "FeatureCollection" };

                // if GeoJSON is of unsupported type
                if (type == null || acceptedTypes.All(s => s.ToLower() != type.ToLower()))
                {
                    statusMessage = "Unsupported GeoJSON type";
                    request.SubmitSuccess = false;
                    request.InitialResponse = statusMessage;
                    request.DateInitialResponse = DateTime.UtcNow;

                    await _unitOfWork.CommitChangesAsync();

                    return BadRequest(statusMessage);
                }


                // try to parse GeoJson, return error if failed
                GeoJSON.Net.GeoJSONObject deserializedResult = type.ToLower() switch
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

                // Success, provide Url Location to Query Task Progress
                var guid = Guid.NewGuid();
                request.SubmitSuccess = true;
                request.InitialResponse = "Accepted";
                request.DateInitialResponse = DateTime.UtcNow;
                request.UniqueId = guid;

                await _unitOfWork.CommitChangesAsync();

                // query process completion at action
                return Accepted($"/telcomdata/querytaskprogress/{guid}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Query Specific Data submission
        /// By guid returned
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("QueryTaskProgress/{guid}")]
        [ProviderAuthorization]
        public async Task<IActionResult> QueryTaskProgress(string guid)
        {
            var successful = Guid.TryParse(guid, out var uniqueId);

            if (!successful)
                return BadRequest("Malformed GUID.");

            var modificationRequest = await _unitOfWork.Query<InfrastructureModificationRequest>()
                .Where(so => so.UniqueId == uniqueId)
                .SingleOrDefaultAsync();

            if (modificationRequest == null)
                return BadRequest("No progress found for GUID.");

            var json = new
            {
                DateStarted = modificationRequest.DateCreated.ToString("yyyy-MM-dd HH:mm"),
                HasValidationConflicts = modificationRequest.HasValidationConflicts?.ToString(),
                DateValidated = modificationRequest.DateValidated.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm"),
                Finalized = modificationRequest.DateFinalized.HasValue,
                DateFinished = modificationRequest.DateFinalized.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm"),
                Published = modificationRequest.Approved ?? false
            };

            return Ok(json);
        }


        /// <summary>
        /// Returns all failed requests specific to
        /// Authorized Company
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("failedrequests")]
        [ProviderAuthorization]
        public async Task<IActionResult> GetFailedRequests()
        {
            var cachedCompany = HttpContext.Items["Company"] as PocoCompany;

            if (cachedCompany == null)
            {
                // TODO Add Logging
                throw new NullReferenceException("Cached Company not found.");
            }

            var company = await _unitOfWork.Query<Company>()
                .SingleOrDefaultAsync(c => c.InternalId == cachedCompany.InternalId);

            // should never happen since all requests are authenticated
            if (company == null)
            {
                throw new NullReferenceException("Company not found.");
            }

            var failedRequests = _unitOfWork
                .Query<InfrastructureModificationRequest>()
                .Where(mr => mr.Approved == false || mr.HasValidationConflicts == true)
                .Where(mr => mr.Company.Oid == company.Oid);

            if (!failedRequests.Any())
                return NoContent();

            foreach (var failedRequest in failedRequests)
            {
                failedRequest.DateNotified = DateTime.UtcNow;
                failedRequest.ProviderConflictNotified = true;
            }

            await _unitOfWork.CommitChangesAsync();

            var response = failedRequests.Select(mr => new { mr.InternalId, mr.RejectionReason });

            return Ok(response);
        }
    }
}
﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using InformationSystems.API.Models;

namespace InformationSystems.API.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        // Display purposes, passwords in plain sight are strictly prohibited
        private const string _authCode = "3b9084e71efa496aac6e3d1693e3889e";
        private readonly UnitOfWork _unitOfWork;

        public AdminController(UnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// This is purely for display purposes
        /// Company creation will be done via back-end
        /// </summary>
        /// <param name="authCode"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("addcompany")]
        [HttpPost]
        public async Task<IActionResult> AddCompany([FromHeader] string authCode, [FromBody] CompanyCreationModel model)
        {
            // handle admin
            if (authCode != _authCode)
                return Unauthorized("Wrong authentication code");

            // handle missing values
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var company = new Company(_unitOfWork)
                {
                    RegisteredName = model.RegisteredName,
                    Address = model.Address,
                    ConflictCallbackUrl = model.CallbackUrl,
                    ReceiveConflictNotification = model.ReceiveConflictNotification,
                    IsProvider = model.IsProvider,
                    Vat = model.Vat,
                    IsActive = true
                };

                company.SetPassword(model.Password);

                await _unitOfWork.CommitChangesAsync();

                return Ok($"Created company");
            }
            catch (Exception ex)
            {
                // TODO Add logging
                return BadRequest(ex.Message);
            }
        }
    }
}

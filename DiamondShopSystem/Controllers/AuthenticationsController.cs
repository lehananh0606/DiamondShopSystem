﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Service.ViewModels.AcountToken;

using Service.ViewModels.Request;
using Service.ViewModels.AccountToken;
using Service.ViewModels.Response;
using Service.Commons;
using DiamondShopSystem.Constants;
using Service.Utils;
using Service.Exceptions;
using Service.IServices;
using DiamondShopSystem.Middleware;
using Microsoft.AspNetCore.Authorization;
using Service.Contants;
using DiamondShopSystem.Authorization;

namespace DiamondShopSystem.Controllers
{
    [ApiController]
    public class AuthenticationsController : BaseController
    {
        private IAuthenticationService _authenticationService;
        private IOptions<JWTAuth> _jwtAuthOptions;
        private IValidator<AccountRequest> _accountRequestValidator;
        private IValidator<AccountTokenRequest> _accountTokenRequestValidator;
        private readonly ILogger<ExceptionMiddleware> _logger;
        // private IValidator<ResetPasswordRequest> _resetPasswordValidator;
        public AuthenticationsController(IAuthenticationService authenticationService, IOptions<JWTAuth> jwtAuthOptions,
            IValidator<AccountRequest> accountRequestValidator, IValidator<AccountTokenRequest> accountTokenRequestValidator,
            ILogger<ExceptionMiddleware> logger)
           // IValidator<ResetPasswordRequest> resetPasswordValidator)
        {
            this._authenticationService = authenticationService;
            this._jwtAuthOptions = jwtAuthOptions;
            this._accountRequestValidator = accountRequestValidator;
            this._accountTokenRequestValidator = accountTokenRequestValidator;
            this._logger = logger;
           // this._resetPasswordValidator = resetPasswordValidator;
        }

        #region Login API
        /// <summary>
        /// Login to access into the system by your account.
        /// </summary>
        /// <param name="account">
        /// Account object contains Email property and Password property. 
        /// Notice that the password must be hashed with MD5 algorithm before sending to Login API.
        /// </param>
        /// <returns>
        /// An Object with a json format that contains Account Id, Email, Role name, and a pair token (access token, refresh token).
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         POST 
        ///         {
        ///             "email": "abc@gmail.com"
        ///             "password": "********"
        ///         }
        /// </remarks>
        /// <response code="200">Login Successfully.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="404">Some Error about request data not found.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [HttpPost(APIEndPointConstant.Authentication.Login)]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostLoginAsync([FromBody] AccountRequest account)
        {
            try
            {
                var validationResult = await _accountRequestValidator.ValidateAsync(account);
                if (!validationResult.IsValid)
                {
                    var errors = ErrorUtil.GetErrorsString(validationResult);
                    throw new BadRequestException(errors);
                }

                var accountResponse = await _authenticationService.LoginAsync(account, _jwtAuthOptions.Value);
                return Ok(accountResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                throw;
            }
        }
        #endregion

        #region Re-GenerateTokens API
        /// <summary>
        /// Re-generate pair token from the old pair token that are provided by the MBKC system before.
        /// </summary>
        /// <param name="accountToken">
        /// AccountToken Object contains access token property and refresh token property.
        /// </param>
        /// <returns>
        /// The new pair token (Access token, Refresh token) to continue access into the MBKC system.
        /// </returns>
        /// <remarks>
        ///     Sample request:
        ///
        ///         POST 
        ///         {
        ///             "accessToken": "abcxyz"
        ///             "refreshToken": "klmnopq"
        ///         }
        /// </remarks>
        /// <response code="200">Re-Generate Token Successfully.</response>
        /// <response code="404">Some Error about request data that are not found.</response>
        /// <response code="400">Some Error about request data and logic data.</response>
        /// <response code="500">Some Error about the system.</response>
        /// <exception cref="NotFoundException">Throw Error about request data that are not found.</exception>
        /// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        /// <exception cref="Exception">Throw Error about the system.</exception>
        [HttpPost(APIEndPointConstant.Authentication.ReGenerationTokens)]
        [ProducesResponseType(typeof(AccountTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        [PermissionAuthorize(RoleConstants.Customer)]
        public async Task<IActionResult> PostReGenerateTokensAsync([FromBody] AccountTokenRequest accountToken)
        {
            var validationResult = await _accountTokenRequestValidator.ValidateAsync(accountToken);
            if (!validationResult.IsValid)
            {
                var errors = ErrorUtil.GetErrorsString(validationResult);
                throw new BadRequestException(errors);
            }

            var accountTokenResponse = await _authenticationService.ReGenerateTokensAsync(accountToken, _jwtAuthOptions.Value);
            return Ok(accountTokenResponse);
        }
        #endregion







        //#region Reset Password
        ///// <summary>
        ///// A new password will be updated after the email is verified before.
        ///// </summary>
        ///// <param name="resetPassword">
        ///// ResetPassword object contains Email property and new password property.
        ///// Notice that the new password must be hashed with MD5 algorithm before sending to Login API.
        ///// </param>
        ///// <returns>
        ///// A success message about the resetring password procedure.
        ///// </returns>
        ///// <remarks>
        /////     Sample request:
        /////
        /////         PUT
        /////         {
        /////             "email": "abc@gmail.com"
        /////             "newPassword": "********"
        /////         }
        ///// </remarks>
        ///// <response code="200">A success message about the resetring password procedure.</response>
        ///// <response code="400">Some Error about request data and logic data.</response>
        ///// <response code="500">Some Error about the system.</response>
        ///// <exception cref="BadRequestException">Throw Error about request data and logic bussiness.</exception>
        ///// <exception cref="Exception">Throw Error about the system.</exception>
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(Error), StatusCodes.Status500InternalServerError)]
        //[Consumes(MediaTypeConstant.ApplicationJson)]
        //[Produces(MediaTypeConstant.ApplicationJson)]
        //[HttpPut(APIEndPointConstant.Authentication.PasswordResetation)]
        //public async Task<IActionResult> PutResetPasswordAsync([FromBody]ResetPasswordRequest resetPassword)
        //{
        //    ValidationResult validationResult = await this._resetPasswordValidator.ValidateAsync(resetPassword);
        //    if(validationResult.IsValid == false)
        //    {
        //        string errors = ErrorUtil.GetErrorsString(validationResult);
        //        throw new BadRequestException(errors);
        //    }
        //    await this._authenticationService.ChangePasswordAsync(resetPassword);
        //    return Ok(new
        //    {
        //        Message = MessageConstant.AuthenticationMessage.ResetPasswordSuccessfully
        //    });
        //}
        //#endregion
    }
}

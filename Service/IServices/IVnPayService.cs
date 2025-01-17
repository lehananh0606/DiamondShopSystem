﻿using Microsoft.AspNetCore.Http;
using Service.Commons;
using Service.ViewModels.Request;
using ShopRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IServices
{
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        Task<VnPaymentResponseModel> PaymentExecute(IQueryCollection collections);

        Task<OperationResult<Transaction>> DepositPayment(VnPaymentResponseModel response);
        Task<OperationResult<bool>> PayOrderWithWalletBalance(int orderId, int userId);
    }
}

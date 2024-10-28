﻿using BAL.Models;
using BAL.RequestModels;
using BAL.RequestModels.Customer;
using BAL.ResponseModels;
using BAL.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.BusinessLogic.Interface
{
    public interface ICustomerHelper
    {
        Task<DataTable> CustomerLogin(string username, string password);
        Task<LoginResponse> AdminLogin(string adminId, string password);        
        Task<string> AddCustomer(CustomerAddRequest customer);
        Task<string> EditCustomer(CustomerEditRequest customer);
        Task<string> DeleteCustomer(string customerId);
        Task<string> AddUpdateBusinessInfo(CustomerBusinessInfo businessInfo);
        Task<UploadResponse> UploadImage(IFormFile image);
        Task<Response<CustomerResponse>> GetCustomerByCustomerId(string customerId);
        Task<Response<Customer>> GetCustomers(string? customerId, string? email, string? mobile);
        Task<Response<Customer>> GetByFilterCriteria(CustomerFilterCriteria filterCriteria);
        Task<Response<CustomerAddress>> GetByCustomerId(string customerId);
        Task<Response<CustomerAddress>> GetAddressById(string addressId);
        Task<Response<CustomerAddress>> AddUpdateAddress(CustomerAddress customerAddress);
        Task<Response<CustomerAddress>> DeleteAddress(string addressId);
        Task<Response<string>> Activate(string customerId, string? comments);
        Task<Response<string>> Deactivate(string customerId, string? comments);
        Task<Response<BeneficiaryDetails>> AddUpdateBeneficiaryDetail(BeneficiaryDetails beneficiaryDetails);
        Task<Response<BeneficiaryDetails>> GetBeneficiaryByCustomerId(string customerId);
        Task<Response<CustomerAuditHistory>> ActivateDeactivateHistoryByCustomerId(string customerId);
        Task<Response<string>> SendChangePasswordLink(string customerId);
        Task<Response<ViewModels.Customer>> ChangePassword(string customerId, string newPassword);
    }
}

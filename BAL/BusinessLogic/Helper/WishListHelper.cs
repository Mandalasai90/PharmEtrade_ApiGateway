﻿using BAL.BusinessLogic.Interface;
using BAL.Models;
using BAL.RequestModels;
using DAL;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using BAL.ResponseModels;
using BAL.ViewModels;
using BAL.Common;
using Microsoft.Extensions.Configuration;


namespace BAL.BusinessLogic.Helper
{
    public class WishListHelper : IWishListHelper
    {
        private IsqlDataHelper _sqlDataHelper;
        private IConfiguration _configuration;

        public WishListHelper(IsqlDataHelper isqlDataHelper)
        {
            _sqlDataHelper = isqlDataHelper;
        }
        private string ConnectionString
        {
            get
            {
                return _configuration.GetConnectionString("APIDBConnectionString") ?? "";
            }
        }

        public async Task<Response<WishList>> AddToWishList(WishListRequest request)
        {
            var response = new Response<WishList>();
            try
            {
                MySqlCommand command = new MySqlCommand("sp_AddUpdateWishlist");
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("p_WishListId", request.WishListId);
                command.Parameters.AddWithValue("p_CustomerId", request.CustomerId);
                command.Parameters.AddWithValue("p_ProductId", request.ProductId);
                command.Parameters.AddWithValue("p_IsActive", 1);
                DataTable tblwishlist = await Task.Run(() => _sqlDataHelper.SqlDataAdapterasync(command));
                if (tblwishlist.Rows.Count > 0)
                {
                    response.Result = MapDataTableToWishListList(tblwishlist);
                }
                else
                {
                    response.Result = new List<WishList>(); // Return an empty list if no rows are returned
                }

                response.StatusCode = 200;
                response.Message = "Successfully Feched data.";
                response.Result = MapDataTableToWishListList(tblwishlist); ;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private static List<WishList> MapDataTableToWishListList(DataTable tblwishlist)
        {
            List<WishList> lstwishlist = new List<WishList>();
            foreach (DataRow wishlistItem in tblwishlist.Rows)
            {
                WishList item = new WishList();
                item.WishListId = wishlistItem["WishListId"].ToString();
                item.IsActive = Convert.ToInt32(wishlistItem["IsActive"]) == 1 ? true : false;
                item.DeletedOn = wishlistItem["DeletedOn"] != DBNull.Value ? Convert.ToDateTime(wishlistItem["DeletedOn"]) : DateTime.MinValue;

                //Add Basic Customer Details
                item.Customer = new CustomerBasicDetails();
                item.Product = new ProductBasicDetails();
                //string str = wishlistItem["CustomerId"].ToString();



                item.Customer.CustomerId = wishlistItem["CustomerId"].ToString() ?? "";
                item.Customer.FirstName = wishlistItem["FirstName"].ToString() ?? "";
                item.Customer.LastName = wishlistItem["LastName"].ToString() ?? "";
                item.Customer.Email = wishlistItem["Email"].ToString() ?? "";
                item.Customer.Mobile = wishlistItem["Mobile"].ToString() ?? "";
                item.Customer.CustomerTypeId = Convert.ToInt32(wishlistItem["CustomerTypeId"] != DBNull.Value ? wishlistItem["CustomerTypeId"] : 0);
                item.Customer.AccountTypeId = Convert.ToInt32(wishlistItem["AccountTypeId"] != DBNull.Value ? wishlistItem["AccountTypeId"] : 0);
                item.Customer.IsUPNMember = Convert.ToInt32(wishlistItem["IsUPNMember"] != DBNull.Value ? wishlistItem["IsUPNMember"] : 0);

                //Add Basic Product Details
                item.Product.ProductID = wishlistItem["ProductID"].ToString() ?? "";
                item.Product.ProductCategoryId = Convert.ToInt32(wishlistItem["ProductCategoryId"] != DBNull.Value ? wishlistItem["ProductCategoryId"] : 0);
                item.Product.ProductGalleryId = wishlistItem["ProductGalleryId"].ToString() ?? "";
                item.Product.ProductName = wishlistItem["ProductName"].ToString() ?? "";
                item.Product.SalePrice = Convert.ToDecimal(wishlistItem["SalePrice"] != DBNull.Value ? wishlistItem["SalePrice"] : 0.0);
                item.Product.BrandName = wishlistItem["BrandName"].ToString() ?? "";
                item.Product.Manufacturer = wishlistItem["Manufacturer"].ToString() ?? "";
                item.Product.ImageUrl = wishlistItem["ImageUrl"].ToString() ?? "";
                item.Product.Caption = wishlistItem["Caption"].ToString() ?? "";
                item.Product.ExpiryDate = Convert.ToDateTime(wishlistItem["ExpiryDate"] != DBNull.Value ? wishlistItem["ExpiryDate"] : (DateTime?)null);

                lstwishlist.Add(item);
            }
            return lstwishlist;
        }

        public async Task<Response<WishList>> GetWishListItems(string customerId = null)
        {
            var response = new Response<WishList>();
            try
            {
                MySqlCommand command = new MySqlCommand("sp_GetWishListItems");
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("p_CustomerId", customerId);
                DataTable tblwishlist = await Task.Run(() => _sqlDataHelper.SqlDataAdapterasync(command));

                response.StatusCode = 200;
                response.Message = "Successfully Feched data.";
                response.Result = MapDataTableToWishListList(tblwishlist);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }
        public async Task<Response<WishList>> GetWishListById(string WishListId = null)
        {
            var response = new Response<WishList>();
            try
            {
                MySqlCommand command = new MySqlCommand("sp_GetWishListById");
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("p_WishListId", WishListId);
                DataTable tblwishlist = await Task.Run(() => _sqlDataHelper.SqlDataAdapterasync(command));

                response.StatusCode = 200;
                response.Message = "Successfully Feched data.";
                response.Result = MapDataTableToWishListList(tblwishlist);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public async Task<Response<WishList>> RemoveWishList(string wishlistId)
        {
            var response = new Response<WishList>();
            using (MySqlConnection sqlcon = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(StoredProcedures.DELETE_WishList, sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@p_WishListId", wishlistId);
                    MySqlParameter paramMessage = new MySqlParameter("@o_Message", MySqlDbType.String)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(paramMessage);

                    try
                    {
                        DataTable tblwishlist = await Task.Run(() => _sqlDataHelper.SqlDataAdapterasync(cmd));

                        if (tblwishlist.Rows.Count > 0)
                        {
                            response.StatusCode = 200;
                            response.Message = string.IsNullOrEmpty(paramMessage.Value.ToString()) ? "Success" : paramMessage.Value.ToString();
                            response.Result = MapDataTableToWishListList(tblwishlist);
                        }
                        else
                        {
                            response.StatusCode = 400;
                            response.Message = "Failed to delete banner.";
                            response.Result = null;
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 500)
                    {
                        //Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertAddToCartProduct : errormessage:" + ex.Message.ToString()));
                        response.StatusCode = 500;
                        response.Message = "ERROR : " + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        //Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertAddToCartProduct : errormessage:" + ex.Message.ToString()));
                        response.StatusCode = 500;
                        response.Message = "ERROR : " + ex.Message;
                    }
                    return response;
                }
            }
        }
    }

}

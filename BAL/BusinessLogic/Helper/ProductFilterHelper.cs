﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BAL.BusinessLogic.Interface;
using BAL.Common;
using DAL;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;
using System.Security.AccessControl;
using DAL.Models;
using MySql.Data.MySqlClient;
namespace BAL.BusinessLogic.Helper
{
    public  class ProductFilterHelper:IProductFilter
    {
        private readonly IsqlDataHelper _isqlDataHelper;
        private readonly string _connectionString;
        private string exFolder = Path.Combine("GetFilteredProductsExceptionLogs");
        private string exPathToSave = string.Empty;

        public ProductFilterHelper(IConfiguration configuration, IsqlDataHelper isqlDataHelper)
        {
            _isqlDataHelper = isqlDataHelper;
            _connectionString = configuration.GetConnectionString("OnlineexamDB");
            exPathToSave = Path.Combine(Directory.GetCurrentDirectory(), exFolder);
        }
        // Author: [Mamatha]
        // Created Date: [01/07/2024]
        // Description: Method for GetProductFilter
        public async Task<DataTable> GetFilteredProducts(string productName)
        {
            using (MySqlConnection sqlcon = new MySqlConnection(_connectionString))
            using (MySqlCommand cmd = new MySqlCommand("SP_GetFilteredProducts", sqlcon))
            {
                try
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("ProductCategoryID", productCategoryId);
                    cmd.Parameters.AddWithValue("inputProductName", productName);
                    await sqlcon.OpenAsync();
                    DataTable dt = await _isqlDataHelper.SqlDataAdapterasync(cmd);
                    return dt;
                }

                catch (Exception ex)
                {
                    await Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(exPathToSave, "GetFilteredProducts_sp :  errormessage:" + ex.Message.ToString()));
                    throw;
                }
            }
        }

        //Author:[Mamatha]
        //Created Date:[02/07/2024]
        //Description:Method for GetProducts
        public async Task<DataTable> GetProducts()
        {
            MySqlConnection sqlcon = new MySqlConnection(_connectionString);
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                cmd = new MySqlCommand("SP_GetAllProducts", sqlcon);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                return await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmd));

            }
            catch(Exception ex)
            {
                Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(exPathToSave, "GetAllProducts_sp :  errormessage:" + ex.Message.ToString()));
                throw ex;

            }
        }
        public async Task<DataTable> GetProductsById(int AddproductID)
        {
            MySqlConnection sqlcon = new MySqlConnection(_connectionString);
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                cmd = new MySqlCommand("SP_GetProductById", sqlcon);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("in_AddproductID", AddproductID);

                return await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmd));
            }
            catch(Exception ex)
            {
                Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(exPathToSave, "GetProductsById_sp:errormessage:" + ex.Message.ToString()));
                throw ex;
            }
        }






    }  
}

﻿using BAL.ViewModel;
using DAL.Models;
using BAL.BusinessLogic.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using DAL;
using BAL.Common;
using BAL.ViewModels;
using System.Net.Http.Headers;
using OfficeOpenXml;

namespace BAL.BusinessLogic.Helper
{
    public class ProductHelper : IProductHelper
    {
        private readonly IsqlDataHelper _isqlDataHelper;
        private readonly string _connectionString;
        private readonly string _exPathToSave;

        public ProductHelper(IConfiguration configuration, IsqlDataHelper isqlDataHelper)
        {
            _isqlDataHelper = isqlDataHelper;
            _connectionString = configuration.GetConnectionString("OnlineexamDB");
            _exPathToSave = Path.Combine(Directory.GetCurrentDirectory(), "ProductExceptionLogs");
        }
        // Author: [swathi]
        // Created Date: [02/07/2024]
        // Description: Method for InsertProducts
        public async Task<string> InsertAddProduct(ProductFilter productviewmodel)
        {
            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertAddProduct", sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Productcategory_id", productviewmodel.Productcategory_id);
                    cmd.Parameters.AddWithValue("@ImageID", productviewmodel.ImageID);
                    cmd.Parameters.AddWithValue("@Sizeid", productviewmodel.Sizeid);
                    cmd.Parameters.AddWithValue("@ProductName", productviewmodel.ProductName);
                    cmd.Parameters.AddWithValue("@NDCorUPC", productviewmodel.NDCorUPC);
                    cmd.Parameters.AddWithValue("@BrandName", productviewmodel.BrandName);
                    cmd.Parameters.AddWithValue("@PriceName", productviewmodel.PriceName);
                    cmd.Parameters.AddWithValue("@UPNmemberPrice", productviewmodel.UPNmemberPrice);
                    cmd.Parameters.AddWithValue("@AmountInStock", productviewmodel.AmountInStock);
                    cmd.Parameters.AddWithValue("@Taxable", productviewmodel.Taxable);
                    cmd.Parameters.AddWithValue("@SalePrice", productviewmodel.SalePrice);
                    cmd.Parameters.AddWithValue("@SalePriceFrom", productviewmodel.SalePriceFrom);
                    cmd.Parameters.AddWithValue("@SalePriceTo", productviewmodel.SalePriceTo);
                    cmd.Parameters.AddWithValue("@Manufacturer", productviewmodel.Manufacturer);
                    cmd.Parameters.AddWithValue("@Strength", productviewmodel.Strength);
                    cmd.Parameters.AddWithValue("@Fromdate", productviewmodel.Fromdate);
                    cmd.Parameters.AddWithValue("@LotNumber", productviewmodel.LotNumber);
                    cmd.Parameters.AddWithValue("@ExpirationDate", productviewmodel.ExpirationDate);
                    cmd.Parameters.AddWithValue("@PackQuantity", productviewmodel.PackQuantity);
                    cmd.Parameters.AddWithValue("@PackType", productviewmodel.PackType);
                    cmd.Parameters.AddWithValue("@PackCondition", productviewmodel.PackCondition);
                    cmd.Parameters.AddWithValue("@ProductDescription", productviewmodel.ProductDescription);

                    try
                    {
                        await sqlcon.OpenAsync();
                        var result = await cmd.ExecuteNonQueryAsync();
                        return "Success"; // Return the number of affected rows
                    }
                    catch (Exception ex)
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProduct :  errormessage:" + ex.Message.ToString()));
                        // Handle the exception as needed
                        throw;
                    }
                }
            }
        }



        // Author: [Mamatha]
        // Created Date: [04/07/2024]
        // Description: Method for EditProductDetails
        public async Task<string> EditProductDetails(int AddproductID,ProductFilter productfilter)
        {
            SqlConnection sqlcon = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("SP_EditProduct", sqlcon);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AddproductID", productfilter.AddproductID);
                cmd.Parameters.AddWithValue("@Productcategory_id", productfilter.Productcategory_id);
                cmd.Parameters.AddWithValue("@ImageID", productfilter.ImageID);
                cmd.Parameters.AddWithValue("@Sizeid", productfilter.Sizeid);
                cmd.Parameters.AddWithValue("@ProductName", productfilter.ProductName);
                cmd.Parameters.AddWithValue("@NDCorUPC", productfilter.NDCorUPC);
                cmd.Parameters.AddWithValue("@BrandName", productfilter.BrandName);
                cmd.Parameters.AddWithValue("@PriceName", productfilter.PriceName);
                cmd.Parameters.AddWithValue("@UPNmemberPrice", productfilter.UPNmemberPrice);
                cmd.Parameters.AddWithValue("@AmountInStock", productfilter.AmountInStock);
                cmd.Parameters.AddWithValue("@Taxable", productfilter.Taxable);
                cmd.Parameters.AddWithValue("@SalePrice", productfilter.SalePrice);
                cmd.Parameters.AddWithValue("@SalePriceFrom", productfilter.SalePriceFrom);
                cmd.Parameters.AddWithValue("@SalePriceTo", productfilter.SalePriceTo);
                cmd.Parameters.AddWithValue("@Manufacturer", productfilter.Manufacturer);
                cmd.Parameters.AddWithValue("@Strength", productfilter.Strength);
                cmd.Parameters.AddWithValue("@Fromdate", productfilter.Fromdate);
                cmd.Parameters.AddWithValue("@LotNumber", productfilter.LotNumber);
                cmd.Parameters.AddWithValue("@ExpirationDate", productfilter.ExpirationDate);
                cmd.Parameters.AddWithValue("@PackQuantity", productfilter.PackQuantity);
                cmd.Parameters.AddWithValue("@PackType", productfilter.PackType);
                cmd.Parameters.AddWithValue("@PackCondition", productfilter.PackCondition);
                cmd.Parameters.AddWithValue("@ProductDescription", productfilter.ProductDescription);

                await sqlcon.OpenAsync();
                string result = await cmd.ExecuteScalarAsync() as string;
                return "Success";
            }
            catch(Exception ex)
            {
                Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "EditProductDetails:ErrorMessage-" + ex.Message.ToString()));
                throw ex;
            }
        }
        //public async Task<int> InsertAddToCartProduct(AddToCartViewModel addToCartModel)
        //{
        //    using (SqlConnection sqlcon = new SqlConnection(_connectionString))
        //    {
        //        using (SqlCommand cmd = new SqlCommand("InsertAddtoCartProduct", sqlcon))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            cmd.Parameters.AddWithValue("@Userid", addToCartModel.Userid);
        //            cmd.Parameters.AddWithValue("@Imageid", addToCartModel.Imageid);
        //            cmd.Parameters.AddWithValue("@ProductId", addToCartModel.ProductId);

        //            try
        //            {
        //                await sqlcon.OpenAsync();
        //                var result = await cmd.ExecuteScalarAsync();

        //                // Check if the product was successfully added
        //                if (result != null && int.TryParse(result.ToString(), out int newAddtoCartId))
        //                {
        //                    return newAddtoCartId; // Return the new AddtoCartId
        //                }
        //                else
        //                {
        //                    // Handle error or duplicate insert scenario
        //                    throw new Exception("Failed to add product to cart.");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertAddToCartProduct : errormessage:" + ex.Message.ToString()));
        //                throw;
        //            }
        //        }
        //    }
        //}
        // Author: [swathi]
        // Created Date: [02/07/2024]
        // Description: Method for AddtoCartProducts
        //public async Task<string> InsertAddToCartProduct(AddToCartViewModel addToCartModel)
        //{
        //    using (SqlConnection sqlcon = new SqlConnection(_connectionString))
        //    {
        //        using (SqlCommand cmd = new SqlCommand("InsertAddtoCartProduct", sqlcon))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            cmd.Parameters.AddWithValue("@Userid", addToCartModel.Userid);
        //            cmd.Parameters.AddWithValue("@Imageid", addToCartModel.Imageid);
        //            cmd.Parameters.AddWithValue("@ProductId", addToCartModel.ProductId);

        //            try
        //            {
        //                await sqlcon.OpenAsync();


        //                bool isProductAlreadyAdded = await IsProductAlreadyAdded(sqlcon, addToCartModel.Userid, addToCartModel.Imageid, addToCartModel.ProductId);

        //                if (isProductAlreadyAdded)
        //                {
        //                    throw new Exception("Product is already added to the cart.");
        //                }

        //                var result = await cmd.ExecuteScalarAsync();


        //                if (result != null && int.TryParse(result.ToString(), out int newAddtoCartId))
        //                {
        //                    return "Success"; 
        //                }
        //                else
        //                {

        //                    throw new Exception("Failed to add product to cart.");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertAddToCartProduct : errormessage:" + ex.Message.ToString()));
        //                throw;
        //            }
        //        }
        //    }
        //}

        // Author: [swathi]
        // Created Date: [02/07/2024]
        // Description: Method for AddtoCartProducts
        public async Task<string> InsertAddToCartProduct(AddToCartViewModel addToCartModel)
        {
            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertAddtoCartProduct", sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Userid", addToCartModel.Userid);
                    cmd.Parameters.AddWithValue("@Imageid", addToCartModel.Imageid);
                    cmd.Parameters.AddWithValue("@ProductId", addToCartModel.ProductId);
                    cmd.Parameters.AddWithValue("@Quantity", addToCartModel.Quantity);

                    SqlParameter newCartIdParam = new SqlParameter("@NewCartId", SqlDbType.Int);
                    newCartIdParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(newCartIdParam);

                    try
                    {
                        await sqlcon.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();

                        if (newCartIdParam.Value != null && int.TryParse(newCartIdParam.Value.ToString(), out int newAddtoCartId))
                        {
                            return "Success";
                        }
                        else
                        {
                            throw new Exception("Failed to add product to cart.");
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 500) // SQL Server user-defined error
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertAddToCartProduct : errormessage:" + ex.Message.ToString()));
                        return "Error: " + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertAddToCartProduct : errormessage:" + ex.Message.ToString()));
                        throw;
                    }
                }
            }
        }


        // Usage example



        private async Task<bool> IsProductAlreadyAdded(SqlConnection sqlcon, int userId, int imageId, int productId)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [PharmEtradeDB].[dbo].[AddtoCartproduct] WHERE Userid = @Userid AND Imageid = @Imageid AND ProductId = @ProductId", sqlcon))
            {
                cmd.Parameters.AddWithValue("@Userid", userId);
                cmd.Parameters.AddWithValue("@Imageid", imageId);
                cmd.Parameters.AddWithValue("@ProductId", productId);

                var count = await cmd.ExecuteScalarAsync();
                return (int)count > 0;
            }
        }

        public Task<Productviewmodel> DummyInterface(Productviewmodel pvm)
        {
            // Dummy implementation
            return Task.FromResult(pvm);
        }
        // Author: [swathi]
        // Created Date: [03/07/2024]
        // Description: Method for GetCartProducts based on userid
        public async Task<IEnumerable<UserProductViewModel>> GetByUserId(int userId)
        {
            var products = new List<UserProductViewModel>();

            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetByUserId", sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    try
                    {
                        await sqlcon.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(new UserProductViewModel
                                {
                                    ProductName = reader["ProductName"].ToString(),
                                    ImageUrl = reader["image_url"].ToString(),
                                    BrandName = reader["BrandName"].ToString(),
                                    PriceName = reader["PriceName"].ToString(),
                                    PackQuantity = Convert.ToInt32(reader["PackQuantity"])
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "GetByUserId : errormessage:" + ex.Message.ToString()));
                        throw;
                    }
                }
            }

            return products;
        }
        // Author: [swathi]
        // Created Date: [04/07/2024]
        // Description: Method for  Delete CartProduct
        public async Task<string> SoftDeleteAddtoCartProduct(int addToCartId)
        {
            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SoftDeleteAddtoCartproduct", sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AddtoCartId", addToCartId);

                    try
                    {
                        await sqlcon.OpenAsync();
                        var result = await cmd.ExecuteScalarAsync();

                        if (result != null && result.ToString() == "AlreadyDeleted")
                        {
                            return "Failed"; 
                        }
                        else if (result != null && result.ToString() == "Success")
                        {
                            return "Success"; 
                        }
                        else
                        {
                            return "Failed"; 
                        }
                    }
                    catch (Exception ex)
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "SoftDeleteAddtoCartProduct : errormessage:" + ex.Message.ToString()));
                        throw;
                    }
                }
            }
        }

        // Author: [swathi]
        // Created Date: [05/07/2024]
        // Description: Method for  Insert WishlistProduct
        public async Task<string> InsertWishlistproduct(Wishlistviewmodel wishlistviewmodel)
        {
            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertWishlistItem", sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Userid", wishlistviewmodel.Userid);
                    cmd.Parameters.AddWithValue("@Imageid", wishlistviewmodel.Imageid);
                    cmd.Parameters.AddWithValue("@ProductId", wishlistviewmodel.ProductId);

                    try
                    {
                        await sqlcon.OpenAsync();

                        // Check if the product already exists for the user
                        bool isProductAlreadyAdded = await WishlistIsProductAlreadyAdded(sqlcon, wishlistviewmodel.Userid, wishlistviewmodel.Imageid, wishlistviewmodel.ProductId);

                        if (isProductAlreadyAdded)
                        {
                            throw new Exception("Product is already added to the wishlist.");
                        }

                        var result = await cmd.ExecuteScalarAsync();

                        // Check if the product was successfully added
                        if (result != null && int.TryParse(result.ToString(), out int newWishlistid))
                        {
                            return "Success"; 
                        }
                        else
                        {
                           
                            throw new Exception("Failed to add product to wishlist.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertWishlistproduct : errormessage:" + ex.Message.ToString()));
                        throw;
                    }
                }


            }

        }
        private async Task<bool> WishlistIsProductAlreadyAdded(SqlConnection sqlcon, int userId, int imageId, int productId)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [PharmEtradeDB].[dbo].[Wishlist] WHERE Userid = @Userid AND Imageid = @Imageid AND ProductId = @ProductId", sqlcon))
            {
                cmd.Parameters.AddWithValue("@Userid", userId);
                cmd.Parameters.AddWithValue("@Imageid", imageId);
                cmd.Parameters.AddWithValue("@ProductId", productId);

                var count = await cmd.ExecuteScalarAsync();
                return (int)count > 0;
            }
        }
        // Author: [swathi]
        // Created Date: [05/07/2024]
        // Description: Method for  GetwishlistProduct by userid
        public async Task<IEnumerable<UserProductViewModel>> GetwhislistByUserId(int userId)
        {
            var products = new List<UserProductViewModel>();

            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetWishlistByUserId", sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    try
                    {
                        await sqlcon.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                products.Add(new UserProductViewModel
                                {
                                    ProductName = reader["ProductName"].ToString(),
                                    ImageUrl = reader["image_url"].ToString(),
                                    BrandName = reader["BrandName"].ToString(),
                                    PriceName = reader["PriceName"].ToString(),
                                    PackQuantity = Convert.ToInt32(reader["PackQuantity"])
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "GetwhislistByUserId : errormessage:" + ex.Message.ToString()));
                        throw;
                    }
                }
            }

            return products;
        }
        // Author: [swathi]
        // Created Date: [05/07/2024]
        // Description: Method for  Delete WishListProduct
        public async Task<string> DeleteWishlistproduct(int wishlistid)
        {
            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SoftDeleteWishlistItem", sqlcon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@WishlistId", wishlistid);

                    try
                    {
                        await sqlcon.OpenAsync();
                        var result = await cmd.ExecuteScalarAsync();

                        if (result != null && result.ToString() == "AlreadyDeleted")
                        {
                            return "Failed";
                        }
                        else if (result != null && result.ToString() == "Success")
                        {
                            return "Success";
                        }
                        else
                        {
                            return "Failed";
                        }
                    }
                    catch (Exception ex)
                    {
                        Task WriteTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "DeleteWishlistproduct : errormessage:" + ex.Message.ToString()));
                        throw;
                    }
                }
            }

        }



    }
}

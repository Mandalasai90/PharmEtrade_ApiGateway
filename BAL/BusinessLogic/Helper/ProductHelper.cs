﻿using BAL.BusinessLogic.Interface;
using Microsoft.Extensions.Configuration;
using System.Data;
using DAL;
using BAL.Common;
using BAL.ViewModels;
using OfficeOpenXml;
using MySql.Data.MySqlClient;
using BAL.ResponseModels;
using BAL.Models;
using Microsoft.AspNetCore.Http;
using BAL.RequestModels;

namespace BAL.BusinessLogic.Helper
{
    public class ProductHelper : IProductHelper
    {
        private readonly IsqlDataHelper _isqlDataHelper;
        private readonly string _connectionString;
        private readonly string _exPathToSave;
        private readonly IConfiguration _configuration;
        private readonly S3Helper _s3Helper;
        public ProductHelper(IConfiguration configuration, IsqlDataHelper isqlDataHelper)
        {
            _s3Helper = new S3Helper(configuration);
            _configuration = configuration;
            _isqlDataHelper = isqlDataHelper;
            _connectionString = configuration.GetConnectionString("APIDBConnectionString");
            _exPathToSave = Path.Combine(Directory.GetCurrentDirectory(), "ProductExceptionLogs");
        }

        public async Task<string> InsertProductsFromExcel(Stream excelFileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelFileStream))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assuming the first worksheet
                var rowCount = worksheet.Dimension.Rows;

                using (MySqlConnection sqlcon = new MySqlConnection(_connectionString))
                {
                    MySqlTransaction transaction = null;
                    try
                    {
                        await sqlcon.OpenAsync();

                        // Begin a transaction
                        transaction = await sqlcon.BeginTransactionAsync();

                        for (int row = 2; row <= rowCount; row++) // Assuming the first row is the header
                        {
                            // Extracting product data from the worksheet
                            var productViewModel = new ProductFilter
                            {
                                Productcategory_id = Convert.ToInt32(worksheet.Cells[row, 1].Value),
                                Caption = worksheet.Cells[row, 2].Value.ToString(),
                                Sizeid = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                                ProductName = worksheet.Cells[row, 4].Value.ToString(),
                                NDCorUPC = worksheet.Cells[row, 5].Value.ToString(),
                                BrandName = worksheet.Cells[row, 6].Value.ToString(),
                                PriceName = Convert.ToDecimal(worksheet.Cells[row, 7].Value),
                                UPNmemberPrice = Convert.ToDecimal(worksheet.Cells[row, 8].Value),
                                AmountInStock = Convert.ToInt32(worksheet.Cells[row, 9].Value),
                                Taxable = Convert.ToBoolean(worksheet.Cells[row, 10].Value),
                                SalePrice = Convert.ToDecimal(worksheet.Cells[row, 11].Value),
                                SalePriceFrom = Convert.ToDateTime(worksheet.Cells[row, 12].Value),
                                SalePriceTo = Convert.ToDateTime(worksheet.Cells[row, 13].Value),
                                Manufacturer = worksheet.Cells[row, 14].Value.ToString(),
                                Strength = worksheet.Cells[row, 15].Value.ToString(),
                                Fromdate = Convert.ToDateTime(worksheet.Cells[row, 16].Value),
                                LotNumber = worksheet.Cells[row, 17].Value.ToString(),
                                ExpirationDate = Convert.ToDateTime(worksheet.Cells[row, 18].Value),
                                PackQuantity = Convert.ToInt32(worksheet.Cells[row, 19].Value),
                                PackType = worksheet.Cells[row, 20].Value.ToString(),
                                PackCondition = worksheet.Cells[row, 21].Value.ToString(),
                                ProductDescription = worksheet.Cells[row, 22].Value.ToString(),
                                MetaKeywords = worksheet.Cells[row, 23].Value.ToString(),
                                MetaTitle = worksheet.Cells[row, 24].Value.ToString(),
                                MetaDescription = worksheet.Cells[row, 25].Value.ToString(),
                                SaltComposition = worksheet.Cells[row, 26].Value.ToString(),
                                UriKey = worksheet.Cells[row, 27].Value.ToString(),
                                AboutTheProduct = worksheet.Cells[row, 28].Value.ToString(),
                                CategorySpecificationId = Convert.ToInt32(worksheet.Cells[row, 29].Value),
                                ProductTypeId = Convert.ToInt32(worksheet.Cells[row, 30].Value),
                                SellerId = worksheet.Cells[row, 31].Value.ToString()
                            };

                            // Assuming image file is stored locally or in a place from where you can read the stream
                            var imageFileName = worksheet.Cells[row, 32].Value.ToString();
                            using (var imageFileStream = new FileStream(imageFileName, FileMode.Open))
                            {
                                int imageID;
                                using (MySqlCommand cmdImage = new MySqlCommand("InsertImageUrl", sqlcon, transaction))
                                {
                                    cmdImage.CommandType = CommandType.StoredProcedure;
                                    cmdImage.Parameters.AddWithValue("@p_ImageUrl", "imageUrl"); // Placeholder URL
                                    cmdImage.Parameters.AddWithValue("@p_Caption", productViewModel.Caption);
                                    MySqlParameter imageIDParam = new MySqlParameter("@p_ImageID", MySqlDbType.Int32)
                                    {
                                        Direction = ParameterDirection.Output
                                    };
                                    cmdImage.Parameters.Add(imageIDParam);

                                    await cmdImage.ExecuteNonQueryAsync();
                                    imageID = (int)imageIDParam.Value;
                                }

                                using (MySqlCommand cmdProduct = new MySqlCommand("InsertAddProduct", sqlcon, transaction))
                                {
                                    cmdProduct.CommandType = CommandType.StoredProcedure;
                                    cmdProduct.Parameters.AddWithValue("@p_Productcategory_id", productViewModel.Productcategory_id);
                                    cmdProduct.Parameters.AddWithValue("@p_ImageID", imageID);
                                    cmdProduct.Parameters.AddWithValue("@p_Sizeid", productViewModel.Sizeid);
                                    cmdProduct.Parameters.AddWithValue("@p_ProductName", productViewModel.ProductName);
                                    cmdProduct.Parameters.AddWithValue("@p_NDCorUPC", productViewModel.NDCorUPC);
                                    cmdProduct.Parameters.AddWithValue("@p_BrandName", productViewModel.BrandName);
                                    cmdProduct.Parameters.AddWithValue("@p_PriceName", productViewModel.PriceName);
                                    cmdProduct.Parameters.AddWithValue("@p_UPNmemberPrice", productViewModel.UPNmemberPrice);
                                    cmdProduct.Parameters.AddWithValue("@p_AmountInStock", productViewModel.AmountInStock);
                                    cmdProduct.Parameters.AddWithValue("@p_Taxable", productViewModel.Taxable);
                                    cmdProduct.Parameters.AddWithValue("@p_SalePrice", productViewModel.SalePrice);
                                    cmdProduct.Parameters.AddWithValue("@p_SalePriceFrom", productViewModel.SalePriceFrom);
                                    cmdProduct.Parameters.AddWithValue("@p_SalePriceTo", productViewModel.SalePriceTo);
                                    cmdProduct.Parameters.AddWithValue("@p_Manufacturer", productViewModel.Manufacturer);
                                    cmdProduct.Parameters.AddWithValue("@p_Strength", productViewModel.Strength);
                                    cmdProduct.Parameters.AddWithValue("@p_Fromdate", productViewModel.Fromdate);
                                    cmdProduct.Parameters.AddWithValue("@p_LotNumber", productViewModel.LotNumber);
                                    cmdProduct.Parameters.AddWithValue("@p_ExpirationDate", productViewModel.ExpirationDate);
                                    cmdProduct.Parameters.AddWithValue("@p_PackQuantity", productViewModel.PackQuantity);
                                    cmdProduct.Parameters.AddWithValue("@p_PackType", productViewModel.PackType);
                                    cmdProduct.Parameters.AddWithValue("@p_PackCondition", productViewModel.PackCondition);
                                    cmdProduct.Parameters.AddWithValue("@p_ProductDescription", productViewModel.ProductDescription);
                                    cmdProduct.Parameters.AddWithValue("@p_MetaKeywords", productViewModel.MetaKeywords);
                                    cmdProduct.Parameters.AddWithValue("@p_MetaTitle", productViewModel.MetaTitle);
                                    cmdProduct.Parameters.AddWithValue("@p_MetaDescription", productViewModel.MetaDescription);
                                    cmdProduct.Parameters.AddWithValue("@p_SaltComposition", productViewModel.SaltComposition);
                                    cmdProduct.Parameters.AddWithValue("@p_UriKey", productViewModel.UriKey);
                                    cmdProduct.Parameters.AddWithValue("@p_AboutTheProduct", productViewModel.AboutTheProduct);
                                    cmdProduct.Parameters.AddWithValue("@p_CategorySpecificationId", productViewModel.CategorySpecificationId);
                                    cmdProduct.Parameters.AddWithValue("@p_ProductTypeId", productViewModel.ProductTypeId);
                                    cmdProduct.Parameters.AddWithValue("@p_SellerId", productViewModel.SellerId);

                                    await cmdProduct.ExecuteNonQueryAsync();
                                }

                                string folderName = "PharmaEtrade";
                                string imageUrl = await _s3Helper.UploadFileAsync(imageFileStream, folderName, imageFileName);

                                using (MySqlCommand cmdUpdateImage = new MySqlCommand("UpdateImageUrl", sqlcon, transaction))
                                {
                                    cmdUpdateImage.CommandType = CommandType.StoredProcedure;
                                    cmdUpdateImage.Parameters.AddWithValue("@p_ImageID", imageID);
                                    cmdUpdateImage.Parameters.AddWithValue("@p_ImageUrl", imageUrl);
                                    cmdUpdateImage.Parameters.AddWithValue("@p_caption", productViewModel.Caption);

                                    await cmdUpdateImage.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        // Commit the transaction
                        await transaction.CommitAsync();
                        return "Success";
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                        }
                        Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProductsFromExcel :  errormessage:" + ex.Message));
                        throw;
                    }
                }
            }
        }

        public async Task<Response<ProductResponse>> AddProduct(Product product)
        {
            Response<ProductResponse> response = new Response<ProductResponse>();
            try
            {
                MySqlCommand cmdProduct = new MySqlCommand("sp_AddUpdateProduct");
                cmdProduct.CommandType = CommandType.StoredProcedure;

                cmdProduct.Parameters.AddWithValue("@p_ProductId", product.ProductID);
                cmdProduct.Parameters.AddWithValue("@p_ProductGalleryId", product.ProductGalleryId);
                cmdProduct.Parameters.AddWithValue("@p_ProductCategoryId", product.ProductCategoryId);
                cmdProduct.Parameters.AddWithValue("@p_ProductSizeId", product.ProductSizeId);
                cmdProduct.Parameters.AddWithValue("@p_ProductName", product.ProductName);
                cmdProduct.Parameters.AddWithValue("@p_NDCorUPC", product.NDCorUPC);
                cmdProduct.Parameters.AddWithValue("@p_BrandName", product.BrandName);
                cmdProduct.Parameters.AddWithValue("@p_PriceName", product.PriceName);
                cmdProduct.Parameters.AddWithValue("@p_UPNMemberPrice", product.UPNMemberPrice);
                cmdProduct.Parameters.AddWithValue("@p_AmountInStock", product.AmountInStock);
                cmdProduct.Parameters.AddWithValue("@p_Taxable", product.Taxable);
                cmdProduct.Parameters.AddWithValue("@p_SalePrice", product.SalePrice);
                cmdProduct.Parameters.AddWithValue("@p_SalePriceValidFrom", product.SalePriceValidFrom);
                cmdProduct.Parameters.AddWithValue("@p_SalePriceValidTo", product.SalePriceValidTo);
                cmdProduct.Parameters.AddWithValue("@p_Manufacturer", product.Manufacturer);
                cmdProduct.Parameters.AddWithValue("@p_Strength", product.Strength);
                cmdProduct.Parameters.AddWithValue("@p_AvailableFromDate", product.AvailableFromDate);
                cmdProduct.Parameters.AddWithValue("@p_LotNumber", product.LotNumber);
                cmdProduct.Parameters.AddWithValue("@p_ExpiryDate", product.ExpiryDate);
                cmdProduct.Parameters.AddWithValue("@p_PackQuantity", product.PackQuantity);
                cmdProduct.Parameters.AddWithValue("@p_PackType", product.PackType);
                cmdProduct.Parameters.AddWithValue("@p_PackCondition", product.PackCondition);
                cmdProduct.Parameters.AddWithValue("@p_ProductDescription", product.ProductDescription);
                cmdProduct.Parameters.AddWithValue("@p_MetaKeywords", product.MetaKeywords);
                cmdProduct.Parameters.AddWithValue("@p_MetaTitle", product.MetaTitle);
                cmdProduct.Parameters.AddWithValue("@p_MetaDescription", product.MetaDescription);
                cmdProduct.Parameters.AddWithValue("@p_SaltComposition", product.SaltComposition);
                cmdProduct.Parameters.AddWithValue("@p_UriKey", product.UriKey);
                cmdProduct.Parameters.AddWithValue("@p_AboutTheProduct", product.AboutTheProduct);
                cmdProduct.Parameters.AddWithValue("@p_CategorySpecificationId", product.CategorySpecificationId);
                cmdProduct.Parameters.AddWithValue("@p_ProductTypeId", product.ProductTypeId);
                cmdProduct.Parameters.AddWithValue("@p_SellerId", product.SellerId);
                cmdProduct.Parameters.AddWithValue("@p_ImageUrl", product.ImageUrl);
                cmdProduct.Parameters.AddWithValue("@p_Caption", product.Caption);
                cmdProduct.Parameters.AddWithValue("@p_States", product.States);
                cmdProduct.Parameters.AddWithValue("@p_Thumbnail1", product.Thumbnail1);
                cmdProduct.Parameters.AddWithValue("@p_Thumbnail2", product.Thumbnail2);
                cmdProduct.Parameters.AddWithValue("@p_Thumbnail3", product.Thumbnail3);
                cmdProduct.Parameters.AddWithValue("@p_Thumbnail4", product.Thumbnail4);
                cmdProduct.Parameters.AddWithValue("@p_Thumbnail5", product.Thumbnail5);
                cmdProduct.Parameters.AddWithValue("@p_Thumbnail6", product.Thumbnail6);
                cmdProduct.Parameters.AddWithValue("@p_VideoUrl", product.VideoUrl);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProduct));

                response.StatusCode = 200;
                response.Message = "Product Added Successfully.";
                response.Result = MapDataTableToProductList(tblProduct);
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProduct :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return response;
        }
        private static List<ProductResponse> MapDataTableToProductList(DataTable tblProduct)
        {
            List<ProductResponse> lstProduct = new List<ProductResponse>();
            foreach (DataRow product in tblProduct.Rows)
            {
                ProductResponse item = new ProductResponse();
                item.ProductID = product["ProductID"].ToString() ?? "";
                item.ProductName = product["ProductName"].ToString() ?? "";
                item.NDCorUPC = product["NDCorUPC"].ToString() ?? "";
                item.BrandName = product["BrandName"].ToString() ?? "";
                item.Manufacturer = product["Manufacturer"].ToString() ?? "";
                item.Strength = product["Strength"].ToString() ?? "";
                item.AvailableFromDate = Convert.ToDateTime(Convert.IsDBNull(product["AvailableFromDate"]) ? DateTime.MinValue : product["AvailableFromDate"]);
                item.FormattedAvailableFromDate = item.AvailableFromDate.ToString("MM/yyyy");
                item.LotNumber = product["LotNumber"].ToString() ?? "";
                item.ExpiryDate = Convert.ToDateTime(Convert.IsDBNull(product["ExpiryDate"]) ? DateTime.MinValue : product["ExpiryDate"]);
                item.FormattedExpiryDate = item.ExpiryDate.ToString("MM/yyyy");
                item.PackQuantity = Convert.ToInt32(Convert.IsDBNull(product["PackQuantity"]) ? 0 : product["PackQuantity"]);
                item.PackType = product["PackType"].ToString() ?? "";
                item.PackCondition = product["PackCondition"].ToString() ?? "";
                item.ProductDescription = product["ProductDescription"].ToString() ?? "";
                item.AboutTheProduct = product["AboutTheProduct"].ToString() ?? "";
                item.SellerId = product["SellerId"].ToString() ?? "";
                item.States = product["States"].ToString() ?? "";
                item.UnitOfMeasure = product["UnitOfMeasure"].ToString() ?? "";

                item.ProductPriceId = product["ProductPriceId"].ToString() ?? "";
                //item.ProductId = product["ProductId"].ToString() ?? "";
                item.UnitPrice = Convert.ToDecimal(Convert.IsDBNull(product["UnitPrice"]) ? 0.0 : product["UnitPrice"]);
                item.UPNMemberPrice = Convert.ToDecimal(Convert.IsDBNull(product["UPNMemberPrice"]) ? 0.0 : product["UPNMemberPrice"]);
                item.Discount = Convert.ToInt32(Convert.IsDBNull(product["Discount"]) ? 0 : product["Discount"]);
                item.SalePrice = Convert.ToDecimal(Convert.IsDBNull(product["SalePrice"]) ? 0.0 : product["SalePrice"]);
                item.SalePriceValidFrom = Convert.ToDateTime(Convert.IsDBNull(product["SalePriceValidFrom"]) ? DateTime.MinValue : product["SalePriceValidFrom"]);
                item.SalePriceValidTo = Convert.ToDateTime(Convert.IsDBNull(product["SalePriceValidTo"]) ? DateTime.MinValue : product["SalePriceValidTo"]);
                item.Taxable = Convert.ToInt32(Convert.IsDBNull(product["Taxable"]) ? 0 : product["Taxable"]) == 1 ? true : false;
                item.ShippingCostApplicable = Convert.ToInt32(Convert.IsDBNull(product["ShippingCostApplicable"]) ? 0 : product["ShippingCostApplicable"]) == 1 ? true : false;
                item.ShippingCost = Convert.ToDecimal(Convert.IsDBNull(product["ShippingCost"]) ? 0.0 : product["ShippingCost"]);
                item.AmountInStock = Convert.ToInt32(Convert.IsDBNull(product["AmountInStock"]) ? 0 : product["AmountInStock"]);

                item.ProductCategory.ProductCategoryId = Convert.ToInt32(product["ProductCategoryId"] != DBNull.Value ? product["ProductCategoryId"] : 0);
                item.ProductCategory.CategoryName = product["CategoryName"].ToString() ?? "";

                item.ProductGallery.ProductGalleryId = product["ProductGalleryId"].ToString() ?? "";
                item.ProductGallery.ImageUrl = product["ImageUrl"].ToString() ?? "";
                item.ProductGallery.Caption = product["Caption"].ToString() ?? "";
                item.ProductGallery.Thumbnail1 = product["Thumbnail1"].ToString() ?? "";
                item.ProductGallery.Thumbnail2 = product["Thumbnail2"].ToString() ?? "";
                item.ProductGallery.Thumbnail3 = product["Thumbnail3"].ToString() ?? "";
                item.ProductGallery.Thumbnail4 = product["Thumbnail4"].ToString() ?? "";
                item.ProductGallery.Thumbnail5 = product["Thumbnail5"].ToString() ?? "";
                item.ProductGallery.Thumbnail6 = product["Thumbnail6"].ToString() ?? "";
                item.ProductGallery.VideoUrl = product["VideoUrl"].ToString() ?? "";

                item.CategorySpecification.CategorySpecificationId = Convert.ToInt32(product["CategorySpecificationId"] != DBNull.Value ? product["CategorySpecificationId"] : 0);
                item.CategorySpecification.SpecificationName = product["SpecificationName"].ToString() ?? "";

                item.ProductType.ProductTypeId = Convert.ToInt32(product["ProductTypeId"] != DBNull.Value ? product["ProductTypeId"] : 0);
                item.ProductType.Description = product["ProductType"].ToString() ?? "";

                lstProduct.Add(item);
            }
            return lstProduct;
        }

        public async Task<UploadResponse> UploadImage(IFormFile image, string sellerId, string productId)
        {
            UploadResponse response = new UploadResponse();
            string folderName = string.Format("PharmaEtrade/Products/{0}/{1}", sellerId, productId);
            try
            {
                // Upload files to S3
                if (image != null)
                {
                    response.ImageUrl = await _s3Helper.UploadFileAsync(image, folderName);
                    response.Status = 200;
                    response.Message = "Image Uploaded Successfully.";
                }
            }
            catch (Exception ex)
            {
                //Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(exPathToSave, "SaveBusinessInfoData: errormessage:" + ex.Message.ToString()));

                response.Status = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<ProductResponse>> GetAllProducts(string productId = null)
        {
            Response<ProductResponse> response = new Response<ProductResponse>();
            try
            {
                MySqlCommand cmdProduct = new MySqlCommand(StoredProcedures.GET_ALL_PRODUCTS);
                cmdProduct.CommandType = CommandType.StoredProcedure;
                cmdProduct.Parameters.AddWithValue("@p_ProductId", productId);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProduct));
                response.StatusCode = 200;
                response.Message = "Successfully Fetched Data.";
                response.Result = MapDataTableToProductList(tblProduct);
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProduct :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public async Task<Response<ProductResponse>> GetProductsBySpecification(int categorySpecificationId)
        {
            Response<ProductResponse> response = new Response<ProductResponse>();
            try
            {
                MySqlCommand cmdProduct = new MySqlCommand("sp_GetProductsBySpecification");
                cmdProduct.CommandType = CommandType.StoredProcedure;
                cmdProduct.Parameters.AddWithValue("@p_CategorySpecificationId", categorySpecificationId);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProduct));
                response.StatusCode = 200;
                response.Message = "Successfully Fetched Data.";
                response.Result = MapDataTableToProductList(tblProduct);
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProduct :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public async Task<Response<ProductResponse>> GetRecentSoldProducts(int numberOfProducts)
        {
            Response<ProductResponse> response = new Response<ProductResponse>();
            try
            {
                MySqlCommand cmdProduct = new MySqlCommand("sp_GetRecentSoldProducts");
                cmdProduct.CommandType = CommandType.StoredProcedure;
                cmdProduct.Parameters.AddWithValue("@p_NumberOfProducts", numberOfProducts);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProduct));
                response.StatusCode = 200;
                response.Message = "Successfully Fetched Data.";
                response.Result = MapDataTableToProductList(tblProduct);
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProduct :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public async Task<Response<ProductResponse>> GetProductsBySeller(string sellerId)
        {
            Response<ProductResponse> response = new Response<ProductResponse>();
            try
            {
                MySqlCommand cmdProduct = new MySqlCommand(StoredProcedures.GET_PRODUCTS_BY_SELLER);
                cmdProduct.CommandType = CommandType.StoredProcedure;
                cmdProduct.Parameters.AddWithValue("@p_SellerId", sellerId);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProduct));
                response.StatusCode = 200;
                response.Message = "Successfully Fetched Data.";
                response.Result = MapDataTableToProductList(tblProduct);
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProduct :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public async Task<Response<ProductSize>> AddUpdateProductSize(ProductSize productSize)
        {
            Response<ProductSize> response = new Response<ProductSize>();
            try
            {
                MySqlCommand cmdProduct = new MySqlCommand("sp_AddUpdateProductSize");
                cmdProduct.CommandType = CommandType.StoredProcedure;

                cmdProduct.Parameters.AddWithValue("@p_ProductSizeId", productSize.ProductSizeId);
                cmdProduct.Parameters.AddWithValue("@p_Height", productSize.Height);
                cmdProduct.Parameters.AddWithValue("@p_Width", productSize.Width);
                cmdProduct.Parameters.AddWithValue("@p_Length", productSize.Length);
                cmdProduct.Parameters.AddWithValue("@p_Weight", productSize.Weight);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProduct));

                var objProductSize = new ProductSize();
                if (tblProduct?.Rows.Count > 0)
                {
                    objProductSize.ProductSizeId = Convert.ToInt32(tblProduct.Rows[0]["ProductSizeId"]);
                    objProductSize.Height = Convert.ToDecimal(tblProduct.Rows[0]["Height"]);
                    objProductSize.Width = Convert.ToDecimal(tblProduct.Rows[0]["Width"]);
                    objProductSize.Length = Convert.ToDecimal(tblProduct.Rows[0]["Length"]);
                    objProductSize.Weight = Convert.ToDecimal(tblProduct.Rows[0]["Weight"]);
                }
                else objProductSize = null;

                response.StatusCode = 200;
                response.Message = "ProductSize Updated Successfully.";
                response.Result = new List<ProductSize>() { objProductSize };
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "Add Update Product Size :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<ProductResponse>> GetProductsByCriteria(ProductCriteria criteria)
        {
            Response<ProductResponse> response = new Response<ProductResponse>();
            try
            {
                MySqlCommand cmdProduct = new MySqlCommand(StoredProcedures.GET_PRODUCTS_BY_CRITERIA);
                cmdProduct.CommandType = CommandType.StoredProcedure;
                cmdProduct.Parameters.AddWithValue("@p_Manufacturer", criteria.Brands);
                cmdProduct.Parameters.AddWithValue("@p_Discount", criteria.Discount);
                cmdProduct.Parameters.AddWithValue("@p_ExpiringMonths", criteria.Expiring);
                cmdProduct.Parameters.AddWithValue("@p_OTCProducts", string.Compare(criteria.OTCProducts, "otc", true));
                cmdProduct.Parameters.AddWithValue("@p_RxProducts", string.Compare(criteria.PrescriptionDrugs, "rx", true));
                cmdProduct.Parameters.AddWithValue("@p_VAWDSeller", criteria.VAWDSeller);
                cmdProduct.Parameters.AddWithValue("@p_TopSelling", string.Compare(criteria.TopSellingProducts, "topselling", true));
                cmdProduct.Parameters.AddWithValue("@p_BuyAgain", string.Compare(criteria.BuyAgain, "buyagain", true));

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProduct));
                response.StatusCode = 200;
                response.Message = "Successfully Fetched Data.";
                response.Result = MapDataTableToProductList(tblProduct);
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "InsertProduct :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public async Task<Response<ProductInfo>> AddUpdateProductInfo(ProductInfo productInfo)
        {
            Response<ProductInfo> response = new Response<ProductInfo>();
            try
            {
                MySqlCommand cmdProductInfo = new MySqlCommand(StoredProcedures.ADD_UPDATE_PRODUCT_INFO);
                cmdProductInfo.CommandType = CommandType.StoredProcedure;

                cmdProductInfo.Parameters.AddWithValue("@p_ProductId", productInfo.ProductID);
                cmdProductInfo.Parameters.AddWithValue("@p_ProductCategoryId", productInfo.ProductCategoryId);
                cmdProductInfo.Parameters.AddWithValue("@p_ProductName", productInfo.ProductName);
                cmdProductInfo.Parameters.AddWithValue("@p_NDCorUPC", productInfo.NDCorUPC);
                cmdProductInfo.Parameters.AddWithValue("@p_BrandName", productInfo.BrandName);
                cmdProductInfo.Parameters.AddWithValue("@p_Manufacturer", productInfo.Manufacturer);
                cmdProductInfo.Parameters.AddWithValue("@p_Size", productInfo.Size);
                cmdProductInfo.Parameters.AddWithValue("@p_UnitOfMeasure", productInfo.UnitOfMeasure);
                cmdProductInfo.Parameters.AddWithValue("@p_Strength", productInfo.Strength);
                cmdProductInfo.Parameters.AddWithValue("@p_AvailableFromDate", productInfo.AvailableFromDate);
                cmdProductInfo.Parameters.AddWithValue("@p_LotNumber", productInfo.LotNumber);
                cmdProductInfo.Parameters.AddWithValue("@p_ExpiryDate", productInfo.ExpiryDate);
                cmdProductInfo.Parameters.AddWithValue("@p_PackQuantity", productInfo.PackQuantity);
                cmdProductInfo.Parameters.AddWithValue("@p_PackType", productInfo.PackType);
                cmdProductInfo.Parameters.AddWithValue("@p_PackCondition", productInfo.PackCondition);
                cmdProductInfo.Parameters.AddWithValue("@p_ProductDescription", productInfo.ProductDescription);
                cmdProductInfo.Parameters.AddWithValue("@p_AboutTheProduct", productInfo.AboutTheProduct);
                cmdProductInfo.Parameters.AddWithValue("@p_CategorySpecificationId", productInfo.CategorySpecificationId);
                cmdProductInfo.Parameters.AddWithValue("@p_ProductTypeId", productInfo.ProductTypeId);
                cmdProductInfo.Parameters.AddWithValue("@p_SellerId", productInfo.SellerId);
                cmdProductInfo.Parameters.AddWithValue("@p_States", productInfo.States);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProductInfo));

                var objProductInfo = new ProductInfo();
                if (tblProduct?.Rows.Count > 0)
                {
                    objProductInfo.ProductID = tblProduct.Rows[0]["ProductId"].ToString() ?? "";
                    objProductInfo.ProductCategoryId = Convert.ToInt32(tblProduct.Rows[0]["ProductCategoryId"] ?? 0);
                    objProductInfo.ProductName = tblProduct.Rows[0]["ProductName"].ToString() ?? "";
                    objProductInfo.NDCorUPC = tblProduct.Rows[0]["NDCorUPC"].ToString() ?? "";
                    objProductInfo.BrandName = tblProduct.Rows[0]["BrandName"].ToString() ?? "";
                    objProductInfo.Manufacturer = tblProduct.Rows[0]["Manufacturer"].ToString() ?? "";
                    objProductInfo.Size = tblProduct.Rows[0]["Size"].ToString() ?? "";
                    objProductInfo.UnitOfMeasure = tblProduct.Rows[0]["UnitOfMeasure"].ToString() ?? "";
                    objProductInfo.Strength = tblProduct.Rows[0]["Strength"].ToString() ?? "";
                    objProductInfo.AvailableFromDate = Convert.ToDateTime(tblProduct.Rows[0]["AvailableFromDate"] ?? DateTime.MinValue);
                    objProductInfo.LotNumber = tblProduct.Rows[0]["LotNumber"].ToString() ?? "";
                    objProductInfo.ExpiryDate = Convert.ToDateTime(tblProduct.Rows[0]["ExpiryDate"] ?? DateTime.MinValue);
                    objProductInfo.PackQuantity = Convert.ToInt32(tblProduct.Rows[0]["PackQuantity"] ?? 0);
                    objProductInfo.PackCondition = tblProduct.Rows[0]["PackCondition"].ToString() ?? "";
                    objProductInfo.PackType = tblProduct.Rows[0]["PackType"].ToString() ?? "";
                    objProductInfo.ProductDescription = tblProduct.Rows[0]["ProductDescription"].ToString() ?? "";
                    objProductInfo.AboutTheProduct = tblProduct.Rows[0]["AboutTheProduct"].ToString() ?? "";
                    objProductInfo.CategorySpecificationId = Convert.ToInt32(tblProduct.Rows[0]["CategorySpecificationId"] ?? 0);
                    objProductInfo.ProductTypeId = Convert.ToInt32(tblProduct.Rows[0]["ProductTypeId"] ?? 0);
                    objProductInfo.SellerId = tblProduct.Rows[0]["SellerId"].ToString() ?? "";
                    objProductInfo.States = tblProduct.Rows[0]["States"].ToString() ?? "";
                }

                response.StatusCode = 200;
                response.Message = "ProductInfo Updated Successfully.";
                response.Result = new List<ProductInfo>() { objProductInfo };
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "Add Update Product Size :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<ProductPrice>> AddUpdateProductPrice(ProductPrice productPrice)
        {
            Response<ProductPrice> response = new Response<ProductPrice>();
            try
            {
                MySqlCommand cmdProductPrice = new MySqlCommand(StoredProcedures.ADD_UPDATE_PRODUCT_PRICE);
                cmdProductPrice.CommandType = CommandType.StoredProcedure;

                cmdProductPrice.Parameters.AddWithValue("@p_ProductPriceId", productPrice.ProductPriceId);
                cmdProductPrice.Parameters.AddWithValue("@p_ProductId", productPrice.ProductId);
                cmdProductPrice.Parameters.AddWithValue("@p_UnitPrice", productPrice.UnitPrice);
                cmdProductPrice.Parameters.AddWithValue("@p_UPNMemberPrice", productPrice.UPNMemberPrice);
                cmdProductPrice.Parameters.AddWithValue("@p_Discount", productPrice.Discount);
                cmdProductPrice.Parameters.AddWithValue("@p_SalePrice", productPrice.SalePrice);
                cmdProductPrice.Parameters.AddWithValue("@p_SalePriceValidFrom", productPrice.SalePriceValidFrom);
                cmdProductPrice.Parameters.AddWithValue("@p_SalePriceValidTo", productPrice.SalePriceValidTo);
                cmdProductPrice.Parameters.AddWithValue("@p_Taxable", productPrice.Taxable);
                cmdProductPrice.Parameters.AddWithValue("@p_ShippingCostApplicable", productPrice.ShippingCostApplicable);
                cmdProductPrice.Parameters.AddWithValue("@p_ShippingCost", productPrice.ShippingCost);
                cmdProductPrice.Parameters.AddWithValue("@p_AmountInStock", productPrice.AmountInStock);

                DataTable tblProduct = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProductPrice));

                var objProductPrice = new ProductPrice();
                if (tblProduct != null && tblProduct.Rows.Count > 0 && tblProduct.Rows[0] != null)
                {
                    objProductPrice.ProductPriceId = tblProduct.Rows[0]["ProductPriceId"].ToString() ?? "";
                    objProductPrice.ProductId = tblProduct.Rows[0]["ProductId"].ToString() ?? "";
                    objProductPrice.UnitPrice = Convert.ToDecimal(tblProduct.Rows[0]["UnitPrice"] ?? 0.0);
                    objProductPrice.UPNMemberPrice = Convert.ToDecimal(tblProduct.Rows[0]["UPNMemberPrice"] ?? 0.0);
                    objProductPrice.Discount = Convert.ToInt32(tblProduct.Rows[0]["Discount"] ?? 0);
                    objProductPrice.SalePrice = Convert.ToDecimal(tblProduct.Rows[0]["SalePrice"] ?? 0.0);
                    objProductPrice.SalePriceValidFrom = Convert.ToDateTime(tblProduct.Rows[0]["SalePriceValidFrom"] ?? DateTime.MinValue);
                    objProductPrice.SalePriceValidTo = Convert.ToDateTime(tblProduct.Rows[0]["SalePriceValidTo"] ?? DateTime.MinValue);
                    objProductPrice.Taxable = Convert.ToInt32(tblProduct.Rows[0]["Taxable"] ?? 0) == 1 ? true : false;
                    objProductPrice.ShippingCostApplicable = Convert.ToInt32(tblProduct.Rows[0]["ShippingCostApplicable"] ?? 0) == 1 ? true : false;
                    objProductPrice.ShippingCost = Convert.ToDecimal(tblProduct.Rows[0]["ShippingCost"] ?? 0.0);
                    objProductPrice.AmountInStock = Convert.ToInt32(tblProduct.Rows[0]["AmountInStock"] ?? 0);
                }

                response.StatusCode = 200;
                response.Message = "Product Price Details Updated Successfully.";
                response.Result = new List<ProductPrice>() { objProductPrice };
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "Add Update Product Size :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<ProductGallery>> AddUpdateProductGallery(ProductGallery productGallery)
        {
            Response<ProductGallery> response = new Response<ProductGallery>();
            try
            {
                MySqlCommand cmdProductGallery = new MySqlCommand(StoredProcedures.ADD_UPDATE_PRODUCT_GALLERY);
                cmdProductGallery.CommandType = CommandType.StoredProcedure;

                cmdProductGallery.Parameters.AddWithValue("@p_ProductGalleryId", productGallery.ProductGalleryId);
                cmdProductGallery.Parameters.AddWithValue("@p_ProductId", productGallery.ProductId);
                cmdProductGallery.Parameters.AddWithValue("@p_ImageUrl", productGallery.ImageUrl);
                cmdProductGallery.Parameters.AddWithValue("@p_Caption", productGallery.Caption);
                cmdProductGallery.Parameters.AddWithValue("@p_Thumbnail1", productGallery.Thumbnail1);
                cmdProductGallery.Parameters.AddWithValue("@p_Thumbnail2", productGallery.Thumbnail2);
                cmdProductGallery.Parameters.AddWithValue("@p_Thumbnail3", productGallery.Thumbnail3);
                cmdProductGallery.Parameters.AddWithValue("@p_Thumbnail4", productGallery.Thumbnail4);
                cmdProductGallery.Parameters.AddWithValue("@p_Thumbnail5", productGallery.Thumbnail5);
                cmdProductGallery.Parameters.AddWithValue("@p_Thumbnail6", productGallery.Thumbnail6);
                cmdProductGallery.Parameters.AddWithValue("@p_VideoUrl", productGallery.VideoUrl);

                DataTable tblProductGallery = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(cmdProductGallery));

                var objProductGallery = new ProductGallery();
                if (tblProductGallery?.Rows.Count > 0)
                {
                    objProductGallery.ProductGalleryId = tblProductGallery.Rows[0]["ProductGalleryId"].ToString() ?? "";
                    objProductGallery.ProductId = tblProductGallery.Rows[0]["ProductId"].ToString() ?? "";
                    objProductGallery.ImageUrl = tblProductGallery.Rows[0]["ImageUrl"].ToString() ?? "";
                    objProductGallery.Caption = tblProductGallery.Rows[0]["Caption"].ToString() ?? "";
                    objProductGallery.Thumbnail1 = tblProductGallery.Rows[0]["Thumbnail1"].ToString() ?? "";
                    objProductGallery.Thumbnail1 = tblProductGallery.Rows[0]["Thumbnail2"].ToString() ?? "";
                    objProductGallery.Thumbnail1 = tblProductGallery.Rows[0]["Thumbnail3"].ToString() ?? "";
                    objProductGallery.Thumbnail1 = tblProductGallery.Rows[0]["Thumbnail4"].ToString() ?? "";
                    objProductGallery.Thumbnail1 = tblProductGallery.Rows[0]["Thumbnail5"].ToString() ?? "";
                    objProductGallery.Thumbnail1 = tblProductGallery.Rows[0]["Thumbnail6"].ToString() ?? "";
                    objProductGallery.VideoUrl = tblProductGallery.Rows[0]["VideoUrl"].ToString() ?? "";
                }

                response.StatusCode = 200;
                response.Message = "Product Gallery Updated Successfully.";
                response.Result = new List<ProductGallery>() { objProductGallery };
            }
            catch (Exception ex)
            {
                Task writeTask = Task.Factory.StartNew(() => LogFileException.Write_Log_Exception(_exPathToSave, "Add Update Product Size :  errormessage:" + ex.Message));
                // Handle the exception as needed
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        private static List<SpecialOffersResponse> MapDataTableToSpecialOffers(DataTable tbloffers)
        {
            List<SpecialOffersResponse> listspecialoffers = new List<SpecialOffersResponse>();
            foreach (DataRow offers in tbloffers.Rows)
            {
                SpecialOffersResponse item = new SpecialOffersResponse();
                item.Discount = Convert.ToInt32(offers["Discount"] != DBNull.Value ? offers["Discount"] : 0);
                item.SpecificationName = offers["SpecificationName"].ToString() ?? "";
                item.CategorySpecificationId = Convert.ToInt32(offers["CategorySpecificationId"] != DBNull.Value ? offers["CategorySpecificationId"] : 0);

                listspecialoffers.Add(item);
            }
            return listspecialoffers;
        }



        public async Task<Response<SpecialOffersResponse>> GetSpecialOffers()
        {
            Response<SpecialOffersResponse> response = new Response<SpecialOffersResponse>();
            try
            {
                MySqlCommand command = new MySqlCommand("sp_GetSpecialOffers");
                command.CommandType = CommandType.StoredProcedure;
                DataTable tbloffers = await Task.Run(() => _isqlDataHelper.SqlDataAdapterasync(command));
                response.StatusCode = 200;
                response.Message = "Successfully Fetched Data.";
                response.Result = MapDataTableToSpecialOffers(tbloffers);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Result = null;
            }
            return response;
        }

    }
}


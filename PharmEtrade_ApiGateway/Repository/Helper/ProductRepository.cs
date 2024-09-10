﻿using BAL.BusinessLogic.Interface;
using BAL.ViewModel;
using BAL.ViewModels;
using PharmEtrade_ApiGateway.Repository.Interface;
using System.Threading.Tasks;
using BAL.Common;
using System.IO;
using BAL.Models;
using BAL.BusinessLogic.Helper;
using BAL.ResponseModels;
using BAL.RequestModels;

namespace PharmEtrade_ApiGateway.Repository.Helper
{
    public class ProductRepository : IProductsRepo
    {
        private readonly IProductHelper _productHelper;

        public ProductRepository(IProductHelper productHelper)
        {
            _productHelper = productHelper;
        }

        public async Task<Response> ProcessExcelFileAsync(IFormFile file)
        {
            Response response = new Response();

            if (file == null || file.Length == 0)
            {
                response.status = 400; // Bad Request
                response.message = "No file uploaded.";
                return response;
            }

            try
            {
                // Convert IFormFile to a Stream
                using (var excelFileStream = file.OpenReadStream())
                {
                    string status = await _productHelper.InsertProductsFromExcel(excelFileStream);

                    if (status.Equals("Success"))
                    {
                        response.status = 200; // OK
                        response.message = Constant.InsertAddProductSuccessMsg;
                    }
                    else
                    {
                        response.status = 500; // Internal Server Error
                        response.message = "An error occurred while processing the file.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.status = 500; // Internal Server Error
                response.message = ex.Message;
            }

            return response;
        }

        public async Task<BAL.ResponseModels.Response<ProductResponse>> AddProduct(Product product)
        {
            return await _productHelper.AddProduct(product);
        }

        public async Task<UploadResponse> UploadImage(IFormFile image, string sellerId, string productId)
        {
            UploadResponse response = new UploadResponse();
            try
            {
                response = await _productHelper.UploadImage(image, sellerId, productId);
            }
            catch (Exception ex)
            {
                //response.Status = 500;
                //response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<ProductResponse>> GetAllProducts(string productId = null)
        {
            return await _productHelper.GetAllProducts(productId);
        }

        public async Task<Response<ProductResponse>> GetProductsBySpecification(int categorySpecificationId)
        {
            return await _productHelper.GetProductsBySpecification(categorySpecificationId);
        }

        public async Task<Response<ProductResponse>> GetRecentSoldProducts(int numberOfProducts)
        {
            return await _productHelper.GetRecentSoldProducts(numberOfProducts);
        }

        public async Task<Response<ProductSize>> AddUpdateProductSize(ProductSize productSize)
        {
            return await _productHelper.AddUpdateProductSize(productSize);
        }

        public async Task<Response<ProductResponse>> GetProductsBySeller(string sellerId)
        {
            return await _productHelper.GetProductsBySeller(sellerId);
        }

        public async Task<Response<ProductResponse>> GetProductsByCriteria(ProductCriteria criteria)
        {
            return await _productHelper.GetProductsByCriteria(criteria);
        }

        public async Task<Response<ProductInfo>> AddUpdateProductInfo(ProductInfo productInfo)
        {
            return await _productHelper.AddUpdateProductInfo(productInfo);
        }

        public async Task<Response<ProductPrice>> AddUpdateProductPrice(ProductPrice productPrice)
        {
            return await _productHelper.AddUpdateProductPrice(productPrice);
        }

        public async Task<Response<ProductGallery>> AddUpdateProductGallery(ProductGallery productGallery)
        {
            return await _productHelper.AddUpdateProductGallery(productGallery);
        }
    }
}

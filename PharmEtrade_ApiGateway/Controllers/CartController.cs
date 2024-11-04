﻿using BAL.Models;
using BAL.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmEtrade_ApiGateway.Repository.Interface;

namespace PharmEtrade_ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private ICartRepository _cartRepository;
        public CartController(ICartRepository cartRepository) { 
            _cartRepository = cartRepository;
        }

        [HttpGet]
        [Route("GetCartItems")]
        public async Task<IActionResult> GetCartItems(string? customerId = null, string? productId = null)
        {
            var response = await _cartRepository.GetCartItems(customerId, productId);
            return Ok(response);
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddToCart(CartRequest request)
        {
            var response = await _cartRepository.AddToCart(request);
            return Ok(response);
        }
        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeleteCart(string CartId)
        {
            if (string.IsNullOrEmpty(CartId))
            {
                return BadRequest("CartId should not be null or empty.");
            }
            var response = await _cartRepository.DeleteCart(CartId);
            return Ok(response);

        }
    }
}
